using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MS.Data;

namespace MS.Test
{
    public class DungeonMapTester : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject arrowLinePrefab;

        private const int ColumnCount = 8;
        private const int CurrentColumn = 1;
        private const float HorizontalMarginRatio = 0.1f;
        private const float VerticalMarginRatio = 0.15f;
        private const float PositionJitter = 30f;
        private const float ArrowThickness = 20.07f;

        private RectTransform panelRT;
        private RectTransform linesRoot;
        private RectTransform nodesRoot;
        private List<List<MapNode>> columns;

        private class MapNode
        {
            public int col;
            public int row;
            public EZoneType type;
            public Vector2 pos;
            public RectTransform buttonRT;
        }

        private void Start()
        {
            panelRT = GetComponent<RectTransform>();
            BuildContainers();
            GenerateNodes();
            SpawnButtons();
            SpawnCurrentColumnArrows();
        }

        private void BuildContainers()
        {
            linesRoot = CreateStretchChild("LinesRoot");
            nodesRoot = CreateStretchChild("NodesRoot");
        }

        private RectTransform CreateStretchChild(string _name)
        {
            GameObject go = new GameObject(_name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(panelRT, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private void GenerateNodes()
        {
            columns = new List<List<MapNode>>();
            for (int c = 0; c < ColumnCount; c++)
            {
                int count = (c == 0 || c == ColumnCount - 1) ? 1 : Random.Range(2, 4);
                List<MapNode> list = new List<MapNode>();
                for (int r = 0; r < count; r++)
                {
                    list.Add(new MapNode { col = c, row = r, type = PickZoneType(c) });
                }
                columns.Add(list);
            }

            Rect rect = panelRT.rect;
            float left = rect.xMin + rect.width * HorizontalMarginRatio;
            float right = rect.xMax - rect.width * HorizontalMarginRatio;
            float top = rect.yMax - rect.height * VerticalMarginRatio;
            float bottom = rect.yMin + rect.height * VerticalMarginRatio;

            for (int c = 0; c < ColumnCount; c++)
            {
                float t = ColumnCount == 1 ? 0.5f : (float)c / (ColumnCount - 1);
                float x = Mathf.Lerp(left, right, t);
                List<MapNode> list = columns[c];
                int n = list.Count;
                for (int r = 0; r < n; r++)
                {
                    float yT = n == 1 ? 0.5f : (float)r / (n - 1);
                    float y = Mathf.Lerp(bottom, top, yT);
                    float jitterX = Random.Range(-PositionJitter, PositionJitter);
                    float jitterY = n == 1 ? 0f : Random.Range(-PositionJitter, PositionJitter);
                    list[r].pos = new Vector2(x + jitterX, y + jitterY);
                }
            }
        }

        private EZoneType PickZoneType(int _col)
        {
            if (_col == 0) return EZoneType.Battle;
            if (_col == ColumnCount - 1) return EZoneType.Boss;
            float roll = Random.value;
            if (roll < 0.60f) return EZoneType.Battle;
            if (roll < 0.75f) return EZoneType.Shop;
            return EZoneType.Event;
        }

        private void SpawnButtons()
        {
            foreach (var col in columns)
            {
                foreach (var node in col)
                {
                    GameObject go = Instantiate(buttonPrefab, nodesRoot);
                    RectTransform rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = node.pos;
                    node.buttonRT = rt;

                    Button btn = go.GetComponent<Button>();
                    if (btn != null)
                    {
                        MapNode captured = node;
                        btn.onClick.AddListener(() => Debug.Log($"Clicked [{captured.col},{captured.row}] {captured.type}"));
                    }
                }
            }
        }

        private void SpawnCurrentColumnArrows()
        {
            var cur = columns[CurrentColumn];
            var next = columns[CurrentColumn + 1];

            foreach (var from in cur)
            {
                foreach (var to in next)
                {
                    SpawnArrow(from.pos, to.pos);
                }
            }
        }

        private void SpawnArrow(Vector2 _from, Vector2 _to)
        {
            GameObject go = Instantiate(arrowLinePrefab, linesRoot);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 dir = _to - _from;
            float len = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.anchoredPosition = _from + dir * 0.5f;
            rt.sizeDelta = new Vector2(len, ArrowThickness);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
