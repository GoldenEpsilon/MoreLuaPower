using HarmonyLib;
using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum SettingType
{
    Toggle,
    Rotation,
    Slider,
    TextField,
    Button,
    Folder,

    Return // Used for folder return button
}

class MPLSetting
{
    public string name;
    public string key;
    public SettingType type;
    public List<string> values;

    public int activeValue = 0;
    public int defaultValue = 0;
    public float defaultSliderValue = 0;

    public bool inFolder = false;

    public GameObject settingobj;
    public Transform control;
}

public static class MPLCustomSettings
{
    public static bool SettingsSetUp = false;
    internal static Dictionary<string, MPLSetting> settings = new Dictionary<string, MPLSetting>();
    public static int settingsPage = 0;
    public static Transform previousPage;
    public static Transform nextPage;

    internal static bool folderActive = false;
    internal static string currentFolder = "FolderSetup|Outside";
    internal static string folderReturnKey = "FolderSetup|ReturnKey";

    public static bool GetSettingToggle(string name)
    {
        if (!settings.ContainsKey(name))
        {
            MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
            return false;
        }
        if (settings[name].type != SettingType.Toggle)
        {
            MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Toggle was asked for.", LogLevel.Major);
            return false;
        }
        return PlayerPrefs.GetInt(name) > 0 ? true : false;
    }
    public static string GetSettingRotation(string name)
    {
        if (!settings.ContainsKey(name))
        {
            MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
            return "";
        }
        if (settings[name].type != SettingType.Rotation)
        {
            MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Rotation was asked for.", LogLevel.Major);
            return "";
        }
        return settings[name].values[PlayerPrefs.GetInt(name)];
    }
    public static float GetSettingSlider(string name)
    {
        if (!settings.ContainsKey(name))
        {
            MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
            return 0;
        }
        if (settings[name].type != SettingType.Slider)
        {
            MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Slider was asked for.", LogLevel.Major);
            return 0;
        }
        return PlayerPrefs.GetFloat(name);
    }
    public static string GetSettingTextBox(string name)
    {
        if (!settings.ContainsKey(name))
        {
            MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
            return null;
        }
        if (settings[name].type != SettingType.TextField)
        {
            MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Text Input was asked for.", LogLevel.Major);
            return null;
        }
        return PlayerPrefs.GetString(name);
    }
    public static List<string> GetSettingFolder(string name)
    {
        if (name == null)
        {
            MPLog.Log("Null setting name given!", LogLevel.Major);
            return null;
        }
        if (!settings.ContainsKey(name))
        {
            MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
            return null;
        }
        if (settings[name].type != SettingType.Folder)
        {
            MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Folder was asked for.", LogLevel.Major);
            return null;
        }

        UpdateFolders();
        return settings[name].values;
    }

    public static void AddSettingToggle(string name, bool defaultval)
    {
        if (!settings.ContainsKey(name))
        {
            MPLSetting setting = new MPLSetting();
            setting.name = name;
            setting.key = name;
            setting.type = SettingType.Toggle;
            setting.defaultValue = defaultval ? 1 : 0;
            settings.Add(name, setting);
        }
        else
        {
            MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
        }
    }
    public static void AddSettingRotation(string name, List<string> values, int defaultval)
    {
        if (!settings.ContainsKey(name))
        {
            MPLSetting setting = new MPLSetting();
            setting.name = name;
            setting.key = name;
            setting.values = values;
            setting.type = SettingType.Rotation;
            setting.defaultValue = defaultval % setting.values.Count; //doing the modulo for extra safety
            settings.Add(name, setting);
        }
        else
        {
            MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
        }
    }
    public static void AddSettingSlider(string name, float defaultval)
    {
        if (!settings.ContainsKey(name))
        {
            MPLSetting setting = new MPLSetting();
            setting.name = name;
            setting.key = name;
            setting.type = SettingType.Slider;
            setting.defaultSliderValue = defaultval;
            settings.Add(name, setting);
        }
        else
        {
            MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
        }
    }
    public static void AddSettingTextBox(string name, string placeholder = "Insert Text")
    {
        if (!settings.ContainsKey(name))
        {
            MPLSetting setting = new MPLSetting();
            setting.name = name;
            setting.key = name;
            setting.type = SettingType.TextField;
            settings.Add(name, setting);

            setting.values = new List<string>();
            setting.values.Add(placeholder);
        }
        else
        {
            MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
        }
    }
    public static void AddSettingButton(string name, List<string> functions)
    {
        if (!settings.ContainsKey(name))
        {
            MPLSetting setting = new MPLSetting();
            setting.name = name;
            setting.key = name;
            setting.type = SettingType.Button;
            setting.values = new List<string>(functions);
            settings.Add(name, setting);
        }
        else
        {
            MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
        }
    }
    public static void AddSettingFolder(string name)
    {
        if (name == null)
        {
            MPLog.Log("Null setting name given!", LogLevel.Minor);
            return;
        }
        if (settings.ContainsKey(name))
        {
            MPLog.Log("Folder '" + name + "' is already an initialized setting", LogLevel.Minor);
            return;
        }

        MPLSetting setting = new MPLSetting();
        setting.name = name;
        setting.key = name;
        setting.values = new List<string>();
        setting.type = SettingType.Folder;

        settings.Add(setting.key, setting);

        if (!settings.ContainsKey(folderReturnKey))
        {
            MPLSetting returnBtn = new MPLSetting();
            returnBtn.type = SettingType.Return;
            returnBtn.values = new List<string>(1) { string.Empty };
            returnBtn.name = folderReturnKey;
            returnBtn.key = folderReturnKey;
            settings.Add(folderReturnKey, returnBtn);
        }
    }

    public static void EditSettingButton(string name, List<string> functions)
    {
        if (settings.ContainsKey(name))
        {
            settings[name].values = new List<string>(functions);
        }
    }

    public static void NextSettingsPage()
    {
        if (settings.Count > settingsPage * 18 + 2)
        {
            settingsPage++;
            UpdateSettingsPage();
        }
    }
    public static void PreviousSettingsPage()
    {
        if (settingsPage > 0)
        {
            settingsPage--;
            UpdateSettingsPage();
        }
    }
    public static void UpdateSettingsPage()
    {
        foreach (KeyValuePair<string, MPLSetting> pair in settings)
        {
            if (pair.Value.control.position.z < -35) { pair.Value.control.Translate(0, 0, (-5) - pair.Value.control.position.z); }
            if (pair.Value.control.GetComponent<TextMeshProUGUI>() != null)
            {
                pair.Value.control.GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);
            }
            pair.Value.settingobj.SetActive(false);
        }
        nextPage.GetChild(0).GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);
        previousPage.GetChild(0).GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);

        int controlNum = 0;

        if (folderActive)
        {
            foreach (string path in settings[currentFolder].values)
            {
                string activeSettingFile = currentFolder + "/" + path;
                if (settings.ContainsKey(activeSettingFile))
                {
                    if (controlNum < 18)
                    {
                        controlNum++;
                        settings[activeSettingFile].settingobj.SetActive(true);
                    }
                }
            }

            settings[folderReturnKey].settingobj.SetActive(true);

            previousPage.gameObject.SetActive(false);
            nextPage.gameObject.SetActive(false);

            string previousFolder = settings[folderReturnKey].values[0];
            if (previousFolder.StartsWith(currentFolder) & S.I.optCtrl.content.activeSelf)
            {
                if ( settings[currentFolder].values.Contains(previousFolder.Replace(currentFolder + "/", string.Empty)) )
                {
                    S.I.optCtrl.btnCtrl.SetFocus(settings[previousFolder].settingobj);
                    return;
                }
            }

            if (S.I.optCtrl.content.activeSelf)
            {
                if (settings[currentFolder].values.Count == 0)
                {
                    S.I.optCtrl.btnCtrl.SetFocus(settings[folderReturnKey].settingobj);
                }
                else
                {
                    foreach (MPLSetting option in settings.Values)
                    {
                        if (option.type != SettingType.Return & option.settingobj.activeSelf)
                        {
                            S.I.optCtrl.btnCtrl.SetFocus(option.settingobj);
                            break;
                        }
                    }
                }
            }

            return;
        }

        List<MPLSetting> settingList = new List<MPLSetting>();
        foreach (KeyValuePair<string, MPLSetting> pair in settings)
        {
            if (!pair.Value.inFolder & pair.Key != folderReturnKey)
            {
                settingList.Add(pair.Value);
            }
            else { pair.Value.settingobj.SetActive(false); }
        }

        foreach (MPLSetting setting in settingList)
        {
            controlNum++;
            if (controlNum < settingsPage * 18 + 2 && settingsPage != 0)
            {
                setting.settingobj.SetActive(false);
            }
            else if (controlNum < (settingsPage + 1) * 18 + 2 + ((settings.Count == settingsPage * 18 + 2) ? 1 : 0))
            {
                setting.settingobj.SetActive(true);
            }
            else
            {
                setting.settingobj.SetActive(false);
            }
        }

        if (currentFolder != "FolderSetup|Outside")
        {
            if (S.I.optCtrl.content)
            {
                S.I.optCtrl.btnCtrl.SetFocus(settings[currentFolder].settingobj);
                currentFolder = "FolderSetup|Outside";
                settings[folderReturnKey].values[0] = string.Empty;
            }
        }
        else
        {
            foreach (MPLSetting option in settings.Values)
            {
                if (option.settingobj.activeSelf & S.I.optCtrl.content.activeSelf) { S.I.optCtrl.btnCtrl.SetFocus(option.settingobj); break; }
            }
        }

        nextPage.gameObject.SetActive(false);
        if (settingsPage == 0)
        {
            previousPage.gameObject.SetActive(false);
        }
        else
        {
            previousPage.gameObject.SetActive(true);
        }
        if (settingList.Count > (settingsPage + 1) * 18 + 2)
        {
            nextPage.gameObject.SetActive(true);
        }
        else
        {
            nextPage.gameObject.SetActive(false);
        }
    }

    internal static void UpdateFolders()
    {
        List<MPLSetting> trueFolders = new List<MPLSetting>();
        foreach (MPLSetting setting in settings.Values)
        {
            if (setting.type == SettingType.Folder) 
            { 
                setting.values.Clear();
                setting.control.GetComponent<TextMeshProUGUI>().text = setting.name;
                trueFolders.Add(setting);
            }
        }

        foreach (KeyValuePair<string, MPLSetting> pair in settings)
        {
            if (pair.Key.Contains("/"))
            {
                string folder = pair.Key.Substring(0, pair.Key.LastIndexOf("/"));
                if (settings.ContainsKey(folder))
                {
                    if (settings[folder].type == SettingType.Folder)
                    {
                        if (pair.Value.name.StartsWith(folder + "/"))
                        {
                            pair.Value.name = pair.Value.name.Replace(folder + "/", string.Empty);
                            pair.Value.control.GetComponent<TextMeshProUGUI>().text =
                            pair.Value.control.GetComponent<TextMeshProUGUI>().text.Replace(folder + "/", string.Empty);
                        }
                        pair.Value.inFolder = true;

                        if (!settings[folder].values.Contains(pair.Key.Replace(folder + "/", string.Empty)))
                        {
                            settings[folder].values.Add(pair.Key.Replace(folder + "/", string.Empty));
                        }

                        if (pair.Value.type == SettingType.TextField)
                        {
                            TextBoxDistanceUpdate(pair.Value);
                        }

                        if (pair.Value.type == SettingType.Slider)
                        {
                            pair.Value.control.GetComponent<TextMeshProUGUI>().name = pair.Value.name;
                        }
                    }
                }
            }
        }

        foreach (MPLSetting setting in trueFolders)
        {
            if (setting.values.Count > 0)
            {
                setting.control.GetComponent<TextMeshProUGUI>().text += " [ +" + setting.values.Count.ToString() + " ]";
            }
            else
            {
                setting.control.GetComponent<TextMeshProUGUI>().text += " []";
            }
        }

    }
    internal static void SortFolders()
    {
        List<KeyValuePair<string, MPLSetting>> sortedSettings = settings.ToList();
        sortedSettings.Sort((x, y) => SortFoldersX(x.Value, y.Value, settings.ToList()));
        settings.Clear();
        foreach (KeyValuePair<string, MPLSetting> pair in sortedSettings)
        {
            settings.Add(pair.Key, pair.Value);
        }
    }
    internal static int SortFoldersX(MPLSetting x, MPLSetting y, List<KeyValuePair<string, MPLSetting>> oldList)
    {
        if (x.type == SettingType.Return) { return 1; }
        if (y.type == SettingType.Return) { return -1; }

        if ((x.type != SettingType.Folder & y.type != SettingType.Folder) || x.type == y.type) 
        {
            int xpos = oldList.FindIndex(a => a.Value == x);
            int ypos = oldList.FindIndex(a => a.Value == y);
            if (xpos < ypos) { return -1; }
            return 1; 
        }

        if (x.type == SettingType.Folder) { return -1; }
        return 1;
    }
    internal static void TextBoxDistanceUpdate(MPLSetting setting)
    {
        setting.control.GetComponent<TextMeshProUGUI>().ForceMeshUpdate();

        string _TextBoxText = setting.control.GetComponent<TextMeshProUGUI>().text;
        float _TextBoxSize1 = setting.control.GetComponent<TextMeshProUGUI>().GetTextInfo(_TextBoxText).characterInfo[0].topLeft.x;
        float _TextBoxSize2 = setting.control.GetComponent<TextMeshProUGUI>().GetTextInfo(_TextBoxText).characterInfo[_TextBoxText.Length - 1].bottomRight.x;
        float _TextBoxDis = setting.control.GetComponent<TextMeshProUGUI>().transform.position.x + (_TextBoxSize2 - _TextBoxSize1 + 2)
                            - setting.control.GetComponent<NavTextfield>().transform.GetChild(0).position.x;

        setting.control.GetComponent<NavTextfield>().transform.GetChild(0).Translate(_TextBoxDis, 0, 0);
    }
}

[HarmonyPatch(typeof(OptionCtrl))]
[HarmonyPatch("Open")]
class SettingsPatch
{
    static void Prefix(OptionCtrl __instance)
    {
        if (MPLCustomSettings.SettingsSetUp == false)
        {
            var button = Object.Instantiate(__instance.navButtonGrid.GetChild(1), __instance.navButtonGrid);
            button.GetComponent<UIButton>().tmpText.text = "MODS";
            button.GetChild(0).GetComponent<I2.Loc.Localize>().Term = "MODS";
            if (button.parent.childCount >= 4) { button.SetSiblingIndex(3); }

            var modSettingsMenu = Object.Instantiate(__instance.settingsPane.gameObject);
            modSettingsMenu.transform.position = __instance.settingsPane.transform.position;
            modSettingsMenu.transform.SetParent(__instance.settingsPane.transform.parent);
            var settingsPane = modSettingsMenu.GetComponent<SettingsPane>();
            GameObject _content = settingsPane.content;
            SlideBody _slideBody = settingsPane.slideBody;
            Animator _anim = settingsPane.anim;
            TMP_Text _title = settingsPane.title;
            UIButton _defaultButton = settingsPane.defaultButton;
            UIButton _backButton = settingsPane.backButton;
            UIButton _originButton = settingsPane.originButton;
            Object.DestroyImmediate(settingsPane);
            modSettingsMenu.AddComponent<NavPanel>();
            var newPane = modSettingsMenu.GetComponent<NavPanel>();
            newPane.content = _content;
            newPane.slideBody = _slideBody;
            newPane.anim = _anim;
            newPane.title = _title;
            newPane.defaultButton = _defaultButton;
            newPane.backButton = _backButton;
            newPane.originButton = _originButton;

            Object.DestroyImmediate(button.GetComponent<Button>());
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => {
                    S.I.optCtrl.OpenPanel(modSettingsMenu.GetComponent<NavPanel>());
                });
                EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Add(entry);
                trigger.enabled = false;
            }

            //button.GetComponent<UIButton>().onAcceptPress.RemoveAllListeners();

            var navPanelBackground = modSettingsMenu.transform.GetChild(0).GetChild(0);
            var navPanelButtons = modSettingsMenu.transform.GetChild(0).GetChild(1);
            var navPanelTitle = modSettingsMenu.transform.GetChild(0).GetChild(2);
            var navPanelExit = modSettingsMenu.transform.GetChild(0).GetChild(3);

            navPanelTitle.GetComponent<TextMeshProUGUI>().text = "MOD SETTINGS";
            navPanelTitle.GetComponent<I2.Loc.Localize>().Term = "MOD SETTINGS";

            for (int i = 0; i < navPanelButtons.transform.childCount; i++)
            {
                navPanelButtons.transform.GetChild(i).gameObject.SetActive(false);
            }

            GameObject PrevButton = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
            PrevButton.SetActive(true);
            PrevButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Previous Page";
            PrevButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            PrevButton.transform.GetChild(0).GetComponent<I2.Loc.Localize>().Term = "Previous Page";
            Object.DestroyImmediate(PrevButton.GetComponent<Button>());
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => { MPLCustomSettings.PreviousSettingsPage(); });
                EventTrigger trigger = PrevButton.AddComponent<EventTrigger>();
                trigger.triggers.Add(entry);
                trigger.enabled = false;
            }
            MPLCustomSettings.previousPage = PrevButton.transform;

            MPLCustomSettings.SortFolders();

            foreach (MPLSetting setting in MPLCustomSettings.settings.Values)
            {
                switch (setting.type)
                {
                    case SettingType.Toggle:
                        if (PlayerPrefs.HasKey(setting.key))
                        {
                            setting.activeValue = PlayerPrefs.GetInt(setting.key);
                        }
                        else
                        {
                            setting.activeValue = setting.defaultValue;
                        }
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + ": " + (setting.activeValue > 0 ? "True" : "False");
                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                setting.activeValue = setting.activeValue > 0 ? 0 : 1;
                                PlayerPrefs.SetInt(setting.key, setting.activeValue);
                                setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + ": " + (setting.activeValue > 0 ? "True" : "False");
                            });
                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }
                        break;
                    case SettingType.Rotation:
                        if (PlayerPrefs.HasKey(setting.key))
                        {
                            setting.activeValue = PlayerPrefs.GetInt(setting.key);
                        }
                        else
                        {
                            setting.activeValue = setting.defaultValue;
                        }
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + ": " + setting.values[setting.activeValue];
                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";
                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                setting.activeValue = (setting.activeValue + 1 < setting.values.Count ? setting.activeValue + 1 : 0);
                                PlayerPrefs.SetInt(setting.key, setting.activeValue);
                                setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + ": " + setting.values[setting.activeValue];
                            });
                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }
                        break;
                    case SettingType.Slider:
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(5).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.settingobj.transform.name = setting.key;
                        setting.control = setting.settingobj.transform.GetChild(3);
                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";
                        setting.control.GetComponent<TextMeshProUGUI>().name = setting.name;

                        if (PlayerPrefs.HasKey(setting.key))
                        {
                            setting.settingobj.GetComponent<Slider>().value = PlayerPrefs.GetFloat(setting.key);
                        }
                        else
                        {
                            setting.settingobj.GetComponent<Slider>().value = setting.defaultSliderValue;
                        }

                        setting.control.GetComponent<TextMeshProUGUI>().transform.Translate(8, 0, 0);

                        PowerMonoBehavior.sliders.Add(setting.settingobj.transform);
                        break;
                    case SettingType.TextField:

                        Transform InputFieldRef = S.I.heCtrl.content.transform.Find("SeedField");
                        TextMeshProUGUI InputFieldGUIRef = navPanelButtons.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                        NavTextfield TextBoxInputRef = S.I.heCtrl.content.transform.Find("SeedField").GetComponent<NavTextfield>();

                        setting.settingobj = Object.Instantiate(InputFieldRef.gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform;
                        setting.control.GetComponent<NavTextfield>().btnCtrl = S.I.optCtrl.btnCtrl;
                        setting.control.GetComponent<NavTextfield>().name = "MoreLuaPowerSettingsNavTextField" + setting.key;

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Image>());
                        //Object.DestroyImmediate(setting.settingobj.GetComponent<TouchScreenKeyboard>());
                        setting.settingobj.AddComponent<TextMeshProUGUI>();

                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + " :";
                        setting.control.GetComponent<TextMeshProUGUI>().fontSize = InputFieldGUIRef.fontSize;
                        setting.control.GetComponent<TextMeshProUGUI>().font = InputFieldGUIRef.font;

                        //setting.control.GetComponent<NavTextfield>().navList = PrevButton.GetComponent<NavButton>().navList;
                        setting.control.GetComponent<NavTextfield>().useParentTransformNav = true;
                        setting.control.GetComponent<NavTextfield>().transform.GetChild(0).GetChild(0).Translate(0, -1f, 0);
                        setting.control.GetComponent<NavTextfield>().down = null;
                        setting.control.GetComponent<NavTextfield>().up = null;

                        setting.control.GetComponent<TMP_InputField>().textComponent.fontSize = InputFieldGUIRef.fontSize;
                        setting.control.GetComponent<TMP_InputField>().textComponent.color = new Color(0.8f, 0.8f, 0.8f);
                        setting.control.GetComponent<TMP_InputField>().textComponent.font = InputFieldGUIRef.font;
                        setting.control.GetComponent<TMP_InputField>().textComponent.alignment = TextAlignmentOptions.Left;
                        setting.control.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
                        setting.control.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().fontSize = InputFieldGUIRef.fontSize;
                        setting.control.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().SetText(setting.values[0]);
                        setting.control.GetComponent<TMP_InputField>().placeholder.GetComponent<I2.Loc.Localize>().Term = "";

                        setting.control.GetComponent<TMP_InputField>().characterLimit = 14;

                        MPLCustomSettings.TextBoxDistanceUpdate(setting);

                        if (PlayerPrefs.HasKey(setting.key))
                        {
                            setting.control.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(setting.key);
                        }
                        else
                        {
                            setting.control.GetComponent<TMP_InputField>().text = string.Empty;
                        }

                        break;
                    case SettingType.Button:

                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name;
                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                Script myLuaScript = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance")
                                    .GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();

                                foreach (string function in setting.values)
                                {
                                    object buttonPress = myLuaScript.Globals[function];
                                    if (buttonPress != null)
                                    {
                                        S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(myLuaScript.CreateCoroutine(buttonPress)));
                                    }
                                }
                            });
                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }
                        break;
                    case SettingType.Folder:
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name;

                        setting.control.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                        setting.control.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;

                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                MPLCustomSettings.folderActive = true;
                                MPLCustomSettings.currentFolder = setting.key;
                                MPLCustomSettings.UpdateSettingsPage();
                            });

                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }

                        break;
                    case SettingType.Return:
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = "Return?";
                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

                        setting.control.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Italic;
                        setting.control.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomLeft;

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                MPLCustomSettings.folderActive = false;
                                if (MPLCustomSettings.currentFolder.Contains("/"))
                                {
                                    if (MPLCustomSettings.settings.ContainsKey( MPLCustomSettings.currentFolder.Substring(0, MPLCustomSettings.currentFolder.LastIndexOf("/") ) )
                                    )
                                    {
                                        if ( MPLCustomSettings.settings[MPLCustomSettings.currentFolder.Substring(0, MPLCustomSettings.currentFolder.LastIndexOf("/") )].type == SettingType.Folder )
                                        {
                                            MPLCustomSettings.folderActive = true;
                                            setting.values[0] = MPLCustomSettings.currentFolder;
                                            MPLCustomSettings.currentFolder = MPLCustomSettings.currentFolder.Substring(0, MPLCustomSettings.currentFolder.LastIndexOf("/"));
                                        }
                                    }
                                }
                                MPLCustomSettings.UpdateSettingsPage();
                            });

                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }
                        break;
                }
                if (setting.type != SettingType.Folder & setting.type != SettingType.Return & setting.control.GetComponent<TextMeshProUGUI>() != null)
                {
                    setting.control.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
                }
            }

            GameObject NextButton = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
            NextButton.SetActive(true);
            NextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Next Page";
            NextButton.transform.GetChild(0).GetComponent<I2.Loc.Localize>().Term = "Next Page";
            NextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            Object.DestroyImmediate(NextButton.GetComponent<Button>());
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => { MPLCustomSettings.NextSettingsPage(); });
                EventTrigger trigger = NextButton.AddComponent<EventTrigger>();
                trigger.triggers.Add(entry);
                trigger.enabled = false;
            }
            MPLCustomSettings.nextPage = NextButton.transform;

            Object.DestroyImmediate(navPanelExit.GetComponent<Button>());
            navPanelExit.gameObject.AddComponent<Button>();
            navPanelExit.GetComponent<Button>().onClick.AddListener(() => {
                S.I.optCtrl.ClosePanel(modSettingsMenu.GetComponent<NavPanel>());
            });
            Traverse.Create(navPanelExit.GetComponent<UIButton>()).Field("button").SetValue(navPanelExit.GetComponent<Button>());

            MPLCustomSettings.UpdateFolders();
            MPLCustomSettings.UpdateSettingsPage();

            MPLCustomSettings.SettingsSetUp = true;
        }
    }

    [HarmonyPatch(typeof(NavPanel), nameof(NavPanel.Open))]
    [HarmonyPostfix]
    private static void FirstOpenFocus(NavPanel __instance)
    {
       S.I.optCtrl.StartCoroutine(_FirstOpenFocus(__instance));
    }
    private static IEnumerator _FirstOpenFocus(NavPanel panel)
    {
        Transform child = null;
        Transform title = null;
        if (panel.transform.childCount > 0)
        {
            child = panel.transform.GetChild(0);
        }
        if (child.transform.childCount >= 2)
        {
            title = child?.GetChild(2);
        }

        if (title != null)
        {
            if (title.GetComponent<TextMeshProUGUI>() != null)
            {
                if (title.GetComponent<TextMeshProUGUI>().text == "MOD SETTINGS")
                {
                    yield return new WaitForSecondsRealtime(0.3f);

                    string settingID = MPLCustomSettings.settings.ToList().Find(x => x.Value.settingobj.activeSelf).Key;
                    if (MPLCustomSettings.settings.ContainsKey(settingID))
                    {
                        MPLSetting activeSetting = MPLCustomSettings.settings[settingID];
                        S.I.optCtrl.btnCtrl.SetFocus(activeSetting.settingobj);
                    }
                }
            }
        }

        yield break;
    }

    [HarmonyPatch(typeof(NavTextfield))]
    [HarmonyPatch(nameof(NavTextfield.Focus))]
    [HarmonyPostfix]
    private static void NavFieldFixIntro(NavTextfield __instance)
    {
        if (__instance.name.StartsWith("MoreLuaPowerSettingsNavTextField"))
        {
            __instance.canvasGroup.alpha = 1f;
            if (__instance.GetComponent<TextMeshProUGUI>() != null)
            {
                __instance.GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.Pink);
            }
        }
    }

    [HarmonyPatch(typeof(NavTextfield))]
    [HarmonyPatch(nameof(NavTextfield.UnFocus))]
    [HarmonyPostfix]
    private static void NavFieldFixOutro(NavTextfield __instance)
    {
        if (__instance.name.StartsWith("MoreLuaPowerSettingsNavTextField"))
        {
            __instance.canvasGroup.alpha = 1f;
            if (__instance.GetComponent<TextMeshProUGUI>() != null)
            {
                __instance.GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);
            }
        }
    }

    [HarmonyPatch(typeof(NavTextfield))]
    [HarmonyPatch(nameof(NavTextfield.OnEndEdit))]
    [HarmonyPostfix]
    private static void NavFieldEndEditSave(NavTextfield __instance)
    {
        if (__instance.name.StartsWith("MoreLuaPowerSettingsNavTextField"))
        {
            if (__instance.GetComponent<TMP_InputField>() != null)
            {
                PlayerPrefs.SetString(
                    __instance.name.Substring("MoreLuaPowerSettingsNavTextField".Length),
                    __instance.GetComponent<TMP_InputField>().text
                    );
            }
        }
    }
}
