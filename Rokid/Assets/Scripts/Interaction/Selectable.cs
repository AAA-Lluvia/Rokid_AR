using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Interaction
{
    /// <summary>
    /// 挂在所有可被选中的物体上（NumberTile 的根节点）。需要一个 Collider 同在。
    /// 冻结契约的一部分：任务逻辑只认 Selectable + Value，不认它是数字块还是字母球。
    /// 对外只暴露属性（Value / VisualRenderer），B 的表现层读属性而非私有字段。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Selectable : MonoBehaviour
    {
        [SerializeField] int value;
        [SerializeField] Renderer visualRenderer; // 供表现层做发光/变色/缩放

        public int Value => value;
        public Renderer VisualRenderer => visualRenderer;

        /// <summary>由任务逻辑在生成内容时写入（如 Schulte 的数字）。</summary>
        public void SetValue(int newValue) => value = newValue;

        void Reset() => visualRenderer = GetComponentInChildren<Renderer>();
    }
}
