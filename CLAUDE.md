# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**Mortal Soul**은 Unity 6 (버전 6000.3.10f1)으로 개발 중인 **핵앤슬래시 2D 횡스크롤 로그라이크** 게임입니다. 플랫폼은 Android이며, Universal Render Pipeline(URP) 기반의 2D 렌더링을 사용합니다. 포트폴리오용 데모버전 수준의 분량을 목표로 합니다.

**레퍼런스 게임**: 스컬, 세피리아, 슬레이더스파이어, 메이플 스토리

### 용어 / 네이밍 규칙

게임 내 UI 표기는 한글, 코드 네이밍은 영문으로 분리한다.

| 구분 | UI 표기 (한글) | 코드 네이밍 |
|------|--------------|------------|
| 캐릭터 교체용 (스컬식 캐릭터 교체 메카닉 단위) | **영혼** | `Soul` |
| 재화 (드랍/분해 자원) | **영혼의 정수** | `SoulEssence` |

**영혼의 정수(SoulEssence) 용도**:
- 몬스터 처치 시 드랍되는 재화
- 레벨업 보상 선택지 새로고침(리롤)
- 상점 구매 재화
- 영혼(Soul) 파괴/분해 시 변환되어 획득

> 이 문서 내 기존 "소울" 표현은 점진적으로 "영혼(UI) / Soul(코드)" 로 통일한다.

### 게임 진행 흐름

1. **타이틀 화면** — 단순 UI. 첫 실행 시 Addressables로 리소스 다운로드, 화면 터치로 게임 시작
2. **마을 (인게임)** — 캐릭터/맵 스폰, 조이스틱 조작. 타이틀 로딩 후 곧바로 마을에서 시작
3. **던전 입장** — 마을에서 던전 입구와 상호작용하여 입장. 랜덤 던전 생성 및 진행
4. **마을 복귀** — 던전 클리어/실패 시 마을로 복귀

**게임 상태 전환**: `GameManager`가 `MSStateMachine<GameManager>`로 Title → Village → Dungeon 상태를 관리

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
- **레벨업 보상 타이밍**: 전투 중 레벨업해도 즉시 보상 없음. 방의 모든 몬스터 처치 후 열리는 **다음 던전 입구 트리거**에 도달하는 시점에 누적된 레벨업 보상 선택지가 일괄 발동된다.
- **다회 레벨업**: 방 안에서 여러 번 레벨업 시 보상 횟수를 누적 → 입구 트리거 도달 시 연속으로 선택지 팝업
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

**소울 스위칭** (`PlayerSoulController`가 관리):
- 서브→메인으로 스위칭 시, **새로 메인이 되는 소울**의 고유 '스위칭 효과' 발동
- 사망/서브 슬롯 비어있음 외 **모든 상태에서 소울 스위칭 가능**. 스왑 시 진행 중인 공격/스킬은 캔슬되고 Idle 상태로 전환

**소울별 스탯**: 각 소울은 고유한 BaseStat을 가짐. 소울 교체 시 baseValue만 새 소울 값으로 swap하고, bonusStat(레벨업/버프)은 유지.
- **체력 보존**: 소울별로 마지막 체력을 `PlayerSoulController.curSubSoulHealth`로 저장. 교체 시 해당 소울의 마지막 체력 상태로 복원
- **스탯 계층**: baseValue(소울 고유, 교체됨) + bonusStat "levelup" 키(레벨업 누적, 유지) + 기타 bonusStat(버프, 유지)

### 스탯 체계 (12종)

단일 `AttributeSet` 클래스에서 12종 스탯을 모두 관리 (플레이어/몬스터 공용).

| # | 스탯 | 설명 | 기본값/단위 |
|---|------|------|-----------|
| 1 | `MaxHealth` | 최대 체력 | - |
| 2 | `BaseAttackPower` | 기본공격 피해량 (WSC) | - |
| 3 | `SkillAttackPower` | 스킬 공격 피해량 (SSC) | - |
| 4 | `Defense` | 방어력. `BattleScaling` 커브로 효율 감소 | - |
| 5 | `MoveSpeed` | 이동속도 % 가중치. `Settings.MoveSpeed` * (값/100) | 기본 100 (%) |
| 6 | `CriticChance` | 치명타 확률 (%) | 0 (%) |
| 7 | `CriticMultiple` | 치명타 피해 배율 (%) | 기본 150 (%) |
| 8 | `Evasion` | 회피율. `BattleScaling` 커브 | - |
| 9 | `LifeSteal` | 공격 시 체력 회복 확률 (%), 발동 시 +1 HP | 0 (%) |
| 10 | `CooltimeAccel` | 쿨타임 감소. `BattleScaling` 커브 | - |
| 11 | `AttackSpeed` | 공격속도 %. 값만큼 애니메이션 재생속도 + 공격/스킬 쿨타임 감소 | 기본 100 (%) |
| 12 | `AttackRange` | 몬스터 전용 공격 사거리 | - |

> `AttributeSet`에 `WeaknessAttributeType`(EDamageAttributeType) 속성도 포함.

**소울 구성 요소**: 각 소울은 다음을 포함
- 부위별 Spine 스킨 키값 (head, body, weapon 등)
- 무기 타입 (양손검, 한손검, 단도, 활, 지팡이 중 1개 — 소울에 종속)
- **기본공격**: WSC(WeaponSystemComponent)가 무기 타입별 `BaseWeaponAttack` 서브클래스 핸들러를 리플렉션으로 생성/보유하고 입력을 위임. 코드 드리븐(무기당 1 클래스, 수치/애니키 인라인). AttackSpeed에 비례해 애니메이션 재생속도 + 공격 주기 빨라짐
- 고유 스킬 2개 (소울에 종속, SSC로 관리)
- 스위칭 효과 (소울 고유)
- 서브슬롯 패시브 효과 (소울 고유)
- 고유 BaseStat (소울별 고유 기본 스탯값)

**조작 키**: 기본공격, 대시, 점프, 스킬1, 스킬2, 소울 스위칭

**조작 규칙**:
- 조이스틱을 사용하여 좌,우 이동을 진행하며 이동시 해당 방향을 바라본다. 이동 애니메이션은 항상 'Run1'을 사용한다.
- 점프 사용 시 'Wait4' 애니메이션을 재생하며 점프를 수행하는 동안에는 다른 애니메이션을 재생할 수 없으며 공중에 있는 동안 좌,우 방향 전환 및 이동과 '대시'가 가능하다.
- 대시는 'Run3' 애니메이션을 재생하며 빠른 속도로 현재 바라보고 있는 방향을 향해 이동한다. 대시를 수행하는 동안 다른 애니메이션 및 동작을 할 수 없다.
- **대시 종료 후 정지(Freeze)**: 대시가 끝나면 `Settings.DashEndFreezeDuration`(0.2초) 동안 속도가 0으로 고정된다. 지상에서는 그 자리 정지, 공중에서는 그 시간만큼 공중에 떠 있다가 이후 중력에 의해 낙하한다. (스컬 원본 동일)
- **공격**은 버튼클릭을 통해 사용하며 공격을 수행하는 동안 **이동(좌우 방향 전환 포함)은 불가**하다. 단, **대시 / 점프 / 스킬** 등 액션 동작으로는 공격 모션을 **즉시 캔슬**할 수 있다(스컬 원본 동일 — 공격 진입 직후부터 캔슬 가능). 공격 중 피격당해도 공격이 취소되지 않음(슈퍼아머). 방향 전환은 콤보 종료 후에만 가능.
- 스킬 캐스팅은 이동,점프,대시,공격 등으로 즉시 취소 가능.
- 소울 스위칭 시 현재 상태(공격/스킬/대시/점프 등)와 무관하게 즉시 교체되며, 교체 완료 후 Idle 상태로 전환된다.

### 기본공격 콤보 시스템

**코드 드리븐 구조**: WSC는 "공격 로직 슬롯" 역할만 하고, 실제 공격 로직은 무기 타입별 `BaseWeaponAttack` 서브클래스(`OneHandSwordAttack` 등)가 통째로 소유한다. 베이스에 콤보 루프/히트 판정 헬퍼를 두지 않고 각 무기가 자기 로직을 인라인으로 작성해 "한 파일 = 한 무기의 전부"라는 가독성을 우선한다. 스킬(`BaseSkill`) 상속 구조와 대칭.

**캔슬 메커니즘의 의도적 비대칭**: 스킬(SSC/BaseSkill)은 명시적 `CancellationToken`을 사용하지만, 기본공격(WSC/BaseWeaponAttack)은 `SpineController`의 TCS 자동 캔슬만 사용한다. 이유: 기본공격은 실행 흐름이 순수하게 Spine 이벤트에만 의존하고(비애니 `await` 없음) 모든 캔슬 경로(대시/점프/스킬/소울스위칭)가 새 애니메이션 재생을 동반하기 때문에 토큰이 불필요하다. 스킬은 `UniTask.Delay`, 외부 조건 대기, 침묵 디버프 등 비애니 캔슬 시나리오가 존재하여 토큰이 필요하다.

**Spine 이벤트 기반 타이밍 제어**:
- `attack` 이벤트: 무기가 휘둘러지는 타이밍 → 히트판정 실행
- `combo_ready` 이벤트: 조기 캔슬 허용 시점 → 예약 ON이면 회수 동작 스킵 후 즉시 다음 콤보로 전환
- `Complete` (Spine 기본): 예약 있으면 다음 콤보, 없으면 종료 및 콤보인덱스 리셋

**선입력(버퍼링)**: 공격 진행 중 아무 때나 공격 버튼을 누르면 예약 플래그 ON. 예약은 한 번만 ON (중복 입력 무시).

**공격속도 반영**: `SpineController.PlayAnimation`에 `_timeScale` 옵셔널 파라미터 — 공격 애니 재생 시 `AttackSpeed.Value / 100f`를 전달하면 트랙 TimeScale만 스케일되어 콤보 주기 전체가 비례 단축된다. 기본공격은 전 흐름이 Spine 이벤트에 gated 되어 있어 별도 쿨타임 계산 불필요.

**상태 소유**: 콤보 인덱스, 예약 플래그, `isAttacking` 등의 공격 상태는 각 `BaseWeaponAttack` 서브클래스가 소유(콤보 개념이 없는 Bow/Staff는 콤보 필드 미보유). WSC는 현재 `BaseWeaponAttack` 핸들러 인스턴스만 들고 있음.

### 애니메이션

**Spine 애니메이션** 사용. `SpineController`(플레이어/몬스터 공용)가 중간 레이어 역할 — Spine 내부 이벤트/Complete 콜백을 await 가능한 헬퍼로 노출:
- `WaitForAnimEventAsync(eventKey)` — 지정한 user data 이벤트가 발생할 때까지 await (`UniTaskCompletionSource` 기반)
- `WaitForAnimCompleteAsync()` — 비루프 애니메이션의 Complete 콜백까지 await
- `PlayAnimation` 호출 시 진행중인 wait TCS는 자동 캔슬됨 → 외부에서 다른 애니로 끊으면 await도 `OperationCanceledException`으로 종료
- Spine 이벤트 키: `Settings.AnimComboRdyEvent = "combo_ready"`
- WSC는 이 헬퍼들을 await 체이닝하여 콤보 흐름을 구현 (구독형 콜백 아님)

### 이펙트 / GameplayCue 시스템

**MSEffect**: `ParticleSystem` 기반 이펙트 래퍼. 오브젝트 풀에서 스폰, duration 기반 자동 회수, 대상 추적(traceTarget) 지원.

**GameplayCue**: `ScriptableObject` 기반 연출 번들. 이펙트(키/오프셋/회전/지속시간/부착 여부) + 사운드(키) + 카메라 셰이크를 하나의 에셋으로 묶어 `Play(FieldObject)` 또는 `Play(Vector3)`로 일괄 발동.

### 전투 오브젝트 시스템

**계층 구조**: `BattleObject(abstract)` → `ProjectileObject` / `AreaObject`

- **BattleObject**: 오브젝트 풀 기반. owner/targetLayer/hitCount/maxAttackCount/duration 관리. 대상 추적(traceTarget) 지원. `BattleObjectManager`가 생명주기 관리
- **ProjectileObject**: 방향+속도 기반 이동. `OnTriggerEnter2D`로 충돌 감지. 대상 추적 시 유도탄 가능
- **AreaObject**: 범위 지속 피해 존. `OnTriggerEnter2D/Exit2D`로 대상 추적, attackInterval 기반 주기적 데미지. delayTime으로 시작 딜레이 지원

## Work Flow
** 반드시 이 규칙을 따라 작업을 수행합니다. **
0. 주석은 최소한으로 사용한다.
1. Workspace\01. WorkReq 폴더의 작업요청 md 문서를 참조할 경우 이 문서의 내용을 바탕으로 작업을 수행한다.
2. 작업을 수행하기 시작하면 사용자 승인 없이 즉시 Workspace\02. InProgress 폴더로 해당 md 파일을 옮긴다.
3. 모든 작업이 완료되면 작업 내용을 요약하여 사용자에게 승인을 요청한다.
4. 승인을 받으면 Workspace\03. Archive 폴더로 해당 md 파일을 옮기고 작업내용/특이사항/태그를 작성한다. 태그는 적절하게 작성한다.
5. 이후 깃허브에 커밋-푸시까지 진행하여 최종적으로 작업을 마무리한다.

## 코드 규칙
1. [SerializeField] 어트리뷰트 등 인스펙터 연결 사용은 최대한 자제한다.
2. 입력 처리는 반드시 `InputSystem_Actions.inputactions`의 액션을 통해서만 받습니다(레거시 Input 사용 금지).
3. **Main 패턴**: 매니저 접근은 `Main.Instance.XXXManager`를 통해 수행합니다. 개별 매니저에 싱글톤을 사용하지 않습니다.
4. **BSC 구조**: 캐릭터의 전투 관련 로직은 `BattleSystemComponent`를 통해 처리합니다.
5. 이벤트는 반드시 event Action 타입으로 선언합니다.
6. 이벤트 구독 함수명은 On*Callback 규칙을 사용합니다.
7. UI 버튼 바인드 함수명은 OnBtn*Clicked 규칙을 사용합니다.
8. 함수(메서드/생성자) 매개변수명은 반드시 언더스코어(`_`) 접두사를 붙입니다. 예: `void SetFacing(bool _right)`. 람다 파라미터는 예외.
9. 주석은 최소한으로 작성하며 '/// <summary>' 패턴은 절대 사용하지 않는다.
10. Current* 변수명은 Cur*로 줄여서 작성
11. 클래스 내 멤버변수로 const 상수 선언은 최대한 자제하며 반드시 사용해야 할 경우 settings를 사용한다.

## 코드 아키텍처

### 폴더 구조 규칙

`Assets/` 내부는 번호 접두사로 정렬됩니다:

- `01. Scenes/` — 게임 씬 파일
- `02. Scripts/` — C# 스크립트 (게임 로직의 핵심)
  - `Core/` — Main 싱글톤, 매니저, 상태머신, 커스텀 UI 컴포넌트
  - `Battle/` — 전투 시스템 (BSC/SSC/WSC, 스탯, 스킬, 무기, 상태이상)
  - `FieldObject/` — 필드 오브젝트 계층 (캐릭터, 몬스터, 전투오브젝트)
  - `Data/` — 전역 enum, SettingData(JSON), GameData(런타임)
  - `Utils/` — Settings 상수, 수학/전투 유틸, Transform 확장
  - `Editor/` — Unity Editor 전용 스크립트
  - `Test/` — 테스트 스크립트
- `03. Resources/` — Prefab, ScriptableObject, Spine, Sound 에셋
- `04. Settings/` — 게임 설정 JSON 데이터 (SettingData/)
- `Settings/` — URP 렌더링 파이프라인 설정

### 핵심 시스템

**입력 시스템**: Unity Input System v1.18.0. `Assets/InputSystem_Actions.inputactions`에서 관리.

**렌더링**: Universal Render Pipeline(URP) v17.3.0 + 2D Renderer.

**Spine 애니메이션**: 캐릭터 애니메이션에 Spine 사용. Spine 이벤트 시스템 활용.

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
- `Main`이 모든 매니저를 보유하여 중앙집중 관리 (`Core.Main : MonoSingleton<Main>`)
- 각 매니저는 일반 클래스 (싱글톤 아님)
- 보유 매니저: `DataManager`, `AddressableManager`, `UIManager`, `SoundManager`, `ObjectPoolManager`, `EffectManager`, `MonsterManager`, `BattleObjectManager`, `GameManager`
- 플레이어 참조: `Main.Instance.Player` (PlayerCharacter 직접 참조)

**GameManager (게임 상태 관리)**:
- `MSStateMachine<GameManager>`로 `EGameState`(Title/Village/Dungeon) 상태 전환
- Title: 비동기 로딩 → Village 전환
- Village: 플레이어 초기화, EffectManager/BattleObjectManager 업데이트
- Dungeon: GameData 초기화 → 진행 → 종료 시 정리/해제

**BattleSystemComponent (BSC)**:
```
BattleSystemComponent (BSC) — 전투 시스템 통합 관리
├─ SkillSystemComponent (SSC)    — 항상 존재 (플레이어/몬스터 공통)
├─ WeaponSystemComponent (WSC)   — 플레이어만 존재 (nullable)
├─ StatusEffect 관리
├─ TakeDamage (회피→크리→약점→방어→체력→흡혈 파이프라인)
└─ 이벤트: OnHit, OnDodged, OnDead
```
- 플레이어 초기화: `BSC.InitBSC(owner, attrSet, weaponType)` → WSC 생성됨
- 몬스터 초기화: `BSC.InitBSC(owner, attrSet)` → WSC = null
- 소울 교체 시: `WSC.ChangeWeaponType(newWeaponType)` 으로 무기 타입 + 콤보 리셋

### 핵심 스크립트

#### Core (`Core/`)

| 파일 | 설명 |
|------|------|
| `Main.cs` | 단일 모노싱글톤(`MonoSingleton<Main>`). 전 매니저 보유 + `Player` 프로퍼티 |
| `Singleton.cs` | `MonoSingleton<T>` 베이스 |
| `MSReactProp.cs` | 반응형 프로퍼티. `onValueChanged(oldVal, newVal)`. UI 데이터 바인딩 |
| `MSEffect.cs` | ParticleSystem 기반 이펙트 래퍼. 풀 회수, 대상 추적, duration 관리 |
| `GameplayCue.cs` | ScriptableObject 연출 번들 (이펙트+사운드+카메라셰이크). `Play(FieldObject)` / `Play(Vector3)` |
| `StateMachine/MSStateMachine.cs` | 제네릭 상태머신. 상태 등록(Enter/Update/Exit) → 전환/업데이트 루프 |
| `StateMachine/MSState.cs` | 상태 정의 구조 |
| `UI/MSButton.cs` | 커스텀 버튼 컴포넌트 |
| `UI/MSImage.cs` | 커스텀 이미지 컴포넌트 |
| `UI/MSToggleButton.cs` | 토글 버튼 컴포넌트 |
| `UI/MSToggleGroup.cs` | 토글 그룹 컴포넌트 |
| `Manager/GameManager.cs` | 게임 상태머신(Title/Village/Dungeon). 상태별 초기화/업데이트/정리 |
| `Manager/DataManager.cs` | `SettingData`(JSON 정적 데이터) + `GameData`(런타임 게임 데이터) 보유. `InitGameData` / `ReleaseGameData` |
| `Manager/AddressableManager.cs` | Addressables 리소스 로드 래퍼. 캐시 기반 동기/비동기 로드 |
| `Manager/UIManager.cs` | View/Popup/System Canvas 관리 (일반 클래스) |
| `Manager/SoundManager.cs` | BGM/SFX 재생. AudioMixer 볼륨 제어, SFX 쿨다운(0.05초), 풀 기반 |
| `Manager/ObjectPoolManager.cs` | Addressable 기반 오브젝트 풀. `CreatePoolAsync`/`Get`/`Return`/`ClearAllPools` |
| `Manager/EffectManager.cs` | MSEffect 생명주기 관리. `PlayEffect`/`StopEffectsByKey`/`ClearEffect` |
| `Manager/MonsterManager.cs` | 몬스터 스폰/해제. `SpawnMonster`/`ReleaseMonster`/`ClearAll`. `ActiveMonsterList` 관리 |
| `Manager/BattleObjectManager.cs` | 전투 오브젝트 생명주기. `SpawnBattleObject<T>`/`ClearBattleObject`. 프레임별 업데이트 + 회수 |

#### Data (`Data/`)

**GlobalDefine.cs** — 전역 enum 정의

| enum | 값 |
|------|------|
| `EGrade` | Normal, Rare, Unique, Legendary |
| `EZoneType` | Battle, Shop, Event, Boss |
| `ESkillValueType` | Default, Damage, Knockback, Move, Buff, Duration, Casting |
| `EWeaponType` | TwoHandSword, OneHandSword, Dagger, Bow, Staff |
| `EGameState` | Title, Village, Dungeon |

**SettingData (`Data/SettingData/`)** — JSON에서 비동기 로드되는 정적 게임 데이터

| 파일 | 설명 |
|------|------|
| `SettingData.cs` | JSON 비동기 로드 진입점. CharacterSettingData, MonsterSettingDict, SkillSettingDict, SoundSettingDict 보유. `LoadAllSettingDataAsync()` |
| `CharacterSettingData.cs` | `GameCharacterSettingData`(LevelSettingData + CharacterSettingDataDict) → `CharacterSettingData`(Grade, AttributeSetSettingData, SkinKeys, WeaponType, SkillKeys, SwitchingEffectKey, SubPassiveKey) → `AttributeSetSettingData`(12종 스탯 + WeaknessAttributeType) |
| `SkillSettingData.cs` | IconKey, CategoryKeyList, AttributeType(EDamageAttributeType), Cooltime, IsPostUseCooltime, `SkillValueDict`(ESkillValueType→float) |
| `MonsterSettingData.cs` | AttributeSetSettingData, DropItemKey, `SkillList`(MonsterSkillSettingData: SkillKey + SkillActivateRate 가중치) |
| `SoundSettingData.cs` | MinVolume, MaxVolume, Loop |

**GameData (`Data/GameData/`)** — 던전 진입 시 생성되는 런타임 데이터 (`DataManager.InitGameData()`)

| 파일 | 설명 |
|------|------|
| `GameData.cs` | 컨테이너. `Soul`, `Level`, `Dungeon`, `Battle` 4개 서브 데이터 보유 |
| `SoulGameData.cs` | MainSoulKey, SubSoulKey, `SoulHealthDict`(소울별 체력 추적) |
| `LevelGameData.cs` | `CurrentLevel`/`CurrentExp` (MSReactProp), `PendingLevelUpCount`, `LevelUpGrowth`(EStatType→float) |
| `DungeonGameData.cs` | CurrentZoneIndex, CurrentZoneType(EZoneType), IsResting |
| `BattleGameData.cs` | KillCount, GoldEarned, `SkillDpsDict`(스킬별 DPS 추적) |

#### FieldObject (`FieldObject/`)

**계층 구조**:
- `FieldObject(abstract)` — FieldObjectType(Player/Monster/BattleObject/FieldItem), FieldObjectLifeState(Live/Dying/Death)
  - `FieldCharacter(abstract)` — BSC, SpineController 보유
    - `PlayerCharacter` — PMC(PlayerMovementController), PSC(PlayerSoulController) 보유
    - `MonsterCharacter` — MonsterController, SkillList 보유
  - `BattleObject(abstract)` — owner, targetLayer, hitCount, duration 관리
    - `ProjectileObject` — 방향+속도 이동, 트리거 충돌
    - `AreaObject` — 범위 지속 피해, 주기적 데미지

| 파일 | 설명 |
|------|------|
| `FieldObject.cs` | 필드 오브젝트 베이스. `FieldObjectType`/`FieldObjectLifeState` enum 정의 |
| `FieldCharacter/FieldCharacter.cs` | `BSC`, `SpineController` 보유. `Update()`에서 `BSC.OnUpdate(dt)` 호출 |
| `FieldCharacter/SpineController.cs` | Spine 애니메이션 관리(플레이어/몬스터 공용). 스킨 합성(`SetSkin`), 방향 전환(`SetScaleX`/`IsScaleXRight`), `WaitForAnimEventAsync`/`WaitForAnimCompleteAsync` 비동기 헬퍼. `PlayAnimation(animName, loop, timeScale)` |
| `FieldCharacter/Player/PlayerCharacter.cs` | PMC + PSC 보유. `InitPlayer(mainSoulKey)`, `GainSubSoul`, `SwapSoul` |
| `FieldCharacter/Player/PlayerMovementController.cs` | Rigidbody2D 기반 이동. `EPlayerState`(Idle/Move/Jump/Dash/Attack) 상태머신. 지면 판정(BoxCast), 대시 프리즈, 낙하 가속. Input System 콜백(`OnMove`/`OnJump`/`OnSprint`/`OnAttack`/`OnPrevious`) |
| `FieldCharacter/Player/PlayerSoulController.cs` | 소울 스위칭 관리. MainSoulKey/SubSoulKey, `CanSwap`, `GainSubSoul`, `SwapSoul`, `GetMainSoulSkill`. `OnSoulSwapped` 이벤트 |
| `FieldCharacter/Monster/MonsterCharacter.cs` | `InitMonster(monsterKey)`, BSC/스킬 초기화, 넉백 처리(`ApplyKnockback`), `OnDead` → MonsterManager 해제 |
| `FieldCharacter/Monster/MonsterController.cs` | 몬스터 AI. `EMonsterState`(Idle/Trace/Attack/Dead) 상태머신. 플레이어 감지(`IsPlayerDetect`), 같은 층 판정(`IsSameLayerPlayer`), 공격 범위 판정(`IsInAttackRange`), 순찰(Patrol) 로직, 가중치 기반 스킬 선택(`GetUseSkillKey`) |
| `BattleObject/BattleObject.cs` | 전투 오브젝트 베이스. owner/targetLayer/hitCount/maxAttackCount/duration, 대상 추적 |
| `BattleObject/ProjectileObject.cs` | 투사체. moveDir/moveSpeed, 트리거 충돌 시 hitCallback 호출 |
| `BattleObject/AreaObject.cs` | 지속 범위 피해. attackInterval 기반 주기적 공격, delayTime 시작 딜레이 |

#### Battle (`Battle/`)

| 파일 | 설명 |
|------|------|
| `Stat.cs` | 스탯 시스템(`MS.Battle`). `EStatType`(12종), `EBonusType`(Flat/Percentage). bonusStatDict 기반. `OnValueChanged` 이벤트. 계산: `(base + flat합) * (1 + %합/100)` |
| `DamageInfo.cs` | `EDamageAttributeType`([Flags] None/Fire/Ice/Electric/Wind/Saint/Dark) enum 정의 + `DamageInfo` 구조체(Attacker/Target/AttributeType/Damage/IsCritic/KnockbackForce/SourceSkill) |
| `BattleSystemComponent.cs` | 전투 통합 관리. SSC + WSC + AttributeSet 보유, StatusEffect dict 관리, TakeDamage(회피→크리→약점→방어→체력→흡혈), `OnHit`/`OnDodged`/`OnDead` 이벤트 |
| `SkillSystemComponent.cs` | 스킬 관리 전담. 리플렉션 생성(`GiveSkill` — `MS.Battle.{skillKey}` 타입), 쿨타임 체크, UniTask 비동기 실행, CancellationToken 취소(`CancelSkill`/`CancelAllSkills`), `runningSkillDict`로 중복 차단. `OnSkillAdded`/`OnSkillUsed` 이벤트 |
| `WeaponSystemComponent.cs` | 기본공격 전담(플레이어 전용). `ChangeWeaponType(weaponType)`으로 핸들러 교체(`MS.Battle.{WeaponType}Attack` 타입 탐색), `ActivateAttack()`으로 입력 포워딩. `OnAttackStarted`/`OnAttackEnded` 이벤트 프록시 |
| `WeaponAttack/BaseWeaponAttack.cs` | 무기 공격 로직 베이스 추상 클래스. `owner/attributeSet/spine/isAttacking` 필드 + `OnAttackStarted/OnAttackEnded` 이벤트 + `ActivateAttack` abstract + Invoke 프록시 |
| `WeaponAttack/OneHandSwordAttack.cs` | 한손검 기본공격 구현. 2콤보(`Attack_OneHand1`/`Attack_OneHand2`), `OverlapCircleAll` 히트 판정, 크리티컬 굴림 |
| `WeaponAttack/TwoHandSwordAttack.cs` | 양손검 기본공격 — 스텁 (미구현) |
| `WeaponAttack/{Dagger,Bow,Staff}Attack.cs` | 나머지 3종 무기 스텁 — 리플렉션 팩토리 인식용. 후속 작업에서 실제 구현 |
| `AttributeSet/AttributeSet.cs` | 단일 클래스. 12종 `Stat` 프로퍼티 + `WeaknessAttributeType` + Health(클램프+이벤트) + `InitAttributeSet`/`SwapBaseValues`/`GetStatByType` |
| `Skill/BaseSkill.cs` | 스킬 추상 기반. 쿨타임(`SetCooltime`/`ResetCooltime`, CooltimeAccel 반영), `ActivateSkillAsync(CancellationToken)`, `CanActivateSkill`, `SkillCastingAsync`, `IsPostUseCooltime` |
| `Skill/TestOneHandAttack.cs` | 테스트용 한손검 스킬 구현체 |
| `StatusEffect/StatusEffect.cs` | 상태이상. duration/elapsed + `OnStatusStartCallback`/`OnStatusUpdateCallback`/`OnStatusEndCallback` 이벤트 |

#### Utils (`Utils/`)

| 파일 | 설명 |
|------|------|
| `Settings.cs` | 전역 상수. 이동(MoveSpeed=5, AirControlMultiplier=0.8), 점프/중력(JumpForce=18, GravityScale=3, FallMultiple=2.5, MaxFallSpeed=-20), 대시(DashSpeed=20, DashDuration=0.2, DashCooldown=0.8, DashEndFreezeDuration=0.2), 지면판정(GroundCheckSize, GroundCheckDistance=0.1), 애니메이션 키(AnimIdle="Wait1", AnimRun="Run1", AnimJump="Wait4", AnimDash="Run3", SpineMainTrack=0, AnimComboRdyEvent="combo_ready"), 전투(BattleScalingConstant=100, BasicAttackKnockback=2, WeaknessAttributeMultiple=1.5, LifeStealValue=1), 몬스터(MonsterDetectionRange=8, MonsterLayerThresholdY=2, MonsterPatrolWaitTime=1), 레이어마스크(Monster/Player/Ground), 색상 팔레트 |
| `BattleUtils.cs` | 전투 계산 유틸. `CalcDefenseStat`, `CalcEvasionStat`, `CalcWeaknessAttribute`, `CalcCriticDamage` |
| `MathUtils.cs` | 수학 유틸. `BattleScaling`, `DecreaseByPercent`, `IsSuccess` 등 |
| `TransformExtensions.cs` | Transform 확장 (`FindChildDeep` 등) |
| `DebugDraw.cs` | 디버그 드로잉 유틸 |

#### Editor (`Editor/`)

| 파일 | 설명 |
|------|------|
| `SpineAnimationInspectorWindow.cs` | Spine 애니메이션 인스펙터 윈도우 |
| `AddressableEditor.cs` | Addressable 관련 에디터 도구 |
| `UI/MSButtonEditor.cs` | MSButton 커스텀 인스펙터 |
| `UI/MSToggleButtonEditor.cs` | MSToggleButton 커스텀 인스펙터 |

#### Test (`Test/`)

| 파일 | 설명 |
|------|------|
| `TestSpineComponent.cs` | 프로토타이핑용 Spine 컴포넌트 (구버전, 실 사용은 SpineController) |
| `TestMoveComponent.cs` | 이동 테스트 |
| `TestUIController.cs` | UI 테스트 |
| `Test.cs` | 공통 테스트 진입점 |

### 외부 의존성

- **Cysharp.Threading.Tasks (UniTask)** — 비동기/await
- **DOTween** — 애니메이션/트윈
- **Unity.AddressableAssets** — 리소스 동적 로드
- **Newtonsoft.Json** — JSON 직렬화
- **Unity Cinemachine v3** — 카메라
- **Unity Input System** — 입력

## 레퍼런스 스크립트 (이전 프로젝트)

> **위치**: `Workspace/04. Note/02. Scripts/`
> **용도**: 이전 프로젝트에서 사용했던 스크립트 모음. 초반 설계/구현 시 참고 자료로 활용. 더 이상 필요 없어지면 이 섹션과 해당 폴더를 삭제할 것.
