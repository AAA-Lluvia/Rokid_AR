using UnityEngine;
using FocusWorld.Core;
using FocusWorld.Tasks;

namespace FocusWorld.Core
{
    /// <summary>
    /// Vertical Slice 的启动器：把 Bootstrap 场景里所有 Manager 串起来，并启动 Schulte。
    /// 二期换成 GameStateMachine + ModuleLoader（Additive 加载模块场景）。
    /// 挂在 Bootstrap 场景一个空物体上，Inspector 拖入场景中的 SchulteTask。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] TaskBase startTask;   // 拖入 SchulteTask
        [SerializeField] float startDelay = 0.5f;

        void Start()
        {
            EventBus.Subscribe(GameEvent.TaskCompleted, OnTaskCompleted);
            Invoke(nameof(BeginTask), startDelay);
        }

        void OnDestroy() => EventBus.Unsubscribe(GameEvent.TaskCompleted, OnTaskCompleted);

        void BeginTask()
        {
            if (startTask != null) startTask.Begin();
            else Debug.LogError("[GameBootstrap] startTask 未指定");
        }

        void OnTaskCompleted(object payload)
        {
            if (payload is TaskRecord rec)
                Debug.Log($"[GameBootstrap] 完成! 用时 {rec.duration:F1}s " +
                          $"正确 {rec.correctCount} 错误 {rec.mistakes} 正确率 {rec.accuracy:P0}");
            // TODO(二期): 切到结果 UI / 返回大厅
        }
    }
}
