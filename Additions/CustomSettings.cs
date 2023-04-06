using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
/*
static class CustomSettingsSetUp
{
	public static bool SettingsSetUp = false;
}

[HarmonyPatch(typeof(OptionCtrl))]
[HarmonyPatch("Open")]
class SettingsPatch
{
	static void Prefix(OptionCtrl __instance) {
		if (CustomSettingsSetUp.SettingsSetUp == false) {
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

			CustomSettingsSetUp.SettingsSetUp = true;

			var navPanelBackground = modSettingsMenu.transform.GetChild(0).GetChild(0);
			var navPanelButtons = modSettingsMenu.transform.GetChild(0).GetChild(1);
			var navPanelTitle = modSettingsMenu.transform.GetChild(0).GetChild(2);
			var navPanelExit = modSettingsMenu.transform.GetChild(0).GetChild(3);

			navPanelTitle.GetComponent<TextMeshProUGUI>().text = "MOD SETTINGS";
			navPanelTitle.GetComponent<I2.Loc.Localize>().Term = "MOD SETTINGS";

			Debug.Log("Nav Buttons");
			for (int i = 0; i < navPanelButtons.transform.childCount; i++) {
				Debug.Log(navPanelButtons.transform.GetChild(i).ToString());
				navPanelButtons.transform.GetChild(i).gameObject.SetActive(false);
			}
			navPanelButtons.transform.GetChild(0).gameObject.SetActive(true);

			var componentList = navPanelTitle.GetComponents(typeof(Component));
			Debug.Log("title components");
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}
			componentList = navPanelBackground.GetComponents(typeof(Component));
			Debug.Log("background components");
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}
			componentList = navPanelExit.GetComponents(typeof(Component));
			Debug.Log("exit components");
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}

			/*
			var componentList = button.GetComponents(typeof(Component));
			Debug.Log("button components");
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}

			Debug.Log("button child components");
			componentList = button.GetChild(0).GetComponents(typeof(Component));
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}

			componentList = modSettingsMenu.GetComponents(typeof(Component));
			Debug.Log("nav panel components");
			foreach (var i in componentList) {
				Debug.Log(i.ToString());
			}

			Debug.Log("nav panel children/components");
			for (int i = 0; i < modSettingsMenu.transform.childCount; i++) {
				componentList = modSettingsMenu.transform.GetChild(i).GetComponents(typeof(Component));
				Debug.Log(i.ToString());
				Debug.Log("nav panel child components");
				foreach (var i2 in componentList) {
					Debug.Log(i2.ToString());
				}
				Debug.Log("nav panel children children");
				for (int i2 = 0; i2 < modSettingsMenu.transform.GetChild(i).childCount; i2++) {
					Debug.Log(modSettingsMenu.transform.GetChild(i).GetChild(i2).ToString());
				}
			}*//*
		}
	}
}*/