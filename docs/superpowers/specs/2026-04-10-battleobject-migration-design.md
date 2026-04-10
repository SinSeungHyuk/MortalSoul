# BattleObject 계층 이주 설계

## 목적

레퍼런스의 SkillObject 계층(SkillObject / ProjectileObject / AreaObject)을 현재 프로젝트에 맞게 이주한다. 3D → 2D 전환, 네이밍 변경, Main 패턴 적용을 수행한다.

## 결정사항

| 항목 | 결정 |
|------|------|
| 클래스명 | SkillObject → BattleObject |
| 네임스페이스 | `MS.Field` (폴더 위치 기반) |
| 파일 위치 | `Assets/02. Scripts/FieldObject/BattleObject/` |
| 콜백 시그니처 | `Action<BattleObject, BattleSystemComponent>` |
| 매니저 통합 | Main 패턴 (일반 클래스, Main에서 보유) |

## 클래스 계층

```
FieldObject (abstract, MonoBehaviour)  ← 기존
├─ FieldCharacter (abstract)           ← 기존
│  ├─ PlayerCharacter
│  └─ MonsterCharacter
└─ BattleObject (abstract)             ← 신규
   ├─ ProjectileObject                 ← 신규
   └─ AreaObject                       ← 신규
```

## BattleObject (레퍼런스: SkillObject)

파일: `Assets/02. Scripts/FieldObject/BattleObject/BattleObject.cs`

### 필드

| 필드 | 타입 | 설명 |
|------|------|------|
| onHitCallback | `Action<BattleObject, BattleSystemComponent>` | 히트 시 콜백 |
| owner | `FieldCharacter` | 스폰한 캐릭터 |
| targetLayer | `LayerMask` | 판정 대상 레이어 |
| hitCountPerAttack | `int` | 1회 판정 당 히트 횟수 (다단히트) |
| maxAttackCount | `int` | 최대 판정 횟수 (관통) |
| traceTarget | `FieldObject` | 추적 대상 |
| targetOffset | `Vector3` | 추적 오프셋 |
| battleObjectKey | `string` | 풀 반환용 키 |
| duration | `float` | 수명 |
| elapsedTime | `float` | 경과 시간 |

### 메서드

- `InitBattleObject(string _battleObjectKey, FieldCharacter _owner, LayerMask _targetLayer)` — 상태 초기화, 기본값 설정
- `virtual OnUpdate(float _deltaTime)` — 수명 체크, Death 전환
- `SetHitCountPerAttack(int)`, `SetMaxHitCount(int)`, `SetDuration(float)`, `SetHitCallback(Action)` — 세터
- `SetTraceTarget(FieldObject, Vector3)`, `ClearTraceTarget()` — 추적 대상 설정
- `protected IsValidTarget(Collider2D _other, out BattleSystemComponent _bsc)` — 레이어 체크 + `TryGetComponent<FieldCharacter>()` → `.BSC` 반환

### 레퍼런스 대비 변경

| 항목 | 레퍼런스 | 현재 |
|------|---------|------|
| 클래스명 | `SkillObject` | `BattleObject` |
| 프로퍼티 | `SkillObjectKey` | `BattleObjectKey` |
| IsValidTarget 파라미터 | `Collider` | `Collider2D` |
| IsValidTarget 반환 | `out SkillSystemComponent` | `out BattleSystemComponent` |
| 타겟 접근 | `TryGetComponent<SSC>()` | `TryGetComponent<FieldCharacter>()` → `.BSC` |
| 콜백 시그니처 | `Action<SkillObject, SSC>` | `Action<BattleObject, BSC>` |
| 네임스페이스 | `MS.Field` | `MS.Field` (동일) |

## ProjectileObject

파일: `Assets/02. Scripts/FieldObject/BattleObject/ProjectileObject.cs`

### 필드

- `Vector2 moveDir` — 이동 방향 (3D Vector3 → 2D Vector2)
- `float moveSpeed` — 이동 속도

### 메서드

- `InitProjectile(Vector2 _moveDir, float _moveSpeed)` — 방향/속도 초기화
- `SetMoveSpeed(float)`, `SetMoveDir(Vector2)` — 세터
- `OnTriggerEnter2D(Collider2D _other)` — 히트 판정 + maxAttackCount 감소 → 0 이하 시 Death
- `override OnUpdate(float _deltaTime)` — 추적 또는 직선 이동

### 레퍼런스 대비 변경

| 항목 | 레퍼런스 | 현재 |
|------|---------|------|
| 충돌 콜백 | `OnTriggerEnter(Collider)` | `OnTriggerEnter2D(Collider2D)` |
| 이동 방향 | `Vector3` | `Vector2` |
| 추적 이동 | `Vector3.MoveTowards` + `LookAt` | `Vector2.MoveTowards` + `Mathf.Atan2` 기반 2D 회전 |
| 직선 이동 | `transform.position += Vector3` | `transform.position += (Vector3)(moveDir * speed * dt)` |

## AreaObject

파일: `Assets/02. Scripts/FieldObject/BattleObject/AreaObject.cs`

### 필드

- `List<FieldCharacter> attackTargetList` — 영역 내 타겟 리스트 (레퍼런스의 `List<SSC>` → `List<FieldCharacter>`)
- `float attackInterval` — 틱 데미지 주기
- `float elapsedAttackTime` — 경과 공격 시간
- `float delayTime` — 초기 딜레이
- `float elapsedDelayTime` — 경과 딜레이 시간

### 메서드

- `InitArea()` — 타겟 리스트 초기화, 타이머 리셋. `AreaRangeMultiple` 스탯 참조 제거
- `SetDelay(float)`, `SetAttackInterval(float)` — 세터
- `OnTriggerEnter2D(Collider2D)` — 유효 타겟이면 리스트 추가 (FieldCharacter로 검증)
- `OnTriggerExit2D(Collider2D)` — 리스트에서 제거
- `override OnUpdate(float _deltaTime)` — 추적 → 딜레이 → 인터벌 → 역순 정리 → 콜백 발동

### OnUpdate 흐름

1. traceTarget 추적 (생존 시 위치 추적, 사망 시 ClearTraceTarget)
2. delayTime 대기 (경과 전이면 return)
3. attackInterval 주기 체크
4. maxAttackCount 차감 (0 이하 시 return)
5. attackTargetList 역순 순회: 죽은 타겟 제거, 생존 타겟에 hitCountPerAttack만큼 콜백 발동
6. elapsedAttackTime 리셋

### 레퍼런스 대비 변경

| 항목 | 레퍼런스 | 현재 |
|------|---------|------|
| 충돌 콜백 | `OnTriggerEnter/Exit(Collider)` | `OnTriggerEnter2D/Exit2D(Collider2D)` |
| 타겟 리스트 | `List<SkillSystemComponent>` | `List<FieldCharacter>` |
| 타겟 사망 체크 | `attackTarget.Owner.ObjectLifeState` | `attackTarget.ObjectLifeState` |
| AreaRangeMultiple | 스탯 기반 스케일 조절 | 제거 |
| 콜백 호출 | `onHitCallback(this, SSC)` | `onHitCallback(this, fieldChar.BSC)` |

## BattleObjectManager

파일: `Assets/02. Scripts/Core/Manager/BattleObjectManager.cs`
네임스페이스: `Core`

### 필드

- `List<BattleObject> battleObjectList`
- `List<BattleObject> releaseBattleObjectList`

### 메서드

- `SpawnBattleObject<T>(string _key, FieldCharacter _owner, LayerMask _targetLayer) : T where T : BattleObject`
  - `Main.Instance.ObjectPoolManager.Get(_key, _owner.transform).GetComponent<T>()`
  - `InitBattleObject` 호출 → 리스트 추가 → 반환
- `OnUpdate(float _deltaTime)` — 리스트 순회, OnUpdate 호출, Death 수거 → 풀 반환
- `ClearBattleObject()` — 리스트 전체 정리
- `async UniTask LoadAllBattleObjectAsync()` — 스켈레톤 (추후 프리팹별 풀 등록)

### Main 통합

- `Main`에 `public BattleObjectManager BattleObjectManager { get; private set; }` 추가
- `Main.Awake()`에서 `BattleObjectManager = new BattleObjectManager()` 생성
- 기존 Update 루프에서 `BattleObjectManager.OnUpdate(Time.deltaTime)` 호출

## 제외 사항

- IndicatorObject
- 개별 스킬 클래스 이주
- BattleObject 프리팹 생성
- WSC/SSC에서 BattleObject 스폰 연동
- TakeDamage 구현
