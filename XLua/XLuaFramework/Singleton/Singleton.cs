public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T _instance;
 
    // 私有构造函数，防止外部实例化
    protected Singleton() { }
 
    // 虚构造函数，子类可以重写
    protected virtual void Initialize() { }
 
    // 静态属性，返回类的唯一实例
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.Initialize(); // 调用虚方法初始化
            }
            return _instance;
        }
    }
}