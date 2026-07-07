using System.Collections;
using UnityEngine;
using FocusWorld.Core;
using FocusWorld.Interaction;

namespace FocusWorld.Presentation
{
    /// <summary>
    /// 表现层：只订阅事件，绝不含任务逻辑（架构文档“单向依赖”的落地）。由 B 负责。
    /// 第一阶段用 Coroutine 做颜色 + 缩放占位，二期换 DOTween 发光/抖动 + 语音。
    /// SchulteTask 完全不知道它的存在，只通过 SelectionResultEvent 通信。
    /// </summary>
    public class FeedbackService : MonoBehaviour
    {
        [SerializeField] Color correctColor = Color.green;
        [SerializeField] Color wrongColor = Color.red;
        [SerializeField] float feedbackDuration = 0.25f;
        [SerializeField] float correctScale = 1.15f;

        void OnEnable() => EventBus.Subscribe(GameEvent.SelectionResult, OnResult);
        void OnDisable() => EventBus.Unsubscribe(GameEvent.SelectionResult, OnResult);

        void OnResult(object payload)
        {
            if (payload is not SelectionResultEvent e || e.target == null) return;

            Debug.Log(e.correct
                ? $"[Feedback] 正确! {e.selectedValue}"
                : $"[Feedback] 再找找哦 (点了 {e.selectedValue}, 目标 {e.expectedValue})");

            StartCoroutine(PlayFeedback(e.target, e.correct));
        }

        IEnumerator PlayFeedback(Selectable target, bool correct)
        {
            var rend = target.VisualRenderer;
            if (rend == null) yield break;

            Color original = rend.material.color;
            rend.material.color = correct ? correctColor : wrongColor;

            if (correct)
            {
                var scale = target.transform.localScale;
                target.transform.localScale = scale * correctScale;
                yield return new WaitForSeconds(feedbackDuration);
                target.transform.localScale = scale;
                // 正确块已由任务逻辑失活；这里无需再处理消失
                // TODO(B, 二期): DOTween 发光 + 上浮消失
            }
            else
            {
                yield return new WaitForSeconds(feedbackDuration);
                if (rend != null) rend.material.color = original;
                // TODO(B, 二期): Shake + 语音“再找找哦”
            }
        }
    }
}
