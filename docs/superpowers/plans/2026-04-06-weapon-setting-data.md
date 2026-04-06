# WeaponSettingData 구축 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 무기별 기본공격(노멀 어택) 콤보 데이터를 담을 `WeaponSettingData` 데이터 구조를 정의하고, Addressables 기반 JSON 파이프라인을 통해 런타임에 `SettingData.WeaponSettingDict`로 로드한다.

**Architecture:** `WeaponSettingData` / `AttackComboData`는 순수 POCO 데이터 컨테이너이며, 기존 `SkillSettingData` / `MonsterSettingData` 패턴을 그대로 따른다. `SettingData.LoadAllSettingDataAsync()`에 로드 블록 하나를 추가하여 `Dictionary<EWeaponType, WeaponSettingData>`를 채운다. WSC 소비 로직은 본 플랜 범위 밖이며, 이 플랜은 "데이터가 런타임에 정상 로드되어 조회 가능"까지를 완성 기준으로 한다.

**Tech Stack:** Unity 6 (6000.3.10f1), C#, Newtonsoft.Json, Unity Addressables, UniTask

**Spec Reference:** [MortalSoulDoc/02. InProgress/WeaponSettingData 구축.md](../../../MortalSoulDoc/02. InProgress/WeaponSettingData 구축.md)

---

## 참고: 테스팅 전략

이 프로젝트는 Unity Test Framework 기반 단위 테스트 인프라가 없고, 본 작업의 코드는 **로직이 없는 POCO 데이터 클래스 + 기존 패턴 반복**이 전부이다. 따라서 표준 TDD 사이클 대신 **Unity 에디터 런타임 검증**을 테스트 수단으로 사용한다:

- 각 태스크 완료 후 Unity 에디터에서 Play 모드 진입
- `SettingData.LoadAllSettingDataAsync()` 완료 후 `WeaponSettingDict`의 내용을 Debug.Log로 출력하는 **임시 검증 코드**를 Task 4에 포함
- 출력이 예상대로 나오면 검증 통과 후 임시 코드 제거

이는 "TDD 생략"이 아니라 "비POCO·비상호작용 데이터 로딩에 대한 YAGNI 적용"이다.

---

## File Structure

| 파일 | 상태 | 책임 |
|---|---|---|
| `Assets/02. Scripts/Data/SettingData/WeaponSettingData.cs` | 수정 | `EWeaponType`(기존) + `WeaponSettingData`, `AttackComboData` 클래스 정의 |
| `Assets/04. Settings/SettingData/WeaponSettingData.json` | 신규 | OneHandSword 2콤보 초기 데이터 |
| `Assets/02. Scripts/Data/SettingData/SettingData.cs` | 수정 | `WeaponSettingDict` 프로퍼티 + 로드 호출 추가 |
| Addressables 그룹 `SettingData` | 수정 (에디터) | `WeaponSettingData.json`을 `"WeaponSettingData"` 키로 등록 |

---

## Task 1: 데이터 클래스 추가

**Files:**
- Modify: `Assets/02. Scripts/Data/SettingData/WeaponSettingData.cs`

- [ ] **Step 1: `WeaponSettingData.cs`를 아래 내용으로 교체**

현재 파일은 enum 하나만 담고 있다. 다음 내용으로 **전체 덮어쓰기**:

```csharp
using System;
using System.Collections.Generic;

namespace MS.Data
{
    public enum EWeaponType
    {
        GreatSword,
        OneHandSword,
        Dagger,
        Bow,
        Staff
    }

    [Serializable]
    public class WeaponSettingData
    {
        public List<AttackComboData> ComboList { get; set; }
    }

    [Serializable]
    public class AttackComboData
    {
        public string AnimKey { get; set; }
        public float DamageMultiplier { get; set; }
        public float HitRange { get; set; }
        public float HitOffset { get; set; }
        public float Knockback { get; set; }
    }
}
```

- [ ] **Step 2: Unity 에디터에서 컴파일 확인**

Unity 에디터로 돌아가 포커스 → 자동 컴파일. Console에 에러 없어야 함.
Expected: `No errors` (Console 창 하단 상태바)

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/Data/SettingData/WeaponSettingData.cs"
git commit -m "feat: WeaponSettingData / AttackComboData 클래스 추가"
```

---

## Task 2: JSON 데이터 파일 생성

**Files:**
- Create: `Assets/04. Settings/SettingData/WeaponSettingData.json`

- [ ] **Step 1: JSON 파일 생성**

`Assets/04. Settings/SettingData/WeaponSettingData.json` 파일을 아래 내용으로 생성한다:

```json
{
  "OneHandSword": {
    "ComboList": [
      {
        "AnimKey": "Attack_OneHand1",
        "DamageMultiplier": 1.0,
        "HitRange": 1.5,
        "HitOffset": 1.2,
        "Knockback": 2.0
      },
      {
        "AnimKey": "Attack_OneHand2",
        "DamageMultiplier": 1.5,
        "HitRange": 1.8,
        "HitOffset": 1.4,
        "Knockback": 4.0
      }
    ]
  }
}
```

- [ ] **Step 2: Unity 에디터가 .meta 파일을 생성하도록 포커스 이동**

Unity 에디터로 포커스 전환 → 프로젝트 창에서 `04. Settings/SettingData` 폴더를 선택하여 `WeaponSettingData.json`과 `WeaponSettingData.json.meta`가 모두 보이는지 확인.
Expected: `WeaponSettingData.json` + `WeaponSettingData.json.meta` 두 파일 존재

- [ ] **Step 3: 커밋**

```bash
git add "Assets/04. Settings/SettingData/WeaponSettingData.json" "Assets/04. Settings/SettingData/WeaponSettingData.json.meta"
git commit -m "feat: WeaponSettingData.json 초기 데이터 추가 (OneHandSword 2콤보)"
```

---

## Task 3: Addressables 그룹 등록 (Unity 에디터 수동 작업)

**Files:**
- Modify: `Assets/AddressableAssetsData/AssetGroups/SettingData.asset` (Unity가 자동 수정)

- [ ] **Step 1: Addressables Groups 창 열기**

Unity 메뉴 → `Window` → `Asset Management` → `Addressables` → `Groups`

- [ ] **Step 2: `SettingData` 그룹에 JSON 드래그 등록**

Project 창에서 `Assets/04. Settings/SettingData/WeaponSettingData.json`을 잡아, Addressables Groups 창의 **`SettingData` 그룹**에 드래그 드롭. 기본 주소(Address)가 긴 경로로 잡히므로 클릭하여 **`WeaponSettingData`** 로 수정.

- [ ] **Step 3: 등록 확인**

SettingData 그룹 안에 `WeaponSettingData`라는 엔트리가 보이고, 해당 엔트리의 Path가 `Assets/04. Settings/SettingData/WeaponSettingData.json`이어야 함.
Expected: 그룹 내 새 엔트리 `WeaponSettingData` 존재

- [ ] **Step 4: 커밋**

```bash
git add "Assets/AddressableAssetsData/AssetGroups/SettingData.asset"
git commit -m "feat: WeaponSettingData Addressable 등록"
```

---

## Task 4: SettingData 로딩 연결 + 런타임 검증

**Files:**
- Modify: `Assets/02. Scripts/Data/SettingData/SettingData.cs`

- [ ] **Step 1: `SettingData.cs`에 프로퍼티 추가**

`SoundSettingDict` 아래에 한 줄 추가:

```csharp
public Dictionary<string, SoundSettingData> SoundSettingDict { get; private set; }
public Dictionary<EWeaponType, WeaponSettingData> WeaponSettingDict { get; private set; }
```

- [ ] **Step 2: `LoadAllSettingDataAsync()`에 로드 블록 추가**

`try` 블록 안 `SoundSettingDict` 로드 코드 다음에 아래를 추가한다:

```csharp
TextAsset soundJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("SoundSettingData");
SoundSettingDict = JsonConvert.DeserializeObject<Dictionary<string, SoundSettingData>>(soundJson.text);

TextAsset weaponJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("WeaponSettingData");
WeaponSettingDict = JsonConvert.DeserializeObject<Dictionary<EWeaponType, WeaponSettingData>>(weaponJson.text);
```

- [ ] **Step 3: 임시 검증 로그 추가**

같은 `try` 블록 맨 끝, catch 직전에 임시 검증 코드를 삽입:

```csharp
// [TEMP VERIFY - Task 4 Step 6에서 제거]
if (WeaponSettingDict != null && WeaponSettingDict.TryGetValue(EWeaponType.OneHandSword, out var oneHand))
{
    Debug.Log($"[WeaponSettingData] OneHandSword 콤보 수: {oneHand.ComboList.Count}");
    foreach (var combo in oneHand.ComboList)
    {
        Debug.Log($"  - {combo.AnimKey} | Dmg x{combo.DamageMultiplier} | Range {combo.HitRange} | Offset {combo.HitOffset} | KB {combo.Knockback}");
    }
}
else
{
    Debug.LogError("[WeaponSettingData] OneHandSword 데이터 로드 실패!");
}
```

- [ ] **Step 4: Unity 에디터에서 컴파일 확인**

Unity 에디터 포커스 → 자동 컴파일. Console에 에러 없어야 함.
Expected: `No errors`

- [ ] **Step 5: Play 모드에서 런타임 검증**

Unity 에디터에서 `▶` 버튼으로 Play 모드 진입. `SettingData.LoadAllSettingDataAsync()`가 호출되는 씬 플로우가 존재해야 검증 가능 — 현재 `Main` 초기화 시점에 호출되는지 확인 후, 호출되지 않는다면 타이틀/로딩 씬에서 수동 트리거.

Expected Console 출력:
```
[WeaponSettingData] OneHandSword 콤보 수: 2
  - Attack_OneHand1 | Dmg x1 | Range 1.5 | Offset 1.2 | KB 2
  - Attack_OneHand2 | Dmg x1.5 | Range 1.8 | Offset 1.4 | KB 4
```

만약 출력이 안 나오면:
- "데이터 로딩 실패" 로그가 있는지 확인 → 있으면 Addressable 키 오타 혹은 그룹 미등록 의심 (Task 3 재점검)
- 로그 자체가 전혀 안 찍히면 `LoadAllSettingDataAsync()`가 호출되는 진입점이 없다는 뜻 → 본 플랜 범위 외이므로 사용자에게 보고하고 수동 트리거 요청

- [ ] **Step 6: 검증 로그 제거**

Step 3에서 추가한 `[TEMP VERIFY - Task 4 Step 6에서 제거]` 블록을 전부 제거. 최종 `SettingData.cs`는 프로퍼티 추가 + 로드 블록 추가만 남는다.

- [ ] **Step 7: 최종 컴파일 확인**

Unity 에디터 포커스 → 자동 컴파일. Console 에러 없어야 함.
Expected: `No errors`

- [ ] **Step 8: 커밋**

```bash
git add "Assets/02. Scripts/Data/SettingData/SettingData.cs"
git commit -m "feat: SettingData에 WeaponSettingDict 로드 추가"
```

---

## Task 5: 작업요청 md 아카이브 이동 및 마무리

**Files:**
- Move: `MortalSoulDoc/02. InProgress/WeaponSettingData 구축.md` → `MortalSoulDoc/03. Archive/WeaponSettingData 구축.md`
- Modify: (이동된 파일)의 `작업 내용` / `특이사항` / `태그` 섹션

- [ ] **Step 1: 사용자 승인 요청**

모든 코드/데이터 작업이 완료되었음을 사용자에게 보고하고 아카이브 이동 승인을 요청한다. (CLAUDE.md Work Flow 규칙 3)

- [ ] **Step 2: 승인 후 파일 이동**

```bash
mv "MortalSoulDoc/02. InProgress/WeaponSettingData 구축.md" "MortalSoulDoc/03. Archive/WeaponSettingData 구축.md"
```

- [ ] **Step 3: 작업 내용 / 특이사항 / 태그 채우기**

이동된 md 파일의 하단 섹션을 아래 형식으로 편집:

```markdown
### 작업 내용
1. `WeaponSettingData.cs`에 `WeaponSettingData` / `AttackComboData` POCO 클래스 추가
2. `Assets/04. Settings/SettingData/WeaponSettingData.json` 신규 생성 (OneHandSword 2콤보)
3. Addressables `SettingData` 그룹에 `"WeaponSettingData"` 키로 등록
4. `SettingData.cs`에 `WeaponSettingDict` 프로퍼티 + `LoadAllSettingDataAsync()` 로드 블록 추가
5. Unity 런타임 검증 완료 (Debug.Log로 콤보 데이터 정상 조회 확인 후 임시 코드 제거)


### 특이사항
1. `AttackComboData.AnimKey`는 현재 `"Attack_OneHand1"` / `"Attack_OneHand2"` 플레이스홀더 — 실제 Spine 캐릭터 세팅 후 실제 애니메이션 이름으로 교체 필요
2. 콤보는 리소스 한계상 최대 2개로 고정
3. 본 작업은 데이터 로딩까지만 범위 — WSC가 이 데이터를 소비하는 로직은 별도 작업
4. `Dictionary<EWeaponType, WeaponSettingData>`의 키는 Newtonsoft 기본 동작으로 enum 이름 문자열("OneHandSword")로 직/역직렬화되므로 `StringEnumConverter` 불필요


---
태그 : #데이터 #SettingData #기본공격 #WeaponSettingData #Addressables
```

- [ ] **Step 4: 커밋 + 푸시**

```bash
git add "MortalSoulDoc/"
git commit -m "docs: WeaponSettingData 구축 작업 아카이브"
git push
```

(자동 커밋-푸시 정책에 따라 승인 불필요)

---

## Spec Coverage Self-Review

| Spec 요구사항 | 구현 태스크 |
|---|---|
| `WeaponSettingData` / `AttackComboData` 클래스 정의 | Task 1 |
| OneHandSword 2콤보 JSON 초기 데이터 | Task 2 |
| Addressables 키 `"WeaponSettingData"` 등록 | Task 3 |
| `SettingData.WeaponSettingDict` 프로퍼티 | Task 4 Step 1 |
| `LoadAllSettingDataAsync()` 로드 블록 | Task 4 Step 2 |
| 런타임 로드 검증 | Task 4 Step 3~7 |
| `StringEnumConverter` 불필요 확인 | Task 4에서 코드상 어트리뷰트 없이 동작 → 명시 검증 |
| 기존 `MS.Data` 네임스페이스 준수 | Task 1 코드 블록 |
| `[Serializable]` 부착으로 기존 컨벤션 통일 | Task 1 코드 블록 |
| 작업요청 md 아카이브 이동 | Task 5 |

모든 스펙 항목이 태스크에 매핑됨. 누락 없음.
