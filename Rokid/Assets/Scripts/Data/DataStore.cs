using System;
using System.IO;
using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Data
{
    /// <summary>
    /// 数据落盘（升级版）。相比初版，提前吃进二期能力：
    ///   1) SessionBuffer —— 训练中每次错误只进内存缓冲，TaskCompleted 才 flush，避免逐次写盘卡顿；
    ///   2) 错误明细 —— 记录 expected/selected/errorType/时刻，为 Symbol 镜像混淆率等分析提供原子数据；
    ///   3) 原子写 —— 先写 .tmp 再替换，防止录屏演示时中途崩坏 JSON；
    ///   4) schema 版本 + 追加式档案 —— 多次 session 累积到同一 PlayerData.json。
    /// 仍用 JsonUtility，不引 Newtonsoft，保持第一天零额外依赖。
    /// </summary>
    public class DataStore : MonoBehaviour
    {
        [SerializeField] string userId = "child01";

        string Path => System.IO.Path.Combine(Application.persistentDataPath, "PlayerData.json");

        // ===== 内存缓冲：一次 session 的实时数据，完成时才落盘 =====
        SessionRecord buffer;
        float sessionStart;

        void OnEnable()
        {
            EventBus.Subscribe(GameEvent.TaskStarted, OnStarted);
            EventBus.Subscribe(GameEvent.SelectionResult, OnResult);
            EventBus.Subscribe(GameEvent.TaskCompleted, OnCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(GameEvent.TaskStarted, OnStarted);
            EventBus.Unsubscribe(GameEvent.SelectionResult, OnResult);
            EventBus.Unsubscribe(GameEvent.TaskCompleted, OnCompleted);
        }

        void OnStarted(object payload)
        {
            if (payload is not TaskStartedEvent e) return;
            sessionStart = e.startTime;
            buffer = new SessionRecord
            {
                task = e.taskType.ToString().ToLowerInvariant(),
                inputSource = InputSourceType.Mouse.ToString().ToLowerInvariant(),
            };
        }

        void OnResult(object payload)
        {
            if (buffer == null || payload is not SelectionResultEvent e) return;

            // 记录本次点击来源（以最后一次为准，一次 session 通常同一设备）
            buffer.inputSource = e.inputSource.ToString().ToLowerInvariant();

            if (!e.correct)
            {
                buffer.errors.Add(new ErrorDetail
                {
                    expected = e.expectedValue,
                    selected = e.selectedValue,
                    type = e.errorType.ToString(),
                    t = Time.time - sessionStart,
                });
            }
        }

        void OnCompleted(object payload)
        {
            if (payload is not TaskRecord rec) return;

            // 用 TaskRecord 的权威汇总填充 buffer，再落盘
            if (buffer == null) buffer = new SessionRecord { task = rec.task };
            buffer.date = DateTimeToDate();
            buffer.duration = rec.duration;
            buffer.correctCount = rec.correctCount;
            buffer.mistakes = rec.mistakes;
            buffer.totalSelections = rec.totalSelections;
            buffer.accuracy = rec.accuracy;

            var data = Load();
            data.user = userId;
            data.sessions.Add(buffer);
            SaveAtomic(data);

            Debug.Log($"[DataStore] session saved ({buffer.task}) -> {Path}\n" +
                      $"  用时{buffer.duration:F1}s 正确{buffer.correctCount} 错误{buffer.mistakes} " +
                      $"正确率{buffer.accuracy:P0} 错误明细{buffer.errors.Count}条");

            buffer = null; // flush 完清空
        }

        // ===== IO =====
        PlayerData Load()
        {
            try
            {
                if (File.Exists(Path))
                    return JsonUtility.FromJson<PlayerData>(File.ReadAllText(Path)) ?? new PlayerData();
            }
            catch (Exception ex) { Debug.LogWarning($"[DataStore] load failed, new profile. {ex.Message}"); }
            return new PlayerData();
        }

        void SaveAtomic(PlayerData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string tmp = Path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(Path)) File.Replace(tmp, Path, null); // 原子替换
            else File.Move(tmp, Path);
        }

        // JsonUtility/存储用；工作流脚本才禁用 DateTime，这里是运行时代码，可用。
        string DateTimeToDate() => DateTime.Now.ToString("yyyy-MM-dd");
    }
}
