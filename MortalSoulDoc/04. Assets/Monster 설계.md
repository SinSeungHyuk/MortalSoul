# 몬스터 설계 메모

> 브레인스토밍 진행 중. 결정된 사항을 시간순/주제순으로 누적 정리. 모든 항목 확정 후 정식 스펙 문서로 옮길 예정.

---

## 0. 전제 (사용자 확정 사항)

- 몬스터는 Spine 애니메이션을 사용해 이동/공격/사망 등을 재생 (플레이어와 동일).
- 몬스터는 무기를 가지지 않으며 **모든 공격은 스킬로 구현/사용**한다.
- 스킬 사용은 `MonsterSettingData`에 미리 정의된 **확률(Rate) 기반 랜덤 선택**.
- 종류별로 각기 다른 프리팹을 가진다.

---

## 1. AI 다양성 범위 — A안 (단일 FSM)

**결정**: A안으로 시작. 추후 B안으로 확장 가능함을 명시해 둠.

- 모든 몬스터는 **단일 상태머신**(`Idle / Trace / Attack / Dead`)을 공유.
- 종류별 차이는 **스탯 / 스킬 / 외형(Spine 스킨)** 으로만 표현.
- 원거리 몬스터는 `AttackRange` 스탯을 크게 잡아 "멀리서 즉시 Attack 진입"으로 처리. 별도 AI 로직 불필요.

### B안 진화 여지 (미래)

원거리 몬스터가 너무 약해 보이거나(카이팅 없음), 점프형/비행형 등 행동 패턴 다양성이 필요해질 경우:

- `IMonsterBehaviour` 인터페이스 도입 → `MeleeBehaviour`, `RangedBehaviour`, `AerialBehaviour` 등으로 분기
- `MonsterCharacter`/`MonsterController`는 그대로 유지, 상태머신 콜백 알맹이만 behaviour로 위임
- `MonsterSettingData`에 `BehaviourType` enum 필드 추가해 데이터로 주입

> 핵심: A안 코드를 짤 때 "behaviour로 분리하기 쉬운 위치"에 로직을 두면 마이그레이션 비용이 작음.

---

## 2. 2D 이동/물리 모델 — A안 (Player와 동일 시스템)

**결정**: `Rigidbody2D` + 중력 + 지면 판정 (`PlayerController`와 동일 방식).

- 같은 물리계 → 충돌/넉백이 일관됨. 넉백은 `rb.linearVelocity` 한 줄로 처리 (레거시 DOTween + NavMeshAgent 흔들기 방식 폐기).
- 점프 가능 → 플레이어가 위층으로 올라가면 몬스터도 따라 점프해야 함.
- 절벽에서 그냥 걸어 나가면 중력으로 자동 낙하 (별도 로직 불필요).
- 데모 던전이 평지 위주면 "절벽 떨어짐 방지" 처리는 불필요.

---

## 3. 감지 / 추적 규칙

- **감지 범위**: `MonsterAttributeSet`에 `DetectionRange` 스탯 추가 (`AttackRange`와 동일 패턴).
- **감지 방식**: 수평 거리만 체크. `Mathf.Abs(player.x - self.x) < DetectionRange`. Y축 무시.
- **추적 영속성**: 한 번 탐지하면 **죽을 때까지 Trace 상태 유지**. Idle로 복귀하지 않음. 점프/낙하 모두 동원해 따라감.

> 스컬 게임 직접 플레이 후 좌우 박스 / 같은 높이 제한 등으로 변경 여지 있음. 코드 한 줄 수준의 변경이라 부담 없음.

---

## 4. Trace 상태 — 점프 판단 로직

**결정**: A안 (단순 휴리스틱).

매 프레임 체크:
- `player.Y - self.Y > 임계값` AND `self.isGrounded` → 점프 시도
- 좌우 이동은 항상 플레이어 방향
- 떨어지는 건 자동 (가장자리에서 걸어 나가면 중력으로 낙하)

장점: 코드 10줄 수준. 즉시 작동.
단점: 못 닿는 곳에서 헛점프 반복 가능 → 너무 멍청해 보이면 벽/절벽 Raycast 보강(B 옵션)으로 진화.

---

## 5. 상태머신 윤곽

```
Idle (탐지 전)  ──탐지──▶  Trace (영구, 점프/낙하 포함)  ◀──▶  Attack
                                                            ↓
                                                          Dead (어디서든 진입 가능)
```

- **Idle**: 탐지 전 대기. 가만히 `PlayIdle()`만 재생. (추후 패트롤로 진화 예정)
- **Trace**: 영구 추적. 좌우 이동 + 점프 판단 + Attack 사거리 진입 체크.
- **Attack**: 스킬 1회 실행. `await SSC.UseSkill(key)` 끝나면 Trace로 복귀.
- **Dead**: 사망 처리 → 풀 반환.

---

## 6. 클래스 분리 — Player와 동일 패턴

**결정**: `MonsterCharacter` + `MonsterController`로 분리.

- **`MonsterCharacter`** (`FieldCharacter` 상속)
  - BSC, AttributeSet 초기화
  - 스킬 리스트 보유 (`List<MonsterSkillSettingData>`)
  - 사망 콜백 / 넉백 / 스턴 처리
  - `InitMonster(monsterKey)` 진입점

- **`MonsterController`** (별도 컴포넌트, `PlayerController`와 동일 패턴)
  - `Rigidbody2D`, `BoxCollider2D` 보유
  - `MSStateMachine<MonsterController>` 상태머신
  - 점프 판단, 좌우 이동, 감지 로직, 사거리 체크, **Rate 기반 스킬 선택**
  - `MonsterCharacter` 참조 (BSC/SSC 호출용)

이유: Player와 구조 일관성. Trace 상태가 가장 복잡해질 곳인데(이동+점프+감지+사거리+스킬 트리거) 단일 클래스에 몰면 금세 비대해짐. 나중에 B안 진화 시 Controller만 교체.

---

## 7. 스킬과 애니메이션 책임 분담

**결정**: 메모(`메모.md` 2-1)에 적힌 방향대로 — **`BaseSkill` 구현체가 `SpineController` await 헬퍼를 직접 사용**.

### 7-1. 패턴

```csharp
// 근접 베기 예시
public override async UniTask ActivateSkill(CancellationToken _ct)
{
    owner.SpineController.PlayAnimation("BasicAttack_Swing_1", false);
    await owner.SpineController.WaitForAnimEventAsync(Settings.SpineEventAttack);
    DoHit();
    await owner.SpineController.WaitForCompleteAsync();
}

// 즉시 발동형(투사체) 예시 — 동일 인터페이스
public override async UniTask ActivateSkill(CancellationToken _ct)
{
    owner.SpineController.PlayAnimation("Cast", false);
    await owner.SpineController.WaitForAnimEventAsync("cast_release");
    SpawnProjectile();
    await owner.SpineController.WaitForCompleteAsync();
}
```

### 7-2. MonsterController.Attack 상태

```csharp
private async UniTaskVoid OnAttackEnter(...)
{
    var skillKey = PickSkillByRate();  // Rate 기반 랜덤
    await character.BSC.UseSkill(skillKey);  // 안에서 애니/이펙트/데미지 다 끝남
    stateMachine.TransitState((int)EState.Trace);
}
```

### 7-3. `MonsterSkillSettingData` 슬림화

레거시 → 본 프로젝트:
- ❌ `AnimTriggerKey` 제거 (스킬 안에서 처리)
- ❌ `SkillDuration` 제거 (`await`로 자연 종료)
- ✅ `SkillKey` 유지
- ✅ `SkillActivateRate` 유지

### 7-4. 복합 타이밍 자연스럽게 표현 가능

2단 히트, 캐스팅→발동, 콤보형 등 모두 await 체이닝으로 한 메서드 안에서 표현. WSC가 콤보를 굴리는 것과 동일한 모델.

---

## 8. Rate 기반 스킬 선택 위치

**결정**: `MonsterController`(또는 `MonsterCharacter`)가 직접 보유. SSC를 건드리지 않음.

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

쿨타임 체크는 `SSC.IsCooltime`으로 한 번 더 거름. 모두 쿨타임이면 Trace 유지.

---

## 9. 사망 처리

### 흐름

```
BSC.OnDeadCallback
  ↓
MonsterController가 Dead 상태로 전환
  ↓
Dead Enter:
  - rb.linearVelocity = Vector2.zero
  - SSC.CancelAllSkills()
  - SpineController.PlayAnimation("Dead", false)
  - await SpineController.WaitForCompleteAsync()
  - ObjectPoolManager.Instance.Return(monsterKey, gameObject)
```

### 규약 / 가드

- **Dead 애니 비루프 보장**: 모든 몬스터의 Dead 애니메이션은 비루프로 제작한다. 안전망 timeout 두지 않음. 누락 시 즉시 발견될 버그로 취급.
- **사망 중 추가 피격 차단**: `BSC.TakeDamage` 진입 시 `ObjectLifeState != Live`면 즉시 무시. Dead 상태에서는 데미지/넉백/스턴 모두 적용 안 됨.

### 범위 외 (이번 사이클 명시적 제외)

- **드롭 아이템 시스템** 전체. 기획 자체가 미확정이라 설계/구현 양쪽 모두 포함하지 않음. 추후 추가 시에도 사망 흐름에 1단계 끼워 넣는 수준이라 비용 작음.

---

## 10. 풀 / 스폰 / MonsterManager

### 10-1. 프리팹 키 규약

- **`monsterKey` = 풀 등록 키 = `MonsterSettingDict` 키**. 셋을 통일.
- 매핑 테이블 불필요. `MonsterSettingDict[key]` 조회와 `ObjectPoolManager.Spawn(key)` 호출이 같은 키 사용.

### 10-2. 풀 등록 시점

- **첫 던전 진입 시 일괄 등록**.
- 마을에서는 몬스터가 등장하지 않으므로 미리 로드할 이유 없음.
- 던전 입장 로딩 화면에서 모든 몬스터 프리팹을 Addressables로 한 번에 로드 → 풀에 등록.
- 던전 종료 시 풀 정리(unload) 여부는 추후 메모리 프로파일링 후 결정. 우선은 유지.

### 10-3. `MonsterManager` 책임 범위 — 얇게

```
MonsterManager
├─ Init / Shutdown
├─ SpawnMonster(monsterKey, position) → MonsterCharacter
├─ DespawnMonster(MonsterCharacter)              ← 풀 반환 + 활성 리스트 제거
├─ ClearAll()                                     ← 던전 종료 시 일괄 정리
└─ activeMonsters: List<MonsterCharacter>
```

- AI 전역 통제, 어그로 조정, 스폰 스케줄링은 **포함하지 않음**.
- 던전 시스템이 따로 생길 예정이며, 스폰 스케줄링은 그쪽 책임.

### 10-4. 활성 몬스터 추적

- `MonsterManager.activeMonsters: List<MonsterCharacter>` 보유.
- **등록 시점**: `SpawnMonster()` 호출 직후.
- **제거 시점**: 사망 → Dead 상태 종료 → `DespawnMonster()` 호출 시 (풀 반환과 동시).
- **용도**:
  - 방 클리어 판정("이 방의 모든 몬스터가 죽었는가?")
  - 던전 종료 시 일괄 풀 반환
  - 디버깅 / 인스펙터 표시

### 10-5. 스폰 흐름

```
DungeonSystem (또는 Test 코드)
  ↓ Main.Instance.MonsterManager.SpawnMonster("Slime", pos)
MonsterManager.SpawnMonster
  ↓ ObjectPoolManager.Spawn("Slime", pos) → GameObject
  ↓ go.GetComponent<MonsterCharacter>().InitMonster("Slime")
  ↓     ├─ MonsterSettingData 조회
  ↓     ├─ BSC.InitBSC(this, monsterAttrSet)
  ↓     ├─ SSC에 스킬 등록 (skillList)
  ↓     └─ MonsterController 초기 상태(Idle) 진입
  ↓ activeMonsters.Add(monster)
  ↓ return monster
```

---

## 11. 구현 작업 분할 (참고)

본 설계가 확정되면 실제 구현은 다음 단위로 분할 가능:

1. **데이터/스탯**: `MonsterAttributeSet`에 `DetectionRange` 추가, `MonsterSkillSettingData` 슬림화(`AnimTriggerKey`/`SkillDuration` 제거)
2. **`MonsterCharacter`**: BSC/SSC 초기화, 사망 콜백, 스킬 리스트 보유, `InitMonster`
3. **`MonsterController`**: Rigidbody2D + FSM(Idle/Trace/Attack/Dead) + 점프 휴리스틱 + Rate 기반 스킬 선택
4. **`MonsterManager`**: 얇은 스폰/풀/활성 추적
5. **테스트용 몬스터 1종**: 슬라임 등 가장 단순한 근접 몬스터로 통합 검증
6. **BSC 가드 보강**: `TakeDamage`에 `ObjectLifeState != Live` 차단
