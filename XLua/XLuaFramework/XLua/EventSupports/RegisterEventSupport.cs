using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using XLua;

namespace XLuaCore.EventSupports
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(LuaBehaviour))]
    public class RegisterEventSupport : MonoBehaviour, ILuaEventSupport
    {
        private Action register;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetEvents(LuaTable table)
        {
            register = table.Get<Action>("register");
        }

        private void CleanEvents()
        {
            register = null;
        }

        void ILuaEventSupport.Initialize(LuaTable table)
        {
            GetEvents(table);
            register?.Invoke();
        }

        void ILuaEventSupport.Reload(LuaTable table)
        {
            GetEvents(table);
            register?.Invoke();
        }

        private void OnDestroy()
        {
            CleanEvents();
        }
    }
}
