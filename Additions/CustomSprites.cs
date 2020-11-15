using UnityEngine;
using System.IO;

static class LuaPowerSprites
{
    static public void MakeSprite(string image, string PATH, string name) {
        string str = Path.Combine(PATH, image);
        if (S.I.GetComponent<PowerMonoBehavior>() == null) {
            S.I.gameObject.AddComponent<PowerMonoBehavior>();
        }

        if (LuaPowerData.sprites.ContainsKey(name)) {
            Debug.Log("ERROR: A Sprite exists with this name already.");
            return;
        }
        LuaPowerData.sprites[name] = null;
        S.I.GetComponent<PowerMonoBehavior>().StartCoroutine(
         PowerMonoBehavior.LoadSprite(str, (Texture2D content) => {
             if (content != null) {
                 content.filterMode = FilterMode.Point;
                 LuaPowerData.sprites[name] = Sprite.Create(content, new Rect(0f, 0f, (float)content.width, (float)content.height), new Vector2(0.5f, 0.5f), 1f);
             } else {
                 Debug.Log("ERROR: MakeSprite didn't find anything in content");
             }
         }));
    }

    static public Sprite GetSprite(string image) {
        if (LuaPowerData.sprites.ContainsKey(image) && LuaPowerData.sprites[image] != null) {
            return LuaPowerData.sprites[image];
        }
        return null;
    }
}