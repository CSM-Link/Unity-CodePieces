using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XLuaCore
{
    public class IncludeManager
    {
        private static readonly Dictionary<string, LuaAssetHandler> m_LoadedModules = new();

        [XLua.LuaCallCSharp]
        public static void Include(string luaAssetName)
        {
            if (m_LoadedModules.Keys.Contains(luaAssetName))
            {
                return;
            }

            var asset = Resources.Load<LuaAsset>(luaAssetName);
            if (asset == null)
            {
                Debug.LogError($"Lua asset '{luaAssetName}' not found.");
                return;
            }

            var handler = new LuaAssetHandler(asset);
            m_LoadedModules.Add(luaAssetName, handler);
            LuaManager.Instance.Env.Global.Set(luaAssetName, handler.ScriptScopeTable);
        }

        public static void CleanUp()
        {
            foreach (var module in m_LoadedModules.Values)
            {
                module.Dispose();
            }
            m_LoadedModules.Clear();
        }

    }
}

