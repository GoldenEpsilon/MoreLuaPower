using UnityEngine;
using System.Collections.Generic;

public class LuaPowerBeingVariables
{
    public static void SetVariable(Being being, string variableName, string variable) {
        if (being.GetComponent<LuaPowerBeingVariablesMonoBehavior>() == null) {
            being.gameObject.AddComponent<LuaPowerBeingVariablesMonoBehavior>();
        }
        being.GetComponent<LuaPowerBeingVariablesMonoBehavior>().BeingVariables[variableName] = variable;
    }
    public static string GetVariable(Being being, string variableName) {
        if (being.GetComponent<LuaPowerBeingVariablesMonoBehavior>() == null) {
            being.gameObject.AddComponent<LuaPowerBeingVariablesMonoBehavior>();
        }
        if (being.GetComponent<LuaPowerBeingVariablesMonoBehavior>().BeingVariables.ContainsKey(variableName)) {
            return being.GetComponent<LuaPowerBeingVariablesMonoBehavior>().BeingVariables[variableName];
        } else {
            return "Variable Not Set";
        }
    }
}

public class LuaPowerBeingVariablesMonoBehavior : MonoBehaviour
{
    public Dictionary<string, string> BeingVariables = new Dictionary<string, string>();
}