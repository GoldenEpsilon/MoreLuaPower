using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum SettingType { 
    Toggle,
	Rotation,
	Slider
}

class MPLSetting {
	public string name;
	public SettingType type;
	public List<string> values;
	public int activeValue = 0;
	public int defaultValue = 0;
	public float defaultSliderValue = 0;
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
		int controlNum = 0;
		foreach (MPLSetting setting in settings.Values) {
			controlNum++;
			if (controlNum < settingsPage * 18 + 2 && settingsPage != 0) {
				setting.settingobj.SetActive(false);
			} else if (controlNum < (settingsPage + 1) * 18 + 2 + ((settings.Count == settingsPage * 18 + 2) ? 1 : 0)) {
				setting.settingobj.SetActive(true);
			} else {
				setting.settingobj.SetActive(false);
			}
		}
		nextPage.gameObject.SetActive(false);
		if (settingsPage == 0) {
			previousPage.gameObject.SetActive(false);
		} else {
			previousPage.gameObject.SetActive(true);
		}
		if (settings.Count > (settingsPage + 1) * 18 + 2) {
			nextPage.gameObject.SetActive(true);
		} else {
			nextPage.gameObject.SetActive(false);
		}
	}
}

[HarmonyPatch(typeof(OptionCtrl))]
[HarmonyPatch("Open")]
class SettingsPatch
{
	static void Prefix(OptionCtrl __instance) {
		if (MPLCustomSettings.SettingsSetUp == false) {
			var button = Object.Instantiate(__instance.navButtonGrid.GetChild(1), __instance.navButtonGrid);
			button.GetComponent<UIButton>().tmpText.text = "MODS";
			button.GetChild(0).GetComponent<I2.Loc.Localize>().Term = "MODS";

			var modSettingsMenu = Object.Instantiate(__instance.settingsPane.gameObject);
			modSettingsMenu.transform.position = __instance.settingsPane.transform.position;
			modSettingsMenu.transform.parent = __instance.settingsPane.transform.parent;
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
			button.gameObject.AddComponent<Button>();
			button.GetComponent<Button>().onClick.AddListener(() => { S.I.optCtrl.OpenPanel(modSettingsMenu.GetComponent<NavPanel>()); });
			Traverse.Create(button.GetComponent<UIButton>()).Field("button").SetValue(button.GetComponent<Button>());

			button.GetComponent<UIButton>().onAcceptPress.RemoveAllListeners();

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
				}
			}

			GameObject NextButton = Object.Instantiate(navPanelButtons.transform.GetChild(0).gameObject, navPanelButtons.transform);
			NextButton.SetActive(true);
			NextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Next Page";
			NextButton.transform.GetChild(0).GetComponent<I2.Loc.Localize>().Term = "Next Page";
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