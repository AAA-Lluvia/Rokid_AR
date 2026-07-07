using System;
using System.Collections.Generic;

namespace FocusWorld.Data
{
    /// <summary>
    /// ===== 落盘存储模型（DataStore 私有，不属于 A↔B 事件契约）=====
    /// 与架构文档的 PlayerData.json 结构对齐：一个玩家档案含多次 session，
    /// 每次 session 含错误明细数组。JsonUtility 可序列化“含 List 的类”。
    ///
    /// 这里提前落地了二期的 schema 版本 + 错误明细，
    /// 目的是让 Symbol 模块的镜像/旋转错误分类一产生就能被分析（如“镜像混淆率”）。
    /// </summary>
    [Serializable]
    public sealed class PlayerData
    {
        public int schema = 1;                 // schema 版本，后续接服务器/改结构可兼容
        public string user = "child01";
        public List<SessionRecord> sessions = new();
    }

    [Serializable]
    public sealed class SessionRecord
    {
        public string task;          // "schulte"
        public string date;          // yyyy-MM-dd
        public string inputSource;   // mouse / pen / gesture
        public float duration;       // 用时（秒）
        public int correctCount;
        public int mistakes;
        public int totalSelections;
        public float accuracy;       // 0~1
        public List<ErrorDetail> errors = new();
    }

    /// <summary>单次错误明细：科学分析的原子数据（谁被误点成谁、错误类型、发生时刻）。</summary>
    [Serializable]
    public sealed class ErrorDetail
    {
        public int expected;   // 应找的值
        public int selected;   // 实际点的值
        public string type;    // ErrorType 字符串（Wrong / MirrorConfusion / ...）
        public float t;        // 距任务开始的秒数
    }
}
