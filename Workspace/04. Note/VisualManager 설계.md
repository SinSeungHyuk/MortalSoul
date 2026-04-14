# VisualManager 설계

## 개요

카메라 제어 + 포스트프로세싱 + 복합 연출을 하나의 매니저에서 관리.
연출이 카메라와 PP를 동시에 사용하는 경우가 많으므로(피격 시 셰이크+비네트 등) 분리하지 않고 응집도를 유지한다.

## 네이밍 결정

- `CameraManager` — 카메라만 관리하는 것처럼 보여 부적절
- `RenderingManager` — 너무 넓음
- `DirectingManager` — 업계 비표준
- **`VisualManager`** — 시각 연출 전반을 표현, 히트스탑 등 확장에도 자연스러움 → 채택

## 레퍼런스 코드와의 차이점

| 항목 | 레퍼런스 CameraManager | 현 프로젝트 VisualManager |
|------|----------------------|-------------------------|
| 싱글톤 | `MonoSingleton<CameraManager>` | Main 패턴 (`Main.Instance.VisualManager`) |
| 직렬화 | `[SerializeField]` 카메라 참조 | Find 방식으로 캐싱 (SerializeField 자제 규칙) |
| 클래스 타입 | MonoBehaviour | 일반 클래스 (Update 불필요, 셰이크는 UniTask) |
| 범위 | 카메라만 | 카메라 + 포스트프로세싱 + 복합 연출 |

## 구조 설계

```
VisualManager (일반 클래스)
├─ Init — CinemachineCamera + Volume(PostProcess) Find로 캐싱
│
├─ 카메라 기능
│   ├─ SetFollowTarget()
│   ├─ ShakeCamera() / StopShake()
│   └─ ZoomFOV()
│
├─ 포스트프로세스 기능
│   ├─ PulseVignette()
│   ├─ FlashChromaticAberration()
│   └─ ...
│
└─ 복합 연출 (카메라 + PP 조합)
    ├─ PlayHitImpact()
    └─ PlayDeathEffect()
```

## Init 방식

- `FindAnyObjectByType<CinemachineCamera>()` → Cinemachine 카메라 + Noise 컴포넌트 캐싱
- `FindAnyObjectByType<Volume>()` → URP Volume + 개별 Override(Vignette 등) 캐싱
- Main.Awake()에서 `new VisualManager()` + `InitVisualManager()` 호출

## 카메라 액션 설계

### 필요한 액션 목록

1. **줌 인/아웃 (시간 기반)** — FOV(또는 Orthographic Size)를 시간에 따라 lerp
2. **줌 인/아웃 (즉시)** — FOV를 즉시 세팅. 1번 함수와 통합 가능(duration=0 분기)
3. **카메라 이동 (시간 기반)** — 특정 Position 또는 Transform을 타겟으로 현재 위치에서 lerp 이동. 기본 Follow(플레이어)를 일시 해제하는 처리 필요
4. **카메라 이동 (즉시)** — 3번의 즉시 버전
5. **카메라 쉐이크** — 레퍼런스 동일 (Cinemachine Noise 기반)
6. **카메라 회전** — 2D이므로 Z축 시계/반시계 회전만. 원하는 각도만큼 시간에 따라 진행
7. **디폴트 복귀 (옵션)** — 모든 연출 해제하고 기본 카메라 상태로 즉시 리셋

### 충돌 해제 규칙 — 카테고리별 채널

카메라 연출이 겹쳐 호출되는 상황은 기본적으로 '버그'지만 방어적으로 처리한다.

**방식: 카테고리별 독립 채널 (4채널)**

- `줌` / `이동` / `쉐이크` / `회전` 4개 독립 채널
- 같은 카테고리 내에서만 덮어쓰기 (기존 연출 즉시 취소 후 신규 실행)
- 서로 다른 카테고리는 동시 실행 가능 (예: 쉐이크 + 줌인 동시 연출)

이 규칙은 복합 연출(쉐이크+비네트 등)과 자연스럽게 맞물리며, 스컬/세피리아류 레퍼런스의 감각과도 일치.

### API 계층 — 2-Layer 구조

**저수준 API (원자 액션 7종)** — 채널별 덮어쓰기 규칙이 작동하는 레고 블럭

- `ZoomFOV(target, duration)` / `SetFOV(target)`
- `MoveTo(position|transform, duration)` / `SetPosition(position)`
- `Shake(intensity, duration)` / `StopShake()`
- `Rotate(angle, duration)`
- `ResetToDefault()` (옵션)

**고수준 연출 함수 (복합)** — 저수준 API를 조립한 재사용 연출

- `PlayHitImpact()` — 쉐이크 + 비네트
- `PlayDeathEffect()` — 즉시 줌인(SetFOV) + 천천히 회전(Rotate)
- 기타 보스 등장, 스킬 컷신 등 추후 확장

**호출 측 사용감**: `Main.Instance.VisualManager.PlayDeathEffect()` 한 줄 — 예전 전용 함수 방식과 동일. 내부만 조립식으로 재구성.

**이점**:
- 복합 연출은 "원자 액션 조립 래퍼"일 뿐이라 채널 규칙은 저수준에서만 처리하면 됨
- 새 연출 추가 시 저수준 조립만으로 완성
- 저수준도 단독 사용 가능 (간단한 쉐이크만 필요한 경우 등)

## 구현 계획

(추후 작성)
