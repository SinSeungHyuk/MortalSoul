# MS UI 확장 클래스 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** RVD UI에서 선별한 확장 기능을 Core 네임스페이스의 MS* 클래스로 구현

**Architecture:** `Assets/02. Scripts/Core/UI/` 하위에 4개 독립 파일(MSButton, MSImage, MSToggleButton, MSToggleGroup) 생성. DOTween과 Main.Instance.SoundManager에만 의존. 각 클래스는 Unity UI 컴포넌트를 상속하여 확장.

**Tech Stack:** Unity 6, DOTween, UnityEngine.UI, C#

---

## 파일 구조

| 파일 | 역할 |
|------|------|
| Create: `Assets/02. Scripts/Core/UI/MSButton.cs` | Button 상속. 클릭 사운드 + SetSprite 헬퍼 |
| Create: `Assets/02. Scripts/Core/UI/MSImage.cs` | Image 상속. DOTween 기반 Show/Hide/Blink/SetColor |
| Create: `Assets/02. Scripts/Core/UI/MSToggleButton.cs` | Selectable 상속. On/Off 상태 + 비주얼 전환 |
| Create: `Assets/02. Scripts/Core/UI/MSToggleGroup.cs` | MonoBehaviour. 라디오 버튼 그룹 관리 |

---

### Task 1: MSButton

**Files:**
- Create: `Assets/02. Scripts/Core/UI/MSButton.cs`

- [ ] **Step 1: Core/UI 폴더 생성**

```bash
mkdir -p "Assets/02. Scripts/Core/UI"
```

- [ ] **Step 2: MSButton.cs 작성**

```csharp
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core
{
    public class MSButton : Button
    {
        [SerializeField] private string clickSfxKey;

        public override void OnPointerClick(PointerEventData _eventData)
        {
            if (!string.IsNullOrEmpty(clickSfxKey))
                Main.Instance.SoundManager.PlaySFX(clickSfxKey);

            base.OnPointerClick(_eventData);
        }

        public void SetSprite(Sprite _sprite)
        {
            image.sprite = _sprite;
        }
    }
}
```

- [ ] **Step 3: Unity 컴파일 확인**

Run: Unity 콘솔에서 컴파일 에러 없는지 확인
Expected: 에러 없음

- [ ] **Step 4: 커밋**

```bash
git add "Assets/02. Scripts/Core/UI/MSButton.cs"
git commit -m "feat: MSButton 추가 - 클릭 사운드 + SetSprite 헬퍼"
```

---

### Task 2: MSImage

**Files:**
- Create: `Assets/02. Scripts/Core/UI/MSImage.cs`

- [ ] **Step 1: MSImage.cs 작성**

```csharp
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class MSImage : Image
    {
        private Tweener curTween;
        private Color originalColor;

        protected override void Awake()
        {
            base.Awake();
            originalColor = color;
        }

        public void Show(float _duration)
        {
            KillTween();
            curTween = this.DOFade(1f, _duration);
        }

        public void Hide(float _duration, Action _onComplete = null)
        {
            KillTween();
            curTween = this.DOFade(0f, _duration);
            if (_onComplete != null)
                curTween.OnComplete(() => _onComplete.Invoke());
        }

        public void DoBlink(float _speed)
        {
            KillTween();
            curTween = this.DOFade(0f, _speed).SetLoops(-1, LoopType.Yoyo);
        }

        public void DoSetColor(Color _target, float _duration)
        {
            KillTween();
            curTween = this.DOColor(_target, _duration);
        }

        public void StopTween()
        {
            KillTween();
            color = originalColor;
        }

        private void KillTween()
        {
            curTween?.Kill();
            curTween = null;
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Run: Unity 콘솔에서 컴파일 에러 없는지 확인
Expected: 에러 없음

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/Core/UI/MSImage.cs"
git commit -m "feat: MSImage 추가 - DOTween 기반 Show/Hide/Blink/SetColor"
```

---

### Task 3: MSToggleButton

**Files:**
- Create: `Assets/02. Scripts/Core/UI/MSToggleButton.cs`

- [ ] **Step 1: MSToggleButton.cs 작성**

```csharp
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core
{
    public class MSToggleButton : Selectable, IPointerClickHandler
    {
        public event Action<bool> onToggleChanged;

        public bool IsOn => isOn;

        [SerializeField] private GameObject activeObject;
        [SerializeField] private GameObject inactiveObject;

        private bool isOn;
        private MSToggleGroup group;

        public void SetIsOn(bool _value, bool _invokeCallback = true)
        {
            if (isOn == _value) return;

            isOn = _value;
            UpdateVisual();

            if (_invokeCallback)
                onToggleChanged?.Invoke(isOn);
        }

        public void SetGroup(MSToggleGroup _group)
        {
            group = _group;
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (group != null)
            {
                group.NotifyToggleClicked(this);
                return;
            }

            SetIsOn(!isOn);
        }

        private void UpdateVisual()
        {
            if (activeObject != null)
                activeObject.SetActive(isOn);
            if (inactiveObject != null)
                inactiveObject.SetActive(!isOn);
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Run: Unity 콘솔에서 컴파일 에러 없는지 확인
Expected: MSToggleGroup 참조로 인해 컴파일 에러 발생 가능 — Task 4 완료 후 함께 확인

---

### Task 4: MSToggleGroup

**Files:**
- Create: `Assets/02. Scripts/Core/UI/MSToggleGroup.cs`

- [ ] **Step 1: MSToggleGroup.cs 작성**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class MSToggleGroup : MonoBehaviour
    {
        public event Action<MSToggleButton, MSToggleButton> onToggleChanged;

        public MSToggleButton CurSelected => curSelected;

        [SerializeField] private bool allowToggleOff;

        private List<MSToggleButton> toggleList = new List<MSToggleButton>();
        private MSToggleButton curSelected;

        private void Awake()
        {
            var children = GetComponentsInChildren<MSToggleButton>(true);
            foreach (var toggle in children)
                AddToggle(toggle);
        }

        public void SelectToggle(int _index, bool _invokeCallback = true)
        {
            if (_index < 0 || _index >= toggleList.Count) return;

            var target = toggleList[_index];
            ApplySelection(target, _invokeCallback);
        }

        public void AddToggle(MSToggleButton _toggle)
        {
            if (toggleList.Contains(_toggle)) return;

            toggleList.Add(_toggle);
            _toggle.SetGroup(this);
        }

        public void RemoveToggle(MSToggleButton _toggle)
        {
            if (!toggleList.Remove(_toggle)) return;

            _toggle.SetGroup(null);

            if (curSelected == _toggle)
                curSelected = null;
        }

        public void NotifyToggleClicked(MSToggleButton _toggle)
        {
            if (curSelected == _toggle)
            {
                if (allowToggleOff)
                    ApplySelection(null, true);
                return;
            }

            ApplySelection(_toggle, true);
        }

        private void ApplySelection(MSToggleButton _newToggle, bool _invokeCallback)
        {
            var prev = curSelected;

            if (prev != null)
                prev.SetIsOn(false, _invokeCallback);

            curSelected = _newToggle;

            if (curSelected != null)
                curSelected.SetIsOn(true, _invokeCallback);

            if (_invokeCallback)
                onToggleChanged?.Invoke(prev, curSelected);
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Run: Unity 콘솔에서 컴파일 에러 없는지 확인 (Task 3 + Task 4 함께)
Expected: 에러 없음

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/Core/UI/MSToggleButton.cs" "Assets/02. Scripts/Core/UI/MSToggleGroup.cs"
git commit -m "feat: MSToggleButton + MSToggleGroup 추가 - 토글/라디오 버튼"
```

---

### Task 5: 작업요청 문서 이동 및 최종 정리

- [ ] **Step 1: 작업요청 문서를 InProgress → Archive로 이동**

```bash
mv "MortalSoulDoc/02. InProgress/MS UI 확장 클래스 생성.md" "MortalSoulDoc/03. Archive/MS UI 확장 클래스 생성.md"
```

- [ ] **Step 2: Archive 문서에 작업 내용 요약 추가**

파일 하단에 다음 내용 추가:

```markdown
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
```

- [ ] **Step 3: 최종 커밋**

```bash
git add -A
git commit -m "feat: MS UI 확장 클래스 구현 완료 - MSButton/MSImage/MSToggleButton/MSToggleGroup"
git push
```
