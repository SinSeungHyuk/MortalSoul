using UnityEngine;

namespace MS.Utils
{
    // Physics2D Overlap 범위 등을 호출 시점에 시각화하기 위한 디버깅 헬퍼.
    // Debug.DrawLine 기반이므로 Scene 뷰에서 항상 보이고, Game 뷰에서는 Gizmos 토글이 ON일 때 보인다.
    // 릴리즈 빌드에서는 본체가 비어 CPU 비용이 0에 수렴한다.
    public static class DebugDraw
    {
        #region DEFAULT
        private static readonly Color DefaultColor = new Color(1f, 0.25f, 0.25f, 1f);
        private const float DefaultDuration = 0.5f;
        private const int DefaultCircleSegments = 32;
        #endregion

        #region SHAPE
        public static void DrawCircle(Vector2 _center, float _radius, Color? _color = null, float _duration = DefaultDuration, int _segments = DefaultCircleSegments)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Color c = _color ?? DefaultColor;
            float step = (Mathf.PI * 2f) / _segments;
            Vector3 prev = new Vector3(_center.x + _radius, _center.y, 0f);
            for (int i = 1; i <= _segments; i++)
            {
                float a = step * i;
                Vector3 cur = new Vector3(_center.x + Mathf.Cos(a) * _radius, _center.y + Mathf.Sin(a) * _radius, 0f);
                Debug.DrawLine(prev, cur, c, _duration);
                prev = cur;
            }
#endif
        }

        public static void DrawBox(Vector2 _center, Vector2 _size, float _angle = 0f, Color? _color = null, float _duration = DefaultDuration)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Color c = _color ?? DefaultColor;
            Vector2 half = _size * 0.5f;
            float rad = _angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector3 p0 = RotateOffset(_center, -half.x, -half.y, cos, sin);
            Vector3 p1 = RotateOffset(_center,  half.x, -half.y, cos, sin);
            Vector3 p2 = RotateOffset(_center,  half.x,  half.y, cos, sin);
            Vector3 p3 = RotateOffset(_center, -half.x,  half.y, cos, sin);

            Debug.DrawLine(p0, p1, c, _duration);
            Debug.DrawLine(p1, p2, c, _duration);
            Debug.DrawLine(p2, p3, c, _duration);
            Debug.DrawLine(p3, p0, c, _duration);
#endif
        }

        public static void DrawCapsule(Vector2 _center, Vector2 _size, CapsuleDirection2D _direction, float _angle = 0f, Color? _color = null, float _duration = DefaultDuration, int _arcSegments = 12)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Color c = _color ?? DefaultColor;

            // 기준 축: Vertical이면 반원이 위/아래, Horizontal이면 좌/우
            float width = _size.x;
            float height = _size.y;
            float radius;
            Vector2 axisDir;   // 직선 구간 방향 (로컬)
            float straightLen;

            if (_direction == CapsuleDirection2D.Vertical)
            {
                radius = width * 0.5f;
                straightLen = Mathf.Max(0f, height - width);
                axisDir = new Vector2(0f, 1f);
            }
            else
            {
                radius = height * 0.5f;
                straightLen = Mathf.Max(0f, width - height);
                axisDir = new Vector2(1f, 0f);
            }

            float rad = _angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector2 axisWorld = new Vector2(axisDir.x * cos - axisDir.y * sin, axisDir.x * sin + axisDir.y * cos);
            Vector2 perpWorld = new Vector2(-axisWorld.y, axisWorld.x);

            Vector2 capA = _center + axisWorld * (straightLen * 0.5f);
            Vector2 capB = _center - axisWorld * (straightLen * 0.5f);

            // 직선 두 줄
            Vector3 sA1 = capA + perpWorld * radius;
            Vector3 sA2 = capB + perpWorld * radius;
            Vector3 sB1 = capA - perpWorld * radius;
            Vector3 sB2 = capB - perpWorld * radius;
            Debug.DrawLine(sA1, sA2, c, _duration);
            Debug.DrawLine(sB1, sB2, c, _duration);

            // 반원 두 개 (capA: perp+ -> axis+ -> perp-, capB: 반대)
            DrawHalfArc(capA, axisWorld, perpWorld, radius, c, _duration, _arcSegments, true);
            DrawHalfArc(capB, axisWorld, perpWorld, radius, c, _duration, _arcSegments, false);
#endif
        }

        public static void DrawCross(Vector2 _center, float _size = 0.15f, Color? _color = null, float _duration = DefaultDuration)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Color c = _color ?? DefaultColor;
            float h = _size * 0.5f;
            Debug.DrawLine(new Vector3(_center.x - h, _center.y, 0f), new Vector3(_center.x + h, _center.y, 0f), c, _duration);
            Debug.DrawLine(new Vector3(_center.x, _center.y - h, 0f), new Vector3(_center.x, _center.y + h, 0f), c, _duration);
#endif
        }
        #endregion

        #region OVERLAP WRAPPER
        public static Collider2D[] OverlapCircleAll(Vector2 _center, float _radius, int _layerMask, Color? _color = null, float _duration = DefaultDuration)
        {
            DrawCircle(_center, _radius, _color, _duration);
            return Physics2D.OverlapCircleAll(_center, _radius, _layerMask);
        }

        public static Collider2D[] OverlapBoxAll(Vector2 _center, Vector2 _size, float _angle, int _layerMask, Color? _color = null, float _duration = DefaultDuration)
        {
            DrawBox(_center, _size, _angle, _color, _duration);
            return Physics2D.OverlapBoxAll(_center, _size, _angle, _layerMask);
        }

        public static Collider2D[] OverlapCapsuleAll(Vector2 _center, Vector2 _size, CapsuleDirection2D _direction, float _angle, int _layerMask, Color? _color = null, float _duration = DefaultDuration)
        {
            DrawCapsule(_center, _size, _direction, _angle, _color, _duration);
            return Physics2D.OverlapCapsuleAll(_center, _size, _direction, _angle, _layerMask);
        }
        #endregion

        #region INTERNAL
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static Vector3 RotateOffset(Vector2 _center, float _x, float _y, float _cos, float _sin)
        {
            return new Vector3(_center.x + _x * _cos - _y * _sin, _center.y + _x * _sin + _y * _cos, 0f);
        }

        private static void DrawHalfArc(Vector2 _origin, Vector2 _axis, Vector2 _perp, float _radius, Color _color, float _duration, int _segments, bool _positiveSide)
        {
            // perp+ 에서 시작해 axis 방향을 거쳐 perp- 까지 180도 호.
            // _positiveSide == true  : axis 방향으로 볼록
            // _positiveSide == false : -axis 방향으로 볼록
            float dir = _positiveSide ? 1f : -1f;
            Vector3 prev = _origin + _perp * _radius;
            for (int i = 1; i <= _segments; i++)
            {
                float t = (float)i / _segments;
                float a = Mathf.PI * t;
                Vector2 p = _origin + (_perp * Mathf.Cos(a) + _axis * dir * Mathf.Sin(a)) * _radius;
                Debug.DrawLine(prev, (Vector3)p, _color, _duration);
                prev = p;
            }
        }
#endif
        #endregion
    }
}
