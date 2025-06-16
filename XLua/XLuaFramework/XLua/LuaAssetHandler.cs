using System;
using UnityEngine;
using XLua;


namespace XLuaCore
{
    internal class LuaAssetHandler : IDisposable
    {
        private LuaAsset m_LuaAsset;
        public LuaTable ScriptScopeTable;

        public LuaAssetHandler(LuaAsset asset)
        {
            m_LuaAsset = asset;

            var Env = LuaManager.Instance.Env;

            ScriptScopeTable = Env.NewTable();
            using var meta = Env.NewTable();
            meta.Set("__index", Env.Global);
            ScriptScopeTable.SetMetaTable(meta);

            ScriptScopeTable.Set("self", this);
            ScriptScopeTable.Set("Global", LuaManager.Instance.Env.Global);
            Env.DoString(m_LuaAsset.Code, m_LuaAsset.name, ScriptScopeTable);

            m_LuaAsset.ReloadEvent += Reload;
        }

        private void Reload()
        {
            LuaManager.Instance.Env.DoString(m_LuaAsset.Code, m_LuaAsset.name, ScriptScopeTable);
        }

        public void Dispose()
        {
            m_LuaAsset.ReloadEvent -= Reload;
            ScriptScopeTable.Dispose();
        }
    }
}