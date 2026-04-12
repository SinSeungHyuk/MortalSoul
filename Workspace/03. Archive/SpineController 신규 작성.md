
작성일 2026-04-07
완료일 2026-04-07

### 작업 요청

기존 `PlayerSpineController`를 Player/Monster 공용 단일 컴포넌트로 개편한다. 추상화/상속 없이 단일 콘크리트 클래스 하나만 둔다.

1. **리네임 및 위치 이동**
   - 클래스명: `PlayerSpineController` → `SpineController`
   - 위치: `Assets/02. Scripts/FieldObject/FieldCharacter/` 폴더 바로 아래
   - 네임스페이스: `MS.Field`

2. **공용 컴포넌트 정책**
   - Player/Monster 양쪽에서 동일하게 부착해서 사용
   - 차이점인 부위별 스킨 조합 메서드(`SetCombinedSkin`)도 같은 클래스에 포함하되, 몬스터 인스턴스에서는 호출하지 않음

3. **추가 기능**
   - `PlayOnce(animKey, onComplete)` — 공격/스킬/피격/사망 등 단발 애니메이션 재생 + 완료 콜백
   - Spine 이벤트(`attack`) → 게임 이벤트 변환 후 노출: `OnAttackEvent`
   - Spine 이벤트(`combo_ready`) → 게임 이벤트 변환 후 노출: `OnComboReadyEvent`
   - Spine `Complete` 트랙 이벤트 → `OnActionCompleted` 이벤트로 노출
   - 기존 `PlayLoop`, `SetFacing`, 이동 상태 래퍼(`PlayIdle`/`PlayMove`/`PlayJump`/`PlayDash`) 유지

### 비범위

- WSC 측 이벤트 구독 연결 (다음 작업에서 진행)
- 몬스터 측 부착 (이번엔 클래스만 공용으로 만들고 실제 부착은 추후)

---

### 작업 내용

1. **파일 이동/리네임**
   - `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerSpineController.cs` → `Assets/02. Scripts/FieldObject/FieldCharacter/SpineController.cs`
   - `.meta` 파일 함께 이동하여 GUID(`ccf15c44c2079124e845ea1e9bb4ffe6`) 보존 → 씬 참조 무손상

2. **`SpineController` 클래스 작성** (`Assets/02. Scripts/FieldObject/FieldCharacter/SpineController.cs`)
   - 이벤트: `OnAttackEvent` / `OnComboReadyEvent` / `OnActionCompleted`
   - Spine `AnimationState.Event` 구독 → user data event 이름이 `"attack"` / `"combo_ready"`일 때 게임 이벤트 발화
   - Spine `AnimationState.Complete` 구독 → `PlayOnce` 재생 중일 때만 `OnActionCompleted` 발화 후 콜백 실행
   - `PlayOnce(animKey, onComplete)` — 단발 액션 + 완료 콜백 (콜백은 1회용, 지역변수 보관 후 호출)
   - `SetCombinedSkin(params string[])` — 부위별 스킨 조합. 인자 없이 호출 시 기존 테스트 스킨 4종 폴백
   - 기존 `PlayLoop` / `PlayIdle` / `PlayMove` / `PlayJump` / `PlayDash` / `SetFacing` 인터페이스 유지
   - `MainTrack` 상수(0)로 트랙 인덱스 통일, `OnDestroy`에서 이벤트 해제

3. **참조 업데이트**
   - `PlayerController.cs`: `PlayerSpineController` 타입/`GetComponent` → `SpineController`
   - `TestScene.unity`: `m_EditorClassIdentifier` 문자열을 `MS.Field.SpineController`로 갱신 (m_Script GUID는 보존)

### 특이사항

- `Start()`에서 임시로 `SetCombinedSkin()` 폴백을 호출하여 기존 테스트 스킨 조합을 그대로 적용. 소울 시스템 연동 시 외부에서 명시적 키 배열로 호출하도록 교체 예정
- `PlayerCharacter`는 현재 `TestSpineComponent`를 사용하므로 이번 작업 범위에서 제외 (별도 작업)
- WSC 측 이벤트 구독, 몬스터 부착은 비범위 그대로 유지

### 태그

#SpineController #Refactor #Player #Monster #Animation #SpineEvent #공용컴포넌트
