# MS UI 확장 클래스 생성

> RVD UI 시스템 분석 문서 기반. RVD에서 선별한 확장 기능을 MS* 네이밍으로 재구현.
> 네임스페이스: `MS.UI`
> 위치: `Assets/02. Scripts/Core/UI/`
> 코드 스타일: 기존 MS 프로젝트 코드 스타일 준수

---

## 작업 범위

### 1. MSButton (RvdButton 발췌)
- `UnityEngine.UI.Button` 상속
- **클릭 사운드 자동 재생**: `[SerializeField] string clickSfxKey` → `OnPointerClick`에서 `Main.Instance.SoundManager.PlaySFX` 호출
- **자식 Graphic 일괄 색상 전환**: `DoStateTransition` 오버라이드 → `colors`의 상태별 색상을 하위 모든 Graphic에 `CrossFadeColor` 적용
- **`applyColorToChildren`**: `[SerializeField] bool`로 자식 색상 전환 ON/OFF 제어
- RVD의 길게누르기, InteractionActive, 전역 레지스트리, 스크립트 연동은 제외

### 2. MSImage (RvdImage 발췌)
- `UnityEngine.UI.Image` 상속
- **Show/Hide**: DOTween `DOFade`로 알파 페이드 인/아웃 + `Action` 콜백
- **DoBlink**: `DOFade` + `SetLoops(-1, LoopType.Yoyo)` 깜빡임
- **DoSetColor**: `DOColor`로 목표 색상 전환
- **StopTween**: 진행중인 트윈 Kill + 원본 색상 복원
- RVD의 InternalData, SpriteAnimation, 자체 ColorTween은 제외

### 3. MSToggleButton (RvdToggleButton 발췌)
- `Selectable` 상속, `IPointerClickHandler` 구현
- **On/Off 상태**: `isOn` bool + `activeObject`/`inactiveObject` 전환
- **그룹 연동**: `MSToggleGroup parent` 참조 → 클릭 시 그룹에 통지
- **콜백**: `event Action<bool> onToggleChanged` (새 상태 전달)
- **SetIsOn(bool, bool invokeCallback)**: 외부에서 상태 변경
- RVD의 UserInputLock, 사운드, 스크립트 연동은 제외

### 4. MSToggleGroup (RvdToggleButtonGroup 발췌)
- `MonoBehaviour`
- **라디오 버튼 동작**: 하나 On → 나머지 Off
- **콜백**: `event Action<MSToggleButton, MSToggleButton> onToggleChanged` (prev, cur)
- **SelectToggle(int index, bool invokeCallback)**: 인덱스로 선택
- **allowToggleOff**: 모두 Off 허용 여부
- **AddToggle / RemoveToggle**: 동적 추가/제거

---

## 의존성

- DOTween (MSImage 트윈)
- Main.Instance.SoundManager (MSButton 사운드)

## 비고

- `[SerializeField]`는 사운드 키, activeObject/inactiveObject 등 인스펙터 설정이 필수인 항목에만 허용
- 각 클래스는 독립적으로 동작 가능해야 함 (MSButton 단독 사용 가능, MSToggleButton도 그룹 없이 사용 가능)
- Core/UI 폴더에 배치하는 이유: UI 컴포넌트이지만 프로젝트 전반에서 재사용되는 확장 라이브러리 성격

---
## 작업 완료

**작업 내용**:
- MSButton: Button 상속, 클릭 사운드 자동 재생 + SetSprite 헬퍼
- MSImage: Image 상속, DOTween 기반 Show/Hide/DoBlink/DoSetColor/StopTween
- MSToggleButton: Selectable 상속, On/Off 비주얼 전환 + 그룹 연동
- MSToggleGroup: 라디오 버튼 동작, 동적 토글 관리

**특이사항**:
- 네임스페이스 Core 사용 (MS.UI가 아닌 Core 확장 라이브러리 성격)
- SafeArea는 스펙에서 제외
- DoStateTransition 자식 색상 전환 대신 SetSprite 헬퍼로 대체

**태그**: #UI #확장컴포넌트 #DOTween #MSButton #MSImage #MSToggle
