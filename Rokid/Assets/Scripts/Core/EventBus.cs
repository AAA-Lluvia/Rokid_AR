using System;
using System.Collections.Generic;

namespace FocusWorld.Core
{
    /// <summary>
    /// 全局事件类型。第一阶段只需要这四个，不要提前设计二十个。
    /// 需要扩展时（如错误分类统计）再新增枚举 + 新增订阅者即可，主循环不动。
    /// </summary>
    public enum GameEvent
    {
        TaskStarted,     // payload: TaskBase
        Selection,       // payload: SelectionEvent —— 用户选中了某个物体（不区分笔/手/鼠标）
        SelectionResult, // payload: SelectionResultEvent —— 任务判定完对错后的结果
        TaskCompleted,   // payload: TaskRecord —— 任务结束
    }

    /// <summary>
    /// 极简事件总线：整个解耦架构的枢纽。约 30 行足够撑起一周 Demo。
    /// 约定：订阅者在 OnEnable/Start 里 Subscribe，必须在 OnDisable/OnDestroy 里 Unsubscribe，
    /// 否则模块场景切换后旧订阅仍挂着，会重复触发或空引用。
    /// </summary>
    public static class EventBus
    {
        static readonly Dictionary<GameEvent, Action<object>> map = new();

        public static void Subscribe(GameEvent e, Action<object> cb)
            => map[e] = (map.TryGetValue(e, out var a) ? a : null) + cb;

        public static void Unsubscribe(GameEvent e, Action<object> cb)
        {
            if (map.TryGetValue(e, out var a)) map[e] = a - cb;
        }

        public static void Publish(GameEvent e, object payload)
        {
            if (map.TryGetValue(e, out var a)) a?.Invoke(payload);
        }

        /// <summary>场景切换/退出时清空，防止残留订阅。</summary>
        public static void Clear() => map.Clear();
    }
}
