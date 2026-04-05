# Skill Async System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 스킬 시스템을 동기 패턴에서 UniTask 기반 비동기 패턴으로 전환하여 캐스팅/취소/쿨타임가속/데이터연동을 지원한다.

**Architecture:** SSC 중심 확장. BaseSkill을 UniTask+CancellationToken 비동기로 변환하고, SSC에 리플렉션 GiveSkill + runningSkillDict 취소 관리를 추가한다. BSC→SSC 위임 구조는 유지.

**Tech Stack:** Unity 6, UniTask (Cysharp.Threading.Tasks), Newtonsoft.Json, Addressables

---

## File Map

| 파일 | 작업 | 역할 |
|------|------|------|
| `Assets/02. Scripts/Utils/MathUtils.cs` | 신규 | 전투 수학 유틸 (BattleScaling, DecreaseByPercent, IsSuccess) |
| `Assets/02. Scripts/Utils/Settings.cs` | 수정 | AnimHash 상수 추가 |
| `Assets/02. Scripts/Battle/DamageInfo.cs` | 수정 | SourceSkill 필드 추가 |
| `Assets/02. Scripts/Battle/Skill/BaseSkill.cs` | 전면 수정 | 비동기 ActivateSkill, SkillSettingData 연동, 쿨타임 카운트다운 |
| `Assets/02. Scripts/Battle/SkillSystemComponent.cs` | 전면 수정 | 리플렉션 GiveSkill, 비동기 UseSkill, CTS 취소 관리 |
| `Assets/02. Scripts/Battle/BattleSystemComponent.cs` | 수정 | UseSkill async 시그니처 변경 |
| `Assets/02. Scripts/Battle/Skill/TestOneHandAttack.cs` | 수정 | async 시그니처 마이그레이션 |
| `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs` | 수정 | GiveSkill 호출 변경 |

---

### Task 1: MathUtils 유틸 클래스 생성

**Files:**
- Create: `Assets/02. Scripts/Utils/MathUtils.cs`

- [ ] **Step 1: MathUtils.cs 생성**

```csharp
using UnityEngine;

namespace MS.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// 전투 스케일링 공식. value가 클수록 효과가 커지지만 체감 증가량은 감소.
        /// 100 → 50%, 200 → 66.7%, 300 → 75%
        /// </summary>
        public static float BattleScaling(float value)
        {
            return value / (value + Settings.BattleScalingConstant) * 100f;
        }

        /// <summary>
        /// 값을 퍼센트만큼 감소. DecreaseByPercent(100, 30) → 70
        /// </summary>
        public static float DecreaseByPercent(float value, float percent)
        {
            return value * (1f - (percent * 0.01f));
        }

        /// <summary>
        /// 확률 판정. percent가 0~100 범위.
        /// </summary>
        public static bool IsSuccess(float percent)
        {
            float chance = Random.Range(0f, 100f);
            return percent >= chance;
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Utils/MathUtils.cs"
git commit -m "feat: MathUtils 유틸 클래스 생성 (BattleScaling, DecreaseByPercent, IsSuccess)"
```

---

### Task 2: Settings AnimHash 상수 추가

**Files:**
- Modify: `Assets/02. Scripts/Utils/Settings.cs`

- [ ] **Step 1: ANIMATOR HASH 리전 추가**

`Settings.cs`의 COLOR SETTING 리전 뒤에 추가:

```csharp
#region ANIMATOR HASH
public static readonly int AnimHashCasting = Animator.StringToHash("Casting");
#endregion
```

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Utils/Settings.cs"
git commit -m "feat: Settings에 AnimHashCasting 상수 추가"
```

---

### Task 3: DamageInfo에 SourceSkill 필드 추가

**Files:**
- Modify: `Assets/02. Scripts/Battle/DamageInfo.cs`

- [ ] **Step 1: SourceSkill 필드 추가**

`DamageInfo` struct에 필드 추가:

```csharp
public struct DamageInfo
{
    public FieldCharacter Attacker;
    public FieldCharacter Target;
    public EDamageAttributeType AttributeType;
    public float Damage;
    public bool IsCritic;
    public float KnockbackForce;
    public BaseSkill SourceSkill;
}
```

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Battle/DamageInfo.cs"
git commit -m "feat: DamageInfo에 SourceSkill 필드 추가"
```

---

### Task 4: BaseSkill 비동기 전환

이 태스크가 핵심. 동기 BaseSkill을 UniTask 기반 비동기로 전면 교체한다.

**Files:**
- Modify: `Assets/02. Scripts/Battle/Skill/BaseSkill.cs`

**주요 변경:**
- `InitSkill(SSC, float)` → `InitSkill(SSC, SkillSettingData)`
- `abstract void ActivateSkill()` → `abstract UniTask ActivateSkill(CancellationToken)`
- 쿨타임: 카운트업 → 카운트다운 방식
- `SetCooltime()`: CooltimeAccel 스탯 반영
- `SetSkillCasting()`: 비동기 캐스팅 메서드 추가

- [ ] **Step 1: BaseSkill.cs 전면 교체**

```csharp
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Utils;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public abstract class BaseSkill
    {
        protected SkillSystemComponent ownerSSC;
        protected FieldCharacter owner;
        protected BaseAttributeSet attributeSet;
        protected SkillSettingData skillData;

        private float curCooltime;
        private float elapsedCooltime;

        public bool IsCooltime => elapsedCooltime > 0;
        public float CooltimeRatio => curCooltime > 0 ? elapsedCooltime / curCooltime : 0f;
        public bool IsPostUseCooltime => skillData.IsPostUseCooltime;


        public virtual void InitSkill(SkillSystemComponent _ownerSSC, SkillSettingData _skillData)
        {
            ownerSSC = _ownerSSC;
            owner = _ownerSSC.Owner;
            attributeSet = _ownerSSC.AttributeSet;
            skillData = _skillData;

            curCooltime = _skillData.Cooltime;
            elapsedCooltime = 0;
        }

        public abstract UniTask ActivateSkill(CancellationToken token);

        public virtual bool CanActivateSkill() => true;

        public void SetCooltime()
        {
            float cooltime = skillData.Cooltime;
            float cooltimeAccel = attributeSet.GetStatValueByType(EStatType.CooltimeAccel);
            if (cooltimeAccel > 0)
            {
                float cooltimePercent = MathUtils.BattleScaling(cooltimeAccel);
                cooltime = MathUtils.DecreaseByPercent(cooltime, cooltimePercent);
            }

            curCooltime = cooltime;
            elapsedCooltime = cooltime;
        }

        public async UniTask SetSkillCasting(CancellationToken token)
        {
            // TODO: FieldCharacter에 Animator/Spine 애니메이션 인터페이스 연결 시 활성화
            // owner.Animator.SetBool(Settings.AnimHashCasting, true);
            await UniTask.WaitForSeconds(skillData.GetValue(ESkillValueType.Casting), cancellationToken: token);
            // owner.Animator.SetBool(Settings.AnimHashCasting, false);
        }

        public void OnUpdate(float _deltaTime)
        {
            if (elapsedCooltime > 0)
                elapsedCooltime -= _deltaTime;
        }
    }
}
```

**참고:** `SetSkillCasting`의 Animator 호출은 현재 FieldCharacter에 Animator 프로퍼티가 없으므로 주석 처리. 비동기 대기 로직은 동작함. Spine 애니메이션 연동 시 활성화 예정.

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Battle/Skill/BaseSkill.cs"
git commit -m "feat: BaseSkill 비동기 전환 (UniTask+CancellationToken, SkillSettingData 연동)"
```

---

### Task 5: SkillSystemComponent 비동기 전환

SSC의 핵심 로직 전면 교체: 리플렉션 GiveSkill, 비동기 UseSkill, CTS 취소 관리.

**Files:**
- Modify: `Assets/02. Scripts/Battle/SkillSystemComponent.cs`

**주요 변경:**
- `GiveSkill(string, BaseSkill, float)` → `GiveSkill(string)` (리플렉션 + DataManager)
- `bool UseSkill(string)` → `async UniTask UseSkill(string)` (CTS + IsPostUseCooltime 분기)
- `runningSkillDict` 추가 (실행 중 스킬 관리)
- `CancelSkill`, `CancelAllSkills`, `IsRunningSkill` 추가

- [ ] **Step 1: SkillSystemComponent.cs 전면 교체**

```csharp
using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public class SkillSystemComponent
    {
        public FieldCharacter Owner { get; private set; }
        public BaseAttributeSet AttributeSet { get; private set; }

        private Dictionary<string, BaseSkill> ownedSkillDict;
        private Dictionary<string, CancellationTokenSource> runningSkillDict;

        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<string> OnSkillUsed;


        public void InitSSC(FieldCharacter _owner, BaseAttributeSet _attributeSet)
        {
            Owner = _owner;
            AttributeSet = _attributeSet;
            ownedSkillDict = new Dictionary<string, BaseSkill>();
            runningSkillDict = new Dictionary<string, CancellationTokenSource>();
        }

        public void GiveSkill(string _skillKey)
        {
            if (ownedSkillDict.ContainsKey(_skillKey))
                return;

            Type skillType = Type.GetType("MS.Battle." + _skillKey);
            if (skillType == null)
            {
                Debug.LogError($"[SSC] 스킬 타입을 찾을 수 없음: MS.Battle.{_skillKey}");
                return;
            }

            BaseSkill skillInstance = Activator.CreateInstance(skillType) as BaseSkill;
            if (skillInstance == null)
            {
                Debug.LogError($"[SSC] 스킬 인스턴스 생성 실패: {_skillKey}");
                return;
            }

            SkillSettingData skillData = Main.Instance.DataManager.SettingData.SkillSettingDict[_skillKey];
            skillInstance.InitSkill(this, skillData);
            ownedSkillDict.Add(_skillKey, skillInstance);
            OnSkillAdded?.Invoke(_skillKey, skillInstance);
        }

        public async UniTask UseSkill(string _skillKey)
        {
            if (!ownedSkillDict.TryGetValue(_skillKey, out BaseSkill skillToUse)) return;
            if (skillToUse.IsCooltime) return;
            if (!skillToUse.CanActivateSkill()) return;
            if (runningSkillDict.ContainsKey(_skillKey)) return;

            CancellationTokenSource cts = new CancellationTokenSource();
            runningSkillDict[_skillKey] = cts;

            try
            {
                if (!skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();
                await skillToUse.ActivateSkill(cts.Token);
                OnSkillUsed?.Invoke(_skillKey);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[SSC] {_skillKey} 스킬 캔슬");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SSC] {_skillKey} 스킬 사용 중 에러: {e.Message}");
            }
            finally
            {
                if (skillToUse.IsPostUseCooltime) skillToUse.SetCooltime();

                if (runningSkillDict.ContainsKey(_skillKey))
                {
                    runningSkillDict.Remove(_skillKey);
                    cts.Dispose();
                }
            }
        }

        public void CancelSkill(string _skillKey)
        {
            if (runningSkillDict.TryGetValue(_skillKey, out CancellationTokenSource cts))
                cts.Cancel();
        }

        public void CancelAllSkills()
        {
            foreach (var cts in runningSkillDict.Values)
                cts.Cancel();
        }

        public bool IsRunningSkill(string _skillKey)
        {
            return runningSkillDict.ContainsKey(_skillKey);
        }

        public bool IsCooltime(string _key)
        {
            if (ownedSkillDict.TryGetValue(_key, out BaseSkill skill))
                return skill.IsCooltime;
            return false;
        }

        public bool HasSkill(string _key)
        {
            return ownedSkillDict.ContainsKey(_key);
        }

        public void ClearSSC()
        {
            CancelAllSkills();
            ownedSkillDict.Clear();
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (var skill in ownedSkillDict.Values)
            {
                skill.OnUpdate(_deltaTime);
            }
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Battle/SkillSystemComponent.cs"
git commit -m "feat: SSC 비동기 전환 (리플렉션 GiveSkill, async UseSkill, CTS 취소 관리)"
```

---

### Task 6: BattleSystemComponent UseSkill async 변경

**Files:**
- Modify: `Assets/02. Scripts/Battle/BattleSystemComponent.cs`

- [ ] **Step 1: UseSkill 메서드 수정**

`BattleSystemComponent.cs`에서 UseSkill 메서드를 async로 변경:

변경 전:
```csharp
public bool UseSkill(string _key)
{
    // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
    // 현재는 항상 통과 — 추후 구현
    return SSC.UseSkill(_key);
}
```

변경 후:
```csharp
public async UniTask UseSkill(string _key)
{
    // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
    // 현재는 항상 통과 — 추후 구현
    await SSC.UseSkill(_key);
}
```

파일 상단에 `using Cysharp.Threading.Tasks;` 추가 필요.

- [ ] **Step 2: Commit**

```bash
git add "Assets/02. Scripts/Battle/BattleSystemComponent.cs"
git commit -m "feat: BSC UseSkill async UniTask 시그니처 변경"
```

---

### Task 7: TestOneHandAttack 마이그레이션 + PlayerCharacter 호출 변경

기존 테스트 스킬과 호출부를 새 시그니처에 맞춰 수정.

**Files:**
- Modify: `Assets/02. Scripts/Battle/Skill/TestOneHandAttack.cs`
- Modify: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs`

- [ ] **Step 1: TestOneHandAttack.cs async 마이그레이션**

```csharp
using Cysharp.Threading.Tasks;
using MS.Field;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public class TestOneHandAttack : BaseSkill
    {
        public override async UniTask ActivateSkill(CancellationToken token)
        {
            Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.AttackPower.Value}");

            if (owner is PlayerCharacter player && player.SpineComponent != null)
            {
                player.SpineComponent.OnAttackOneHand();
            }

            await UniTask.CompletedTask;
        }
    }
}
```

- [ ] **Step 2: PlayerCharacter.cs GiveSkill 호출 변경**

`PlayerCharacter.cs` Awake()에서 GiveSkill 호출 변경:

변경 전:
```csharp
BSC.SSC.GiveSkill("TestOneHandAttack", new TestOneHandAttack(), 1f);
```

변경 후:
```csharp
BSC.SSC.GiveSkill("TestOneHandAttack");
```

**중요:** 리플렉션 GiveSkill은 `Main.Instance.DataManager.SettingData.SkillSettingDict["TestOneHandAttack"]`에서 데이터를 조회한다. 현재 SkillSettingData JSON에 "TestOneHandAttack" 키가 없으면 런타임 에러가 발생한다. JSON 데이터 파일이 아직 없는 상태라면, PlayerCharacter의 GiveSkill 호출을 임시로 주석 처리하거나, 테스트용 JSON 데이터를 추가해야 한다. 구현 시 DataManager 로딩 상태를 확인하고 적절히 처리할 것.

- [ ] **Step 3: Commit**

```bash
git add "Assets/02. Scripts/Battle/Skill/TestOneHandAttack.cs" "Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs"
git commit -m "feat: TestOneHandAttack async 마이그레이션 + PlayerCharacter GiveSkill 호출 변경"
```

---

### Task 8: 컴파일 검증 및 최종 정리

**Files:** 모든 수정 파일

- [ ] **Step 1: Unity 컴파일 확인**

Unity Editor에서 프로젝트를 열고 Console에서 컴파일 에러가 없는지 확인한다.

**확인 항목:**
- `using Cysharp.Threading.Tasks;` — UniTask 패키지가 설치되어 있는지
- `using MS.Data;` — SkillSettingData 네임스페이스 참조
- `using MS.Utils;` — MathUtils, Settings 참조
- `Type.GetType("MS.Battle.TestOneHandAttack")` — 리플렉션 타입 해석 가능한지
- `Main.Instance.DataManager.SettingData.SkillSettingDict` — 프로퍼티 체인 정상 접근

- [ ] **Step 2: 컴파일 에러 수정 (필요 시)**

에러 발생 시 원인 파악 후 수정. 일반적으로 예상되는 에러:
- `using` 누락
- SkillSettingData JSON 미존재로 인한 null 참조 (런타임)
- 다른 파일에서 이전 GiveSkill 시그니처를 호출하는 경우

- [ ] **Step 3: 최종 Commit**

```bash
git add -A
git commit -m "fix: 스킬 비동기 시스템 컴파일 에러 수정"
```

---

## Verification

1. **컴파일 확인**: Unity Console에 에러 없음
2. **리플렉션 테스트**: `Type.GetType("MS.Battle.TestOneHandAttack")`가 null이 아닌지 확인 (Debug.Log로 검증)
3. **UseSkill 비동기**: 스킬 사용 후 쿨타임 정상 적용, `IsCooltime`이 true 반환 확인
4. **CancelAllSkills**: 실행 중 스킬의 CancellationToken이 정상 취소되는지 확인
5. **CooltimeAccel**: CooltimeAccel 스탯에 50을 넣으면 `BattleScaling(50) = 33.3%` → 쿨타임이 33.3% 감소하는지 확인
