# Skill Async System Design Spec

## Context

현재 스킬 시스템(BaseSkill, SSC)은 동기 함수(`void ActivateSkill()`)로 구현되어 있어, 캐스팅 대기/취소/비동기 스킬 로직을 지원하지 못한다. 레퍼런스 코드의 UniTask 기반 비동기 패턴으로 전환하여, 실제 게임에서 필요한 스킬 사용 흐름(비동기 활성화, 실행 중 취소, 쿨타임 가속, 데이터 연동)을 구현한다.

## Scope

**포함:**
- BaseSkill 비동기화 (UniTask + CancellationToken)
- SSC 리플렉션 GiveSkill + 비동기 UseSkill + 실행 중 스킬 취소 관리
- BSC UseSkill 시그니처 async 변경
- DamageInfo에 sourceSkill 필드 추가
- MathUtils 유틸 클래스 신규 생성
- Settings에 AnimHash 상수 추가

**제외:**
- TakeDamage 파이프라인 (별도 작업)
- 구체 스킬 구현 (FireBall, IceBall 등)
- SkillObjectManager, MonsterManager 구현
- DPS 추적 (totalDamageDealt, acquiredTime)

---

## 1. BaseSkill 변경

**파일:** `Assets/02. Scripts/Battle/Skill/BaseSkill.cs`

### 변경 사항

| 항목 | 변경 전 | 변경 후 |
|------|---------|---------|
| InitSkill 시그니처 | `(SSC, float cooltime)` | `(SSC, SkillSettingData)` |
| ActivateSkill | `abstract void` | `abstract UniTask (CancellationToken)` |
| 쿨타임 소스 | 생성자 파라미터 고정값 | SkillSettingData.Cooltime |
| SetCooltime | 단순 리셋 | CooltimeAccel 스탯 반영 |
| 캐스팅 | 없음 | SetSkillCasting 비동기 메서드 |

### 필드/프로퍼티

```csharp
protected SkillSystemComponent ownerSSC;
protected FieldCharacter owner;
protected BaseAttributeSet attributeSet;
protected SkillSettingData skillData;     // 신규: 데이터 참조

private float curCooltime;     // 변경: 매번 SetCooltime에서 재계산
private float elapsedCooltime; // 카운트다운 방식: cooltime에서 시작, 0이 되면 사용 가능

public bool IsCooltime => elapsedCooltime > 0;                 // 변경: 카운트다운
public float CooltimeRatio => curCooltime > 0 ? elapsedCooltime / curCooltime : 0f;  // 1.0=쿨중, 0.0=사용가능
public bool IsPostUseCooltime => skillData.IsPostUseCooltime;  // 신규
```

### InitSkill

```csharp
public virtual void InitSkill(SkillSystemComponent _ownerSSC, SkillSettingData _skillData)
{
    ownerSSC = _ownerSSC;
    owner = _ownerSSC.Owner;
    attributeSet = _ownerSSC.AttributeSet;
    skillData = _skillData;

    curCooltime = _skillData.Cooltime;
    elapsedCooltime = 0; // 즉시 사용 가능 (카운트다운 0 = 쿨타임 없음)
}
```

### ActivateSkill (시그니처 변경)

```csharp
public abstract UniTask ActivateSkill(CancellationToken token);
```

### SetCooltime (CooltimeAccel 반영)

```csharp
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
    elapsedCooltime = cooltime; // 카운트다운 시작
}
```

### SetSkillCasting (신규)

```csharp
public async UniTask SetSkillCasting(CancellationToken token)
{
    owner.Animator.SetBool(Settings.AnimHashCasting, true);
    await UniTask.WaitForSeconds(skillData.GetValue(ESkillValueType.Casting), cancellationToken: token);
    owner.Animator.SetBool(Settings.AnimHashCasting, false);
}
```

### OnUpdate (카운트다운 방식으로 변경)

```csharp
public void OnUpdate(float _deltaTime)
{
    if (elapsedCooltime > 0)
        elapsedCooltime -= _deltaTime;
}
```

### CanActivateSkill (변경 없음)

```csharp
public virtual bool CanActivateSkill() => true;
```

---

## 2. SkillSystemComponent 변경

**파일:** `Assets/02. Scripts/Battle/SkillSystemComponent.cs`

### 변경 사항

| 항목 | 변경 전 | 변경 후 |
|------|---------|---------|
| GiveSkill | 인스턴스 직접 전달 | 리플렉션 (`string key` → DataManager 조회) |
| UseSkill | 동기 `bool` | 비동기 `async UniTask` |
| 실행 중 스킬 관리 | 없음 | `runningSkillDict` + CTS |
| 스킬 취소 | 없음 | CancelSkill / CancelAllSkills |

### 필드 추가

```csharp
private Dictionary<string, CancellationTokenSource> runningSkillDict;
// InitSSC에서 초기화
```

### GiveSkill (리플렉션)

```csharp
public void GiveSkill(string _skillKey)
{
    if (ownedSkillDict.ContainsKey(_skillKey))
        return;

    Type skillType = Type.GetType("MS.Battle." + _skillKey);
    if (skillType == null)
    {
        Debug.LogError($"스킬 타입을 찾을 수 없음: MS.Battle.{_skillKey}");
        return;
    }

    BaseSkill skillInstance = Activator.CreateInstance(skillType) as BaseSkill;
    if (skillInstance == null)
    {
        Debug.LogError($"스킬 인스턴스 생성 실패: {_skillKey}");
        return;
    }

    // DataManager에서 SkillSettingData 조회
    SkillSettingData skillData = Main.Instance.DataManager.SettingData
        .SkillSettingData.GetData(_skillKey);

    skillInstance.InitSkill(this, skillData);
    ownedSkillDict.Add(_skillKey, skillInstance);
    OnSkillAdded?.Invoke(_skillKey, skillInstance);
}
```

**참고:** `SettingData.SkillSettingData.GetData(key)` 경로는 현재 DataManager 구조에 맞춤. 실제 메서드명은 구현 시 확인.

### UseSkill (비동기)

```csharp
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
        Debug.Log($"{_skillKey} 스킬 캔슬");
    }
    catch (Exception e)
    {
        Debug.LogError($"{_skillKey} 스킬 사용 중 에러: {e.Message}");
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
```

**IsPostUseCooltime 분기 설명:**
- `false` (기본): 스킬 시작 전에 쿨타임 적용. 스킬 실행 중에도 쿨타임이 진행됨.
- `true`: 스킬 완료/취소 후(finally)에 쿨타임 적용. 채널링/캐스팅 스킬에 적합.

### CancelSkill / CancelAllSkills (신규)

```csharp
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
```

### IsRunningSkill (신규)

```csharp
public bool IsRunningSkill(string _skillKey)
{
    return runningSkillDict.ContainsKey(_skillKey);
}
```

### ClearSSC (변경)

```csharp
public void ClearSSC()
{
    CancelAllSkills();
    ownedSkillDict.Clear();
}
```

---

## 3. BattleSystemComponent 변경

**파일:** `Assets/02. Scripts/Battle/BattleSystemComponent.cs`

### UseSkill 시그니처 변경

```csharp
// 변경 전
public bool UseSkill(string _key)
{
    return SSC.UseSkill(_key);
}

// 변경 후
public async UniTask UseSkill(string _key)
{
    // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
    // 현재는 항상 통과 — 추후 구현
    await SSC.UseSkill(_key);
}
```

---

## 4. DamageInfo 변경

**파일:** `Assets/02. Scripts/Battle/DamageInfo.cs`

### sourceSkill 필드 추가

```csharp
public struct DamageInfo
{
    public FieldCharacter Attacker;
    public FieldCharacter Target;
    public EDamageAttributeType AttributeType;
    public float Damage;
    public bool IsCritic;
    public float KnockbackForce;
    public BaseSkill SourceSkill;     // 신규: 데미지 소스 스킬 참조
}
```

---

## 5. MathUtils 신규 생성

**파일:** `Assets/02. Scripts/Utils/MathUtils.cs` (신규)

레퍼런스의 MathUtils에서 현재 필요한 메서드만 가져옴.

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

---

## 6. Settings 추가

**파일:** `Assets/02. Scripts/Utils/Settings.cs`

BATTLE SETTING 리전에 애니메이터 해시 추가:

```csharp
#region ANIMATOR HASH
public static readonly int AnimHashCasting = Animator.StringToHash("Casting");
#endregion
```

---

## 7. 기존 TestOneHandAttack 마이그레이션

**파일:** `Assets/02. Scripts/Battle/Skill/TestOneHandAttack.cs`

기존 동기 스킬을 새 시그니처에 맞춰 수정:

```csharp
// 변경 전
public override void ActivateSkill() { ... }

// 변경 후
public override async UniTask ActivateSkill(CancellationToken token)
{
    Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.AttackPower.Value}");

    if (owner is PlayerCharacter player && player.SpineComponent != null)
    {
        player.SpineComponent.OnAttackOneHand();
    }

    await UniTask.CompletedTask;
}
```

---

## 8. PlayerCharacter GiveSkill 호출 변경

**파일:** `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs`

GiveSkill 시그니처가 변경되므로 호출부 수정:

```csharp
// 변경 전
BSC.SSC.GiveSkill("TestOneHandAttack", new TestOneHandAttack(), 1f);

// 변경 후
BSC.SSC.GiveSkill("TestOneHandAttack");
```

**전제:** DataManager에 "TestOneHandAttack" 키의 SkillSettingData가 등록되어 있어야 함. 없으면 테스트용 JSON 데이터 추가 필요.

---

## 수정 대상 파일 요약

| 파일 | 작업 |
|------|------|
| `Assets/02. Scripts/Battle/Skill/BaseSkill.cs` | 비동기화, SkillSettingData 연동, SetCooltime 개선, SetSkillCasting 추가 |
| `Assets/02. Scripts/Battle/SkillSystemComponent.cs` | 리플렉션 GiveSkill, 비동기 UseSkill, runningSkillDict, Cancel 메서드 |
| `Assets/02. Scripts/Battle/BattleSystemComponent.cs` | UseSkill async 변경 |
| `Assets/02. Scripts/Battle/DamageInfo.cs` | sourceSkill 필드 추가 |
| `Assets/02. Scripts/Battle/Skill/TestOneHandAttack.cs` | async 시그니처 마이그레이션 |
| `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs` | GiveSkill 호출 변경 |
| `Assets/02. Scripts/Utils/MathUtils.cs` | 신규 생성 |
| `Assets/02. Scripts/Utils/Settings.cs` | AnimHash 상수 추가 |

## Verification

1. **컴파일 확인**: Unity에서 컴파일 에러 없이 빌드되는지 확인
2. **GiveSkill 리플렉션 테스트**: "TestOneHandAttack" 키로 GiveSkill 호출 시 인스턴스 정상 생성 확인 (Debug.Log)
3. **UseSkill 비동기 테스트**: 스킬 사용 후 쿨타임 정상 적용, 재사용 차단 확인
4. **CancelSkill 테스트**: CancelAllSkills 호출 시 실행 중 스킬의 CancellationToken이 정상 취소되는지 확인
5. **CooltimeAccel 테스트**: CooltimeAccel 스탯에 값을 넣고 SetCooltime 시 쿨타임 감소 확인
