using UnityEngine;
using FocusWorld.Core;
using FocusWorld.Interaction;

namespace FocusWorld.Tasks
{
    /// <summary>
    /// 舒尔特方格 —— 整个系统的 Vertical Slice 母版。
    /// 只实现差异点：GenerateContent + Evaluate + 4 个进度 getter，其余全靠 TaskBase。
    /// 后三个模块就是复制这个类，改这几处 + 换 Prefab/Config。
    /// </summary>
    public class SchulteTask : TaskBase
    {
        [Header("配置（二期可迁到 ScriptableObject）")]
        [SerializeField] int gridSize = 5;                 // 5×5
        [SerializeField] float spacing = 0.12f;            // 相邻块间距（米）
        [SerializeField] Selectable tilePrefab;            // NumberTile 预制体（B 提供）
        [SerializeField] Transform gridContainer;          // 生成父节点（放在 TaskAnchor 下）

        int target;   // 当前要找的数字，从 1 开始
        int count;    // 总格子数

        // ===== 进度值（供基类组装 UI 事件）=====
        protected override int TotalCount => count;
        protected override int FirstExpectedValue => 1;
        protected override int CurrentExpectedValue => target <= count ? target : 0;
        protected override int CompletedCount => target - 1; // 已找到 target-1 个

        protected override void GenerateContent()
        {
            count = gridSize * gridSize;
            target = 1;

            // 1..count 洗牌
            var values = new int[count];
            for (int i = 0; i < count; i++) values[i] = i + 1;
            Shuffle(values);

            // 5×5 居中排布
            float half = (gridSize - 1) * spacing * 0.5f;
            for (int i = 0; i < count; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;
                var pos = new Vector3(col * spacing - half, half - row * spacing, 0f);

                var tile = Instantiate(tilePrefab, gridContainer);
                tile.transform.localPosition = pos;
                tile.SetValue(values[i]);
                SetTileLabel(tile, values[i]);
            }
        }

        protected override EvalResult Evaluate(Selectable s)
        {
            bool correct = s.Value == target;
            if (correct)
            {
                target++;
                // 表现层可做消失动画；这里也直接失活，避免重复点击
                s.gameObject.SetActive(false);
            }
            return new EvalResult(correct, correct ? ErrorType.None : ErrorType.Wrong);
        }

        protected override bool IsFinished() => target > count;

        // --- helpers ---
        void SetTileLabel(Selectable tile, int value)
        {
            var tmp = tile.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = value.ToString();
        }

        void Shuffle(int[] a)
        {
            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (a[i], a[j]) = (a[j], a[i]);
            }
        }
    }
}
