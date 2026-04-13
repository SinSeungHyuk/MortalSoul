# Monster 설계 노트

## MonsterController 리팩토링 — virtual override 방식

### 현재 문제
- `MonsterController`가 하나의 클래스에 모든 상태 로직(Idle/Trace/Attack/Dead)을 포함
- 보스, 원거리 몬스터 등 종류가 늘어나면 상태 로직이 달라져야 함

### 해결: 추상 베이스 + 서브클래스 override

```
MonsterController (abstract base, MonoBehaviour)
├── 공통 필드: rb, spineController, monster, stateMachine, curVelocityX
├── 공통 헬퍼: UpdateFallGravity, UpdateScaleX, IsSameLayerPlayer, IsPlayerDetect, IsInAttackRange, GetMoveSpeed
├── 공통 상태: Dead (모든 몬스터 동일)
├── protected virtual: OnIdleEnter/Update, OnTraceEnter/Update, OnAttackEnter/Update
│
├── GroundMonsterController  ← 현재 근거리 로직 이동
├── RangedMonsterController  ← Attack override (원거리 공격)
└── BossMonsterController    ← 여러 상태 override + 추가 상태 가능
```

### 적용 방식
- `MonsterController`를 abstract로 변경, 상태 메서드를 protected virtual로 선언
- 각 몬스터 종류별 서브클래스가 필요한 상태만 override
- 프리팹에 해당 서브클래스 컴포넌트를 직접 부착
- `MonsterCharacter`는 `GetComponent<MonsterController>()`로 받으므로 어떤 서브클래스든 동일하게 동작

### 플레이어 참조
- `Main.Instance.Player` 직접 호출 방식 유지
- 프로젝트 컨벤션(`Main.Instance.XXXManager`)과 일관성 유지
- 항상 최신 참조 보장, null 상황 자연스럽게 처리

### 비교 검토한 대안들
| 방법 | 판단 |
|------|------|
| virtual override | **채택** — 변경 최소, 직관적 |
| 상태 클래스 분리 (전략 패턴) | 클래스 수 폭증, private 필드 접근 문제 |
| 컨트롤러 완전 분리 | 공통 코드 중복 |
