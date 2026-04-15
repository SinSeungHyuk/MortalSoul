
작성일 2026-04-12

## 목적

튜토리얼 및 게임 최초 시작 시 진입하는 마을에서 간단한 대화를 통한 스토리 진행 (포트폴리오 기술 어필)

## 뼈대

- 플레이어와 동일한 스파인 에셋 사용, 스킨은 코드로 정해진 스킨이 입혀진 상태에서 시작
- FieldCharacter 기반의 NPC — 플레이어와 상호작용 가능
- 우선 대화 시스템부터 구현
- 추후 기능 확장 시 대화 이후 추가 기능 연결 가능 (플레이어 강화, 난이도 세팅 등)

## 구현 순서 (예상)

1. 대화(Dialog) 시스템 — 대화 데이터 구조 + UI + 진행 로직
2. NPC 필드 오브젝트 — FieldCharacter 상속, 스파인 스킨 적용, 상호작용 트리거
3. 확장 포인트 — 대화 종료 후 콜백으로 추가 기능 연결

## 메모

- 대화 데이터는 JSON으로 관리 (SettingData 패턴)
- 상호작용은 플레이어 접근 + 버튼 입력 방식
- NPC는 전투 기능 없음 (BSC 불필요)

---

## 다이얼로그 시스템 브레인스토밍 정리 (2026-04-15)

### 확정 사항

| 항목 | 결정 | 근거 |
|---|---|---|
| 라인 데이터 구조 | 화자명 + 포트레이트 SpineKey + 스킨키 + **좌/우 플래그** + 텍스트 | B안. 좌우 분기 처리 공수가 A안과 거의 동일하여 확장성 확보 |
| 저장 방식 | JSON `DialogueSettingData` (Dictionary 구조, DataManager 로드) | 기존 SettingData 컨벤션 일관성 |
| 타이핑 효과 | TMP `maxVisibleCharacters` + **2단계 터치** (타이핑 중 터치 → 즉시 완성 / 완성 후 터치 → 다음 라인) | 실질 코드량 20~30줄로 공수 작음 |
| 타이핑 속도 | `Settings.DialogueTypingSpeed` (초당 글자 수) 상수 | 프로젝트 Settings 패턴 |
| 아키텍처 | **전용 매니저 없음**. `UIManager.ShowDialogueAsync(key)` + `DialoguePopup` (UI 스크립트가 로직 소유) | 다이얼로그는 UI 안에서 상태/생명주기 완결 — 매니저 레이어가 passthrough가 되어 중복 |
| 호출 패턴 | `UniTask` 반환 → 호출부가 `await` 또는 `Forget()` 선택 | 프로젝트 전반 UniTask 기반, 콜백 방식보다 가독성/에러처리 우수 |

### 미결 사항 (재검토 필요)

**IInteractable 인터페이스의 비동기 시그니처 문제**

현재 상호작용 시스템 설계의 `void Interact(PlayerCharacter)`는 동기 시그니처인데,
대화 이후 후속 로직(예: 게이트키퍼 NPC → 대화 종료 → 던전 입장)을 자연스럽게 이어가려면 `await` 흐름이 필요.

**논의된 해결안**: 인터페이스 자체를 `UniTask InteractAsync(PlayerCharacter)`로 정의

```csharp
public interface IInteractable
{
    string InteractIconKey { get; }
    UniTask InteractAsync(PlayerCharacter _player);
}
```

- 동기 구현체: `return UniTask.CompletedTask;` 한 줄로 끝
- 비동기 구현체: `async UniTask` + 자연스러운 await 체이닝
- PlayerInteractor: `CurTarget?.InteractAsync(player).Forget();` — 결과 관심 없으면 Forget

**SSC의 `ActivateSkillAsync` 패턴과 일관되어 이론상 문제 없음.
그러나 "모든 상호작용이 반드시 비동기일 필요가 있나?"에 대한 디자인 판단이 필요. 차후 재검토.**

### 참고 구현 스케치

```csharp
// UIManager
public async UniTask ShowDialogueAsync(string _dialogueKey)
{
    var view = await OpenPopupAsync<DialoguePopup>();
    await view.PlayAsync(_dialogueKey);
    ClosePopup(view);
}

// DialoguePopup
public async UniTask PlayAsync(string _dialogueKey)
{
    var data = Main.Instance.DataManager.SettingData.DialogueDict[_dialogueKey];
    foreach (var line in data.Lines)
    {
        SetPortrait(line);
        SetSpeaker(line);
        await TypeTextAsync(line.Text);
        await WaitForNextTouchAsync();
    }
}

// 타이핑
async UniTask TypeTextAsync(string _text, CancellationToken _ct)
{
    textMesh.text = _text;
    textMesh.maxVisibleCharacters = 0;
    isTyping = true;

    float interval = 1f / Settings.DialogueTypingSpeed;
    for (int i = 1; i <= _text.Length; i++)
    {
        textMesh.maxVisibleCharacters = i;
        await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: _ct);
    }
    isTyping = false;
}
```

### 호출부 예시

```csharp
// 케이스 1: 대화 후 후속 동작
await Main.Instance.UIManager.ShowDialogueAsync("gatekeeper_intro");
Main.Instance.GameManager.EnterDungeon();

// 케이스 2: 단순 대화만
Main.Instance.UIManager.ShowDialogueAsync("village_idle_chat_01").Forget();
```

### 다음 스텝

- IInteractable async 전환 여부 판단
- 판단 후 작업요청 md 작성 (`Workspace/01. WorkReq/다이얼로그 시스템 구현.md`)
- 상호작용 시스템 md에 인터페이스 시그니처 변경도 반영 (필요 시)
