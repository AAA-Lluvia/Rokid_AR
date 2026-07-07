using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Interaction
{
    /// <summary>
    /// Phase 2 占位实现：Rokid 控制笔（6DoF 位姿 + 扳机）。
    /// Day3 接入眼镜时，把 TODO 换成 Rokid SDK 的笔位姿/按键即可，
    /// 其余所有代码（RayPointer/Task/Feedback/UI）一行都不用改 —— 这就是 IInputSource 的价值。
    /// </summary>
    public class PenInputSource : MonoBehaviour, IInputSource
    {
        // TODO(Day3): 替换为 Rokid 控制笔 Transform 与扳机按键读取
        public Transform penTip;   // 笔尖（Inspector 拖入或由 Rokid SDK 提供）

        public InputSourceType SourceType => InputSourceType.Pen;

        public bool TryGetRay(out Ray ray, out bool triggered)
        {
            if (penTip == null) { ray = default; triggered = false; return false; }
            ray = new Ray(penTip.position, penTip.forward);

            // TODO(Day3): triggered = RokidPen.TriggerPressedThisFrame;
            triggered = false;
            return true;
        }
    }
}
