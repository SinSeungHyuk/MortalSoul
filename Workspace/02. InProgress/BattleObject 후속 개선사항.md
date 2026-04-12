# BattleObject 후속 개선사항

코드 리뷰에서 도출된 Important 이슈들. 실제 스킬/무기 구현 연동 전에 처리 필요.

## Important (스킬 연동 전 수정)

### 1. BattleObjectManager.OnUpdate — O(n²) 제거 패턴
`battleObjectList.Remove(releaseObject)`가 루프 안에서 O(n) 선형 탐색.
투사체 다수 동시 존재 시 성능 저하 가능.
→ swap-and-remove-last 또는 `RemoveAll` 패턴으로 교체

### 2. AreaObject 타겟 리스트 중복 등록
캐릭터가 콜라이더 여러 개를 가질 경우 `OnTriggerEnter2D`가 콜라이더당 1회 발생.
같은 FieldCharacter가 중복 등록되어 틱 데미지 중복 적용됨.
→ `attackTargetList.Contains` 체크 추가 또는 `HashSet<FieldCharacter>` 사용

### 3. 오너 사망 시 BattleObject 정리
오너가 사망해 풀 반환된 후에도 BattleObject가 살아있으면 콜백이 비활성 오브젝트에 호출됨.
→ `ClearBattleObjectsByOwner(FieldCharacter)` 메서드 추가, 사망 흐름에서 호출

### 4. 풀 반환 시 참조 정리
`ObjectPoolManager.Return`은 `SetActive(false)` + 리페어런팅만 수행.
반환된 BattleObject가 owner/traceTarget/attackTargetList 등 참조를 계속 보유 → GC 방해.
→ `OnReturnToPool()` 가상 메서드 추가, 매니저에서 반환 전 호출

### 5. LoadAllBattleObjectAsync 부트 연동
현재 빈 스켈레톤. 실제 BattleObject 프리팹 추가 시 풀 등록 코드 채우고
`Main.BootAsync()`에서 호출 배선 필요.

## Suggestion (선택)

### 6. traceTarget 사망 시 동작 비대칭
- ProjectileObject: 추적 대상 사망 → 직선 이동 유지
- AreaObject: 추적 대상 사망 → 마지막 위치에 고정
→ 의도적 설계인지 확인 후 문서화 또는 통일

### 7. Main.Update 무조건 실행
타이틀/로딩 중에도 `BattleObjectManager.OnUpdate` 호출됨 (리스트 빈 상태라 무해).
매니저 업데이트가 늘어나면 `IsBootCompleted` 게이트 고려.

### 8. 세터 체이닝 (fluent API)
Set* 메서드가 void 반환. 스폰 시 호출이 여러 줄.
→ `BattleObject` 반환으로 바꾸면 체이닝 가능 (스타일 선호도 문제)
