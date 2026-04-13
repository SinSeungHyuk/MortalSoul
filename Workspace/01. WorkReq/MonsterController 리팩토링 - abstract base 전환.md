
작성일 2026-04-13

### 작업 요청

MonsterController를 abstract base 클래스로 전환하고, 현재 로직을 GroundMonsterController 서브클래스로 분리한다.
추후 보스/원거리 등 다양한 몬스터 타입을 서브클래스 override로 확장할 수 있는 구조를 마련한다.

> 참조: `Workspace/04. Note/Monster 설계.md`

---

### 1. MonsterController를 abstract base로 전환

`Assets/02. Scripts/FieldObject/FieldCharacter/Monster/MonsterController.cs`

- 클래스를 `abstract`로 변경
- 공통 필드 유지: rb, spineController, monster, stateMachine, curVelocityX
- 공통 메서드 유지: OnUpdate, OnFixedUpdate, SetMonsterState, UpdateFallGravity, UpdateScaleX, IsSameLayerPlayer, IsPlayerDetect, IsInAttackRange, GetMoveSpeed
- Dead 상태는 base에 고정 (모든 몬스터 공통)
- 상태 메서드를 `protected virtual`로 선언: OnIdleEnter/Update, OnTraceEnter/Update, OnAttackEnter/Update
- InitController에서 상태머신 등록은 base에서 수행 (virtual 메서드를 등록)

---

### 2. GroundMonsterController 신규 작성

`Assets/02. Scripts/FieldObject/FieldCharacter/Monster/GroundMonsterController.cs`

- MonsterController 상속
- 현재 MonsterController의 Idle 패트롤 / Trace 추적 / Attack 로직을 그대로 override
- 패트롤 관련 필드(elapsedPatrolTime, patrolMoveTime 등)는 이 클래스로 이동

---

### 3. MonsterCharacter 수정

- `GetComponent<MonsterController>()`는 추상 타입이므로 서브클래스도 자동 인식 — 변경 불필요 확인

---

### 4. 기존 몬스터 프리팹 업데이트

- MonsterController 컴포넌트 → GroundMonsterController로 교체

---

### 선행 조건

없음

### 비범위

- RangedMonsterController, BossMonsterController 구현 (별도 작업)
- 추가 상태(EMonsterState) 확장

---
태그 : #Monster #MonsterController #리팩토링 #abstract #virtual
