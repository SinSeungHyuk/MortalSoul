
작성일 2026-04-07 (재정리)

### 작업 요청

전투 시스템(BSC/WSC/SSC)의 기초 골격을 보완하고 PlayerCharacter와 연결한다.
공격/스킬의 애니메이션 타이밍 동기화는 **이벤트 구독 방식이 아닌 SpineController의 await 헬퍼 방식**으로 통일한다.

---

### 1. SpineController 보완 — await 헬퍼 도입

`Assets/02. Scripts/FieldObject/FieldCharacter/SpineController.cs`

- **외부에 노출되는 Action 이벤트(`OnAttackEvent`/`OnComboReadyEvent`/`OnActionCompleted`)는 제거**한다. SpineController 외부에서는 raw 이벤트를 직접 구독하지 않는다.
- 내부에서 Spine raw 이벤트(`Event`/`Complete`)를 받아 TCS(`UniTaskCompletionSource`)를 resolve하는 구조로 캡슐화한다.
- 다음 await 헬퍼를 public API로 제공:

```csharp
UniTask WaitForAnimEvent(string _eventKey);
UniTask WaitForComplete();
```

> 캔슬 토큰 인자는 두지 않는다. 캔슬 출처가 단일(SpineController 자체의 TCS 정리)이라 외부 토큰을 받을 이유가 없다. 캐릭터 파괴 등 종료 시점도 SpineController가 `OnDestroy`에서 일괄 `TrySetCanceled`로 처리한다.

- 동작 원리:
  - 내부에 `Dictionary<string, UniTaskCompletionSource> pendingEventWaits` 보유
  - `WaitForAnimEvent` 호출 시 해당 키에 TCS 생성/저장 후 반환 (이미 존재하면 덮어쓰기)
  - Spine `Event` 콜백에서 키 매칭 시 TCS resolve + 딕셔너리에서 제거
  - `WaitForComplete`는 단일 TCS 필드(`pendingCompleteWait`)로 처리. Spine `Complete` 콜백에서 resolve
  - 새 애니메이션 재생 또는 캐릭터 파괴 시 미해결 TCS는 `TrySetCanceled`로 정리
- 게임 기획상 동일 캐릭터의 공격/스킬은 동시에 발생하지 않으므로 같은 이벤트 키에 대기자가 둘 이상 생기지 않는다.
- 기존 `PlayOnce(_animationName, _onComplete)`의 콜백 인자는 제거하고 `WaitForComplete`로 대체. 필요하면 `Play(animKey, loop)` 형태로 정리.
- 기존 `PlayIdle/Move/Jump/Dash` 등 루프 재생 헬퍼는 그대로 유지 (PlayerController가 사용 중).

---

### 2. WeaponSystemComponent (WSC) 신규 작성

`Assets/02. Scripts/Battle/WeaponSystemComponent.cs`

**책임**: 플레이어 전용 기본공격 콤보 시스템. 대리인 + 실행자 역할.

**내부 상태**:
- `FieldCharacter owner`
- `EWeaponType currentWeaponType`
- `WeaponSettingData currentWeaponData` (현재 무기의 콤보 리스트 캐시)
- `int comboIndex`
- `bool reserveNextCombo` — 선입력(버퍼링) 플래그
- `bool isAttacking`

**공개 API**:
- `void InitWSC(FieldCharacter _owner, EWeaponType _weaponType)`
- `void RequestAttack()` — 선입력 버퍼링 진입점
- `void ChangeWeapon(EWeaponType _weaponType)` — 무기 타입 + 콤보 상태 리셋
- `bool IsAttacking { get; }`
- `EWeaponType CurrentWeaponType { get; }`

> `OnUpdate` / `CancelAttack`는 두지 않는다. 캔슬은 SpineController의 cancel-on-Play 안전망을 통해 외부 Play 호출 시 자연스럽게 발생하므로 별도 API 불필요. 매 프레임 할 일도 없으므로 `OnUpdate` 미구현.

**`RequestAttack` 동작**:
- `currentWeaponData == null` 또는 `ComboList` 비어있음 → `Debug.LogError` 후 즉시 반환 (장착 무기 없음)
- `isAttacking == false` → 콤보 인덱스 0부터 `RunComboLoopAsync` 실행
- `isAttacking == true` → `reserveNextCombo = true` (1회만 ON, 중복 호출 무시)

**`ChangeWeapon` 동작**:
- `WeaponSettingDict`에서 데이터 조회 → 없으면 `Debug.LogError` 후 반환
- 단순 무기 데이터 교체 + `comboIndex = 0` 리셋
- 진행 중 콤보를 강제 캔슬하지 않는다. **공격(콤보)을 포함한 모든 액션 진행 중에는 호출 측(상위 시스템)이 무기/소울 교체를 막을 책임이 있다**. WSC는 그 가정을 신뢰한다.

**콤보 루프 (선형 async 코드)**:

```csharp
private async UniTaskVoid RunComboLoopAsync()
{
    isAttacking = true;
    comboIndex = 0;
    var spine = owner.SpineController;

    try
    {
        while (true)
        {
            var combo = currentWeaponData.ComboList[comboIndex];

            spine.Play(combo.AnimKey, false);

            // 1) 히트 타이밍 대기 → 공격판정 (현재는 placeholder)
            await spine.WaitForAnimEvent(Settings.SpineEventAttack);
            DoHitDetection(combo); // 추후 BSC.TakeDamage 연동

            // 2) 캔슬 가능 시점 또는 종료 시점 대기 (먼저 들어오는 쪽으로 분기)
            int finishedIdx = await UniTask.WhenAny(
                spine.WaitForAnimEvent(Settings.SpineEventComboReady),
                spine.WaitForComplete()
            );

            bool reachedComboReady = (finishedIdx == 0);

            if (reserveNextCombo)
            {
                reserveNextCombo = false;
                comboIndex = (comboIndex + 1) % currentWeaponData.ComboList.Count;
                continue; // 회수 동작 스킵하고 다음 콤보로
            }

            if (!reachedComboReady)
                break; // Complete가 먼저 들어왔고 예약도 없음 → 종료

            // combo_ready가 먼저 왔지만 예약 없음 → 회수 동작 끝까지 재생 후 종료
            await spine.WaitForComplete();
            break;
        }
    }
    catch (OperationCanceledException)
    {
        // 외부 Play 호출로 SpineController가 pending TCS를 캔슬한 경우 (안전망) — 정상 종료
    }
    finally
    {
        comboIndex = 0;
        reserveNextCombo = false;
        isAttacking = false;
    }
}
```

- `DoHitDetection`은 이번 작업에서는 placeholder (로그 출력 + 추후 `BSC.TakeDamage` / OverlapBox 연동 자리). `combo.HitRange` / `combo.HitOffset` / `combo.DamageMultiplier`는 추후 사용.
- `ChangeWeapon` 호출 시 진행 중 콤보가 있으면 `CancelAttack` 후 무기 데이터 교체.

---

### 3. BattleSystemComponent (BSC) 보완

`Assets/02. Scripts/Battle/BattleSystemComponent.cs`

- 필드 추가: `public WeaponSystemComponent WSC { get; private set; }` (몬스터는 null)
- 플레이어용 초기화 오버로드 추가:
  ```csharp
  public void InitBSC(FieldCharacter _owner, BaseAttributeSet _attrSet, EWeaponType _weaponType)
  ```
  - 내부에서 기존 SSC 초기화 + WSC 생성 / `InitWSC` 호출
- 기존 몬스터용 `InitBSC(_owner, _attrSet)` 시그니처는 유지 (WSC = null)
- `OnUpdate`에서 WSC를 호출하지 않는다 (WSC는 await 기반이라 매 프레임 작업 없음)
- `TakeDamage(DamageInfo _info)` 메서드 골격 추가 (placeholder — 실제 데미지 계산은 추후. 지금은 로그 정도)

---

### 4. SkillSystemComponent (SSC) 점검

- 거의 완성 상태로 판단됨. 이번 작업에서는 변경 없음.
- 단, 향후 `BaseSkill` 구현체들이 `owner.SpineController.WaitForAnimEvent(...)` 패턴으로 작성될 것임을 가정. (이번 범위 아님)

---

### 5. PlayerCharacter ↔ BSC 연결

`Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs`

- `TestSpineComponent` 의존 제거. `FieldCharacter.SpineController`를 통해 접근.
- 초기화 흐름:
  ```csharp
  var attrSet = new PlayerAttributeSet();
  attrSet.InitBaseAttributeSet(new Dictionary<EStatType, float> { ... });

  BSC = new BattleSystemComponent();
  BSC.InitBSC(this, attrSet, EWeaponType.OneHandSword);
  ```
- 테스트용 기본 무기: `EWeaponType.OneHandSword`
- 스탯(`PlayerAttributeSet`)은 임시 기본값 유지

---

### 비범위 (이번 작업에서 다루지 않음)

- 소울 시스템 자체 구현 (WSC는 단독 동작 가능, 소울 연결은 추후)
- 실제 데미지 계산 / OverlapBox 히트박스 로직 (TakeDamage / DoHitDetection은 placeholder)
- UI 공격 버튼과의 통합 (별도 작업)
- PlayerController 상태머신과 공격의 통합 (별도 작업)
- 몬스터 측 SpineController · 랜덤 스킬 선택 로직 (메모 참조, 별도 작업)
- `ISpineController` 공통 인터페이스 추상화 (몬스터 측이 생길 때 함께 도입)

---

### 핵심 설계 원칙 (메모 반영)

- **이벤트 구독(`+=/-=`) 방식 폐기**. 호출부는 `await spine.WaitFor...` 선형 코드만 사용.
- 구독 수명 관리 · 핸들러 함수 · pending 필드는 모두 SpineController 내부로 일원화.
- 같은 패턴을 추후 `BaseSkill` 구현체에서도 그대로 사용 → SSC/WSC 모두 일관된 비동기 흐름.
- 애니메이션 클립은 자기가 왜 재생되는지 모름. `hit` 이벤트는 단일 의미만 가지며, 의미 분기는 호출자(WSC vs Skill)가 자기 컨텍스트에서 해석.
