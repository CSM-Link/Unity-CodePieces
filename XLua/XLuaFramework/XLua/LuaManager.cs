using XLua;
using System;
using UnityEngine;
using System.IO;
using XLua.LuaDLL;
using System.Collections.Generic;
using XLuaCore;
using System.Collections;
using LuaCSharp;

public class LuaManager : SingletonMono<LuaManager>
{
    public readonly LuaEnv Env = new();
    private float lastGCTime = 0;
    private const float GC_INTERVAL = 1; // 1 second 

    public override void Awake()
    {
        base.Awake();
        Env.DoString("Include=CS.XLuaCore.IncludeManager.Include");
        Env.DoString("Include('DefaultInclude')");
    }


    private void Update()
    {
        if (Time.unscaledTime - lastGCTime >= GC_INTERVAL)
        {
            lastGCTime = Time.unscaledTime;
            Env.Tick();
        }
    }

    private void OnDestroy()
    {
        IncludeManager.CleanUp(); // 清理加载的 Lua 模块

        DisposeEnv(); // 销毁 Lua 环境
    }

    private void DisposeEnv()
    {
        // Env?.Dispose(); // LuaBehaviour 的 OnDestroy 会调用 Lua 侧的 OnDestroy 方法，如果同时调用本函数的 OnDestroy，无法保证 lua 侧的在本函数之前执行，因此有时会报错
    }


}

