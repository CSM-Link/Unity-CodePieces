using XLua;

namespace XLuaCore.EventSupports
{
    internal interface ILuaEventSupport
    {
        internal void Initialize(LuaTable table);

        internal void Reload(LuaTable table);
    }
}
