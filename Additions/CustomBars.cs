using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

static class LuaPowerBars
{
    public static void AddBar(string name, string barPosition, Being being, float current, float max, int segments, float colorR, float colorG, float colorB, Sprite sprite, bool showValue, bool showMax)
    {
        if(CustomBars_Data.customBars.ContainsKey(name))
        {
            Debug.Log("Bar '" + name + "' already exists.");
            return;
        }
        if ((barPosition == "OverBeing" || barPosition == "BesideBeing" || barPosition == "UnderBeing") && being == null)
        {
            Debug.Log("Cannot attach bar to null being");
            return;
        }

        //Debug.Log("Adding bar");
        // Create bar
        var bar = Object.Instantiate(S.I.deCtrl.duelDiskPrefab.manaBar);
        //Debug.Log("bar added");
        if (bar == null)
        {
            Debug.LogError("Failed to create fillbar");
            return;
        }
        bar.showMaxNum = showMax;
        bar.cut.color = Color.white;
        bar.fill.color = new Color(colorR / 255, colorG / 255, colorB / 255, 1);
        bar.maxLines = segments - 1;
        bar.enabled = true;
        bar.displayNum.outlineWidth = 0.5f;
        bar.displayNum.outlineColor = Color.black;
        bar.displayNum.fontSizeMax = bar.displayNum.fontSize + 4;
        bar.displayNum.fontSize = bar.displayNum.fontSize + 4;
        bar.displayNum.enabled = showValue;
        bar.gameObject.SetActive(true);
        Debug.Log("Bar values set");
        if(sprite != null)
        {
            ((Image)bar.gem.GetComponent(typeof(Image))).sprite = sprite;
            ((Canvas)bar.gem.GetComponent(typeof(Canvas))).sortingOrder = 1;
            bar.gem.SetAsLastSibling();
        }


        int relativePosition = 0;
        if(barPosition == "OverBeing" || barPosition == "BesideBeing" || barPosition == "UnderBeing") {
            relativePosition = CustomBars_Data.customBars.Count(b => b.Value.barPosition == barPosition && b.Value.being == being);
        } else
        {
            relativePosition = CustomBars_Data.customBars.Count(b => b.Value.barPosition == barPosition);
        }
        switch (barPosition)
        {
            case "TopLeft":
                bar.transform.position = new Vector3(S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.x, S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.y - 12 - (relativePosition * (bar.rect.sizeDelta.y + 5)), S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.z);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 10, bar.gem.position.y, bar.gem.position.z);
                break;
            case "TopRight":
                bar.transform.position = new Vector3(S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.x * -1, S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.y - 12 - (relativePosition * (bar.rect.sizeDelta.y + 5)), S.I.deCtrl.duelDiskPrefab.healthBar.transform.position.z);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 10, bar.gem.position.y, bar.gem.position.z);
                break;
            case "Left":
                bar.transform.RotateAround(bar.gem.transform.position, new Vector3(0, 0, 1), 90);
                bar.transform.position = new Vector3(-211 - (relativePosition * bar.rect.sizeDelta.y), 0, 0);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x, bar.gem.position.y + 4, bar.gem.position.z);
                bar.gem.transform.Rotate(new Vector3(0, 0, -90));
                break;
            case "OverMana":
                bar.transform.position = new Vector3(bar.transform.position.x + 5 + (relativePosition * bar.rect.sizeDelta.x), bar.transform.position.y + 12, bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 8, bar.gem.position.y, bar.gem.position.z);
                break;
            case "UnderMana":
                bar.transform.position = new Vector3(bar.transform.position.x + 5 + (relativePosition * bar.rect.sizeDelta.x), bar.transform.position.y - 12, bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 8, bar.gem.position.y, bar.gem.position.z);
                break;
            case "BesideMana":
                bar.transform.position = new Vector3(bar.transform.position.x + ((1 + relativePosition) * bar.rect.sizeDelta.x), bar.transform.position.y, bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 8, bar.gem.position.y, bar.gem.position.z);
                break;
            case "OverBeing":
                bar.transform.position = being.transform.position;
                bar.transform.parent = being.transform;
                bar.transform.position = new Vector3(bar.transform.position.x, bar.transform.position.y + 60 + (bar.rect.sizeDelta.y * relativePosition), bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.35f, 0.5f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.4f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 2, bar.gem.position.y, bar.gem.position.z);
                bar.displayNum.fontSizeMax = bar.displayNum.fontSizeMax + 12;
                bar.displayNum.fontSize = bar.displayNum.fontSize + 12;
                break;
            case "UnderBeing":
                bar.transform.position = being.transform.position;
                bar.transform.parent = being.transform;
                bar.transform.position = new Vector3(bar.transform.position.x, bar.transform.position.y - 12 - (bar.rect.sizeDelta.y * relativePosition), bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.35f, 0.5f, 1);
                bar.gem.transform.localScale = new Vector3(0.6f, 0.4f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x + 2, bar.gem.position.y, bar.gem.position.z);
                bar.displayNum.fontSizeMax = bar.displayNum.fontSizeMax + 12;
                bar.displayNum.fontSize = bar.displayNum.fontSize + 12;
                break;
            case "BesideBeing":
                bar.transform.RotateAround(bar.gem.transform.position, new Vector3(0, 0, 1), 90);
                bar.transform.position = being.transform.position;
                bar.transform.parent = being.transform;
                bar.transform.position = new Vector3(bar.transform.position.x - 25 - (bar.rect.sizeDelta.y * relativePosition), bar.transform.position.y + 20, bar.transform.position.z);
                bar.transform.localScale = new Vector3(0.45f, 0.45f, 1);
                bar.gem.transform.localScale = new Vector3(0.45f, 0.45f, 1);
                bar.gem.position = new Vector3(bar.gem.position.x, bar.gem.position.y + 2, bar.gem.position.z);
                bar.gem.transform.Rotate(new Vector3(0, 0, -90));
                bar.displayNum.fontSizeMax = bar.displayNum.fontSizeMax + 12;
                bar.displayNum.fontSize = bar.displayNum.fontSize + 12;
                break;
        }

        bar.UpdateBar(current, max);
        //Debug.Log("Bar updated");
        CustomBars_Data.customBars.Add(name, new LuaPowerBar(sprite, bar, being, barPosition));
        //Debug.Log("Bar saved");

        //Component[] components = bar.gem.GetComponents(typeof(Component));
        //foreach (Component component in components)
        //{
        //    Debug.Log(component.ToString());
        //}
    }

    public static void UpdateBar(string name, float current, float max)
    {
        if(CustomBars_Data.customBars.ContainsKey(name))
        {
            CustomBars_Data.customBars[name].fillBar.UpdateBar(current, max);
        } else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void ChangeBarSprite(string name, Sprite sprite)
    {
        if (CustomBars_Data.customBars.ContainsKey(name) && sprite != null)
        {
            CustomBars_Data.customBars[name].sprite = sprite;
            try
            {
                ((Image)CustomBars_Data.customBars[name].fillBar.gem.GetComponent(typeof(Image))).sprite = sprite;
            }
            catch
            {
                Debug.Log("Failed to change sprite");
            }
        }
        else if (sprite == null) {
            Debug.Log("Sprite not found");
        } else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void ChangeBarColor(string name, float colorR, float colorG, float colorB)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            CustomBars_Data.customBars[name].fillBar.fill.color = new Color(colorR / 255, colorG / 255, colorB / 255, 1);
        }
        else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void ChangeBarAttributes(string name, bool showValue, bool showMax, int segments)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            CustomBars_Data.customBars[name].fillBar.showMaxNum = showMax;
            CustomBars_Data.customBars[name].fillBar.maxLines = segments - 1;
            CustomBars_Data.customBars[name].fillBar.displayNum.enabled = showValue;
        }
        else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void HideBar(string name)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            ((Canvas)CustomBars_Data.customBars[name].fillBar.GetComponent(typeof(Canvas))).enabled = false;
            ((Canvas)CustomBars_Data.customBars[name].fillBar.gem.GetComponent(typeof(Canvas))).enabled = false;
        }
        else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void ShowBar(string name)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            ((Canvas)CustomBars_Data.customBars[name].fillBar.GetComponent(typeof(Canvas))).enabled = true;
            ((Canvas)CustomBars_Data.customBars[name].fillBar.gem.GetComponent(typeof(Canvas))).enabled = true;
        }
        else
        {
            Debug.Log("Bar '" + name + "' was not found");
        }
    }

    public static void RemoveBar(string name)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            try
            {
                UnityEngine.GameObject.Destroy(CustomBars_Data.customBars[name].fillBar);
                CustomBars_Data.customBars.Remove(name);
            }
            catch { Debug.Log("RemoveBar: Bar does not exist"); }
        }
    }

    public static FillBar GetBar(string name)
    {
        if (CustomBars_Data.customBars.ContainsKey(name))
        {
            return CustomBars_Data.customBars[name].fillBar;
        }
        return null;
    }
}

public class LuaPowerBar
{
    public FillBar fillBar;
    public Sprite sprite;
    public Being being;
    public string barPosition;

    public LuaPowerBar(Sprite sprite, FillBar bar, Being being, string barPosition)
    {
        this.sprite = sprite;
        this.fillBar = bar;
        this.being = being;
        this.barPosition = barPosition;
    }
}

static class CustomBars_Data
{
    public static Dictionary<string, LuaPowerBar> customBars = new Dictionary<string, LuaPowerBar>();
}

//public enum BarPosition
//{
//    Top,
//    Left,
//    OverMana,
//    UnderMana,
//    BesideMana,
//    OverBeing,
//    UnderBeing,
//    BesideBeing
//}