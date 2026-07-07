using System;
using UnityEngine;
using FocusWorld.Interaction; // Selectable 在此命名空间；契约引用它，需 using

namespace FocusWorld.Core
{
    /// <summary>
    /// ===== 冻结契约（Day1 上午两人一起定死，之后只增字段、不改/删已有字段）=====
    ///
    /// 本文件不是功能代码，而是 A（逻辑）与 B（表现/UI）之间的“通信语言”。
    /// A 不需要知道 UI 怎么做动画；B 不需要知道舒尔特怎么判定顺序。
    /// 双方只共同认识这里定义的几个消息包：
    ///   TaskStartedEvent    —— 任务开始，UI 初始化（目标/总数/计时起点）
    ///   SelectionEvent      —— 用户选中了谁（不含对错）
    ///   SelectionResultEvent—— 一次点击的判定结果（UI/反馈/音频/数据共同订阅）
    ///   TaskRecord          —— 单次任务最终结果（落盘 + 结果面板）
    ///
    /// 补齐说明：相比初版，为 UI 增加了
    ///   expectedValue / nextExpectedValue / completedCount / totalCount / TaskStartedEvent
    /// 这些是 UI 显示“当前目标 / 进度 X/25 / 计时”所必需的，先补齐再冻结。
    /// </summary>

    public enum TaskType { Schulte, Letter, Symbol, Language }

    /// <summary>
    /// 错误类型。第一阶段只用 None/Wrong；Symbol 模块的镜像/旋转分类是第二阶段扩展点，
    /// 枚举先占位，避免二期改数据结构。
    /// </summary>
    public enum ErrorType
    {
        None,
        Wrong,             // 第一阶段：泛化“错了”
        MirrorConfusion,   // 二期：b↔d, p↔q
        VerticalFlip,      // 二期：b↔p, d↔q
        RotationConfusion, // 二期：6↔9
        ShapeSimilar
    }

    /// <summary>本次选择来自哪种输入设备（用于数据分析/调试，UI 可忽略）。</summary>
    public enum InputSourceType { Mouse, Pen, Gesture }

    /// <summary>
    /// 任务开始时发布，供 UI 初始化：知道总数、第一个目标、计时起点。
    /// 发布者：TaskBase.Begin()；订阅者：UIService、AudioService。
    /// </summary>
    public sealed class TaskStartedEvent
    {
        public TaskType taskType;
        public int totalCount;         // 本任务总目标数（Schulte = 25）
        public int firstExpectedValue; // 第一个要找的目标（Schulte = 1）
        public float startTime;        // Time.time，UI 据此计时
    }

    /// <summary>
    /// 输入层发布：用户选择了哪个对象。只描述事实，不判断对错。
    /// 发布者：RayPointer；订阅者：TaskBase。
    /// </summary>
    public sealed class SelectionEvent
    {
        public Selectable target;
        public Vector3 worldPos;
        public float time;
        public InputSourceType inputSource;
    }

    /// <summary>
    /// 任务层发布：一次点击的判定结果。表现/音频/数据/UI 共同订阅。
    /// 发布者：TaskBase.OnSelect（字段由子类 Evaluate + 基类补全）；
    /// 订阅者：UIService、FeedbackService、AudioService、DataStore。
    /// </summary>
    public sealed class SelectionResultEvent
    {
        public bool correct;
        public Selectable target;

        public int selectedValue;     // 用户实际点击的值
        public int expectedValue;     // 点击前系统要求寻找的值
        public int nextExpectedValue; // 处理后下一个要找的值；任务完成时为 0

        public int completedCount;    // 已正确完成数量（供进度条）
        public int totalCount;        // 总数

        public ErrorType errorType;   // 一期恒为 None/Wrong
        public InputSourceType inputSource; // 本次点击来源（透传自 SelectionEvent，供数据分析）
    }

    /// <summary>
    /// 单次任务的最终结果记录。任务结束时发布并落盘 + 结果面板展示。
    /// 命名用 duration（明确是“用时”，不是时间戳）。
    /// accuracy = 正确次数 /（正确 + 错误），而非 25/25，否则错再多也是 100%。
    /// </summary>
    [Serializable]
    public sealed class TaskRecord
    {
        public string task;
        public float duration;      // 用时（秒）
        public int correctCount;    // 正确选择次数
        public int mistakes;        // 错误次数
        public int totalSelections; // 总点击 = correct + mistakes
        public float accuracy;      // 0~1

        public TaskRecord() { }

        public TaskRecord(TaskType type, float duration, int correctCount, int mistakes)
        {
            task = type.ToString().ToLowerInvariant();
            this.duration = duration;
            this.correctCount = correctCount;
            this.mistakes = mistakes;
            totalSelections = correctCount + mistakes;
            accuracy = totalSelections > 0 ? correctCount / (float)totalSelections : 0f;
        }
    }
}
