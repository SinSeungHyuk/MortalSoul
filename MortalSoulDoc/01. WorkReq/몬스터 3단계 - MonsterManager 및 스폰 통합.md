
작성일 2026-04-09

### 작업 요청

MonsterManager를 구현하고 풀/스폰 흐름을 연결한 뒤, 테스트용 몬스터 1종으로 전체 파이프라인을 통합 검증한다.

> 참조: `MortalSoulDoc/04. Assets/Monster 설계.md` (§10, §11)
> 선행: 몬스터 2단계 완료 필수

---

### 1. MonsterManager 구현

`Assets/02. Scripts/Core/Manager/MonsterManager.cs`

설계 문서 §10-3에 따라 얇은 책임:

```
MonsterManager
├─ SpawnMonster(string _monsterKey, Vector3 _position) → MonsterCharacter
├─ DespawnMonster(MonsterCharacter _monster)
├─ ClearAll()
└─ activeMonsters: List<MonsterCharacter>
```

- `SpawnMonster`:
  - `ObjectPoolManager.Get(_monsterKey, _position)` → GameObject
  - `GetComponent<MonsterCharacter>().InitMonster(_monsterKey)`
  - `activeMonsters.Add(monster)`
  - return monster

- `DespawnMonster`:
  - `activeMonsters.Remove(_monster)`
  - `ObjectPoolManager.Return(_monsterKey, _monster.gameObject)`

- `ClearAll`:
  - 활성 몬스터 전부 `DespawnMonster` 처리 (던전 종료 시)

- AI 전역 통제, 어그로 조정, 스폰 스케줄링은 포함하지 않음 (던전 시스템 책임)

---

### 2. 풀 등록 흐름

- 프리팹 키 규약: `monsterKey` = 풀 등록 키 = `MonsterSettingDict` 키 (통일)
- 풀 등록 시점: 테스트 단계에서는 테스트 코드에서 `ObjectPoolManager.CreatePoolAsync` 호출
- 추후 던전 입장 로딩 화면에서 일괄 등록하는 구조로 전환 예정

---

### 3. 테스트용 몬스터 1종 통합 검증

몬스터 에셋(Spine) 구매 후 진행. 가장 단순한 근접 몬스터로 다음을 검증:

- [ ] 프리팹 구성: MonsterCharacter + MonsterController + Rigidbody2D + BoxCollider2D + SpineController
- [ ] MonsterSettingData.json에 테스트 몬스터 데이터 등록
- [ ] 풀 생성 → 스폰 → Idle 패트롤 동작 확인
- [ ] 플레이어 접근 시 Trace 전이 + 추적 동작 확인
- [ ] AttackRange 진입 시 Attack 전이 + 스킬 실행 확인
- [ ] 사망 시 Dead 애니메이션 → 풀 반환 → activeMonsters 제거 확인
- [ ] BSC TakeDamage 사망 가드 (Dead 상태에서 추가 피격 무시) 확인

---

### 선행 조건

- 몬스터 2단계 완료 (MonsterCharacter + MonsterController)
- 몬스터 Spine 에셋 구매 및 프리팹 구성

### 비범위

- 던전 시스템 연동 (스폰 스케줄링, 방 클리어 판정 등)
- 드롭 아이템 시스템
- 몬스터 풀 정리(unload) 최적화 (메모리 프로파일링 후)

---
태그 : #Monster #MonsterManager #ObjectPool #스폰 #통합테스트
