using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
enum SettingType {
    Toggle,
    Rotation,
    Slider,
    Folder,

    Return // Used for folder return button
}

class MPLSetting {
	public string name;
	public SettingType type;
	public List<string> values;

	public int activeValue = 0;
	public int defaultValue = 0;
	public float defaultSliderValue = 0;

	public bool inFolder = false;

	public GameObject settingobj;
	public Transform control;
}

static class MPLCustomSettings
{
	public static bool SettingsSetUp = false;
	public static Dictionary<string, MPLSetting> settings = new Dictionary<string, MPLSetting>();
	public static int settingsPage = 0;
	public static Transform previousPage;
	public static Transform nextPage;

    	internal static bool folderActive = false;
    	internal static string currentFolder = "FolderSetup|Outside";
    	internal static string folderReturnKey = "FolderSetup|ReturnKey";

    	public static bool GetSettingToggle(string name) {
		if (!settings.ContainsKey(name)) {
			MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
			return false;
		}
		if (settings[name].type != SettingType.Toggle) {
			MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Toggle was asked for.", LogLevel.Major);
			return false;
		}
		return PlayerPrefs.GetInt(name) > 0 ? true : false;
	}
	public static string GetSettingRotation(string name) {
		if (!settings.ContainsKey(name)) {
			MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
			return "";
		}
		if (settings[name].type != SettingType.Rotation) {
			MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Rotation was asked for.", LogLevel.Major);
			return "";
		}
		return settings[name].values[PlayerPrefs.GetInt(name)];
	}
	public static float GetSettingSlider(string name) {
		if (!settings.ContainsKey(name)) {
			MPLog.Log("Setting " + name + " does not exist!", LogLevel.Major);
			return 0;
		}
		if (settings[name].type != SettingType.Slider) {
			MPLog.Log("Setting " + name + " is a " + settings[name].type.ToString() + ", when a Slider was asked for.", LogLevel.Major);
			return 0;
		}
		return PlayerPrefs.GetFloat(name);
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

    public static void AddSettingToggle(string name, bool defaultval) {
		if (!settings.ContainsKey(name)) {
			MPLSetting setting = new MPLSetting();
			setting.name = name;
			setting.type = SettingType.Toggle;
			setting.defaultValue = defaultval ? 1 : 0;
			settings.Add(name, setting);
		} else {
			MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
		}
	}
	public static void AddSettingRotation(string name, List<string> values, int defaultval) {
		if (!settings.ContainsKey(name)) {
			MPLSetting setting = new MPLSetting();
			setting.name = name;
			setting.values = values;
			setting.type = SettingType.Rotation;
			setting.defaultValue = defaultval % setting.values.Count; //doing the modulo for extra safety
			settings.Add(name, setting);
		} else {
			MPLog.Log("Setting " + name + " was not added as it was already an initialized setting", LogLevel.Minor);
		}
	}
	public static void AddSettingSlider(string name, float defaultval) {
		if (!settings.ContainsKey(name)) {
			MPLSetting setting = new MPLSetting();
			setting.name = name;
			setting.type = SettingType.Slider;
			setting.defaultSliderValue = defaultval;
			settings.Add(name, setting);
		} else {
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
        setting.values = new List<string>();
        setting.type = SettingType.Folder;

        List<KeyValuePair<string, MPLSetting>> sorted = settings.ToList();
        int folderCount = 0;
        foreach (MPLSetting value in settings.Values) { if (value.type == SettingType.Folder) { folderCount++; } }
        sorted.Insert(folderCount, new KeyValuePair<string, MPLSetting>(name, setting));
        settings.Clear();
        foreach (KeyValuePair<string, MPLSetting> pair in sorted) { settings.Add(pair.Key, pair.Value); }
    }

    public static void NextSettingsPage() {
		if (settings.Count > settingsPage * 18 + 2) {
			settingsPage++;
			UpdateSettingsPage();
		}
	}
	public static void PreviousSettingsPage() {
		if (settingsPage > 0) {
			settingsPage--;
			UpdateSettingsPage();
		}
	}
	public static void UpdateSettingsPage() {
        if (settings.ContainsKey(folderReturnKey)) { settings[folderReturnKey].settingobj.SetActive(false); }

        foreach (KeyValuePair<string, MPLSetting> pair in settings)
        {
            pair.Value.control.GetComponent<TextMeshProUGUI>().color= U.I.GetColor(UIColor.White);
        }
        nextPage.GetChild(0).GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);
        previousPage.GetChild(0).GetComponent<TextMeshProUGUI>().color = U.I.GetColor(UIColor.White);

        int controlNum = 0;

        if (folderActive)
		{
			foreach (KeyValuePair<string, MPLSetting> pair in settings)
			{
				MPLSetting option = pair.Value;
                option.settingobj.SetActive(false);
                if (pair.Key.StartsWith(currentFolder + "/") & pair.Value.inFolder)
				{
                    if (controlNum < 18)
					{
						controlNum++;
                        option.settingobj.SetActive(true);
                    }
                }
            }

			settings[folderReturnKey].settingobj.SetActive(true);

            previousPage.gameObject.SetActive(false);
            nextPage.gameObject.SetActive(false);

            foreach (MPLSetting option in settings.Values)
			{
                if (option.settingobj.activeSelf & S.I.optCtrl.content.activeSelf) { S.I.optCtrl.btnCtrl.SetFocus(option.settingobj); break; }
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

        foreach (MPLSetting setting in settingList) {
			controlNum++;
			if (controlNum < settingsPage * 18 + 2 && settingsPage != 0) {
				setting.settingobj.SetActive(false);
			} else if (controlNum < (settingsPage + 1) * 18 + 2 + ((settings.Count == settingsPage * 18 + 2) ? 1 : 0)) {
				setting.settingobj.SetActive(true);
			} else {
				setting.settingobj.SetActive(false);
			}
		}

		if (currentFolder != "FolderSetup|Outside")
		{
			if (S.I.optCtrl.content)
			{
				S.I.optCtrl.btnCtrl.SetFocus(settings[currentFolder].settingobj);
                currentFolder = "FolderSetup|Outside";
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
		if (settingsPage == 0) {
			previousPage.gameObject.SetActive(false);
		} else {
			previousPage.gameObject.SetActive(true);
		}
		if (settingList.Count > (settingsPage + 1) * 18 + 2) {
			nextPage.gameObject.SetActive(true);
		} else {
			nextPage.gameObject.SetActive(false);
		}
	}
	internal static void UpdateFolders()
	{
		foreach (MPLSetting setting in settings.Values)
		{
			if (setting.type == SettingType.Folder) { setting.values.Clear(); }
		}

		foreach (KeyValuePair<string, MPLSetting> pair in settings)
		{
            if (pair.Key.Contains("/")) 
			{
                string folder = pair.Key.Substring(0, pair.Key.IndexOf("/"));
                if (settings.ContainsKey(folder))
                {
                    if (pair.Value.name.StartsWith(folder + "/"))
					{
                        pair.Value.name = pair.Value.name.Substring(pair.Value.name.IndexOf("/") + 1);
                    }
                    pair.Value.inFolder = true;

                    if (!settings[folder].values.Contains(pair.Value.name))
                    {
                        settings[folder].values.Add(pair.Value.name);
                    }
                }
            }
        }

		if (!settings.ContainsKey(folderReturnKey))
		{
			MPLSetting returnBtn = new MPLSetting();
			returnBtn.type = SettingType.Return;
			settings.Add(folderReturnKey, returnBtn);
		}
	}
}

[HarmonyPatch(typeof(OptionCtrl))]
[HarmonyPatch("Open")]
class SettingsPatch
{
	static void Prefix(OptionCtrl __instance) {
		if (MPLCustomSettings.SettingsSetUp == false) {
            MPLCustomSettings.UpdateFolders();

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

			for (int i = 0; i < navPanelButtons.transform.childCount; i++) {
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

            foreach (MPLSetting setting in MPLCustomSettings.settings.Values) {
				switch (setting.type) {
					case SettingType.Toggle:
						if (PlayerPrefs.HasKey(setting.name)) {
							setting.activeValue = PlayerPrefs.GetInt(setting.name);
						} else {
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
								PlayerPrefs.SetInt(setting.name, setting.activeValue);
								setting.control.GetComponent<TextMeshProUGUI>().text = setting.name + ": " + (setting.activeValue > 0 ? "True" : "False");
							});
							EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
							trigger.triggers.Add(entry);
							trigger.enabled = false;
						}
						break;
					case SettingType.Rotation:
						if (PlayerPrefs.HasKey(setting.name)) {
							setting.activeValue = PlayerPrefs.GetInt(setting.name);
						} else {
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
								PlayerPrefs.SetInt(setting.name, setting.activeValue);
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
						setting.settingobj.transform.name = setting.name;
						setting.control = setting.settingobj.transform.GetChild(3);
						setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

						if (PlayerPrefs.HasKey(setting.name)) {
							setting.settingobj.GetComponent<Slider>().value = PlayerPrefs.GetFloat(setting.name);
						} else {
							setting.settingobj.GetComponent<Slider>().value = setting.defaultSliderValue;
						}
						PowerMonoBehavior.sliders.Add(setting.settingobj.transform);
						break;
					case SettingType.Folder:
                        setting.settingobj = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
                        setting.settingobj.SetActive(true);
                        setting.control = setting.settingobj.transform.GetChild(0);
                        setting.control.GetComponent<TextMeshProUGUI>().text = setting.name;
						if (setting.values.Count > 0) 
						{ 
							setting.control.GetComponent<TextMeshProUGUI>().text += " [ +" + setting.values.Count.ToString() + " ]"; 
						}
						else
						{
                            setting.control.GetComponent<TextMeshProUGUI>().text += " []";
                        }

                        setting.control.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                        setting.control.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;

                        setting.control.GetComponent<I2.Loc.Localize>().Term = "-";

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
                        {
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
								MPLCustomSettings.folderActive = true;
								MPLCustomSettings.currentFolder = setting.name;
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

                        Object.DestroyImmediate(setting.settingobj.GetComponent<Button>());
						{
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            entry.callback.AddListener((data) => {
                                MPLCustomSettings.folderActive = false;
                                MPLCustomSettings.UpdateSettingsPage();
                            });

                            EventTrigger trigger = setting.settingobj.AddComponent<EventTrigger>();
                            trigger.triggers.Add(entry);
                            trigger.enabled = false;
                        }
                        break;

				}
				if (setting.type != SettingType.Folder)
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
			navPanelExit.GetComponent<Button>().onClick.AddListener(() => { S.I.optCtrl.ClosePanel(modSettingsMenu.GetComponent<NavPanel>()); });
			Traverse.Create(navPanelExit.GetComponent<UIButton>()).Field("button").SetValue(navPanelExit.GetComponent<Button>());

			MPLCustomSettings.UpdateSettingsPage();

			MPLCustomSettings.SettingsSetUp = true;
		}
	}
}
