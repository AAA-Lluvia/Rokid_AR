using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Interaction
{
    /// <summary>
    /// 输入抽象。把“用户想选中射线指向的物体”从“用笔/手/鼠标”里剥离出来。
    /// 上层（RayPointer）只调这个接口，因此设备没到时用鼠标也能跑通全部逻辑。
    /// SourceType 让 RayPointer 能在 SelectionEvent 里标注来源（供数据分析/调试）。
    /// </summary>
    public interface IInputSource
    {
        /// <summary>本帧的选择射线 + 是否触发了“选中”动作。</summary>
        bool TryGetRay(out Ray ray, out bool triggered);

        /// <summary>该输入源的类型（Mouse/Pen/Gesture）。</summary>
        InputSourceType SourceType { get; }
    }
}
