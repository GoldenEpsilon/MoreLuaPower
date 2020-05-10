using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Xml;
using System.Globalization;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using AssetBundles;
using UnityEngine.UI;
using Rewired.UI.ControlMapper;
using System.Xml.Linq;
using UnityEngine.Internal;
using UnityEngine.Networking;
using System.Threading;

[HarmonyPatch]
static class CustomFileLoader_Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.LoadItemData))]
    static bool LoadItems(ItemManager __instance)
    {
        if (S.I.modCtrl.processing)
        {
            CustomFileLoader.AfterInstall();
            return true;
        }
        return true;
    }
}

internal static class CustomFileLoader
{
    private static List<AsyncOperation> images_remaining = new List<AsyncOperation>();

    private static Mutex image_lock = new Mutex();

    public static void AfterInstall()
    {
        var modCtrl = S.I.modCtrl;
        images_remaining = new List<AsyncOperation>();
        //Debug.Log("Round 1");
        string[] directories = Directory.GetDirectories(ModCtrl.MODS_PATH);
        string[] strArray = directories;
        var list = new List<CustomFileType>();
        for (int index = 0; index < strArray.Length; ++index)
        {
            string path2 = strArray[index];
            string str = Path.Combine(ModCtrl.MODS_PATH, path2);
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            //Debug.Log((object)("Installing " + directoryInfo.Name + " at " + str));
            var temp = new List<CustomFileType>();
            var tempAssemblies = new List<FileInfo>();
            FindCustomDataInstructions(directoryInfo, temp, tempAssemblies, true);
            list.AddRange(temp);
        }
        if (SteamManager.Initialized)
        {
            foreach (LapinerTools.Steam.Data.WorkshopItem workshopItem in LapinerTools.Steam.SteamMainBase<LapinerTools.Steam.SteamWorkshopMain>.Instance.m_activeItems.Values)
            {
                if (workshopItem.IsActive)
                {
                    //Debug.Log((object)("Installing " + workshopItem.Name + " at " + workshopItem.InstalledLocalFolder));
                    DirectoryInfo dir = new DirectoryInfo(workshopItem.InstalledLocalFolder);
                    var temp = new List<CustomFileType>();
                    var tempAssemblies = new List<FileInfo>();
                    FindCustomDataInstructions(dir, temp, tempAssemblies, true);
                    list.AddRange(temp);
                }
            }
        }
        ReadAllFiles(list, strArray);
    }

    private static void ReadAllFiles(List<CustomFileType> types, string[] strArray)
    {
        for (int index = 0; index < strArray.Length; ++index)
        {
            string path2 = strArray[index];
            string str = Path.Combine(ModCtrl.MODS_PATH, path2);
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            //Debug.Log((object)("Installing " + directoryInfo.Name + " at " + str));
            ReadAllFilesInDirectory(types, directoryInfo, true);
        }
        if (SteamManager.Initialized)
        {
            foreach (LapinerTools.Steam.Data.WorkshopItem workshopItem in LapinerTools.Steam.SteamMainBase<LapinerTools.Steam.SteamWorkshopMain>.Instance.m_activeItems.Values)
            {
                if (workshopItem.IsActive)
                {
                    //Debug.Log((object)("Installing " + workshopItem.Name + " at " + workshopItem.InstalledLocalFolder));
                    var directory = new DirectoryInfo(workshopItem.InstalledLocalFolder);
                    ReadAllFilesInDirectory(types, directory, true);
                }
            }
        }
    }

    private static void ReadAllFilesInDirectory(List<CustomFileType> types, DirectoryInfo dir, bool first)
    {
        Debug.Log(dir.FullName);
        var files = dir.GetFiles();
        var modCtrl = S.I.modCtrl;
        if (!first)
        {
            var baseFiles = new List<FileInfo>(files);
            for (int i = 0; i < baseFiles.Count(); i++)
            {
                if (baseFiles[i].Name.EndsWith(".png"))
                {
                    modCtrl.StartCoroutine(LoadImageFile(baseFiles[i]));
                    baseFiles.Remove(baseFiles[i]);
                    i--;
                }
            }
            Debug.Log("Running default installing for folder " + dir.Name);
            Debug.Log("Huh: " + dir.FullName);
            var coroutine = modCtrl._InstallTheseMods(files, dir.FullName);
            while (coroutine.MoveNext()) ;
            Debug.Log("Loaded every default filetype in: " + dir.Name);
        }

        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i];

            if (!file.Name.Contains(".meta"))
            {
                var name = file.Name;

                foreach (var type in types)
                {
                    if ((!type.contains && name == type.filename) || (type.contains && name.Contains(type.filename)))
                    {
                        Debug.Log("Loading file: " + file.FullName);
                        type.handler.Invoke(null, new object[] { file });
                    }
                }
            }
        }
        foreach (var directory in dir.GetDirectories())
        {
            ReadAllFilesInDirectory(types, directory, false);
        }
    }

    private static void FindCustomDataInstructions(DirectoryInfo dir, List<CustomFileType> re, List<FileInfo> assembly_files, bool first)
    {
        Debug.Log(dir.FullName);
        var files = dir.GetFiles();
        var modCtrl = S.I.modCtrl;
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i];
            if (!file.Name.Contains(".meta"))
            {
                switch (file.Name)
                {
                    case "CustomFileTypes.xml":
                        Debug.Log("Found Data: " + file.FullName);
                        re.AddRange(ReadCustomFileTypeInstructions(file.FullName));
                        break;
                    default:
                        if (file.Name.EndsWith(".dll"))
                        {
                            assembly_files.Add(file);
                        }
                        break;
                }

            }
        }
        foreach (DirectoryInfo directory in dir.GetDirectories())
        {
            FindCustomDataInstructions(directory, re, assembly_files, false);
        }
        if (first)
        {
            foreach (CustomFileType customdatatype in re)
            {
                foreach (var info in assembly_files)
                {
                    try
                    {
                        var assem = Assembly.LoadFrom(info.FullName);
                        Debug.Log(assem.FullName);
                        var type = assem.GetType(customdatatype.typename);
                        Debug.Log(type);
                        customdatatype.handler = type.GetMethod(customdatatype.methodname);
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
            }
            re = (from c in re where c.handler != null select c).ToList();
        }
    }

    private static List<CustomFileType> ReadCustomFileTypeInstructions(string file)
    {
        var list = new List<CustomFileType>();
        XDocument doc = XDocument.Load(file);
        foreach (var node in doc.Root.Elements())
        {
            Debug.Log("node name: " + node.Name);
            if (node.Name == "CustomFileType")
            {
                CustomFileType type = new CustomFileType();

                foreach (var el in node.Elements())
                {

                    if (!el.IsEmpty)
                    {
                        Debug.Log("inner node name: " + el.Name);
                        switch (el.Name.ToString())
                        {
                            case "Filename":
                                if (el.HasAttributes)
                                {
                                    try
                                    {
                                        type.contains = bool.Parse(el.Attribute("contains").Value);
                                    } 
                                    catch (Exception e)
                                    {
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

                if (type.filename != "")
                {
                    if (type.methodname != "" && type.typename != "")
                    {
                        Debug.Log("Adding file type to temporary list : " + type.filename);
                        list.Add(type);

                    }
                    else
                    {
                        Debug.LogError(file + " : Invalid Type");
                    }
                }
                else
                {
                    Debug.LogError(file + " : " + node.Name + " : Empty filename");
                }
            }

        }
        return list;
    }

    private static IEnumerator LoadImageFile(FileInfo theFile)
    {
        if (!theFile.Name.Contains(".meta"))
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + theFile.FullName.ToString()))
            {
                var task = uwr.SendWebRequest();
                image_lock.WaitOne();
                images_remaining.Add(task);
                image_lock.ReleaseMutex();
                task.completed += delegate (AsyncOperation op)
                {
                    image_lock.WaitOne();
                    images_remaining.Remove(task);
                    if (images_remaining.Count() == 0)
                    {
                        if (!S.I.modCtrl.processing)
                        {
                            S.I.itemMan.LoadItemData();
                        }
                        else
                        {
                            S.I.modCtrl.StartCoroutine(LoadItemDataAfterInstall());
                        }
                    }
                    image_lock.ReleaseMutex();
                };
                yield return (object)task;
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log((object)uwr.error);
                }
                else
                {
                    Texture2D content = DownloadHandlerTexture.GetContent(uwr);
                    S.I.itemMan.sprites[Path.GetFileNameWithoutExtension(theFile.FullName.ToString())] = Sprite.Create(content, new Rect(0.0f, 0.0f, (float)content.width, (float)content.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    private static IEnumerator LoadItemDataAfterInstall()
    {
        var modCtrl = S.I.modCtrl;
        while (modCtrl.processing)
        {
            yield return new WaitForSeconds(0.1f);
        }
        S.I.itemMan.LoadItemData();
    }

    internal class CustomFileType
    {
        public bool contains = false;

        public string filename = "";

        public string typename = "";

        public string methodname = "";

        public MethodInfo handler = null;
    }

}
