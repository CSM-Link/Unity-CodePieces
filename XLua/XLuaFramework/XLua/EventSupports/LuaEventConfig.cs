using System;
using System.Collections.Generic;

namespace XLuaCore.EventSupports
{
    [Flags]
    public enum EventCullFlag
    {
        None = 0,
        Basic = 1 << 0, Updating = 1 << 1, Enabling = 1 << 2,
        Physics2D = 1 << 3, Physics3D = 1 << 4, Register = 1 << 5,
    }

    public class LuaEventConfig
    {
        public static readonly Dictionary<EventCullFlag, Type> EventSupportMap = new()
        {
            [EventCullFlag.Basic] = typeof(BasicEventSupport),
            [EventCullFlag.Updating] = typeof(UpdatingEventSupport),
            [EventCullFlag.Enabling] = typeof(EnablingEventSupport),
            [EventCullFlag.Physics2D] = typeof(Physics2DEventSupport),
            [EventCullFlag.Physics3D] = typeof(Physics3DEventSupport),
            [EventCullFlag.Register] = typeof(RegisterEventSupport),
        };
    }

}
