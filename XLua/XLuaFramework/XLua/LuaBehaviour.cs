using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using XLuaCore.EventSupports;
using Object = UnityEngine.Object;

namespace XLuaCore
{
    public class LuaBehaviour : MonoBehaviour
    {
        public LuaAsset Code;
        
        public EventCullFlag EventCulling = EventCullFlag.Basic | EventCullFlag.Updating;
        public LuaInjection[] Injections;
        
        internal LuaTable ScriptScopeTable;

        private ILuaEventSupport[] registeredEvents;
        
        private void Awake()
        {
            
            
            ScriptScopeTable = LuaManager.Instance.Env.NewTable();
            
            using var meta = LuaManager.Instance.Env.NewTable();
            meta.Set("__index", LuaManager.Instance.Env.Global);
            ScriptScopeTable.SetMetaTable(meta);
            
            ScriptScopeTable.Set("self", this);
            ScriptScopeTable.Set("Global", LuaManager.Instance.Env.Global);

#if UNITY_EDITOR
            var injectionNames = new HashSet<string>();
#endif
            foreach (var injection in Injections)
            {
                if (string.IsNullOrEmpty(injection.Name) || !injection.Object)
                {
                    continue;
                }
#if UNITY_EDITOR
                if (!injectionNames.Add(injection.Name))
                {
                    Debug.LogError($"Injection '{injection.Name}' is duplicated.");
                    continue;
                }
#endif
                if (injection.Component)
                {
                    ScriptScopeTable.Set(injection.Name, injection.Component);
                }
                else
                {
                    ScriptScopeTable.Set(injection.Name, injection.Object);
                }
            }
            
            LuaManager.Instance.Env.DoString(Code.Code, Code.name, ScriptScopeTable);
            Code.ReloadEvent += Reload;

            var events = new List<ILuaEventSupport>();
            foreach (var flag in Enum.GetValues(typeof(EventCullFlag)))
            {
                if ((EventCulling & (EventCullFlag)flag) == 0)
                {
                    continue;
                }
                if (LuaEventConfig.EventSupportMap.TryGetValue((EventCullFlag)flag, out var type))
                {
                    var eventSupport = (ILuaEventSupport)gameObject.AddComponent(type);
                    eventSupport.Initialize(ScriptScopeTable);
                    events.Add(eventSupport);
                }
            }

            registeredEvents = events.ToArray();
        }

        [LuaCallCSharp]
        public LuaTable Get()
        {
            return ScriptScopeTable;
        }

        private void OnDestroy()
        {
            Code.ReloadEvent -= Reload;
            ScriptScopeTable.Dispose();
        }
        
        private void Reload()
        {
            LuaManager.Instance.Env.DoString(Code.Code, Code.name, ScriptScopeTable);
            foreach (var ev in registeredEvents)
            {
                ev.Reload(ScriptScopeTable);
            }
        }
    }
}
