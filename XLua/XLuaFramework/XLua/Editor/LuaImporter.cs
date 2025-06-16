#if UNITY_2018_1_OR_NEWER
using UnityEngine;
using UnityEditor;

using System.IO;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace XLuaCore.Editor
{
    [ScriptedImporter(2, new[] { "lua" })]
    public class LuaImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var id = AssetDatabase.AssetPathToGUID(ctx.assetPath);
            var asset = LuaAsset.Create(File.ReadAllText(ctx.assetPath), id);
            ctx.AddObjectToAsset("LuaCode", asset);
            ctx.SetMainObject(asset);
        }
    }
}

#endif
