using TMPro;
using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Presentation
{
    /// <summary>
    /// 世界空间 UI 服务（B 负责）。只订阅事件、只读契约字段，不含任务逻辑。
    /// 展示：当前目标 / 进度 X/总数 / 计时 / 提示语 / 结果面板。
    /// Canvas 必须 Render Mode = World Space，贴在方格旁，不要贴满视野。
    ///
    /// 依赖的契约字段全部来自补齐后的 Contract.cs：
    ///   TaskStartedEvent.firstExpectedValue / totalCount / startTime
    ///   SelectionResultEvent.completedCount / nextExpectedValue / expectedValue
    ///   TaskRecord.duration / mistakes / accuracy
    /// </summary>
    public class SchulteUIService : MonoBehaviour
    {
        [Header("In-task UI")]
        [SerializeField] TMP_Text currentTargetText;
        [SerializeField] TMP_Text progressText;
        [SerializeField] TMP_Text timerText;
        [SerializeField] TMP_Text hintText;

        [Header("Result UI")]
        [SerializeField] GameObject resultPanel;
        [SerializeField] TMP_Text durationText;
        [SerializeField] TMP_Text mistakesText;
        [SerializeField] TMP_Text accuracyText;

        bool timerRunning;
        float taskStartTime;

        void OnEnable()
        {
            EventBus.Subscribe(GameEvent.TaskStarted, OnTaskStarted);
            EventBus.Subscribe(GameEvent.SelectionResult, OnSelectionResult);
            EventBus.Subscribe(GameEvent.TaskCompleted, OnTaskCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(GameEvent.TaskStarted, OnTaskStarted);
            EventBus.Unsubscribe(GameEvent.SelectionResult, OnSelectionResult);
            EventBus.Unsubscribe(GameEvent.TaskCompleted, OnTaskCompleted);
        }

        void Update()
        {
            if (!timerRunning || timerText == null) return;
            float elapsed = Time.time - taskStartTime;
            timerText.text = $"用时：{elapsed:F1} 秒";
        }

        void OnTaskStarted(object payload)
        {
            if (payload is not TaskStartedEvent e) return;

            taskStartTime = e.startTime;
            timerRunning = true;

            if (currentTargetText) currentTargetText.text = $"当前目标：{e.firstExpectedValue}";
            if (progressText) progressText.text = $"进度：0 / {e.totalCount}";
            if (hintText) hintText.text = "请从 1 开始按顺序寻找";
            if (resultPanel) resultPanel.SetActive(false);
        }

        void OnSelectionResult(object payload)
        {
            if (payload is not SelectionResultEvent e) return;

            if (progressText) progressText.text = $"进度：{e.completedCount} / {e.totalCount}";

            if (e.correct)
            {
                if (hintText) hintText.text = "很好，继续！";
                if (currentTargetText)
                    currentTargetText.text = e.nextExpectedValue > 0
                        ? $"当前目标：{e.nextExpectedValue}"
                        : "已完成";
            }
            else
            {
                if (hintText) hintText.text = $"再找找，当前目标是 {e.expectedValue}";
            }
        }

        void OnTaskCompleted(object payload)
        {
            if (payload is not TaskRecord rec) return;

            timerRunning = false;
            if (resultPanel) resultPanel.SetActive(true);
            if (durationText) durationText.text = $"完成时间：{rec.duration:F1} 秒";
            if (mistakesText) mistakesText.text = $"错误次数：{rec.mistakes}";
            if (accuracyText) accuracyText.text = $"正确率：{rec.accuracy:P0}";
        }
    }
}
