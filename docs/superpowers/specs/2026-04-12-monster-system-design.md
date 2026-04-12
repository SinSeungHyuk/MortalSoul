# Monster System Design

## Overview

몬스터 시스템의 핵심 구현. 단일 FSM 기반 AI, Rigidbody2D 물리 모델, Rate 기반 스킬 선택, 오브젝트 풀 스폰/회수를 포함한다.

## Architecture

### Class Structure

```
FieldObject (abstract, MonoBehaviour)
└─ FieldCharacter (abstract)
   └─ MonsterCharacter : FieldCharacter
      ├─ BSC (BattleSystemComponent)
      ├─ SpineController (FieldCharacter에서 상속)
      ├─ MonsterController (별도 컴포넌트)
      │   ├─ Rigidbody2D
      │   ├─ BoxCollider2D
      │   └─ MSStateMachine<MonsterController>
      └─ skillList: List<MonsterSkillSettingData>
```

### MonsterCharacter

`FieldCharacter`를 상속. 전투 데이터 초기화와 생명주기를 담당한다.

**책임:**
- `InitMonster(monsterKey)` — MonsterSettingData 조회, AttributeSet 생성, BSC 초기화(WSC 없이), SSC에 스킬 등록, MonsterController 초기화
- 사망 콜백 수신 (`BSC.OnDead`) → MonsterController에 Dead 상태 전환 요청
- 넉백 처리 — `rb.linearVelocity = dir * force` 순간 속도 부여
- 스킬 리스트 보유 (`List<MonsterSkillSettingData>`)

**초기화 흐름:**
```
MonsterManager.SpawnMonster(monsterKey, pos)
  → ObjectPoolManager.Spawn(monsterKey, pos) → GameObject
  → go.GetComponent<MonsterCharacter>().InitMonster(monsterKey)
      ├─ MonsterSettingData 조회
      ├─ AttributeSet 생성 + Init(settingData.AttributeSetSettingData)
      ├─ BSC = new BattleSystemComponent()
      ├─ BSC.InitBSC(this, attributeSet)  // WSC 없음
      ├─ SSC에 스킬 등록 (foreach skillList → BSC.SSC.GiveSkill)
      ├─ BSC.OnDead += OnDeadCallback
      └─ MonsterController.InitController(this)
```

### MonsterController

별도 MonoBehaviour 컴포넌트. PlayerMovementController와 동일 패턴으로 AI 이동/상태머신을 담당한다.

**보유 필드:**
- `Rigidbody2D`, `BoxCollider2D`
- `MSStateMachine<MonsterController>` (Idle/Trace/Attack/Dead)
- `MonsterCharacter` 참조
- 패트롤/추적 관련 변수

**물리 모델 (PlayerMovementController와 동일):**
- `Rigidbody2D` + 중력 + 지면 판정 (BoxCast)
- 점프 불가 — 좌우 이동 + 중력만
- 넉백은 물리계와 일관 (`rb.linearVelocity` 직접 설정)
- 낙하 가속 적용 (`FallMultiple`)

## State Machine

```
Idle (탐지 전)  ──탐지──▶  Trace (영구)  ◀──▶  Attack
                                                 ↓
                                               Dead (어디서든 진입)
```

### Idle — 비전투 패트롤

- 좌우로 천천히 패트롤: 일정 거리 이동 → 잠깐 정지 → 반대 방향 이동 반복
- 패트롤 속도는 MoveSpeed의 일정 비율 (예: 50%)
- 매 Update에서 감지 체크 → 플레이어 감지 시 Trace로 전이
- 패트롤 거리/대기시간은 `Settings.cs` 상수

### Trace — 영구 추적

- 한번 탐지하면 죽을 때까지 Trace 유지. Idle로 복귀 안 함
- **같은 층**: 플레이어를 향해 MoveSpeed로 직진. 방향 전환(SpineController.SetScaleX)
- **다른 층** (`Mathf.Abs(player.y - self.y) > 임계값`): 안절부절 패트롤 — 플레이어 x좌표 부근에서 짧은 거리 좌우 왕복
- AttackRange 안에 들어오면 Attack으로 전이
- 사거리 체크: `Mathf.Abs(player.x - self.x) < AttackRange`

### Attack — 스킬 1회 실행

```csharp
OnAttackEnter:
  rb.linearVelocity = Vector2.zero
  플레이어 방향으로 SetScaleX
  skillKey = PickSkillByRate()  // Rate 기반 랜덤 선택
  if (모두 쿨타임) → Trace 복귀
  await BSC.UseSkill(skillKey)  // 스킬 안에서 애니/이펙트/데미지 모두 완료
  → Trace 복귀
```

**Rate 기반 스킬 선택:**
```csharp
int totalRate = skillList.Sum(x => x.SkillActivateRate);
int rand = Random.Range(0, totalRate);
int sum = 0;
foreach (var s in skillList)
{
    sum += s.SkillActivateRate;
    if (rand < sum) return s.SkillKey;
}
```
쿨타임 체크 `SSC.IsCooltime`으로 한번 더 거름. 모두 쿨타임이면 Trace 유지.

### Dead — 사망 처리

```
BSC.OnDead 발동
  → MonsterCharacter.OnDeadCallback
    → ObjectLifeState = Dying
    → MonsterController.Dead 상태 전환

Dead Enter:
  rb.linearVelocity = Vector2.zero
  BSC.SSC.CancelAllSkills()
  SpineController.PlayAnimation("Dead", false)
  await SpineController.WaitForCompleteAsync()
  ObjectLifeState = Death
  MonsterManager.DespawnMonster(this)
```

- Dead 애니메이션은 반드시 비루프. timeout 없음.
- 드롭 아이템 시스템은 이번 사이클에서 제외.

## Detection & Tracking

- **감지 범위**: `Settings.MonsterDetectionRange` 전역 상수. 모든 몬스터 동일.
- **감지 방식**: 수평 거리만 — `Mathf.Abs(player.x - self.x) < DetectionRange`
- **Y축 무시**: 같은 x에 있어도 다른 층이면 감지하지 못함
- **추적 영속성**: 한번 탐지하면 죽을 때까지 유지
- **다른 층 판정**: `Mathf.Abs(player.y - self.y) > Settings.MonsterLayerThresholdY`

## Knockback

- `rb.linearVelocity = knockbackDir * knockbackForce` 순간 속도 부여
- `ObjectLifeState != Live`면 무시
- 물리엔진이 자연스럽게 감속

## MonsterManager

얇은 레이어. 스폰/풀/활성 추적만 담당.

```
MonsterManager
├─ SpawnMonster(monsterKey, position) → MonsterCharacter
├─ DespawnMonster(MonsterCharacter)   // 풀 반환 + activeMonsters 제거
├─ ClearAll()                          // 던전 종료 시 일괄 정리
└─ activeMonsters: List<MonsterCharacter>
```

- AI 전역 통제, 어그로 조정, 스폰 스케줄링 미포함 (던전 시스템 책임)
- `monsterKey` = 풀 등록 키 = `MonsterSettingDict` 키 (통일)
- 풀 등록 시점: 첫 던전 진입 시 일괄 (마을에서는 미등록)

## Data

### MonsterSettingData (기존 — 변경 없음)

```csharp
public class MonsterSettingData
{
    public AttributeSetSettingData AttributeSetSettingData { get; set; }
    public string DropItemKey { get; set; }
    public List<MonsterSkillSettingData> SkillList { get; set; }
}

public class MonsterSkillSettingData
{
    public string SkillKey { get; set; }
    public int SkillActivateRate { get; set; }
}
```

`AnimTriggerKey`/`SkillDuration` 이미 제거됨. 스킬 내부에서 Spine await로 타이밍 제어.

### Settings.cs 추가 상수

```
MonsterDetectionRange     // 감지 범위 (수평 거리)
MonsterLayerThresholdY    // 다른 층 판정 Y 임계값
MonsterPatrolDistance      // Idle 패트롤 거리
MonsterPatrolWaitTime      // 패트롤 정지 대기 시간
MonsterPatrolSpeedRatio    // 패트롤 속도 비율 (MoveSpeed 대비)
MonsterFidgetDistance      // Trace 안절부절 패트롤 거리
```

## Scope Exclusions

- 드롭 아이템 시스템 (기획 미확정)
- 사망 중 추가 피격 차단 (추후 필요 시 추가)
- B안 행동 분기 (IMonsterBehaviour, 카이팅, 비행형)
- 보스 몬스터 강화 로직 (SetBossMonster)
- 풀 등록/해제 타이밍 (던전 시스템과 함께)
- 테스트용 몬스터 스킬 구현 (별도 작업)

## Implementation Units

1. **Settings 상수 추가** — MonsterDetectionRange, MonsterLayerThresholdY 등
2. **MonsterCharacter** — FieldCharacter 상속, BSC/SSC 초기화, InitMonster, 사망/넉백 콜백
3. **MonsterController** — Rigidbody2D + FSM(Idle/Trace/Attack/Dead), 패트롤, 감지, 추적, Rate 스킬 선택
4. **MonsterManager** — SpawnMonster/DespawnMonster/ClearAll/activeMonsters
5. **Main.cs 연결** — MonsterManager 등록
