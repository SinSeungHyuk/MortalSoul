# MS UI 확장 클래스 설계

> RVD UI 시스템에서 선별한 확장 기능을 MS* 네이밍으로 재구현
> 네임스페이스: `Core`
> 위치: `Assets/02. Scripts/Core/UI/`

---

## 파일 구조

```
Assets/02. Scripts/Core/UI/
├── MSButton.cs
├── MSImage.cs
├── MSToggleButton.cs
└── MSToggleGroup.cs
```

각 클래스는 독립 파일. MSToggleButton ↔ MSToggleGroup만 상호 참조.

---

## 1. MSButton

`UnityEngine.UI.Button` 상속.

### 필드

| 접근자 | 타입 | 이름 | 설명 |
|--------|------|------|------|
| `[SerializeField]` | `string` | `clickSfxKey` | 클릭 사운드 키. 비어있으면 무음 |

### 메서드

| 메서드 | 설명 |
|--------|------|
| `OnPointerClick(PointerEventData _eventData)` | override. clickSfxKey가 있으면 `Main.Instance.SoundManager.PlaySFX(clickSfxKey)` 호출 후 `base` 호출 |
| `SetSprite(Sprite _sprite)` | `image.sprite = _sprite` 헬퍼. 버튼에 붙은 Image 컴포넌트의 sprite 교체 |

---

## 2. MSImage

`UnityEngine.UI.Image` 상속. DOTween 기반 트윈 유틸리티.

### 필드

| 접근자 | 타입 | 이름 | 설명 |
|--------|------|------|------|
| `private` | `Tweener` | `curTween` | 현재 진행 중인 트윈 참조 |
| `private` | `Color` | `originalColor` | Awake 시점에 캐싱한 원본 색상 |

### 트윈 관리 규칙

새 트윈 호출 시 `curTween?.Kill()` → 현재 상태 그대로 유지 → 새 트윈 시작.

### 메서드

| 메서드 | 설명 |
|--------|------|
| `Show(float _duration)` | Kill 후 `DOFade(1f, _duration)` |
| `Hide(float _duration, Action _onComplete = null)` | Kill 후 `DOFade(0f, _duration)` + OnComplete 콜백 |
| `DoBlink(float _speed)` | Kill 후 `DOFade(0f, _speed).SetLoops(-1, LoopType.Yoyo)` |
| `DoSetColor(Color _target, float _duration)` | Kill 후 `DOColor(_target, _duration)` |
| `StopTween()` | Kill + `color`를 `originalColor`로 복원 |

---

## 3. MSToggleButton

`UnityEngine.UI.Selectable` 상속, `IPointerClickHandler` 구현.

### 필드

| 접근자 | 타입 | 이름 | 설명 |
|--------|------|------|------|
| `private` | `bool` | `isOn` | 현재 토글 상태 |
| `[SerializeField]` | `GameObject` | `activeObject` | On일 때 표시할 오브젝트 |
| `[SerializeField]` | `GameObject` | `inactiveObject` | Off일 때 표시할 오브젝트 |
| `private` | `MSToggleGroup` | `group` | 소속 그룹 (null이면 독립 동작) |

### 프로퍼티

| 이름 | 설명 |
|------|------|
| `bool IsOn => isOn` | 읽기 전용 |

### 이벤트

| 이벤트 | 설명 |
|--------|------|
| `event Action<bool> onToggleChanged` | 상태 변경 시 새 상태 전달 |

### 메서드

| 메서드 | 설명 |
|--------|------|
| `SetIsOn(bool _value, bool _invokeCallback = true)` | 상태 변경 + activeObject/inactiveObject 전환 + 선택적 콜백 발행 |
| `SetGroup(MSToggleGroup _group)` | 그룹 참조 설정 |
| `OnPointerClick(PointerEventData _eventData)` | 그룹 있으면 `group.NotifyToggleClicked(this)`, 없으면 직접 토글 |

`UpdateVisual()` — private. `activeObject`/`inactiveObject`의 SetActive를 isOn 상태에 맞춰 전환.

---

## 4. MSToggleGroup

`MonoBehaviour`.

### 필드

| 접근자 | 타입 | 이름 | 설명 |
|--------|------|------|------|
| `private` | `List<MSToggleButton>` | `toggleList` | 소속 토글 목록 |
| `private` | `MSToggleButton` | `curSelected` | 현재 선택된 토글 |
| `[SerializeField]` | `bool` | `allowToggleOff` | 모두 Off 허용 여부 |

### 프로퍼티

| 이름 | 설명 |
|------|------|
| `MSToggleButton CurSelected => curSelected` | 읽기 전용 |

### 이벤트

| 이벤트 | 설명 |
|--------|------|
| `event Action<MSToggleButton, MSToggleButton> onToggleChanged` | (prev, cur) 전달 |

### 메서드

| 메서드 | 설명 |
|--------|------|
| `SelectToggle(int _index, bool _invokeCallback = true)` | 인덱스로 선택. 기존 Off → 새 On |
| `AddToggle(MSToggleButton _toggle)` | 토글 추가 + `_toggle.SetGroup(this)` |
| `RemoveToggle(MSToggleButton _toggle)` | 토글 제거 + `_toggle.SetGroup(null)` |
| `NotifyToggleClicked(MSToggleButton _toggle)` | 토글 클릭 통지. 라디오 로직 처리 |

### 초기화

`Awake`에서 `GetComponentsInChildren<MSToggleButton>()`으로 자동 수집 → 각 토글에 `SetGroup(this)` 호출.

---

## 의존성

| 의존성 | 사용처 |
|--------|--------|
| DOTween | MSImage 트윈 |
| Main.Instance.SoundManager | MSButton 클릭 사운드 |

---

## 비고

- 각 클래스 독립 사용 가능 (MSButton 단독, MSToggleButton 그룹 없이 사용 가능)
- `Core/UI/` 폴더 배치: UI이지만 프로젝트 전반에서 재사용하는 확장 라이브러리 성격
