using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Interaction
{
    /// <summary>
    /// Phase 1 输入实现：Editor 内用鼠标模拟。
    /// 射线 = 从主相机穿过鼠标屏幕位置；triggered = 左键按下。
    /// 这让 Day1 设备没到位时也能把整条 Pipeline 跑通。
    /// </summary>
    public class MouseInputSource : MonoBehaviour, IInputSource
    {
        Camera cam;

        void Awake() => cam = Camera.main;

        public InputSourceType SourceType => InputSourceType.Mouse;

        public bool TryGetRay(out Ray ray, out bool triggered)
        {
            if (cam == null) cam = Camera.main;
            ray = cam != null ? cam.ScreenPointToRay(Input.mousePosition) : default;
            triggered = Input.GetMouseButtonDown(0);
            return cam != null;
        }
    }
}
