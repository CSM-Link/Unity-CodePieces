#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace CustomToolbar
{
    public static class StaticFuncs
    {

        private static void OpenAssetFolder()
        {
            Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            if (selectedAssets.Length == 0)
            {
                Debug.LogWarning("No asset selected.");
                return;
            }

            foreach (Object asset in selectedAssets)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                EditorUtility.RevealInFinder(assetPath);
            }
        }
    }
}

#endif // UNITY_EDITOR