using UnityEngine;
using MoonSharp.Interpreter;
using System.Collections.Generic;

class LuaPowerTrigger
{
    public FTrigger _trigger;
    public string _func;
    public LuaPowerTrigger(FTrigger trigger, string func) {
        _trigger = trigger;
        _func = func;
    }
}
static class LuaPowerData
{
    static public List<string> statuses = new List<string>();
    static public List<string> brands = new List<string>();
    static public List<string> effects = new List<string>();
    static public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    static public Dictionary<string, Material> materials = new Dictionary<string, Material>();
    static public List<Script> scripts = new List<Script>();
    static public List<LuaPowerTrigger> luaHooks = new List<LuaPowerTrigger>();
    static public Dictionary<string, string> DPS = new Dictionary<string, string>() //Default Particle System
    {
        { "sprite", "Normal" },             //sprite name from MakeSprite
        { "xOff", "0" },
        { "yOff", "0" },
        { "zOff", "0" },
        { "loop", "false" },                //true, True, TRUE, 1, false, False, FALSE, 0
        { "duration", "0.15" },
        { "startDelay", "0" },
        { "startLifetime", "0.5" },
        { "startSpeed", "500" },
        { "startSize", "2" },
        { "startSizeMin", "-1" },           //These overwrite startSize when both are a positive number
        { "startSizeMax", "-1" },           //This is the same as changing size over time.
        { "startRotation", "0" },
        { "startColor", "#FFF" },           //hex color (you can also pass alpha in the hex color)
        { "startColorMin", "" },            //These overwrite startColor when both are a valid color.
        { "startColorMax", "" },            //This is the same as changing color over time.
        { "gravityModifier", "0" },
        { "simulationSpace", "Local" },     //Local, Global
        { "emissionRate", "10" },
        { "burstCount", "1" },              //Integers only
        { "shape", "Sphere" },              //Sphere, Hemisphere, Cone, Donut, Box, Circle, Edge, Rectangle //TODO
        { "TEST", "TEST" }                  //To Add: Color Over Lifetime
    };
}