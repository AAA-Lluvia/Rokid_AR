using UnityEngine;
using FocusWorld.Core;
using FocusWorld.Interaction;

namespace FocusWorld.Tasks
{
    /// <summary>
    /// 任务基类（Vertical Slice 简化版）。第一阶段刻意不加 DifficultyCurve / 自适应 / 多态配置。
    /// 后三个模块继承它，只需：覆写 GenerateContent + Evaluate，并暴露 4 个进度值。
    ///
    /// 生命周期：Begin() → 发 TaskStartedEvent → 订阅 Selection → 每次选中走 OnSelect →
    ///          Evaluate 判定 → 基类补全进度字段 → 发 SelectionResultEvent → 完成则 Finish()。
    ///
    /// 职责划分：子类只回答“对不对/错在哪”（EvalResult）与“进度到哪了”（4 个 getter）；
    /// 组装完整 SelectionResultEvent（含 expected/next/completed/total）由基类统一完成，
    /// 保证四个模块发给 UI 的数据结构一致。
    /// </summary>
    public abstract class TaskBase : MonoBehaviour
    {
        [SerializeField] protected TaskType taskType;

        protected int correctCount;
        protected int mistakes;
        protected float startTime;

        /// <summary>子类只需返回“对错 + 错误类型”，其余字段基类补全。</summary>
        protected struct EvalResult
        {
            public bool correct;
            public ErrorType errorType;
            public EvalResult(bool correct, ErrorType errorType)
            { this.correct = correct; this.errorType = errorType; }
        }

        // ===== 子类必须实现：两个差异点 + 四个进度值 =====
        protected abstract void GenerateContent();          // 生成内容（各模块不同）
        protected abstract EvalResult Evaluate(Selectable s); // 判定对错（各模块不同）
        protected abstract bool IsFinished();               // 是否完成

        protected abstract int TotalCount { get; }           // 总目标数（Schulte=25）
        protected abstract int FirstExpectedValue { get; }   // 第一个目标（Schulte=1）
        protected abstract int CurrentExpectedValue { get; } // 当前要找的目标（完成后为 0）
        protected abstract int CompletedCount { get; }       // 已正确完成数量

        /// <summary>外部（GameBootstrap/ModuleLoader）调用以启动任务。</summary>
        public void Begin()
        {
            correctCount = 0; mistakes = 0;
            startTime = Time.time;

            GenerateContent(); // 先生成，TotalCount/FirstExpectedValue 才有效

            EventBus.Subscribe(GameEvent.Selection, OnSelection);
            EventBus.Publish(GameEvent.TaskStarted, new TaskStartedEvent
            {
                taskType = taskType,
                totalCount = TotalCount,
                firstExpectedValue = FirstExpectedValue,
                startTime = startTime,
            });
        }

        void OnDisable() => EventBus.Unsubscribe(GameEvent.Selection, OnSelection);

        void OnSelection(object payload)
        {
            if (payload is SelectionEvent e && e.target != null)
                OnSelect(e.target, e.inputSource);
        }

        /// <summary>统一入口：所有输入最终都汇到这里。</summary>
        protected void OnSelect(Selectable s, InputSourceType inputSource = InputSourceType.Mouse)
        {
            int expectedBefore = CurrentExpectedValue;

            var eval = Evaluate(s); // 子类在此推进自己的目标（若正确）
            if (eval.correct) correctCount++; else mistakes++;

            bool finished = IsFinished();

            EventBus.Publish(GameEvent.SelectionResult, new SelectionResultEvent
            {
                correct = eval.correct,
                target = s,
                selectedValue = s.Value,
                expectedValue = expectedBefore,
                nextExpectedValue = finished ? 0 : CurrentExpectedValue,
                completedCount = CompletedCount,
                totalCount = TotalCount,
                errorType = eval.errorType,
                inputSource = inputSource,
            });

            if (finished) Finish();
        }

        protected virtual void Finish()
        {
            EventBus.Unsubscribe(GameEvent.Selection, OnSelection);
            var rec = new TaskRecord(taskType, Time.time - startTime, correctCount, mistakes);
            EventBus.Publish(GameEvent.TaskCompleted, rec);
        }
    }
}
