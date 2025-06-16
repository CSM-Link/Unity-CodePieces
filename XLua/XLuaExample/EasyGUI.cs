using System;
using UnityEngine;
using XLua;


public class EasyGUI : MonoBehaviour
{
    public void OnGUI()
    {
        if (GUI.Button(new Rect(800, 40, 100, 100), "Trigger"))
        {
            LuaCSharp.CSharpFunctions.FunctionStore?.Invoke();
        }
        if (GUI.Button(new Rect(400, 40, 100, 100), "Clear"))
        {
            LuaCSharp.CSharpFunctions.FunctionStore = null;
        }
        if (GUI.Button(new Rect(600, 40, 100, 100), "Destroy"))
        {
            Destroy(gameObject);
        }
    }
}
