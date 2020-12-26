using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Xml.Linq;
using System;
using UnityEngine.Networking;
using System.Threading;
using System.Security;

//Prefix patches LoadItemData because it is called right before modloading ends.
[HarmonyPatch]
static class CustomFileLoader_Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.LoadItemData))]
    static bool LoadItems(ItemManager __instance) {
        //Mod processing would not be marked as done when this is called during mod loading.
        if (S.I.modCtrl.processing) {
            CustomFileLoader.AfterInstall();
            return true;
        }
        return true;
    }
}

//Don't let anything access this.
//Basically, this thing is written to stop the install button from switching back to its default when mods are being loaded again.
internal static class CustomFileLoader
{

    //Function that is called when mod loading is almost done. Goes through all mods and looks for CustomFileTypes.xml, then passes it off to the loader for everything.
    public static void AfterInstall() {
        string[] directories = Directory.GetDirectories(ModCtrl.MODS_PATH);
        string[] strArray = directories;
        var list = new List<CustomFileType>();
        for (int index = 0; index < strArray.Length; ++index) {
            string path2 = strArray[index];
            string str = Path.Combine(ModCtrl.MODS_PATH, path2);
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            var temp = new List<CustomFileType>();
            var tempAssemblies = new List<FileInfo>();
            FindCustomFileTypes(directoryInfo, temp, tempAssemblies, true);
            list.AddRange(temp);
        }
        if (SteamManager.Initialized) {
            foreach (LapinerTools.Steam.Data.WorkshopItem workshopItem in LapinerTools.Steam.SteamMainBase<LapinerTools.Steam.SteamWorkshopMain>.Instance.m_activeItems.Values) {
                if (workshopItem.IsActive) {
                    DirectoryInfo dir = new DirectoryInfo(workshopItem.InstalledLocalFolder);
                    var temp = new List<CustomFileType>();
                    var tempAssemblies = new List<FileInfo>();
                    FindCustomFileTypes(dir, temp, tempAssemblies, true);
                    list.AddRange(temp);
                }
            }
        }
        ReadAllFiles(list, strArray);
        S.I.modCtrl.luaMods = S.I.modCtrl.luaMods.Distinct().ToList();
        S.I.modCtrl.pactMods = S.I.modCtrl.pactMods.Distinct().ToList();
        S.I.modCtrl.spellMods = S.I.modCtrl.spellMods.Distinct().ToList();
        S.I.modCtrl.artMods = S.I.modCtrl.artMods.Distinct().ToList();
    }

    //Looks for CustomFileTypes.xml throughout a mod. Looks for assemblies as well in order to load those file types with the correct handler.
    private static void FindCustomFileTypes(DirectoryInfo dir, List<CustomFileType> re, List<FileInfo> assembly_files, bool first) {
        var files = dir.GetFiles();
        var modCtrl = S.I.modCtrl;
        for (int i = 0; i < files.Length; i++) {
            FileInfo file = files[i];
            if (!file.Name.Contains(".meta")) {
                switch (file.Name) {
                    case "CustomFileTypes.xml":
                        re.AddRange(ReadCustomFileTypeInstructions(file.FullName));
                        break;
                    default:
                        if (file.Name.EndsWith(".dll")) {
                            assembly_files.Add(file);
                        }
                        break;
                }

            }
        }
        //Go through sub folders.
        foreach (DirectoryInfo directory in dir.GetDirectories()) {
            if (directory.Name.ToLower() != "disabled") FindCustomFileTypes(directory, re, assembly_files, false);
        }
        //If this is the base folder, find the correct handler for each file type.
        if (first) {
            foreach (CustomFileType customdatatype in re) {
                bool found = false;
                //Mods have multiple assemblies sometimes, so you gotta check all of them...
                foreach (var info in assembly_files) {
                    try 
                    {
                        var assem = Assembly.LoadFrom(info.FullName);
                        var type = assem.GetType(customdatatype.typename);
                        if (type != null) Debug.Log("Type for handler of CustomFileType '" + customdatatype.filename + "' found.");
                        customdatatype.handler = type.GetMethod(customdatatype.methodname);
                        found = true;
                        break;
                    } 
                    catch (FileLoadException ex) 
                    {
                        Debug.LogError("Could not load assembly for custom file loading. Found but cannot be loaded.");
                        Debug.LogException(ex);
                    } 
                    catch (SecurityException ex)
                    {
                        Debug.LogError("Could not load assembly for custom file loading. Security Exception.");
                        Debug.LogException(ex);
                    }
                    catch
                    {
                        //Errors are expected, just gotta deal with it.
                    }
                }
                if (!found)
                {
                    Debug.LogError("Could not find handler " + customdatatype.typename + ":" + customdatatype.methodname + " for files: " + customdatatype.filename);
                }
            }
            //Remove file types with null handler.
            re = (from c in re where c.handler != null select c).ToList();
        }
    }

    //XML Parser for CustomFileTypes.xml
    private static List<CustomFileType> ReadCustomFileTypeInstructions(string file) {
        var list = new List<CustomFileType>();
        XDocument doc = XDocument.Load(file);
        foreach (var node in doc.Root.Elements()) {
            if (node.Name == "CustomFileType") {
                CustomFileType type = new CustomFileType();

                foreach (var el in node.Elements()) {

                    if (!el.IsEmpty) {
                        switch (el.Name.ToString()) {
                            case "Filename":
                                if (el.HasAttributes) {
                                    try {
                                        type.contains = bool.Parse(el.Attribute("contains").Value);
                                    } catch {
                                        Debug.Log("Error parsing filename of custom file type, should only have 'contains' attribute with boolean value");
                                    }

                                }
                                type.filename = el.Value;
                                break;
                            case "HandlerType":
                                type.typename = el.Value;
                                break;
                            case "HandlerMethod":
                                type.methodname = el.Value;
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (type.filename != "") {
                    if (type.methodname != "" && type.typename != "") {
                        Debug.Log("Listed Type for Handler for files '" + type.filename + "' is " + type.typename);
                        Debug.Log("Listed Method for Handler for files '" + type.filename + "' is " + type.methodname);
                        list.Add(type);

                    } else {
                        Debug.LogError(file + " : Invalid Type");
                    }
                } else {
                    Debug.LogError(file + " : " + node.Name + " : Empty filename");
                }
            }

        }
        return list;
    }

    //Function that goes through all mods and calls the recursive ReadAllFilesInDirectory
    private static void ReadAllFiles(List<CustomFileType> types, string[] strArray) {
        for (int index = 0; index < strArray.Length; ++index) {
            string path2 = strArray[index];
            string str = Path.Combine(ModCtrl.MODS_PATH, path2);
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            ReadAllFilesInDirectory(types, directoryInfo, true);
        }
        if (SteamManager.Initialized) {
            foreach (LapinerTools.Steam.Data.WorkshopItem workshopItem in LapinerTools.Steam.SteamMainBase<LapinerTools.Steam.SteamWorkshopMain>.Instance.m_activeItems.Values) {
                if (workshopItem.IsActive) {
                    var directory = new DirectoryInfo(workshopItem.InstalledLocalFolder);
                    ReadAllFilesInDirectory(types, directory, true);
                }
            }
        }
    }

    public static List<string> disabledFolders = new List<string>(new string[] {"disabled", "bin", "obj", "packages", "obj", "dependencies" });

    //Recursive. 'first' indicates if it is the main directory(which was already loaded) and loads everything it can without calling a subroutine. 
    private static void ReadAllFilesInDirectory(List<CustomFileType> types, DirectoryInfo dir, bool first) {
        if (disabledFolders.Contains(dir.Name.ToLower())) {
            return;
        }
        var files = dir.GetFiles();
        var modCtrl = S.I.modCtrl;
        if (!first) {
            var baseFiles = new List<FileInfo>(files);
            //This section is massively simplified because of the rework to loading sprites
            modCtrl._InstallTheseMods(files, dir.FullName);
        }

        //Go through files looking for the custom file types found earlier.
        for (int i = 0; i < files.Length; i++) {
            FileInfo file = files[i];

            if (!file.Name.Contains(".meta")) {
                var name = file.Name;

                foreach (var type in types) {
                    if ((!type.contains && name == type.filename) || (type.contains && name.Contains(type.filename))) {
                        //We don't break in this section so that files can be handled by multiple mods if necessary.
                        Debug.Log("Loading file: " + file.FullName);
                        try
                        {
                            type.handler.Invoke(null, new object[] { file });
                        } 
                        catch  {
                            Debug.LogError("Could not invoke handler on file " + file.FullName);
                        }
                        
                    }
                }
            }
        }
        foreach (var directory in dir.GetDirectories()) {
            //Now look through subfolders that arent named 'disabled'.
            if (!disabledFolders.Contains(dir.Name.ToLower())) ReadAllFilesInDirectory(types, directory, false);
        }
    }

    //Stores information for custom file types.
    internal class CustomFileType
    {
        public bool contains = false;

        public string filename = "";

        public string typename = "";

        public string methodname = "";

        public MethodInfo handler = null;
    }

}
