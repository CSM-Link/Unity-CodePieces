using System;
using System.Collections.Generic;
using XLuaCore;
using UnityEngine;

namespace LuaCSharp
{
    [XLua.LuaCallCSharp]
    public class CSharpFunctions
    {
        public static Action FunctionStore;

        public static void RegisterFunction(Action func)
        {
            if (func != null)
            {
                Debug.Log($"Registered a lua function");
                FunctionStore += func;
            }
        }

        // 清理注册的函数，lua 侧在注册前调用以及销毁时调用，可以设计成附带其他参数，例如脚本名字，方便 C# 侧对事件进行更加独立的管理
        public static void ClearRegisteredFunctions()
        {
            Debug.Log("clearing registered Lua functions...");
            FunctionStore = null; // Clear previous registrations
        }

    }
}

