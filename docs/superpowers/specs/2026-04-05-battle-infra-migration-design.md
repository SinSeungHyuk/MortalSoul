# Battle Infrastructure 1차 마이그레이션 설계

## Context

Mortal Soul 프로젝트의 핵심 메카닉이 **무기 교체 → 소울 교체**로 전환되면서, 전투 시스템 아키텍처를 재설계했다.

- **BSC(BattleSystemComponent)**: 플레이어/몬스터 공통 전투 컴포넌트. FieldCharacter가 보유.
- **SSC(SkillSystemComponent)**: BSC 내부에서 스킬 관리만 전담.
- **SoulSystem**: 플레이어 전용. PlayerCharacter 레벨에서 BSC 외부에 위치.

현재 프로젝트에는 전투 코드가 전무하고, 이전 프로젝트(MortalSoulDoc/04. Assets/)에 레퍼런스 코드가 있다. **외부 의존성 없는 기초 인프라부터 단계적으로 마이그레이션**하는 것이 이번 작업의 목표.

## Scope (1차 마이그레이션)

### 포함

| 파일 | 설명 |
|------|------|
| `Stat.cs` | 기존 `Core/Stat.cs`를 `Battle/`로 이동 + `MS.Battle` 네임스페이스 적용. `EStatType` enum 포함 |
| `DamageInfo.cs` | 데미지 정보 구조체 + `EDamageAttributeType` enum 포함 |
| `BaseAttributeSet.cs` | 추상 기반 AttributeSet |
| `PlayerAttributeSet.cs` | 플레이어 전용 스탯 확장 |
| `MonsterAttributeSet.cs` | 몬스터 전용 스탯 확장 |
| `StatusEffect.cs` | 상태이상 기본 클래스 |

### 제외 (2차 이후)

- SSC / BSC (매니저 의존)
- BaseSkill (SSC 의존)
- BattleUtils / StatusEffectUtils (매니저/이펙트 의존)
- SoulSystem (BSC 완성 후)

## 네임스페이스

**`MS.Battle`** — 전투 시스템 전체를 포괄하는 네임스페이스.

기존 Stat.cs는 네임스페이스 없이 `Core/`에 위치했으나, 전투 전용 스탯으로 확정되었으므로 `MS.Battle`로 편입.

## 폴더 구조

```
Assets/02. Scripts/Battle/
├── Stat.cs              (EStatType enum 포함)
├── DamageInfo.cs        (EDamageAttributeType enum 포함)
├── AttributeSet/
│   ├── BaseAttributeSet.cs
│   ├── PlayerAttributeSet.cs
│   └── MonsterAttributeSet.cs
└── StatusEffect/
    └── StatusEffect.cs
```

## 상세 설계

### 1. Stat.cs

기존 `Core/Stat.cs`를 `Battle/Stat.cs`로 이동. **EStatType enum도 이 파일에 포함.**

```csharp
namespace MS.Battle
{
    public enum EStatType
    {
        MaxHealth, AttackPower, Defense, MoveSpeed, Evasion,
        CriticChance, CriticMultiple, LifeSteal, CooltimeAccel,
        ProjectileCount, AreaRangeMultiple, KnockbackMultiple,
        CoinMultiple, AttackRange
    }
}
```

핵심 로직 유지:
- `BonusStat` 구조체 (EBonusType: Flat/Percentage)
- `Stat` 클래스: baseValue + `Dictionary<string, BonusStat>` bonusStatDict
- 계산: `(base + flatSum) * (1 + percentSum/100)`, 최소 0 클램프
- `OnValueChanged` 이벤트
- API: `AddBaseValue`, `AddBonusStat(key, type, value)`, `RemoveBonusStat(key)`, `ClearBonusStat()`

변경사항:
- 네임스페이스 `MS.Battle` 적용
- `EStatType` enum 동일 파일에 포함
- 기존 코드 구조 그대로 유지 (검증된 로직)

### 2. DamageInfo.cs

**EDamageAttributeType enum도 이 파일에 포함.**

```csharp
namespace MS.Battle
{
    [System.Flags]
    public enum EDamageAttributeType
    {
        None     = 0,
        Fire     = 1 << 0,
        Ice      = 1 << 1,
        Electric = 1 << 2,
        Wind     = 1 << 3,
        Saint    = 1 << 4,
        Dark     = 1 << 5
    }

    public struct DamageInfo
    {
        public object Attacker;              // FieldCharacter (타입 의존 회피)
        public object Target;                // FieldCharacter
        public EDamageAttributeType AttributeType;
        public float Damage;
        public bool IsCritic;
        public float KnockbackForce;
    }
}
```

> 기존 레퍼런스에서 `FieldCharacter`와 `BaseSkill`을 직접 참조했으나, 1차에서는 FieldCharacter/BaseSkill이 아직 MS.Battle에 없으므로 `object`로 선언. SSC/BSC 마이그레이션 시 구체 타입으로 교체.

### 3. BaseAttributeSet.cs

```csharp
namespace MS.Battle
{
    public abstract class BaseAttributeSet
    {
        protected Dictionary<EStatType, Stat> statDict;
        
        public float Health { get; set; }
        public float HealthRatio => statDict[EStatType.MaxHealth].Value > 0 
            ? Health / statDict[EStatType.MaxHealth].Value : 0;
        public EDamageAttributeType WeaknessAttributeType { get; set; }
        
        public event Action<float, float> OnHealthChanged; // current, max
        
        // 템플릿 메서드: 공통 초기화 후 서브클래스별 추가 스탯 등록
        public void InitAttributeSet(Dictionary<EStatType, float> baseValues)
        {
            // 1. 공통 스탯 생성 (MaxHealth, AttackPower, Defense, MoveSpeed)
            // 2. RegisterAdditionalStats() 호출 — 서브클래스 확장점
            // 3. baseValues에서 각 Stat의 baseValue 설정
        }
        
        protected abstract void RegisterAdditionalStats();
        
        public Stat GetStatByType(EStatType type);
        public float GetStatValueByType(EStatType type);
    }
}
```

초기화 흐름: `InitAttributeSet(baseValues)` → 공통 스탯 생성 → `RegisterAdditionalStats()` (서브클래스 확장) → baseValues로 초기값 설정. DataManager 연동은 이후.

### 4. PlayerAttributeSet.cs

BaseAttributeSet 상속. 추가 스탯 9개:
- CriticChance, CriticMultiple, Evasion, LifeSteal
- CooltimeAccel, ProjectileCount, AreaRangeMultiple
- KnockbackMultiple, CoinMultiple

### 5. MonsterAttributeSet.cs

BaseAttributeSet 상속. 추가 스탯 1개:
- AttackRange

### 6. StatusEffect.cs

```csharp
namespace MS.Battle
{
    public class StatusEffect
    {
        public float Duration { get; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished => Duration > 0 && ElapsedTime >= Duration;
        
        public Action OnStatusStartCallback;
        public Action<float> OnStatusUpdateCallback; // deltaTime
        public Action OnStatusEndCallback;
        
        public StatusEffect(float duration);
        public void Start();
        public void Update(float deltaTime);
        public void End();
    }
}
```

순수 타이머 + 콜백 구조. 구체적인 효과(화상, 기절 등)는 StatusEffectUtils로 2차에서 구현.

## 기존 코드 영향

- `Core/Stat.cs` → `Battle/Stat.cs`로 이동. 기존 위치의 파일 삭제.
- 현재 Stat.cs를 참조하는 코드가 없으므로 (Test 코드에서도 미사용) 영향 없음.

## 설계 원칙

1. **외부 의존성 제로**: UniTask, 매니저, MonoBehaviour 참조 없음
2. **레퍼런스 로직 유지**: 검증된 계산 로직(Stat 계산식, 비트플래그 등) 그대로 사용
3. **이전 아키텍처 잔재 제거**: 싱글톤 직접 참조, SettingData 의존 등 제거
4. **임시 타입 최소화**: DamageInfo의 object 타입은 2차에서 교체 예정

## 검증 방법

1. Unity 에디터에서 컴파일 에러 없이 빌드 확인
2. 기존 Test 씬/스크립트가 정상 동작하는지 확인 (Stat 이동으로 인한 참조 깨짐 없는지)
3. 각 클래스의 기본 동작 검증:
   - Stat: 보너스 추가/제거 시 Value 계산 정확성
   - AttributeSet: 초기화 후 스탯 조회
   - StatusEffect: Duration 경과 시 IsFinished 전환
