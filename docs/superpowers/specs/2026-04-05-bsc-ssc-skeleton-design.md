# BSC/SSC 뼈대 + 스킬 테스트 (2차 마이그레이션) 설계

## Context

1차 마이그레이션에서 `MS.Battle` 네임스페이스로 기초 인프라(Stat, DamageInfo, AttributeSet, StatusEffect)를 구축했다. 2차에서는 **BSC/SSC 뼈대를 만들고, FieldCharacter에 BSC를 넣고, 테스트 UI 버튼 클릭으로 스킬을 사용하여 공격 애니메이션이 재생되는 것까지** 확인한다.

## Scope

### 포함

| 파일 | 설명 |
|------|------|
| `BaseSkill.cs` | 스킬 추상 기반 (쿨타임 + ActivateSkill 동기식) |
| `SkillSystemComponent.cs` | SSC — 스킬 등록/사용/쿨타임 관리 |
| `BattleSystemComponent.cs` | BSC — SSC 래핑 + 상태이상 체크 + StatusEffect 관리 |
| `FieldCharacter.cs` | BSC 프로퍼티 추가 + Update 위임 |
| `PlayerCharacter.cs` | BSC 초기화 + 테스트 스킬 등록 |
| `TestOneHandAttack.cs` | 테스트용 구체 스킬 (로그 + 공격 애니메이션) |
| `TestUIController.cs` | 공격 버튼 → BSC.UseSkill 연결 |

### 제외

- TakeDamage (BattleUtils 의존)
- UniTask / async 패턴 (다음 차수)
- SkillSettingData / DataManager 연동
- 리플렉션 기반 스킬 생성 (DataManager 없으므로 인스턴스 직접 전달)
- 구체 스킬 구현 (TestOneHandAttack 1개만)

## 네임스페이스

- BSC, SSC, BaseSkill, TestOneHandAttack → `MS.Battle`
- FieldCharacter, PlayerCharacter → `MS.Field` (기존 유지)

## 폴더 구조

```
Assets/02. Scripts/Battle/
├── (기존 1차 파일들)
├── BattleSystemComponent.cs
├── SkillSystemComponent.cs
└── Skill/
    ├── BaseSkill.cs
    └── TestOneHandAttack.cs
```

## 상세 설계

### 1. BaseSkill.cs

```csharp
namespace MS.Battle
{
    public abstract class BaseSkill
    {
        protected SkillSystemComponent ownerSSC;
        protected FieldCharacter owner;  // MS.Field 참조
        protected BaseAttributeSet attributeSet;

        private float cooltime;
        private float elapsedCooltime;

        public bool IsCooltime => elapsedCooltime < cooltime;
        public float CooltimeRatio => cooltime > 0 ? Mathf.Clamp01(elapsedCooltime / cooltime) : 1f;

        public void InitSkill(SkillSystemComponent _ownerSSC, float _cooltime)
        {
            ownerSSC = _ownerSSC;
            owner = _ownerSSC.Owner;
            attributeSet = _ownerSSC.AttributeSet;
            cooltime = _cooltime;
            elapsedCooltime = _cooltime; // 초기: 쿨타임 완료 상태
        }

        public abstract void ActivateSkill();

        public virtual bool CanActivateSkill() => true;

        public void SetCooltime()
        {
            elapsedCooltime = 0f;
        }

        public void OnUpdate(float _deltaTime)
        {
            if (elapsedCooltime < cooltime)
                elapsedCooltime += _deltaTime;
        }
    }
}
```

- SkillSettingData 없이 `InitSkill`에서 쿨타임을 직접 받음
- `ActivateSkill()`은 동기식 (UniTask 없음)
- `CanActivateSkill()`은 서브클래스 오버라이드 가능한 추가 조건

### 2. SkillSystemComponent.cs (SSC)

```csharp
namespace MS.Battle
{
    public class SkillSystemComponent
    {
        public FieldCharacter Owner { get; private set; }
        public BaseAttributeSet AttributeSet { get; private set; }

        private Dictionary<string, BaseSkill> ownedSkillDict;

        // 이벤트
        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<string> OnSkillUsed;

        public void InitSSC(FieldCharacter _owner, BaseAttributeSet _attributeSet) { ... }
        public void GiveSkill(string _key, BaseSkill _skill, float _cooltime) { ... }
        // 내부에서 _skill.InitSkill(this, _cooltime) 호출
        public bool UseSkill(string _key) { ... }  // 쿨타임 체크 → ActivateSkill → SetCooltime
        public bool IsCooltime(string _key) { ... }
        public bool HasSkill(string _key) { ... }
        public void ClearSSC() { ... }
        public void OnUpdate(float _deltaTime) { ... }  // 스킬 쿨타임 틱
    }
}
```

- `GiveSkill`은 인스턴스를 직접 받음 (리플렉션 X)
- `UseSkill`은 bool 반환 (성공/실패)
- `OnUpdate`에서 모든 보유 스킬의 쿨타임 갱신

### 3. BattleSystemComponent.cs (BSC)

```csharp
namespace MS.Battle
{
    public class BattleSystemComponent
    {
        public SkillSystemComponent SSC { get; private set; }
        public BaseAttributeSet AttributeSet { get; private set; }

        private Dictionary<string, StatusEffect> statusEffectDict;

        public void InitBSC(FieldCharacter _owner, BaseAttributeSet _attributeSet)
        {
            AttributeSet = _attributeSet;
            SSC = new SkillSystemComponent();
            SSC.InitSSC(_owner, _attributeSet);
            statusEffectDict = new Dictionary<string, StatusEffect>();
        }

        public bool UseSkill(string _key)
        {
            // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
            // 현재는 항상 통과
            return SSC.UseSkill(_key);
        }

        public void ApplyStatusEffect(string _key, StatusEffect _effect) { ... }

        public void OnUpdate(float _deltaTime)
        {
            SSC.OnUpdate(_deltaTime);
            // StatusEffect 업데이트 + 만료 제거
            UpdateStatusEffects(_deltaTime);
        }
    }
}
```

- BSC.UseSkill → 상태이상 검증 후 SSC.UseSkill 위임
- StatusEffect 생명주기 관리 (Apply/Update/Remove)
- TakeDamage는 이번에 제외

### 4. FieldCharacter.cs 수정

```csharp
namespace MS.Field
{
    public abstract class FieldCharacter : FieldObject
    {
        public BattleSystemComponent BSC { get; private set; }

        protected void InitBSC(BaseAttributeSet _attributeSet)
        {
            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, _attributeSet);
        }

        virtual protected void Awake() { }

        virtual protected void Update()
        {
            BSC?.OnUpdate(Time.deltaTime);
        }
    }
}
```

- `InitBSC` 헬퍼 메서드: 서브클래스에서 AttributeSet 준비 후 호출
- `Update`에서 BSC.OnUpdate 위임

### 5. PlayerCharacter.cs 수정

```csharp
namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        // TestSpineComponent 참조 (공격 애니메이션 재생용)
        public TestSpineComponent SpineComponent { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SpineComponent = GetComponent<TestSpineComponent>();

            // 테스트용 AttributeSet + BSC 초기화
            var attrSet = new PlayerAttributeSet();
            attrSet.InitBaseAttributeSet(new Dictionary<EStatType, float>
            {
                { EStatType.MaxHealth, 100f },
                { EStatType.AttackPower, 10f },
                { EStatType.Defense, 5f },
                { EStatType.MoveSpeed, 5f }
            });
            InitBSC(attrSet);

            // 테스트 스킬 등록 (쿨타임 1초)
            BSC.SSC.GiveSkill("TestOneHandAttack", new TestOneHandAttack(), 1f);
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
```

- 테스트용으로 하드코딩된 스탯값과 스킬 등록
- `GetComponent<TestSpineComponent>()`로 SpineComponent 참조 획득

### 6. TestOneHandAttack.cs

```csharp
namespace MS.Battle
{
    public class TestOneHandAttack : BaseSkill
    {
        public override void ActivateSkill()
        {
            Debug.Log($"[TestOneHandAttack] 스킬 사용! ATK: {attributeSet.AttackPower.Value}");

            // PlayerCharacter에서 SpineComponent로 공격 애니메이션 재생
            if (owner is PlayerCharacter player && player.SpineComponent != null)
            {
                player.SpineComponent.OnAttackOneHand();
            }
        }
    }
}
```

- 쿨타임 1초 (SSC.GiveSkill 호출 시 전달, GiveSkill 내부에서 InitSkill 자동 호출)
- ActivateSkill에서 로그 출력 + 공격 애니메이션 재생

### 7. TestUIController.cs 수정

```csharp
public class TestUIController : MonoBehaviour
{
    private Button btnAttack;
    private Button btnDash;
    private Button btnJump;

    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        btnAttack = transform.FindChildComponentDeep<Button>("btnAttack");
        btnDash = transform.FindChildComponentDeep<Button>("btnDash");
        btnJump = transform.FindChildComponentDeep<Button>("btnJump");
    }

    private void Start()
    {
        // 씬에서 PlayerCharacter 찾기
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();

        btnAttack.onClick.AddListener(() =>
        {
            playerCharacter?.BSC.UseSkill("TestOneHandAttack");
        });
    }
}
```

- PlayerCharacter를 `FindFirstObjectByType`으로 찾기
- 공격 버튼 → `BSC.UseSkill("TestOneHandAttack")` 호출

## 데이터 흐름

```
btnAttack 클릭
  → PlayerCharacter.BSC.UseSkill("TestOneHandAttack")
    → BSC: 상태이상 체크 (현재 항상 통과)
    → SSC.UseSkill("TestOneHandAttack")
      → 쿨타임 체크
      → TestOneHandAttack.ActivateSkill()
        → Debug.Log("스킬 사용!")
        → PlayerCharacter.SpineComponent.OnAttackOneHand()
          → Spine "Attack_OneHand1" 애니메이션 재생
```

## 주의: TestMoveComponent와의 관계

현재 `TestMoveComponent`의 Attack 상태에서 직접 `spineComponent.OnAttackOneHand()`를 호출하고 있다. 이번 2차에서는 **TestMoveComponent의 Attack 로직은 건드리지 않는다**. TestUIController의 공격 버튼을 통한 BSC 경유 스킬 사용과, TestMoveComponent의 직접 공격은 별개 경로로 공존한다. 추후 통합 시 TestMoveComponent의 Attack 상태가 BSC를 경유하도록 변경할 예정.

## 설계 원칙

1. **BSC/SSC 모두 일반 클래스** — MonoBehaviour 아님, FieldCharacter가 생명주기 제어
2. **외부 의존성 최소화** — UniTask, DataManager, SkillSettingData 없음
3. **테스트 가능한 최소 단위** — 로그 + 애니메이션으로 동작 확인
4. **기존 코드 최소 수정** — TestMoveComponent는 건드리지 않음

## 검증 방법

1. Unity 에디터 컴파일 에러 없음
2. 테스트 씬에서 공격 버튼 클릭 시:
   - 콘솔에 `[TestOneHandAttack] 스킬 사용!` 로그 출력
   - Spine 캐릭터가 Attack_OneHand1 애니메이션 재생
3. 쿨타임 동작: 1초 내 연타 시 스킬 미발동 확인
4. 기존 TestMoveComponent 동작 정상 유지 (이동, 점프, 대시 등)
