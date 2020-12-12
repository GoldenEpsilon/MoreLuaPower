using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

class LuaPowerParticles
{
    static public void ParticleEffect(Being being, Dictionary<string, string> system) {
        GameObject partSys = new GameObject();
        partSys.transform.parent = being.transform;
        var ps = partSys.AddComponent<ParticleSystem>();
        var psrend = partSys.GetComponent<ParticleSystemRenderer>();
        ParticleSystem.MainModule psmain = ps.main;
        ParticleSystem.ShapeModule psshape = ps.shape;
        ParticleSystem.EmissionModule psemiss = ps.emission;
        var pso = new Dictionary<string, string>();     //particle system options
        LuaPowerData.DPS.ToList().ForEach((x) => { pso[x.Key] = x.Value; });
        system.ToList().ForEach((x) => { pso[x.Key] = x.Value; });

        partSys.transform.localPosition = new Vector3(0, 0, 0);
        if (!LuaPowerData.materials.ContainsKey(pso["sprite"])) {
            LuaPowerData.materials[pso["sprite"]] = new Material(being.gameObject.GetComponent<Renderer>().material);
            Debug.Log(being.gameObject.GetComponent<Renderer>().material.name);
            if (LuaPowerData.sprites.ContainsKey(pso["sprite"])) {
                LuaPowerData.materials[pso["sprite"]].mainTexture = LuaPowerData.sprites[pso["sprite"]].texture;
            } else {
                Debug.Log("ERROR: " + pso["sprite"] + " is not a texture!");
            }

        }
        psrend.material = LuaPowerData.materials[pso["sprite"]];
        if (float.TryParse(pso["startSize"], out float ss)) {
            psmain.startSize = ss;
        }
        if (float.TryParse(pso["startSizeMin"], out float ssmn) && float.TryParse(pso["startSizeMax"], out float ssmx) && ssmn > 0 && ssmx > 0) {
            psmain.startSize = new ParticleSystem.MinMaxCurve(ssmn, ssmx);
        }
        if (float.TryParse(pso["startRotation"], out float sr)) {
            psmain.startRotation = sr * (float)(Math.PI / 180f);
        }
        Color tempColor;
        if (ColorUtility.TryParseHtmlString(pso["startColor"], out tempColor)) {
            psmain.startColor = tempColor;
        }
        Color tempColor2;
        Color tempColor3;
        if (ColorUtility.TryParseHtmlString(pso["startColorMin"], out tempColor2) && ColorUtility.TryParseHtmlString(pso["startColorMax"], out tempColor3)) {
            psmain.startColor = new ParticleSystem.MinMaxGradient(tempColor2, tempColor3);
        }
        psmain.startSpeed = float.Parse(pso["startSpeed"]);
        psmain.duration = float.Parse(pso["duration"]);
        psmain.loop = (pso["loop"].ToLower() == "true" || pso["loop"] == "1") && !(pso["loop"].ToLower() == "false" || pso["loop"] == "0");
        psmain.startLifetime = float.Parse(pso["startLifetime"]);
        psshape.shapeType = ParticleSystemShapeType.Sphere; //TODO
        psshape.position = new Vector3(float.Parse(pso["xOff"]), float.Parse(pso["yOff"]), float.Parse(pso["zOff"]));
        psemiss.burstCount = int.Parse(pso["burstCount"]);
        psemiss.rateOverTime = float.Parse(pso["emissionRate"]);
        ps.Play();
    }
}