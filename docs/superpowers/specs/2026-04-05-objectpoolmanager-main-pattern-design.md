# ObjectPoolManager + Main 패턴 의존성 주입 설계

## Context

Main 패턴에서 매니저들이 일반 클래스로 전환됨에 따라, MonoBehaviour 없이 Unity 오브젝트(Transform 컨테이너 등)를 사용해야 하는 매니저에 대한 의존성 주입 패턴이 필요하다. ObjectPoolManager를 첫 번째 대상으로 구현하여 이 패턴을 확립한다.

## 의존성 주입 방식: 코드 동적 생성 (B 방식)

Main이 Awake()에서 코드로 자식 GameObject를 생성하여 매니저의 `Initialize()` 메서드로 주입한다.

- 씬 배치(A 방식) 대비 장점: 씬 설정 실수(이름 오타 등) 방지, 코드로 완결
- 이 패턴은 향후 다른 매니저(SoundManager 등)에도 동일하게 적용 예정

## 변경 대상 파일

### 1. ObjectPoolManager.cs (`Assets/02. Scripts/Core/Manager/`)

레퍼런스(`MortalSoulDoc/04. Assets/02. Scripts/Manager/CoreManager/ObjectPoolManager.cs`) 로직을 그대로 가져오되, 아래 4가지만 변경:

| 항목 | 레퍼런스 | 변경 후 |
|------|----------|---------|
| 기반 클래스 | `MonoSingleton<ObjectPoolManager>` | 일반 클래스 |
| 초기화 | `Awake()` | `Initialize(Transform poolParent)` |
| Instantiate/Destroy | `this.Instantiate()` / `this.Destroy()` | `Object.Instantiate()` / `Object.Destroy()` |
| 네임스페이스 | `MS.Manager` | `Core` |

API는 레퍼런스와 동일:
- `CreatePoolAsync(string key, int initialCount)` — Addressables로 프리팹 로드 + 초기 풀 생성
- `Get(string key, Vector3, Quaternion)` / `Get(string key, Transform)` — 풀에서 꺼내기
- `Return(string key, GameObject)` — 풀로 반환
- `ClearAllPools()` — 전체 풀 정리 + Addressables 핸들 해제

풀 확장 동작(풀 소진 시 동적 Instantiate)도 레퍼런스 그대로 유지.

### 2. Main.cs (`Assets/02. Scripts/Core/`)

ObjectPoolManager 초기화 부분만 수정:

```csharp
// Awake() 내부
Transform poolContainer = new GameObject("ObjectPool").transform;
poolContainer.SetParent(transform);

ObjectPoolManager = new ObjectPoolManager();
ObjectPoolManager.Initialize(poolContainer);
```

다른 매니저들은 현재 상태 유지 (`new Manager()`).

## 범위 외

- UIManager Canvas 주입 — 별도 논의 예정
- 다른 매니저 구현 — 이번 작업 범위 아님
- 풀 확장 개선(최대 사이즈 제한, 축소 등) — 프로토타입 단계에서 불필요

## 검증 방법

- Unity 에디터에서 Main 오브젝트 하위에 "ObjectPool" 자식이 생성되는지 확인
- 기존 테스트 코드가 ObjectPoolManager를 사용하는 부분이 있다면 정상 동작 확인
