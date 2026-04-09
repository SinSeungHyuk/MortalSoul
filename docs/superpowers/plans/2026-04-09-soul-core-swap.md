# 소울 코어 스왑 시스템 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 소울 스왑 파이프라인 구현 — 메인/서브 소울 교체 시 스탯, 체력, 스킨, 무기, 스킬 활성 슬롯이 전환되는 시스템

**Architecture:** PlayerCharacter가 오케스트레이터로 SwapSoul()을 호출. PlayerSoulController(PSC)가 소울 데이터/가드/활성 스킬키를 관리하고, 기존 시스템(WSC/SSC/SpineController/AttributeSet)은 각자 영역만 처리. 스킬은 4개 상주형으로 쿨타임 동시 진행.

**Tech Stack:** Unity 6, C#, UniTask, Spine

**Spec:** `docs/superpowers/specs/2026-04-09-soul-core-swap-design.md`

---

## 파일 구조

| 파일 | 변경 | 역할 |
|---|---|---|
| `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerSoulController.cs` | 신규 | 소울 슬롯 데이터, 가드 체크, 활성 스킬키 제공 |
| `Assets/02. Scripts/Battle/Stat.cs` | 수정 | `SetBaseValue()` 메서드 추가 |
| `Assets/02. Scripts/Battle/AttributeSet/PlayerAttributeSet.cs` | 수정 | `SwapBaseValues()` 메서드 추가 |
| `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerMovementController.cs` | 수정 | `TransitToIdle()`, `OnPrevious()` 추가 |
| `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs` | 수정 | PSC 생성, `SwapSoul()`, `AcquireSoul()` 추가, InitPlayer 시그니처 유지 |
| `Assets/02. Scripts/Test/Test.cs` | 수정 | 테스트용 2소울 지급 |
| `CLAUDE.md` | 수정 | 액션 캔슬 규칙 업데이트 |

---

### Task 1: Stat.SetBaseValue 추가

**Files:**
- Modify: `Assets/02. Scripts/Battle/Stat.cs:43-58`

- [ ] **Step 1: Stat에 SetBaseValue 메서드 추가**

`Assets/02. Scripts/Battle/Stat.cs` — `AddBaseValue` 메서드(55행) 아래에 추가:

```csharp
public void SetBaseValue(float _value)
{
    baseValue = _value;
    OnValueChanged?.Invoke(Value);
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Battle/Stat.cs"
git commit -m "Stat.SetBaseValue 추가"
```

---

### Task 2: PlayerAttributeSet.SwapBaseValues 추가

**Files:**
- Modify: `Assets/02. Scripts/Battle/AttributeSet/PlayerAttributeSet.cs:15-42`

- [ ] **Step 1: SwapBaseValues 메서드 추가**

`Assets/02. Scripts/Battle/AttributeSet/PlayerAttributeSet.cs` — `InitPlayerAttributeSet` 메서드(42행) 아래에 추가:

```csharp
public void SwapBaseValues(PlayerAttributeSetSettingData _data)
{
    MaxHealth.SetBaseValue(_data.MaxHealth);
    BaseAttackPower.SetBaseValue(_data.BaseAttackPower);
    SkillAttackPower.SetBaseValue(_data.SkillAttackPower);
    Defense.SetBaseValue(_data.Defense);
    MoveSpeed.SetBaseValue(_data.MoveSpeed);
    CriticChance.SetBaseValue(_data.CriticChance);
    CriticMultiple.SetBaseValue(_data.CriticMultiple);
    Evasion.SetBaseValue(_data.Evasion);
    LifeSteal.SetBaseValue(_data.LifeSteal);
    CooltimeAccel.SetBaseValue(_data.CooltimeAccel);
    AttackSpeed.SetBaseValue(_data.AttackSpeed);
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Battle/AttributeSet/PlayerAttributeSet.cs"
git commit -m "PlayerAttributeSet.SwapBaseValues 추가"
```

---

### Task 3: PlayerSoulController 신규 생성

**Files:**
- Create: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerSoulController.cs`

- [ ] **Step 1: PSC 클래스 작성**

```csharp
using Core;
using MS.Data;
using System;
using System.Collections.Generic;

namespace MS.Field
{
    public class PlayerSoulController
    {
        public string MainSoulKey { get; private set; }
        public string SubSoulKey { get; private set; }

        public event Action OnSoulSwapped;

        private PlayerCharacter owner;
        private float subSoulHealth;

        public void InitPSC(PlayerCharacter _owner, string _mainSoulKey)
        {
            owner = _owner;
            MainSoulKey = _mainSoulKey;
            SubSoulKey = null;
            subSoulHealth = 0f;
        }

        public bool CanSwap()
        {
            if (SubSoulKey == null) return false;
            if (owner.BSC.AttributeSet.Health <= 0f) return false;
            return true;
        }

        public float SwapSlots(float _curHealth)
        {
            (MainSoulKey, SubSoulKey) = (SubSoulKey, MainSoulKey);

            float restoredHealth = subSoulHealth;
            subSoulHealth = _curHealth;

            return restoredHealth;
        }

        public void SetSubSoul(string _soulKey)
        {
            SubSoulKey = _soulKey;
        }

        public void InitSubSoulHealth(float _maxHealth)
        {
            subSoulHealth = _maxHealth;
        }

        public CharacterSettingData GetMainSoulData()
        {
            var dict = Main.Instance.DataManager.SettingData.CharacterSettingData.CharacterSettingDataDict;
            dict.TryGetValue(MainSoulKey, out CharacterSettingData data);
            return data;
        }

        public CharacterSettingData GetSubSoulData()
        {
            if (SubSoulKey == null) return null;
            var dict = Main.Instance.DataManager.SettingData.CharacterSettingData.CharacterSettingDataDict;
            dict.TryGetValue(SubSoulKey, out CharacterSettingData data);
            return data;
        }

        public List<string> GetActiveSkillKeys()
        {
            var data = GetMainSoulData();
            if (data?.SkillKeys == null) return new List<string>();
            return new List<string>(data.SkillKeys);
        }

        public void InvokeOnSoulSwapped()
        {
            OnSoulSwapped?.Invoke();
        }
    }
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerSoulController.cs"
git commit -m "PlayerSoulController 신규 생성"
```

---

### Task 4: PlayerMovementController에 TransitToIdle, OnPrevious 추가

**Files:**
- Modify: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerMovementController.cs`

- [ ] **Step 1: TransitToIdle 메서드 추가**

`PlayerMovementController.cs` — `OnAttackEndedCallback` 메서드(77행) 아래에 추가:

```csharp
public void TransitToIdle()
{
    stateMachine.TransitState((int)EMoveState.Idle);
}
```

- [ ] **Step 2: OnPrevious 입력 핸들러 추가**

`PlayerMovementController.cs` — `OnAttack` 메서드(337행) 아래에 추가:

```csharp
public void OnPrevious(InputValue _value)
{
    if (_value.isPressed)
        player.SwapSoul();
}
```

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerMovementController.cs"
git commit -m "PMC에 TransitToIdle, OnPrevious 입력 핸들러 추가"
```

---

### Task 5: PlayerCharacter에 PSC 통합, SwapSoul, AcquireSoul 구현

**Files:**
- Modify: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs`

- [ ] **Step 1: PSC 필드 추가 및 InitPlayer에서 PSC 초기화**

`PlayerCharacter.cs` 수정 — PSC 필드를 추가하고, InitPlayer에서 PSC 초기화 + 메인 소울 스킬 등록을 추가한다.

수정 후 전체 코드:

```csharp
using System;
using Core;
using Cysharp.Threading.Tasks;
using MS.Battle;
using MS.Data;
using UnityEngine;

namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        private PlayerMovementController pmc;
        private PlayerSoulController psc;

        public PlayerMovementController PMC => pmc;
        public PlayerSoulController PSC => psc;


        protected override void Awake()
        {
            base.Awake();
            pmc = GetComponent<PlayerMovementController>();
        }

        private void Start()
        {
            InitTestAsync().Forget();
        }

        private async UniTaskVoid InitTestAsync()
        {
            await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
            InitPlayer("test");
        }

        public void InitPlayer(string _mainSoulKey)
        {
            psc = new PlayerSoulController();
            psc.InitPSC(this, _mainSoulKey);

            var mainData = psc.GetMainSoulData();
            if (mainData == null)
            {
                Debug.LogError($"[PlayerCharacter] CharacterSettingData 없음: {_mainSoulKey}");
                return;
            }

            var playerAttributeSet = new PlayerAttributeSet();
            playerAttributeSet.InitPlayerAttributeSet(mainData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, playerAttributeSet, mainData.WeaponType);

            if (mainData.SkillKeys != null)
            {
                foreach (var skillKey in mainData.SkillKeys)
                    BSC.SSC.GiveSkill(skillKey);
            }

            if (mainData.SkinKeys != null && mainData.SkinKeys.Count > 0)
                SpineController.SetCombinedSkin(mainData.SkinKeys);

            pmc.InitController(BSC.WSC);
        }

        public void AcquireSoul(string _soulKey)
        {
            if (psc.SubSoulKey != null) return;

            psc.SetSubSoul(_soulKey);

            var subData = psc.GetSubSoulData();
            if (subData == null) return;

            if (subData.SkillKeys != null)
            {
                foreach (var skillKey in subData.SkillKeys)
                    BSC.SSC.GiveSkill(skillKey);
            }

            psc.InitSubSoulHealth(subData.AttributeSetSettingData.MaxHealth);
        }

        public void SwapSoul()
        {
            if (!psc.CanSwap()) return;

            BSC.SSC.CancelAllSkills();

            var attrSet = (PlayerAttributeSet)BSC.AttributeSet;
            float restoredHealth = psc.SwapSlots(attrSet.Health);

            var newSoulData = psc.GetMainSoulData();

            attrSet.SwapBaseValues(newSoulData.AttributeSetSettingData);
            attrSet.Health = Mathf.Min(restoredHealth, attrSet.MaxHealth.Value);

            BSC.WSC.ChangeWeaponType(newSoulData.WeaponType);
            SpineController.SetCombinedSkin(newSoulData.SkinKeys);

            pmc.TransitToIdle();
            psc.InvokeOnSoulSwapped();
        }
    }
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs"
git commit -m "PlayerCharacter에 PSC 통합, SwapSoul, AcquireSoul 구현"
```

---

### Task 6: Test.cs에서 2소울 테스트 환경 구성

**Files:**
- Modify: `Assets/02. Scripts/Test/Test.cs`

- [ ] **Step 1: Test.cs 수정**

테스트용으로 게임 시작 직후 서브 소울을 획득하도록 수정. `Test.cs`는 PlayerCharacter와 같은 씬에 배치된 MonoBehaviour이므로 PlayerManager를 통해 접근한다.

```csharp
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        InitTestSoulAsync().Forget();
    }

    private async UniTaskVoid InitTestSoulAsync()
    {
        await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
        await UniTask.DelayFrame(1);

        var player = Main.Instance.PlayerManager.CurPlayer;
        if (player == null) return;

        player.AcquireSoul("test2");
        Debug.Log("[Test] 서브 소울 'test2' 지급 완료");
    }
}
```

> **주의**: `"test2"` 키가 `CharacterSettingData.json`에 존재해야 한다. 기존 `"test"` 데이터를 복제하여 다른 WeaponType/SkinKeys/스탯을 부여해 스왑 시 차이를 확인할 수 있도록 한다.

- [ ] **Step 2: CharacterSettingData.json에 test2 소울 데이터 추가**

`Assets/04. Settings/SettingData/CharacterSettingData.json`에 `"test2"` 항목을 추가한다. 기존 `"test"` 데이터를 복제하되, 구분 가능하도록 WeaponType과 스탯 일부를 다르게 설정한다. 현재 JSON 구조를 읽고 `"test"` 엔트리를 참조하여 `"test2"`를 추가.

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/Test/Test.cs" "Assets/04. Settings/SettingData/CharacterSettingData.json"
git commit -m "Test.cs에서 2소울 테스트 환경 구성"
```

---

### Task 7: CLAUDE.md 액션 캔슬 규칙 업데이트

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 1: 소울 스위칭 관련 규칙 업데이트**

CLAUDE.md에서 아래 항목들을 수정:

1. **소울 시스템 > 소울 스위칭** 섹션:
   - "**공격/콤보 진행 중에는 소울 스위칭 불가** — WSC의 '공격 진행 중' 플래그로 차단" → "사망/서브 슬롯 비어있음 외 **모든 상태에서 소울 스위칭 가능**. 스왑 시 진행 중인 공격/스킬은 캔슬되고 Idle 상태로 전환"

2. **조작 규칙** 섹션에 추가:
   - "소울 스위칭 시 현재 상태(공격/스킬/대시/점프 등)와 무관하게 즉시 교체되며, 교체 완료 후 Idle 상태로 전환된다."

- [ ] **Step 2: 커밋**

```bash
git add CLAUDE.md
git commit -m "CLAUDE.md 소울 스위칭 캔슬 규칙 업데이트"
```

---

### Task 8: 통합 테스트 (Unity 에디터)

- [ ] **Step 1: Unity 에디터에서 플레이**

1. 씬 진입 후 `"test"` 소울로 초기화되는지 확인 (콘솔 로그)
2. `Test.cs`에서 `"test2"` 서브 소울이 지급되는지 확인 (콘솔 로그)
3. `Previous` 키 입력 시:
   - Idle 상태로 전환되는지
   - 스킨이 변경되는지
   - 무기가 변경되는지 (콤보 패턴이 달라지는지)
   - 체력이 소울별로 독립적으로 유지되는지
4. 공격/대시/점프 중 스위칭 시 모션이 즉시 캔슬되고 Idle로 돌아가는지
5. 서브 슬롯이 비어있을 때 (AcquireSoul 전) 스위칭 시도 시 아무 일도 안 일어나는지

- [ ] **Step 2: 최종 커밋**

문제가 있다면 수정 후 커밋. 문제 없다면 이 단계는 스킵.
