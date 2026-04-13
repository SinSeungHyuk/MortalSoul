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

## 구현 계획

(추후 작성)
