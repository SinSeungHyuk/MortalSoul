using MS.Manager;
using MS.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MS.UI
{
    public class Tooltip : BaseUI
    {
        private RectTransform tooltipContainer;
        private TextMeshProUGUI txtTooltip;
        private Button btnClose;

        private const float defaultPadding = 8f;

        public void InitTooltip(string _tooltipKey, Vector2 screenPosition, params object[] _args)
        {
            FindComponents();

            string _tooltipText = StringTable.Instance.Get("Tooltip", _tooltipKey, _args);
            txtTooltip.text = _tooltipText;

            // 화면 좌표 -> 캔버스 로컬 좌표로 변환 후 위치 설정
            Canvas parentCanvas = tooltipContainer.GetComponentInParent<Canvas>();
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
                out localPoint);

            tooltipContainer.anchoredPosition = localPoint;

            // 레이아웃 강제 갱신 후 화면 밖으로 나가지 않게 보정
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipContainer);
            ClampToParent(tooltipContainer, Vector2.one * defaultPadding);
        }

        private void FindComponents()
        {
            if (tooltipContainer != null) return;

            tooltipContainer = transform.FindChildComponentDeep<RectTransform>("TooltipContainer");
            txtTooltip = transform.FindChildComponentDeep<TextMeshProUGUI>("TxtTooltip");
            btnClose = transform.FindChildComponentDeep<Button>("BtnClose");
            btnClose.onClick.AddListener(Close);
        }

        // child의 anchoredPosition을 parent(RectTransform) 영역 내로 보정
        private void ClampToParent(RectTransform child, Vector2 padding)
        {
            if (child == null || child.parent == null) return;

            RectTransform parentRect = child.parent as RectTransform;
            if (parentRect == null) return;

            Vector2 childSize = child.rect.size;
            Vector2 anchoredPos = child.anchoredPosition;
            Vector2 pivot = child.pivot;

            // child 좌/우/하/상 계산 (parent의 로컬 좌표계 기준)
            float left = anchoredPos.x - childSize.x * pivot.x;
            float right = left + childSize.x;
            float bottom = anchoredPos.y - childSize.y * pivot.y;
            float top = bottom + childSize.y;

            float minX = parentRect.rect.xMin + padding.x;
            float maxX = parentRect.rect.xMax - padding.x;
            float minY = parentRect.rect.yMin + padding.y;
            float maxY = parentRect.rect.yMax - padding.y;

            if (left < minX)
                anchoredPos.x += (minX - left);
            if (right > maxX)
                anchoredPos.x -= (right - maxX);
            if (bottom < minY)
                anchoredPos.y += (minY - bottom);
            if (top > maxY)
                anchoredPos.y -= (top - maxY);

            child.anchoredPosition = anchoredPos;
        }
    }
}