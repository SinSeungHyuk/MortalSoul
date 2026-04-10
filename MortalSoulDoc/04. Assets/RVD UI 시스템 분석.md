# RVD UserInterfaceSystem 분석

> 출처: `D:\Workspace\ETC\Old\Scripts\RVD\UserInterfaceSystem\`
> 이전 프로젝트(RVD)에서 사용한 커스텀 UI 라이브러리.
> MS 프로젝트에 이미 기본 UI 프레임워크(BaseUI/BasePopup/UIManager 등)가 존재하므로, **RVD에서 "확장 기능"만 선별적으로 가져오기 위한** 분석 문서.

---

## 0. MS 기존 UI 구조 (이미 구현됨 — 그대로 사용)

MS 레퍼런스에 이미 UI 시스템의 뼈대가 존재한다. 아래는 건드리지 않고 그대로 사용한다.

```
MortalSoulDoc/04. Assets/02. Scripts/UI/
├── BaseUI.cs                    ← Show()/Close() 기본 베이스
├── Common/
│   ├── DamageText.cs            ← 데미지 텍스트
│   ├── ExpBar.cs                ← 경험치 바
│   ├── HPBar.cs                 ← HP 바
│   ├── Notification.cs          ← 알림
│   ├── PlayerStatInfo.cs        ← 플레이어 스탯 표시
│   ├── SkillSlot.cs             ← 스킬 슬롯
│   └── Tooltip.cs               ← 툴팁
├── Panel/
│   ├── BattlePanel.cs           ← 전투 패널
│   ├── BattlePanelViewModel.cs  ← MVVM 뷰모델
│   ├── MainPanel.cs             ← 메인 패널
│   └── TitlePanel.cs            ← 타이틀 패널
└── Popup/
    ├── BasePopup.cs             ← UIManager.ClosePopup 연동 팝업 베이스
    ├── ArtifactPopup.cs
    ├── DownloadPopup.cs
    ├── PausePopup.cs
    ├── SkillRewardPopup.cs
    ├── StageEndPopup.cs
    └── StatRewardPopup.cs
```

**핵심 포인트**:
- `BaseUI`: `Show()` / `Close()` — SetActive 기반의 단순한 구조
- `BasePopup`: `Close()` → `UIManager.Instance.ClosePopup(this)` 연동
- UI 시스템 틀(매니저, 캔버스 관리, Push/Pop 등)은 MS 자체 것을 사용
- RVD의 `RvdUIBase`, `RvdPopup`, `RvdUserInterfaceSystem` 등 **시스템 레벨 클래스는 가져오지 않음**

---

## 1. RVD에서 가져올 확장 기능 목록

MS에 없는, RVD UI 컴포넌트의 **확장 기능**만 선별한다.

### 1-1. MSButton (RvdButton에서 발췌)

RvdButton(UnityEngine.UI.Button 상속)에서 가져올 기능:

| 기능 | 설명 | 가져올지 |
|------|------|---------|
| **클릭 사운드 FX 키** | `m_OnClickFxBundleKey` — 버튼에 사운드 키를 등록하면 클릭 시 자동 재생 | O (MS SoundManager 연동) |
| **자식 Graphic 일괄 색상 전환** | `DoStateTransition` 오버라이드 — 버튼 상태 변경 시 하위 모든 Image/Text에 색상 적용 | O (깔끔한 UX) |
| **콜백 시스템** | `SetOnClickCallback(Action<Button>)` 패턴 | △ (Unity 기본 onClick으로 충분할 수 있음) |
| **BeforeClick 가드** | `SetOnBeforeClickCallback(Func<bool>)` — 클릭 전 조건 체크 | △ (필요시) |
| 길게 누르기(Clicking) | ClickStart → ClickingWait → Clicking 상태머신 | X (핵슬에 불필요) |
| InteractionActive UI 전환 | 활성/비활성 시 다른 UI 표시 | X (과도함) |
| 전역 레지스트리 | `RvdUserInterfaceSystem`에 자동 등록 | X |
| 스크립트/커맨드 연동 | `ScriptSystem` 연동 | X |

**MS 적용 시 핵심 코드 (DoStateTransition)**:
```csharp
// 버튼 상태 변경 시 자식 Graphic 전체에 색상 전환 적용
protected override void DoStateTransition(SelectionState state, bool instant)
{
    base.DoStateTransition(state, instant);
    // 하위 모든 Graphic에 tintColor 적용 (CrossFadeColor)
}
```

**MS 적용 시 핵심 코드 (사운드 FX)**:
```csharp
// OnPointerClick에서 사운드 키가 있으면 자동 재생
if (!string.IsNullOrEmpty(m_OnClickFxKey))
    Main.Instance.SoundManager.PlaySFX(m_OnClickFxKey);
```

### 1-2. MSImage (RvdImage에서 발췌)

RvdImage(UnityEngine.UI.Image 상속)에서 가져올 기능:

| 기능 | 설명 | 가져올지 |
|------|------|---------|
| **Show/Hide (페이드 인/아웃)** | 알파 트윈 + 콜백 (Start/Showing/Finish 3단계) | O (DOTween으로 재구현) |
| **DoBlink** | 깜빡임 효과 | O (피격 등에 활용) |
| **DoSetColor** | 목표 색상으로 부드러운 전환 | O |
| 자체 ColorTween | RvdColorTween 의존 | X → DOTween 대체 |
| InternalData | object 딕셔너리 | X |
| SpriteAnimation | 스프라이트 프레임 재생 | X (Spine 사용) |

**MS 적용 시 핵심**: RVD의 자체 ColorTween 대신 DOTween으로 Show/Hide/Blink 구현
```csharp
public void Show(float _duration) => this.DOFade(1f, _duration);
public void Hide(float _duration, Action _onComplete = null)
    => this.DOFade(0f, _duration).OnComplete(() => _onComplete?.Invoke());
public void DoBlink(float _speed) => this.DOFade(0f, _speed).SetLoops(-1, LoopType.Yoyo);
```

### 1-3. ToggleButton + ToggleGroup (축소 도입)

| 기능 | 설명 | 가져올지 |
|------|------|---------|
| **On/Off 상태 + ActiveObject/InActiveObject 전환** | 시각적 토글 표현의 핵심 | O |
| **라디오 버튼 동작** (Group) | 하나 On → 나머지 Off | O |
| **OnToggleChange 콜백** (prev, cur) | 전환 시 콜백 | O |
| BeforeToggleChange 가드 | 전환 거부 가능 | △ |
| AllowToggleOffState | 모두 Off 허용 | △ |
| 사운드/스크립트 연동 | RVD 전용 | X |
| UserInputLock | 클릭 잠금 | X (interactable로 충분) |

### 1-4. SafeArea (그대로 가져옴)

노치 대응은 단순하고 필수. RVD의 `RvdUISafeArea` 로직을 거의 그대로 사용.
```csharp
// RectTransform 앵커를 Screen.safeArea에 맞게 조정
m_RectTransform.anchorMin = _minAnchor;
m_RectTransform.anchorMax = _maxAnchor;
```

---

## 2. 가져오지 않는 것 (MS 자체 구현 또는 불필요)

| RVD 항목 | 이유 |
|----------|------|
| `RvdUIBase` / `RvdPopup` | MS에 `BaseUI` / `BasePopup` 이미 존재 |
| `RvdUserInterfaceSystem` (중앙 매니저) | MS에 `UIManager` 이미 존재 |
| Push/Pop 캐싱 패턴 | MS UIManager에서 자체 구현 |
| `RvdText` | TMPro 사용 예정. StringTable/자동줄바꿈/크기조정 모두 TMPro 자체 기능으로 대체 |
| `RvdGaugeBar` | MS에 `HPBar`, `ExpBar` 이미 존재. 필요시 DOTween으로 fillAmount 트윈 추가 |
| `IRvdUi` + InternalData | object 박싱, 타입 안전하지 않음 |
| `RvdCoreUi` / `BaseUI(RVD)` / `RootUI` | Deprecated이거나 MS 구조와 불일치 |
| `RvdDataRow` / `RvdDropdown` | 데모 수준에서 불필요 |
| `VirtualScrollView` | 32K 토큰 대형 클래스. 데모에서 대량 리스트 없음 |
| `TabPageController` | Toggle 기반으로 필요시 직접 조합하면 충분 |
| `RvdUIAnimation` | DOTween 직접 사용으로 대체 |
| 전역 버튼/토글 레지스트리 | MS에서 불필요 |

---

## 3. 외부 의존성 매핑

RVD 의존성을 MS에서 어떻게 대체하는지 정리:

| RVD 의존성 | MS 대체 |
|-----------|---------|
| `RvdSoundSystem` | `Main.Instance.SoundManager` |
| `ScriptSystem` / `CommandSystem` | 제거 |
| `RvdColorTween` / `RvdTimeTween` | DOTween |
| `RvdResourceSystem` | `Main.Instance.AddressableManager` |
| `RvdStringTable` | 제거 (추후 별도 구현) |
| `RvdLogSystem` | `Debug.Log` |
| `RvdSafeAreaManager` | MS 전용 축소 구현 |

---

## 4. 리팩토링 규칙 (RVD → MS 변환 시)

- 네임스페이스: `Rvd.UserInterfaceSystem.*` → `MS.UI`
- `[SerializeField]` 최소화 → 코드에서 참조 해결 (사운드 FX 키 정도는 SerializeField 허용)
- 매개변수 `_` 접두사 유지
- `event Action` 타입으로 이벤트 선언
- 콜백 함수명: `On*Callback` 규칙
- 주석 최소화, `/// <summary>` 사용 금지
- `Main.Instance.SoundManager` 등 Main 패턴 사용
- RVD의 자체 트윈(ColorTween/TimeTween) → DOTween으로 전환
