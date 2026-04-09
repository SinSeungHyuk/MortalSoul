# 플레이어 공격 중 액션 허용

## 요약

공격(Attack) 상태에서 점프/대시 입력 시 공격을 즉시 캔슬하고 해당 동작을 수행한다. 이동은 불허. 스킬 캔슬은 스킬 버튼 구현 시 추후 처리.

## 현재 문제

- `PlayerMovementController`의 Attack 상태에 Update 함수가 `null`로 등록되어 있어 공격 중 모든 입력이 무시됨
- CLAUDE.md 조작 규칙에 명시된 "공격 중 대시/점프/스킬로 즉시 캔슬 가능" 동작이 구현되지 않음

## 설계

### 변경 대상

`PlayerMovementController.cs` 단일 파일 수정

### 변경 내용

1. `OnAttackUpdate` 메서드 추가 — 대시/점프 입력 체크 후 상태 전환
2. Attack 상태 등록 시 Update에 `OnAttackUpdate` 연결 (`null` → `OnAttackUpdate`)

### 캔슬 메커니즘

상태 전환 → Jump/Dash Enter에서 `SpineController.PlayAnimation` 호출 → SpineController 내부에서 `CancelAllWaitTcs()` 실행 → WSC의 `ActivateAttackAsync` await 체인이 `OperationCanceledException`으로 종료 → finally 블록에서 `isAttacking = false`, `OnAttackEnded` 발행

WSC 코드 수정 불필요. 기존 안전망이 그대로 동작.

### OnAttackUpdate 로직

```
대시 입력 && 쿨다운 완료 → Dash 상태 전환
점프 입력 && 지상 → Jump 상태 전환
(이동 입력은 무시 — curVelocityX 갱신 안 함)
```

### 주의사항

- `OnAttackEndedCallback`이 Idle로 전환하는데, 이미 Jump/Dash로 전환된 뒤 WSC finally에서 `OnAttackEnded`가 발생하면 Idle로 덮어써질 수 있음
- 해결: `OnAttackEndedCallback`에서 현재 상태가 Attack일 때만 Idle 전환하도록 가드 추가
