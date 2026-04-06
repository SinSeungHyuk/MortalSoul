# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**Mortal Soul**은 Unity 6 (버전 6000.3.10f1)으로 개발 중인 **핵앤슬래시 2D 횡스크롤 로그라이크** 게임입니다. 플랫폼은 Android이며, Universal Render Pipeline(URP) 기반의 2D 렌더링을 사용합니다. 포트폴리오용 데모버전 수준의 분량을 목표로 합니다.

**레퍼런스 게임**: 스컬, 세피리아, 슬레이더스파이어, 메이플 스토리

### 게임 진행 흐름

1. **타이틀 화면** — 단순 UI. 첫 실행 시 Addressables로 리소스 다운로드, 화면 터치로 게임 시작
2. **마을 (인게임)** — 캐릭터/맵 스폰, 조이스틱 조작. 타이틀 로딩 후 곧바로 마을에서 시작
3. **던전 입장** — 마을에서 던전 입구와 상호작용하여 입장. 랜덤 던전 생성 및 진행
4. **마을 복귀** — 던전 클리어/실패 시 마을로 복귀

### 던전 시스템

- **랜덤 생성 구조** (슬레이더스파이어 방식): 루트가 존재하며, 입구에서 최종보스 방까지 진행
- **분기 선택**: 현재 구역에서 다음 구역으로 이동 시 팝업 UI로 선택지 제공
  ```
           [전투]
  [시작] ─<        >─ [이벤트] → [보스]
           [상점]
  ```
- 플레이어가 전략적으로 다음 구역을 선택

### 경험치 / 레벨업 시스템

- **경험치/레벨은 공용**: 소울이 바뀌어도 경험치와 레벨은 하나. 캐릭터(육체)는 한 명이고 영혼만 교체되는 컨셉
- **레벨업 보상 타이밍**: 전투 중 레벨업해도 즉시 보상 없음. **방 클리어 후 휴무 상태**에 진입하면 그때 레벨업 보상 선택지 팝업
- **다회 레벨업**: 방 안에서 여러 번 레벨업 시 보상 횟수를 누적 → 휴무 진입 시 연속으로 선택지 팝업
- **보상 방식**: 랜덤 스탯 선택지
- **레벨업 스탯 저장**: `Stat.baseValue`는 소울 고유 전용. 레벨업 보상은 `bonusStat`에 "levelup" 키(Flat)로 누적 합산값을 덮어써서 관리. 별도 `Dictionary<EStatType, float> levelUpGrowth`로 누적량 추적

### 소울 시스템

**핵심 컨셉**: 무기 교체가 아닌 **소울(Soul) 교체**가 핵심 메카닉. 스컬과 유사한 캐릭터 교체 방식. 소울을 교체하면 캐릭터의 외형(Spine 스킨), 무기, 스킬이 모두 변경된다.

**Spine 스킨 교체**: 플레이어 프리팹은 하나만 사용. 소울 교체 시 JSON에 정의된 부위별 스킨 키값을 읽어 Spine 런타임에서 스킨을 교체하여 적용.

**소울 등급**: Normal / Rare / Unique / Legendary

**소울 슬롯**:
- **메인 슬롯**: 현재 조작 중인 소울 1개. 기본공격 + 고유 스킬 2개 사용 가능
- **서브 슬롯**: 대기 중인 소울 1개. 패시브 효과로 추가 능력치 부여
- 게임 시작 시 기본 소울 1개로 시작. 던전 진행 중 새로운 소울 획득
- 2슬롯이 꽉 찼을 때 새 소울 획득 시 교체/버리기 선택 (되돌리기 없음)
- 소울은 던전 내 휘발성 — 던전 종료 시 초기화

**소울 스위칭**:
- 서브→메인으로 스위칭 시, **새로 메인이 되는 소울**의 고유 '스위칭 효과' 발동
- **공격/콤보 진행 중에는 소울 스위칭 불가** — WSC의 "공격 진행 중" 플래그로 차단

**소울별 스탯**: 각 소울은 고유한 BaseStat을 가짐. 소울 교체 시 baseValue만 새 소울 값으로 swap하고, bonusStat(레벨업/버프)은 유지.
- **체력 보존**: 소울별로 마지막 체력(lastHealth)을 저장. 교체 시 해당 소울의 마지막 체력 상태로 복원
- **스탯 계층**: baseValue(소울 고유, 교체됨) + bonusStat "levelup" 키(레벨업 누적, 유지) + 기타 bonusStat(버프, 유지)

**소울 구성 요소**: 각 소울은 다음을 포함
- 부위별 Spine 스킨 키값 (head, body, weapon 등)
- 무기 타입 (대검, 한손검, 단도, 활, 지팡이 중 1개 — 소울에 종속)
- **기본공격**: WSC(WeaponSystemComponent)가 직접 실행. 스킬이 아님. 무기 종류별로 WeaponSettingData를 참조하는 데이터 드리븐 방식. AttackSpeed에 비례해 애니메이션 재생 속도 + 공격 주기 빨라짐
- 고유 스킬 2개 (소울에 종속, SSC로 관리)
- 스위칭 효과 (소울 고유)
- 서브슬롯 패시브 효과 (소울 고유)
- 고유 BaseStat (소울별 고유 기본 스탯값)

**조작 키**: 기본공격, 대시, 점프, 스킬1, 스킬2, 소울 스위칭

**조작 규칙**:
- 조이스틱을 사용하여 좌,우 이동을 진행하며 이동시 해당 방향을 바라본다. 이동 애니메이션은 항상 'Run1'을 사용한다.
- 점프 사용 시 'Wait1' 애니메이션을 재생하며 점프를 수행하는 동안에는 다른 애니메이션을 재생할 수 없으며 공중에 있는 동안 좌,우 방향 전환 및 이동과 '대시'가 가능하다.
- 대시는 'Wait1' 애니메이션을 재생하며 빠른 속도로 현재 바라보고 있는 방향을 향해 이동한다. 대시를 수행하는 동안 다른 애니메이션 및 동작을 할 수 없다.
- **공격**은 버튼클릭을 통해 사용하며 공격을 수행하는 동안에는 다른 조작(이동,대시,점프,스킬)을 모두 할 수 없다. 공격 중 피격당해도 공격이 취소되지 않음(슈퍼아머). 공격 중 방향 전환 불가 — 콤보 종료 후에만 가능.
- 스킬 캐스팅은 이동,점프,대시,공격 등으로 즉시 취소 가능.

### 기본공격 콤보 시스템

**WeaponSettingData** 기반 데이터 드리븐. WSC가 대리인+실행자 역할을 겸함.

**Spine 이벤트 기반 타이밍 제어**:
- `hit` 이벤트: 무기가 휘둘러지는 타이밍 → 히트판정 실행
- `combo_ready` 이벤트: 조기 캔슬 허용 시점 → 예약 ON이면 회수 동작 스킵 후 즉시 다음 콤보로 전환
- `Complete` (Spine 기본): 예약 있으면 다음 콤보, 없으면 종료 및 콤보인덱스 리셋

**선입력(버퍼링)**: 공격 진행 중 아무 때나 공격 버튼을 누르면 예약 플래그 ON. 예약은 한 번만 ON (중복 입력 무시).

**WSC 내부 상태**: 현재 WeaponType, 콤보 인덱스, 다음 콤보 예약 플래그, 공격 진행 중 여부

### 애니메이션

**Spine 애니메이션** 사용. SpineComponent가 중간 레이어 역할 — Spine 내부 이벤트를 게임 이벤트로 변환 후 노출:
- `OnHitEvent` → WSC.HandleHit()
- `OnComboReadyEvent` → WSC.HandleComboReady()
- `OnActionCompleted` → WSC.HandleComplete()

## Work Flow
** 반드시 이 규칙을 따라 작업을 수행합니다. **
0. 주석은 최소한으로 사용한다.
1. MortalSoulDoc\01. WorkReq 폴더의 작업요청 md 문서를 참조할 경우 이 문서의 내용을 바탕으로 작업을 수행한다.
2. 작업을 수행하기 시작하면 사용자 승인 없이 즉시 MortalSoulDoc\02. InProgress 폴더로 해당 md 파일을 옮긴다.
3. 모든 작업이 완료되면 작업 내용을 요약하여 사용자에게 승인을 요청한다.
4. 승인을 받으면 MortalSoulDoc\03. Archive 폴더로 해당 md 파일을 옮기고 작업내용/특이사항/태그를 작성한다. 태그는 적절하게 작성한다.
5. 이후 깃허브에 커밋-푸시까지 진행하여 최종적으로 작업을 마무리한다.

## 코드 규칙
1. [SerializeField] 어트리뷰트 등 인스펙터 연결 사용은 최대한 자제한다.

## 코드 아키텍처

### 폴더 구조 규칙

`Assets/` 내부는 번호 접두사로 정렬됩니다:

- `01. Scenes/` — 게임 씬 파일
- `02. Scripts/` — C# 스크립트 (게임 로직의 핵심)
  - `Editor/` — Unity Editor 전용 스크립트
  - `Test/` — 테스트 스크립트
- `03. Resources/` — Prefab 및 ScriptableObject 에셋
- `04. Settings/` — 게임 설정 관련 파일 (JSON 데이터 포함)
- `Settings/` — URP 렌더링 파이핀 설정

### 핵심 시스템

**입력 시스템**: Unity Input System v1.18.0. `Assets/InputSystem_Actions.inputactions`에서 관리.

**렌더링**: Universal Render Pipeline(URP) v17.3.0 + 2D Renderer.

**Spine 애니메이션**: 캐릭터 애니메이션에 Spine 사용. Spine 이벤트 시스템 활용.

### 스크립트 작성 지침

- 입력 처리는 반드시 `InputSystem_Actions.inputactions`의 액션을 통해서만 받습니다(레거시 Input 사용 금지).
- **Main 패턴**: 매니저 접근은 `Main.Instance.XXXManager`를 통해 수행합니다. 개별 매니저에 싱글톤을 사용하지 않습니다.
- **BSC 구조**: 캐릭터의 전투 관련 로직은 `BattleSystemComponent`를 통해 처리합니다.

### 씬 구조

```
[Scene]
└─ Main (MonoSingleton)
   ├─ ObjectPool        ← 동적 생성
   ├─ ViewCanvas        ← 에디터 배치
   ├─ PopupCanvas       ← 에디터 배치
   └─ SystemCanvas      ← 에디터 배치
```

UIManager는 일반 클래스. `Main.Awake()`에서 `new` 후 `InitUIManager(transform)` 호출. `FindChildDeep`으로 Canvas를 찾아 참조 저장. `[SerializeField]` 미사용.

### 아키텍처 구조

**Main 패턴 (단일 모노싱글톤)**:
- `Main`이 모든 매니저를 보유하여 중앙집중 관리
- 각 매니저는 일반 클래스 (싱글톤 아님)

**BattleSystemComponent (BSC)**:
```
BattleSystemComponent (BSC) — 전투 시스템 통합 관리
├─ SkillSystemComponent (SSC)    — 항상 존재 (플레이어/몬스터 공통)
├─ WeaponSystemComponent (WSC)   — 플레이어만 존재 (nullable)
├─ StatusEffect 관리
└─ TakeDamage
```
- 플레이어 초기화: `BSC.InitBSC(owner, attrSet, weaponType)` → WSC 생성됨
- 몬스터 초기화: `BSC.InitBSC(owner, attrSet)` → WSC = null
- 소울 교체 시: `WSC.ChangeWeapon(newWeaponType)` 으로 무기 타입 + 콤보 리셋

### 핵심 스크립트

#### Core (`Core/`)

| 파일 | 설명 |
|------|------|
| `Stat.cs` | 스탯 시스템. baseValue + bonusStat(Flat/Percentage). `OnValueChanged` 이벤트. 계산: `base * (1 + %합/100) + flat합` |
| `MSReactProp<T>` | 반응형 프로퍼티. `onValueChanged(oldVal, newVal)`. UI 데이터 바인딩 |
| `MSStateMachine.cs` | 제네릭 상태머신. 상태 등록/전환/업데이트 루프 |
| `Main.cs` | 단일 모노싱글톤. DataManager, UIManager, SoundManager, ObjectPoolManager, PlayerManager, MonsterManager 보유 |

#### Data (`Data/`)

| 파일 | 설명 |
|------|------|
| `GlobalDefine.cs` | `EGrade`(Normal/Rare/Unique/Legendary), `EZoneType`(Battle/Shop/Event/Boss), `ESkillValueType` |
| `SettingData.cs` | JSON 비동기 로드. CharacterSettingData, MonsterSettingData, SkillSettingData, SoundSettingData, WeaponSettingData 관리 |
| `CharacterSettingData.cs` | 캐릭터 설정. Grade, AttributeSet(13개 스탯), SkinKeys, WeaponType, SkillKeys[], SwitchingEffectKey, SubPassiveKey |
| `WeaponSettingData.cs` | 무기별 기본공격 데이터. `EWeaponType`(GreatSword/OneHandSword/Dagger/Bow/Staff) 키. `AttackComboData`(AnimKey, DamageMultiplier, HitRange, HitOffset, Knockback) |
| `SkillSettingData.cs` | 스킬 설정. OwnerType, 속성, 쿨타임, SkillValueDict |
| `MonsterSettingData.cs` | 몬스터 설정. 6개 스탯 + 약점속성 + 스킬리스트 |

#### FieldObject (`FieldObject/`)

**계층 구조**: `FieldObject(abstract)` → `FieldCharacter(abstract)` → `PlayerCharacter` / `MonsterCharacter`

| 파일 | 설명 |
|------|------|
| `PlayerCharacter.cs` | 플레이어 최상위. BSC, PlayerController, PlayerSpineController 조합 |
| `PlayerController.cs` | Rigidbody2D 기반 이동. 상태머신(Idle/Move/Jump/Dash). 지면 판정(BoxCast), 중력, 에어컨트롤 |
| `PlayerSpineController.cs` | Spine 애니메이션 관리. Idle/Move/Jump/Dash 루프 재생, 방향 전환(ScaleX) |

#### Battle (`Battle/`)

| 파일 | 설명 |
|------|------|
| `BattleSystemComponent.cs` | 전투 통합 관리. SSC + WSC 보유, StatusEffect 관리, TakeDamage |
| `SkillSystemComponent.cs` | 스킬 관리 전담. 리플렉션으로 스킬 생성(`GiveSkill`), 쿨타임 체크, UniTask 비동기 실행, CancellationToken 취소 |
| `WeaponSystemComponent.cs` | 기본공격 전담(플레이어 전용). 콤보 인덱스/예약 플래그 관리. SpineComponent 이벤트 구독 |
| `BaseAttributeSet.cs` | 스탯 딕셔너리(`EStatType` → `Stat`). Health 관리, OnHealthChanged 이벤트 |
| `PlayerAttributeSet.cs` | 플레이어 전용 스탯(CriticChance, Evasion, LifeSteal 등) |
| `MonsterAttributeSet.cs` | 몬스터 전용 스탯(AttackRange) |
| `DamageInfo.cs` | 데미지 정보 구조체. Attacker/Target/AttributeType/Damage/IsCritic/KnockbackForce. 기본공격 속성은 Void(무속성) 고정 |
| `BaseSkill.cs` | 스킬 추상 기반. 쿨타임, `ActivateSkill(CancellationToken)` 비동기 |
| `StatusEffect.cs` | 상태이상. duration/elapsed + Start/Update/End 콜백 |

#### Utils (`Utils/`)

| 파일 | 설명 |
|------|------|
| `Settings.cs` | 전역 상수. 이동(MoveSpeed=5, JumpForce=12, DashSpeed=30, DashDuration=0.3), 애니메이션 키(Idle="Wait1", Run="Run1", Jump="Wait4", Dash="Run3"), 레이어마스크, 색상 팔레트 |

#### Test (`Test/`)

| 파일 | 설명 |
|------|------|
| `TestSpineComponent.cs` | 프로토타이핑용 Spine 컴포넌트. 이동/점프/대시/공격/스킬/피격/사망 애니메이션, 상태 관리, 완료 콜백 |

### 외부 의존성

- **Cysharp.Threading.Tasks (UniTask)** — 비동기/await
- **DOTween** — 애니메이션/트윈
- **Unity.AddressableAssets** — 리소스 동적 로드
- **Newtonsoft.Json** — JSON 직렬화
- **Unity Cinemachine v3** — 카메라
- **Unity Input System** — 입력

## 레퍼런스 스크립트 (이전 프로젝트)

> **위치**: `MortalSoulDoc/04. Assets/02. Scripts/`
> **용도**: 이전 프로젝트에서 사용했던 스크립트 모음. 초반 설계/구현 시 참고 자료로 활용. 더 이상 필요 없어지면 이 섹션과 해당 폴더를 삭제할 것.
