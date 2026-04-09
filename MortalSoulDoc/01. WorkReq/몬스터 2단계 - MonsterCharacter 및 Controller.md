
작성일 2026-04-09

### 작업 요청

몬스터의 핵심 로직인 MonsterCharacter와 MonsterController를 구현한다.
Player와 동일한 패턴(Character + Controller 분리)을 따르며, 단일 FSM(Idle/Trace/Attack/Dead)으로 동작한다.

> 참조: `MortalSoulDoc/04. Assets/Monster 설계.md` (§1, §2, §3, §4, §5, §6, §7, §8, §9)
> 선행: 몬스터 1단계 완료 필수

---

### 1. MonsterCharacter 신규 작성

`Assets/02. Scripts/FieldObject/FieldCharacter/Monster/MonsterCharacter.cs`

**FieldCharacter 상속**. 책임: BSC/SSC 초기화, 스킬 리스트 보유, 사망 콜백.

- `InitMonster(string _monsterKey)` 진입점:
  - `MonsterSettingData` 조회 (`SettingData.MonsterSettingDict[_monsterKey]`)
  - `MonsterAttributeSet` 생성 + `InitAttributeSet` 호출
  - `BSC.InitBSC(this, monsterAttrSet)` (WSC 없음)
  - SSC에 스킬 등록: `SettingData.SkillList`의 각 `SkillKey`로 `BSC.SSC.GiveSkill(_skillKey)` 호출
  - `monsterKey` 필드 저장 (풀 반환용)
  - `ObjectType = FieldObjectType.Monster`
  - `ObjectLifeState = FieldObjectLifeState.Live`
  - MonsterController 초기 상태(Idle) 진입 트리거

- `MonsterController` 참조 (`Awake`에서 `GetComponent`)
- 스킬 리스트: `List<MonsterSkillSettingData>` (Rate 기반 선택에 사용)
- 사망 콜백: BSC의 `AttributeSet.OnDeadCallback` 구독 → MonsterController에 Dead 전환 알림

---

### 2. MonsterController 신규 작성

`Assets/02. Scripts/FieldObject/FieldCharacter/Monster/MonsterController.cs`

**MonoBehaviour**. `PlayerMovementController`와 동일 패턴. Rigidbody2D 기반 좌우 이동 + FSM.

#### 2-1. 물리/이동 기반

- `Rigidbody2D` + `BoxCollider2D` 참조 (`Awake`에서 `GetComponent`)
- 지면 판정: `PlayerMovementController`와 동일한 BoxCast 방식
- 중력/낙하: `PlayerMovementController`와 동일 (GravityScale, FallMultiplier)
- 점프 없음. 좌우 이동 + 중력만
- 방향 전환: `SpineController.SetFacing` 사용

#### 2-2. 상태머신 (MSStateMachine)

```
Idle ──탐지──> Trace <──> Attack
                           ↓
                         Dead (어디서든 진입)
```

**Idle** (탐지 전 패트롤):
- 일정 거리 한쪽으로 이동 → 잠깐 정지 → 반대 방향. 임시 상수값 사용
- 매 Update에서 감지 체크: `Mathf.Abs(player.x - self.x) < DetectionRange`이면 Trace 전이
- Y축 무시 (수평 거리만)
- 이동 애니메이션: `SpineController.PlayLoop(Settings.AnimRun)` / 정지 시 `PlayLoop(Settings.AnimIdle)`

**Trace** (영구 추적):
- 한 번 탐지하면 죽을 때까지 유지. Idle로 복귀 안 함
- 같은 층 판정: `Mathf.Abs(player.y - self.y)` < 임계값
- 같은 층: 플레이어 방향으로 직진 (MoveSpeed 적용)
- 다른 층: 플레이어 x좌표 부근에서 짧은 거리 좌우 안절부절 패트롤
- `AttackRange` 진입 체크: 수평 거리 < `AttackRange`이면 Attack 전이
- 이동 애니메이션: `PlayLoop(Settings.AnimRun)`

**Attack** (스킬 1회 실행):
- Rate 기반 랜덤 스킬 선택 (§8 로직)
- 쿨타임 체크: `SSC.IsCooltime`으로 필터. 모두 쿨타임이면 Trace 유지
- `await BSC.UseSkill(skillKey)` 실행 (내부에서 애니메이션/데미지 완결)
- 스킬 완료 후 Trace로 복귀

**Dead** (사망):
- `rb.linearVelocity = Vector2.zero`
- `BSC.SSC.CancelAllSkills()`
- `SpineController.PlayAnimation("Dead", false)`
- `await SpineController.WaitForCompleteAsync()`
- `MonsterManager.DespawnMonster` 호출 (풀 반환 + 활성 리스트 제거)

#### 2-3. Rate 기반 스킬 선택

`MonsterCharacter`의 스킬 리스트에서 가중치 랜덤 선택:
```
totalRate = sum(SkillActivateRate)
rand = Random.Range(0, totalRate)
누적 합산으로 해당 스킬 결정
```
쿨타임인 스킬은 후보에서 제외.

#### 2-4. 플레이어 참조

`Main.Instance.PlayerManager`를 통해 플레이어 위치 참조.

---

### 선행 조건

- 몬스터 1단계 완료 (DetectionRange 스탯, SkillSettingData 슬림화, BSC 가드)

### 후행 작업

- 몬스터 3단계: MonsterManager + 스폰 + 통합 테스트

### 비범위

- 몬스터 프리팹/Spine 에셋 제작 (에셋 구매 후 별도 진행)
- 넉백/스턴 처리 (추후 전투 시스템 고도화 시)
- B안 행동 분기 (카이팅, 비행형 등)
- 드롭 아이템 시스템

---
태그 : #Monster #MonsterCharacter #MonsterController #FSM #AI #Rigidbody2D #스킬 #패트롤
