using UnityEngine;
using FocusWorld.Core;
using FocusWorld.Interaction;

namespace FocusWorld.Debugging
{
    /// <summary>
    /// B 的开发利器：在没有 A 的真实舒尔特逻辑时，用假事件驱动整套 UI/反馈/音频。
    /// 放在 B 自己的测试场景 Scenes/Dev/PresentationTest.unity（不碰 A 的 Bootstrap）。
    ///   C 键 → 模拟点击正确
    ///   W 键 → 模拟点击错误
    ///   S 键 → 模拟任务开始
    ///   F 键 → 模拟任务完成
    /// A 的真实逻辑接入后，删除或禁用本组件，真实事件自然接管。
    /// </summary>
    public class UIEventTester : MonoBehaviour
    {
        [SerializeField] Selectable testTile;   // 随便拖一个带 Selectable 的方块
        [SerializeField] int totalCount = 25;

        int fakeExpected = 1;
        int fakeCompleted;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S)) PublishStarted();
            if (Input.GetKeyDown(KeyCode.C)) PublishResult(true);
            if (Input.GetKeyDown(KeyCode.W)) PublishResult(false);
            if (Input.GetKeyDown(KeyCode.F)) PublishCompleted();
        }

        void PublishStarted()
        {
            fakeExpected = 1; fakeCompleted = 0;
            EventBus.Publish(GameEvent.TaskStarted, new TaskStartedEvent
            {
                taskType = TaskType.Schulte,
                totalCount = totalCount,
                firstExpectedValue = 1,
                startTime = Time.time,
            });
        }

        void PublishResult(bool correct)
        {
            int expectedBefore = fakeExpected;
            if (correct) { fakeCompleted++; fakeExpected++; }

            bool finished = fakeExpected > totalCount;
            EventBus.Publish(GameEvent.SelectionResult, new SelectionResultEvent
            {
                correct = correct,
                target = testTile,
                selectedValue = correct ? expectedBefore : 99,
                expectedValue = expectedBefore,
                nextExpectedValue = finished ? 0 : fakeExpected,
                completedCount = fakeCompleted,
                totalCount = totalCount,
                errorType = correct ? ErrorType.None : ErrorType.Wrong,
            });
        }

        void PublishCompleted()
        {
            EventBus.Publish(GameEvent.TaskCompleted,
                new TaskRecord(TaskType.Schulte, 38.6f, fakeCompleted, totalCount - fakeCompleted));
        }
    }
}
