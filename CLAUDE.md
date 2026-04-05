# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**Mortal Soul**은 Unity 6 (버전 6000.3.10f1)으로 개발 중인 **핵앤슬래시 2D 횡스크롤 로그라이크** 게임입니다. 플랫폼은 Android이며, Universal Render Pipeline(URP) 기반의 2D 렌더링을 사용합니다. 포트폴리오용 데모버전 수준의 분량을 목표로 합니다.

**레퍼런스 게임**: 스컬, 세피리아, 슬레이더스파이어

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

### 캐릭터 및 무기 시스템

**무기 종류**: 대검, 한손검, 단도, 활, 지팡이(스태프) — 총 5종

**장비 슬롯**:
- **메인 슬롯**: 현재 장착 중인 무기 1개. 고유 스킬 2개 + 기본공격 제공
- **서브 슬롯**: 보조무기 1개. 패시브 효과로 추가 능력치 부여 (등급 비례)
- 던전 진행 중 획득하며, 2슬롯이 꽉 찼을 때 장착/교체/버리기 가능 (되돌리기 없음)

**무기 스위칭**:
- B→A 무기로 스위칭 시 A 무기의 고유 '스위칭 효과' 발동 (예: 단검 → n초간 공속 증가)
- 스위칭 효과는 등급에 비례
- 캐릭터를 뒤덮는 이펙트로 잠깐 가린 뒤 스킨 교체

**조작 키**: 기본공격(무기별 고유), 대시, 점프, 스킬1, 스킬2 (장착 무기의 고유 스킬)

**조작 규칙**:
- 조이스틱을 사용하여 좌,우 이동을 진행하며 이동시 해당 방향을 바라본다. 이동 애니메이션은 항상 'Run1'을 사용한다.
- 버튼 클릭을 통해 점프를 사용할 수 있다. 점프 사용 시 'Wait1' 애니메이션을 재생하며 점프를 수행하는 동안에는 다른 애니메이션을 재생할 수 없으며 공중에 있는 동안 좌,우 방향 전환 및 이동과 '대시'가 가능하다.
- 대시는 버튼클릭을 통해 사용하고 'Wait1' 애니메이션을 재생하며 빠른 속도로 현재 바라보고 있는 방향을 향해 이동한다. 대시를 수행하는 동안 다른 애니메이션 및 동작을 할 수 없다.
- 공격은 버튼클릭을 통해 사용하며 각 무기의 종류별 고유한 애니메이션 키값을 사용하게 된다. 공격을 수행하는 동안에는 다른 조작(이동,대시,점프,스킬)을 모두 할 수 없으며 공격 애니메이션이 모두 종료된 이후에 다른 동작을 수행할 수 있다.
- 스킬 캐스팅은 특정 스킬을 발동할 때 재생되는 애니메이션으로, 캐스팅은 시전 도중 다른 조작을 통해 취소할 수 있다.(이동,점프,대시,공격 등 모두 캐스팅을 즉시 취소)

### 애니메이션

- **Spine 애니메이션** 사용. Spine 애니메이션 이벤트를 적극 활용 (구현 시기에 상세 논의)

### 아키텍처 개편 사항

**BattleSystemComponent (BSC) 도입**:
- 기존 SSC(SkillSystemComponent)를 확장하여 BSC가 대체
- BSC 내부에 축소된 SSC가 존재하는 구조: `BSC { SSC(스킬 전담), TakeDamage, StatusEffect, ... }`
- SSC는 오로지 스킬 관리만 담당

**Main 패턴 (싱글톤 제거)**:
- 기존: 모든 매니저가 개별 싱글톤
- 개편: `Main`이라는 단일 모노싱글톤 클래스가 모든 매니저를 보유하여 중앙집중 관리
- 각 매니저는 일반 클래스로 구현 (싱글톤 제거)
- 기존 모노싱글톤을 사용하던 매니저에는 의존성 주입으로 필요 오브젝트 주입
- 이점: 진입점 통일, 초기화 순서 제어

## Work Flow
** 반드시 이 규칙을 따라 작업을 수행합니다. **
1. MortalSoulDoc\01. WorkReq 폴더의 작업요청 md 문서를 참조할 경우 이 문서의 내용을 바탕으로 작업을 수행한다.
2. 작업을 수행하기 시작하면 사용자 승인 없이 즉시 MortalSoulDoc\02. InProgress 폴더로 해당 md 파일을 옮긴다.
3. 모든 작업이 완료되면 작업 내용을 요약하여 사용자에게 승인을 요청한다.
4. 승인을 받으면 MortalSoulDoc\03. Archive 폴더로 해당 md 파일을 옮기고 작업내용/특이사항/태그를 작성한다. 태그는 적절하게 작성한다.
5. 이후 깃허브에 커밋-푸시까지 진행하여 최종적으로 작업을 마무리한다.

## 코드 아키텍처

### 폴더 구조 규칙

`Assets/` 내부는 번호 접두사로 정렬됩니다:

- `01. Scenes/` — 게임 씬 파일
- `02. Scripts/` — C# 스크립트 (게임 로직의 핵심)
  - `Editor/` — Unity Editor 전용 스크립트
  - `Test/` — 테스트 스크립트
- `03. Resources/` — Prefab 및 ScriptableObject 에셋
  - `Prefabs/` — 재사용 가능한 게임 오브젝트
  - `ScriptableObjects/` — 데이터 컨테이너(설정값, 게임 데이터)
- `04. Settings/` — 게임 설정 관련 파일
- `Settings/` — URP 렌더링 파이핀 설정 (Renderer2D.asset, UniversalRP.asset)

### 핵심 시스템

**입력 시스템**: Unity Input System v1.18.0 사용. 입력 정의는 `Assets/InputSystem_Actions.inputactions`에서 관리됩니다. 현재 정의된 액션 맵:
- Player: Move(Vector2), Look(Vector2), Attack(Button), Interact(Button+Hold), Crouch(Button)

**렌더링**: Universal Render Pipeline(URP) v17.3.0 + 2D Renderer. 모바일 최적화가 목표이므로 Draw Call 최소화와 Sprite Atlas 활용을 고려해야 합니다.

**2D 핵심 패키지**:
- `com.unity.2d.animation` v13.0.4 — 캐릭터 애니메이션
- `com.unity.2d.tilemap` v1.0.0 — 타일맵 기반 레벨
- `com.unity.2d.spriteshape` v13.0.0 — 유기적인 지형 표현

**Spine 애니메이션**: 캐릭터 애니메이션에 Spine 사용. Spine 이벤트 시스템 활용

**Visual Scripting**: `com.unity.visualscripting` v1.9.9 포함 (코드와 병행 사용 가능)

### 스크립트 작성 지침

- 새 스크립트는 기능에 따라 `Assets/02. Scripts/` 하위 폴더(예: `Player/`, `Enemy/`, `UI/`, `Manager/`)를 만들어 정리합니다.
- 데이터 중심 설계를 위해 설정값은 `ScriptableObject`로 분리하여 `03. Resources/ScriptableObjects/`에 배치합니다.
- 입력 처리는 반드시 `InputSystem_Actions.inputactions`의 액션을 통해서만 받습니다(레거시 Input 사용 금지).
- **Main 패턴**: 매니저 접근은 `Main.Instance.XXXManager`를 통해 수행합니다. 개별 매니저에 싱글톤을 사용하지 않습니다.
- **BSC 구조**: 캐릭터의 전투 관련 로직은 `BattleSystemComponent`를 통해 처리합니다. 스킬 관리는 BSC 내부의 `SkillSystemComponent`가 담당합니다.

## 레퍼런스 스크립트 (이전 프로젝트)

> **위치**: `MortalSoulDoc/04. Assets/02. Scripts/`
> **용도**: 이전 프로젝트에서 사용했던 스크립트 모음. 초반 설계/구현 시 참고 자료로 활용. 더 이상 필요 없어지면 이 섹션과 해당 폴더를 삭제할 것.

### 전체 아키텍처 요약

싱글톤 기반 매니저 패턴 + 상태머신 + MVVM UI + 오브젝트 풀링 + 데이터 드리븐 설계. UniTask(async/await), DOTween, Addressables, Newtonsoft.Json 사용.

### Core (`Core/`)

| 파일 | 설명 |
|------|------|
| `Singleton.cs` | `Singleton<T>` (일반 클래스), `MonoSingleton<T>` (MonoBehaviour) — 모든 매니저의 기반 |
| `Stat.cs` | 스탯 시스템. baseValue + bonusStat(Flat/Percentage) 딕셔너리. `OnValueChanged` 이벤트. 계산: `base * (1 + %합/100) + flat합` |
| `MSEffect.cs` | 파티클 이펙트 래퍼. 오브젝트 풀 연동, 대상 추적(traceTarget), 지속시간 관리 |
| `GameplayCue.cs` | ScriptableObject. 이펙트+사운드+카메라쉐이크를 하나로 묶어 Play(owner/pos)로 실행 |
| `MSReactProp<T>` | 옵저버 패턴 반응형 프로퍼티. `onValueChanged(oldVal, newVal)`. UI 데이터 바인딩에 사용 |
| `MSState.cs` | 상태머신 상태. Enter/Update/Exit 콜백을 Action 델리게이트로 주입 |
| `MSStateMachine.cs` | 제네릭 상태머신. 상태 등록/전환(지연 전환 방식)/업데이트 루프 |

### Data (`Data/`)

| 파일 | 설명 |
|------|------|
| `GlobalDefine.cs` | `EGrade` enum(Normal/Rare/Unique/Legendary), 등급별 색상/확률 딕셔너리 |
| `StageStatisticsData.cs` | 스테이지 결과 데이터(킬수, 골드, 스킬별 DPS 등) |
| `CharacterSettingData.cs` | 캐릭터 설정. `AttributeSetSettingData`에 13개 스탯(MaxHealth, AttackPower, Defense, Evasion, MoveSpeed, CriticChance, CriticMultiple, LifeSteal, CooltimeAccel, ProjectileCount, AreaRangeMultiple, KnockbackMultiple, CoinMultiple) |
| `MonsterSettingData.cs` | 몬스터 설정. 6개 스탯 + 약점속성 + 스킬리스트(발동확률, 애니메이션키, 지속시간) |
| `StageSettingData.cs` | 스테이지 웨이브 설정. 웨이브별 몬스터 스폰 정보, 보스키, 스폰간격, 아이템드롭확률 |
| `SkillSettingData.cs` | 스킬 설정. OwnerType, 속성, 쿨타임, `ESkillValueType`(Default/Damage/Knockback/Move/Buff/Duration/Casting) |
| `ItemSettingData.cs` | 아이템 설정. `EItemType`(Coin, RedCrystal, GreenCrystal, BlueCrystal, BossChest, Artifact) |
| `ArtifactSettingData.cs` | 아티팩트 설정. 트리거(OnAcquire/OnSkillUse/OnItemAcquire/OnHit) + 조건 + 액션 |
| `StatRewardSettingData.cs` | 스탯 보상 설정(스탯타입, 등급, 보상값) |
| `SoundSettingData.cs` | 사운드 설정(볼륨 범위, 루프) |

### Manager (`Manager/`)

| 파일 | 역할 |
|------|------|
| `GameManager.cs` | 게임 라이프사이클. 60FPS, 모드 전환(`ChangeMode`), 구글플레이+Firebase 로그인(`GameManager_SDK.cs`) |
| `DataManager.cs` | JSON에서 모든 설정 데이터 로드. Addressables + Newtonsoft.Json |
| `AddressableManager.cs` | 리소스 로드/캐싱 추상화 (Load/Release) |
| `StringTable.cs` | 로컬라이제이션. `Get(category, key, args)` |
| `UIManager.cs` | UI 프리팹 캐싱, View/Popup 관리(LIFO 스택), 데미지텍스트/회피텍스트 |
| `CameraManager.cs` | Cinemachine v3 카메라 + 화면 흔들림(ShakeCamera) |
| `SoundManager.cs` | BGM/SFX 분리, AudioMixer, PlayerPrefs 볼륨 저장 |
| `ObjectPoolManager.cs` | 범용 오브젝트 풀. `CreatePoolAsync` → `Get` → `Return` |
| `EffectManager.cs` | MSEffect 생명주기 관리, 풀링 |
| `GameplayCueManager.cs` | GameplayCue 에셋 로드 및 Play(key) |
| `PlayerManager.cs` | 플레이어 스폰/파괴 |
| `MonsterManager.cs` | 몬스터 스폰(풀 기반), `GetNearestMonster`/`GetNearestMonsters` 공간 쿼리 |
| `SkillObjectManager.cs` | 스킬 오브젝트(투사체/범위) 스폰, 지연 정리 패턴 |
| `FieldItemManager.cs` | 필드 아이템 관리 + **GPU Instancing** (Matrix4x4[1023] 배치 렌더링) |

### FieldObject (`FieldObject/`)

**계층 구조**: `FieldObject(abstract)` → `FieldCharacter(abstract)` → `PlayerCharacter` / `MonsterCharacter`

| 파일 | 설명 |
|------|------|
| `FieldObject.cs` | 최상위 추상 클래스. `FieldObjectType`(Player/Monster/SkillObject/FieldItem), `FieldObjectLifeState`(Live/Dying/Death) |
| `FieldMap.cs` | 맵 관리. 랜덤 스폰포인트(NavMesh), 층 활성화 애니메이션(DOTween) |
| `FieldCharacter.cs` | 캐릭터 기반. `SkillSystemComponent(SSC)` 보유, 넉백/스턴 추상 메서드 |
| `MonsterCharacter.cs` | 4상태 상태머신(Idle→Trace→Attack→Dead). NavMeshAgent 경로탐색, 확률적 스킬 선택, 보스 변환(250%HP/200%ATK) |
| `PlayerCharacter.cs` | PlayerController + PlayerLevelSystem + PlayerArtifact 조합. SSC로 스킬 관리 |
| `PlayerController.cs` | InputSystem → CharacterController 이동. NavMesh 유효성 검증 |
| `PlayerLevelSystem.cs` | 레벨/경험치/골드. `MSReactProp`으로 UI 자동 갱신 |
| `PlayerArtifact.cs` | 아티팩트 트리거 시스템. 이벤트 구독 방식(OnSkillUse, OnHit 등) |
| `MovementLockState.cs` | Animator StateMachineBehaviour. 특정 애니메이션 중 이동 잠금 |

**필드 아이템**: `FieldObject` → `FieldItem(abstract)` → `ResourceItem`(코인/크리스탈 버프) / `InteractionItem`(보스상자→스킬보상, 아티팩트)

**스킬 오브젝트**: `SkillObject(abstract)` → `ProjectileObject`(투사체, 직선/호밍) / `AreaObject`(범위, 간격 공격, 딜레이) / `IndicatorObject`(범위 표시기)

### Skill (`Skill/`)

| 파일 | 설명 |
|------|------|
| `DamageInfo.cs` | 데미지 정보 구조체. `EDamageAttributeType`(Fire/Ice/Electric/Wind/Saint/Dark, 비트 플래그) |
| `BaseSkill.cs` | 스킬 추상 기반. 쿨타임 관리, `ActivateSkill(CancellationToken)` 비동기, 레벨/DPS 추적 |
| `BaseAttributeSet.cs` | 스탯 딕셔너리(`EStatType` → `Stat`). 26개 스탯 타입 |
| `PlayerAttributeSet.cs` | 플레이어 전용 9개 추가 스탯(CriticChance, Evasion, LifeSteal 등) |
| `MonsterAttributeSet.cs` | 몬스터 전용 1개 추가 스탯(AttackRange) |
| `StatusEffect.cs` | 상태이상. duration/elapsed + Start/Update/End 콜백 |
| `StatusEffectUtils.cs` | 확장 메서드: `ApplyStatEffect`, `ApplyStunEffect`, `ApplyBurnEffect`(2%HP/1초), `ApplyCharmEffect`(-50%속도/방어), `ApplyFrostEffect` |

**SSC (SkillSystemComponent)**: 캐릭터의 스킬/상태이상/데미지 처리 핵심 컴포넌트
- `GiveSkill()`: 리플렉션으로 스킬 문자열 키 → 인스턴스 생성
- `TakeDamage()`: 회피 → 약점배율 → 방어력 감소 → 흡혈 → 넉백
- `UseSkill()`: 쿨타임 체크 → 비동기 실행 → CancellationToken으로 취소 가능

**플레이어 스킬 패턴** (20+개):
- **투사체형**: FireBall(화상), IceBall(빙결장판), BloodBall(흡혈), StunBall(기절), Charm(매혹)
- **범위형**: BigCrystal/FastCrystal(빙결), Blizzard(17히트), FOBS(버스트), Meteor(화상), Plexus(기절)
- **유틸**: Teleport(NavMesh 레이캐스트 이동)
- **버프형**: SlashGreen(+50%이속)
- **공통 로직**: MonsterManager에서 타겟 획득 → BattleUtils로 데미지 계산 → SkillObjectManager로 오브젝트 스폰

**몬스터 스킬**: BowAttackA(원거리), OnehandAttackA/SheildAttackA/TwoHandAttackA(근접, 딜레이/범위 차이)

### UI (`UI/`)

| 파일 | 설명 |
|------|------|
| `BaseUI.cs` | UI 추상 기반. Show()/Close() |
| `BasePopup.cs` | 팝업 기반. Close 시 UIManager 스택에서 제거 |
| `BattlePanelViewModel.cs` | **MVVM 데이터 레이어**. MSReactProp으로 KillCount/WaveCount/Timer/Gold/Level/Exp 바인딩 |
| `BattlePanel.cs` | 전투 UI. 스킬슬롯(최대6개), HP바, 경험치바, 보스HP바, 웨이브/킬/타이머 표시 |
| `MainPanel.cs` | 메인 메뉴. 서바이벌 모드 시작 버튼 |
| `TitlePanel.cs` | 타이틀. Addressables CCD 패치 체크/다운로드 |
| `SkillRewardPopup.cs` | 스킬 보상 선택 팝업 (timeScale=0) |
| `StatRewardPopup.cs` | 스탯 보상 선택 팝업 (등급별 색상) |
| `ArtifactPopup.cs` | 아티팩트 정보 팝업 |
| `StageEndPopup.cs` | 스테이지 결과 팝업 (클리어/실패, 스킬DPS 통계) |
| `DamageText.cs` | 플로팅 데미지 텍스트. 크리티컬 강조, 풀 반환 |
| `SkillSlot.cs` | 스킬 슬롯. 쿨타임 오버레이 |
| `Tooltip.cs` | 툴팁. 화면 경계 클램핑 |
| `Notification.cs` | 알림. 페이드 인/아웃 애니메이션(UniTask+DOTween) |
| `HPBar.cs` / `ExpBar.cs` | HP/경험치 바 |
| `PlayerStatInfo.cs` | 플레이어 스탯 상세 표시. EStatType별 행 생성 |

### Mode (`Mode/`)

| 파일 | 설명 |
|------|------|
| `GameModeBase.cs` | 모드 추상 기반. MSStateMachine 사용. StartMode → OnRegisterStates → OnUpdate → EndMode |
| `LobbyMode.cs` | 로비. MainPanel 표시 + BGM |
| `SurvivalMode.cs` | 서바이벌 본체. 상태(Load/BattleStart/LastWave), 킬/웨이브/타이머 반응형 프로퍼티, EndMode에서 전체 정리 |
| `SurvivalMode_Load.cs` | 로드 상태. Effect→SkillObject→FieldItem→Monster→Map→Player 순서 비동기 로드 |
| `SurvivalMode_BattleStart.cs` | 전투 상태. 가중치 랜덤 몬스터 스폰, 웨이브 타이머, 보스 스폰(EndWaveAsync), 다음 웨이브 전환(ActivateNextWaveAsync) |
| `SurvivalMode_LastWave.cs` | 최종 웨이브. 메테오 환경 위험(2-4초 간격, 2-4개, 50데미지, 인디케이터 경고) |

### Utils (`Utils/`)

| 파일 | 설명 |
|------|------|
| `Settings.cs` | 전역 상수. 전투(MaxWaveCount=5, BattleScalingConstant=100, WeaknessMultiple=1.3), 레이어, 애니메이터 해시, 색상 팔레트 |
| `MathUtils.cs` | 수학 유틸. 화면 밖 체크, 확률 판정, 퍼센트 증감, `BattleScaling(v/(v+100)*100)`, 등급 랜덤 |
| `BattleUtils.cs` | 전투 계산. 방어력/회피/약점속성/스킬데미지/크리티컬/랜덤위치(NavMesh) |
| `TransformExtensions.cs` | Transform 확장. `FindChildDeep`, `FindChildComponentDeep<T>`, `GetOrAddComponent<T>` |
| `DataUtils.cs` | 스킬 설명 포맷팅. StringTable에서 가져와 `{key}` 치환 + 금색 컬러 |
| `ArtifactUtil.cs` | 아티팩트 조건/액션 전략 패턴. 델리게이트 딕셔너리로 확장 가능 |
| `CanvasBillboard.cs` | Canvas를 항상 카메라 방향으로 회전 |

### 핵심 디자인 패턴 정리

1. **싱글톤**: 모든 매니저 (`Singleton<T>`, `MonoSingleton<T>`)
2. **상태머신**: `MSStateMachine` — 몬스터 AI, 게임 모드
3. **MVVM**: `BattlePanelViewModel` + `MSReactProp` → UI 자동 갱신
4. **오브젝트 풀링**: `ObjectPoolManager` — 몬스터, 이펙트, 투사체, 아이템 전부 풀링
5. **데이터 드리븐**: JSON 설정 → `DataManager` → 각 시스템
6. **전략 패턴**: `ArtifactUtil` 델리게이트 딕셔너리
7. **옵저버 패턴**: `MSReactProp`, `Stat.OnValueChanged`, SSC 이벤트
8. **템플릿 메서드**: `BaseSkill.ActivateSkill()` 추상 → 각 스킬 구현
9. **리플렉션 팩토리**: `SSC.GiveSkill()` — 문자열 키로 스킬 인스턴스 생성
10. **GPU Instancing**: `FieldItemManager` — Matrix4x4[1023] 배치 렌더링

### 외부 의존성

- **Cysharp.Threading.Tasks (UniTask)** — 비동기/await
- **DOTween** — 애니메이션/트윈
- **Unity.AddressableAssets** — 리소스 동적 로드
- **Newtonsoft.Json** — JSON 직렬화
- **Firebase Auth + Google Play Games SDK** — 인증
- **Unity Cinemachine v3** — 카메라
- **Unity Input System** — 입력
