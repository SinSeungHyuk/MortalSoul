# BattleObject 계층 이주

## 목적
레퍼런스 프로젝트의 SkillObject 계층(SkillObject / ProjectileObject / AreaObject)을 현재 프로젝트에 맞게 이주한다. 설계 문서 "히트판정 및 스킬 구현 설계.md"에 정의된 변경사항을 반영한다.

## 작업 범위

### 1. BattleObject (레퍼런스: SkillObject)
- 네임스페이스: `MS.Battle`
- 상속: `FieldObject` (기존 그대로)
- 변경점:
  - 클래스명 `SkillObject` → `BattleObject`
  - `IsValidTarget(Collider)` → `IsValidTarget(Collider2D)`: `TryGetComponent<FieldCharacter>()` → `.BSC`로 접근
  - 콜백 시그니처: `Action<BattleObject, FieldCharacter>` (레퍼런스의 SSC 대신 FieldCharacter 전달, BSC 접근은 호출측에서)
  - `SkillObjectKey` → `BattleObjectKey`

### 2. ProjectileObject
- `OnTriggerEnter(Collider)` → `OnTriggerEnter2D(Collider2D)`
- 이동: `Vector3` → `Vector2`
- 추적: `LookAt` 제거 → 2D 회전(angle 기반) 적용
- `MoveTowards` → `Vector2.MoveTowards`

### 3. AreaObject
- `OnTriggerEnter/Exit(Collider)` → `OnTriggerEnter2D/Exit2D(Collider2D)`
- 타겟 리스트: `List<FieldCharacter>` (레퍼런스의 SSC 대신)
- `AreaRangeMultiple` 스탯 참조 제거 (현재 프로젝트에 해당 스탯 없음)
- 죽은 타겟 정리 로직 유지

### 4. BattleObjectManager (레퍼런스: SkillObjectManager)
- 싱글톤 제거 → 일반 클래스 (`Main` 패턴에 따라)
- `Main`에 `BattleObjectManager` 프로퍼티 추가
- `SpawnBattleObject<T>`, `OnUpdate`, `ClearBattleObject` 메서드
- `ObjectPoolManager` 접근: `Main.Instance.ObjectPoolManager`
- `LoadAllBattleObjectAsync`는 스켈레톤만 작성 (풀 등록은 실제 BattleObject 프리팹 추가 시 채움)

## 제외 사항
- IndicatorObject (별도 작업)
- 개별 스킬 클래스 이주 (별도 작업)
- BattleObject 프리팹 생성 (리소스 작업)
- WSC/SSC에서 BattleObject 스폰 연동 (별도 작업)
- TakeDamage 구현 (별도 작업)

---

### 작업 내용
- BattleObject / ProjectileObject / AreaObject / BattleObjectManager 4개 클래스 이주 완료
- 3D→2D 변환, 싱글톤→Main 패턴 전환, 네이밍 변경 적용

### 특이사항
- 후속 개선사항(O(n²) 제거, 타겟 중복, 오너 사망 정리 등)은 별도 작업으로 InProgress에 존재

---
태그 : #BattleObject #ProjectileObject #AreaObject #이주 #완료
