using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
namespace EditorTools
{

    public class SingletonSOEditorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<Type> singletonTypes = new();
        private Dictionary<Type, ScriptableObject> existingInstances = new();

        [MenuItem("Tools/SingletonSO 管理器")]
        public static void ShowWindow()
        {
            var window = GetWindow<SingletonSOEditorWindow>();
            window.titleContent = new GUIContent("SingletonSO 管理器");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            // 获取所有 SingletonSO 的子类
            singletonTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(SingletonSO)) && !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToList();

            // 查找现有实例
            existingInstances.Clear();
            var allInstances = Resources.FindObjectsOfTypeAll<ScriptableObject>()
                .Where(so => so is SingletonSO);

            foreach (var instance in allInstances)
            {
                var type = instance.GetType();
                if (!existingInstances.ContainsKey(type))
                {
                    existingInstances.Add(type, instance);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SingletonSO 实例管理", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("刷新列表", GUILayout.Height(30)))
            {
                RefreshData();
            }

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 表头
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型", EditorStyles.boldLabel, GUILayout.Width(175));
            EditorGUILayout.LabelField("状态", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 列表内容
            foreach (var type in singletonTypes)
            {
                EditorGUILayout.BeginHorizontal();

                // 类型名称
                EditorGUILayout.LabelField(type.Name, GUILayout.Width(175));

                // 状态
                bool hasInstance = existingInstances.ContainsKey(type);
                EditorGUILayout.LabelField(hasInstance ? "已创建" : "未创建", GUILayout.Width(100));

                // 操作按钮
                if (hasInstance)
                {
                    if (GUILayout.Button("选择实例", GUILayout.Width(80)))
                    {
                        Selection.activeObject = existingInstances[type];
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }

                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("确认删除",
                            $"确定要删除 {type.Name} 的实例吗？", "删除", "取消"))
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(existingInstances[type]));
                            RefreshData();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("创建实例", GUILayout.Width(80)))
                    {
                        CreateSOInstance(type);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateSOInstance(Type type)
        {
            // 确保Resources文件夹存在
            if (!AssetDatabase.IsValidFolder("Assets/Resources/SO"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "SO");
            }

            // 创建实例
            var instance = CreateInstance(type);
            string assetPath = $"Assets/Resources/SO/{type.Name}.asset";

            // 确保路径唯一
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 选中新创建的对象
            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);

            RefreshData();
        }
    }
    
}