using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Interaction
{
    /// <summary>
    /// 每帧向当前 IInputSource 要一条射线，做 Physics.Raycast，命中 Selectable 且触发时
    /// 发布 SelectionEvent（带输入来源）。整个项目唯一产生“选中”事件的地方。
    /// 切换输入设备 = 换掉 source 字段，别处不动。
    /// </summary>
    public class RayPointer : MonoBehaviour
    {
        [SerializeField] MonoBehaviour inputSourceBehaviour; // 拖入 MouseInputSource / PenInputSource
        [SerializeField] LayerMask selectableMask = ~0;
        [SerializeField] float maxDistance = 10f;

        IInputSource source;

        void Awake() => source = inputSourceBehaviour as IInputSource;

        /// <summary>运行时切换输入源（Mouse→Pen→Gesture）。</summary>
        public void SetSource(IInputSource s) => source = s;

        void Update()
        {
            if (source == null) return;
            if (!source.TryGetRay(out var ray, out var triggered)) return;
            if (!triggered) return;

            if (Physics.Raycast(ray, out var hit, maxDistance, selectableMask))
            {
                var sel = hit.collider.GetComponentInParent<Selectable>();
                if (sel != null)
                {
                    EventBus.Publish(GameEvent.Selection, new SelectionEvent
                    {
                        target = sel,
                        worldPos = hit.point,
                        time = Time.time,
                        inputSource = source.SourceType,
                    });
                }
            }
        }
    }
}
