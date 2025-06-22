using UnityEngine;

public class SingletonSO : ScriptableObject
{
#if UNITY_EDITOR
    protected SingletonSO()
    {
        if (!Application.isEditor)
        {
            Debug.LogError("不允许在运行时创建 SingletonSO 实例！请使用专用工具创建。");
            return;
        }
    }
#endif
}