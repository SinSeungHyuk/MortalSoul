# 소울 코어 스왑 시스템 구현 설계

## 범위

### 포함
- PlayerSoulController (PSC) 신규 구현
- 스왑 파이프라인 (스탯/체력/스킨/무기 교체)
- 소울 획득 함수 (AcquireSoul)
- 스킬 4개 상주형 등록
- 가드 조건
- 입력 액션 바인딩
- 테스트 환경 (하드코딩 2소울 지급)

### 제외
- 스위칭 효과 (별도 구현)
- 서브 패시브 (별도 구현)
- 드랍/획득 플로우
- UI/팝업
- "2슬롯 꽉 찼을 때" UX

---

## 아키텍처

```
PlayerCharacter (파사드 - 오케스트레이터)
├─ PlayerMovementController (MonoBehaviour - 이동/물리)
├─ BSC (plain class - 전투)
│  ├─ SSC (스킬 - 4개 상주, 쿨타임 동시 진행)
│  └─ WSC (무기/기본공격)
├─ SpineController (MonoBehaviour - 애니메이션)
└─ PlayerSoulController (plain class - 소울 데이터/가드)
```

- **PlayerCharacter.SwapSoul()** 이 오케스트레이터 역할
- **PSC**는 소울 데이터 관리 + 가드 체크 + 활성 스킬키 제공
- 각 시스템(WSC/SSC/SpineController/AttributeSet)은 자기 영역만 수행

---

## PlayerSoulController (PSC)

- **파일**: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerSoulController.cs`
- **타입**: plain class (MonoBehaviour 아님)
- **소유자**: PlayerCharacter가 생성 및 보유

### 데이터

| 필드 | 타입 | 설명 |
|---|---|---|
| mainSoulKey | string | 현재 메인 소울 키 |
| subSoulKey | string (nullable) | 서브 소울 키 |
| subSoulHealth | float | 서브 소울의 저장된 체력 |

- 메인 소울의 체력은 `AttributeSet.Health`에서 실시간 참조 (별도 저장 불필요)
- `CharacterSettingData`는 `SettingData.CharacterSettingDict`에서 직접 조회

### 메서드

| 메서드 | 설명 |
|---|---|
| `InitPSC(string _mainSoulKey)` | 메인 소울 설정, 서브 null |
| `CanSwap() → bool` | 서브 없음/사망 시 false, 그 외 true |
| `SwapSlots(float _curHealth)` | mainKey ↔ subKey 교환, subSoulHealth ↔ curHealth 교환 |
| `SetSubSoul(string _soulKey)` | 서브 슬롯에 소울 설정 |
| `InitSubSoulHealth(float _maxHealth)` | 서브 소울 체력을 MaxHealth로 초기화 |
| `GetMainSoulData() → CharacterSettingData` | 현재 메인 소울의 설정 데이터 |
| `GetSubSoulData() → CharacterSettingData` | 서브 소울의 설정 데이터 (nullable) |
| `GetActiveSkillKeys() → List<string>` | 현재 메인 소울의 SkillKeys (UI용) |

### 이벤트

| 이벤트 | 용도 |
|---|---|
| `OnSoulSwapped: event Action` | UI 갱신 (활성 스킬 버튼 전환 등) |

---

## 가드 조건

| 상태 | 결과 |
|---|---|
| 서브 슬롯 비어있음 | 차단 |
| 사망 | 차단 |
| 그 외 모든 상태 | 허용 |

스왑 완료 후 PMC를 무조건 Idle 상태로 전이.

---

## 스왑 파이프라인

```
PlayerCharacter.SwapSoul()
  1. if (!PSC.CanSwap()) return
  2. SSC.CancelAllSkills()
  3. PSC.SwapSlots(AttributeSet.Health)
  4. var newSoulData = PSC.GetMainSoulData()
  5. AttributeSet.SwapBaseValues(newSoulData.AttributeSetSettingData)
  6. AttributeSet.Health = Min(PSC의 스왑된 체력, MaxHealth.Value)
  7. WSC.ChangeWeaponType(newSoulData.WeaponType)
  8. SpineController.SetCombinedSkin(newSoulData.SkinKeys)
  9. PMC.TransitToIdle()
  10. PSC.OnSoulSwapped?.Invoke()
```

---

## 스킬 등록 (4개 상주형)

- **초기화 시**: 메인 소울 스킬 2개만 SSC에 등록
- **소울 획득 시** (`AcquireSoul`): 서브 소울 스킬 2개 추가 등록
- **스왑 시**: SSC 스킬 등록/해제 없음. 쿨타임 4개 동시 진행
- **스킬 사용 제어**: UI가 `PSC.GetActiveSkillKeys()`로 메인 소울 스킬만 표시. UI 버튼이 자연스럽게 필터링
- **소울 종류 교체 시** (새 소울로 대체, 이번 범위 제외): 그때만 RemoveSkill → GiveSkill

---

## 소울 획득

```
PlayerCharacter.AcquireSoul(string _soulKey)
  if (PSC.SubSoulKey != null) return    // 서브 꽉 참 (교체 UX는 범위 제외)
  PSC.SetSubSoul(_soulKey)
  var subData = PSC.GetSubSoulData()
  foreach (skillKey in subData.SkillKeys)
    SSC.GiveSkill(skillKey)
  PSC.InitSubSoulHealth(subData.AttributeSetSettingData.MaxHealth)
```

---

## 상태 보존/리셋 규칙

| 항목 | 스왑 시 동작 |
|---|---|
| baseValue (11종) | 새 소울 값으로 교체 |
| bonusStat "levelup" | 유지 (소울 공용) |
| bonusStat 기타 (버프) | 유지 |
| 체력 | 소울별 독립. subSoulHealth로 교환 |
| 스킬 쿨타임 | 유지 (4개 상주, 동시 진행) |
| 상태이상 (StatusEffect) | 유지 (육체에 귀속) |
| 콤보 인덱스 | 리셋 (WSC.ChangeWeaponType이 처리) |
| 진행중 스킬 | 캔슬 (SSC.CancelAllSkills) |

---

## 체력 규칙

- 메인 소울 HP 0 → 게임오버 (서브로 자동교체 없음)
- 서브 소울 자연회복 없음
- 소울별 체력 완전 독립
- 체력 클램프: `Min(lastHealth, curMaxHealth)`

---

## 입력 바인딩

`InputSystem_Actions`의 `Previous` 액션 사용. PMC에 `OnPrevious` 핸들러 추가:

```csharp
public void OnPrevious(InputValue _value)
{
    if (_value.isPressed)
        player.SwapSoul();
}
```

---

## 새로 필요한 메서드

| 위치 | 메서드 | 설명 |
|---|---|---|
| PlayerAttributeSet | `SwapBaseValues(PlayerAttributeSetSettingData _data)` | 11개 Stat의 baseValue만 새 값으로 교체 |
| PlayerMovementController | `TransitToIdle()` | 상태머신 Idle 전이 (OnIdleEnter가 velocity 초기화 처리) |

---

## 변경 파일 목록

| 파일 | 변경 내용 |
|---|---|
| **신규** `PlayerSoulController.cs` | PSC 전체 구현 |
| `PlayerCharacter.cs` | PSC 생성, SwapSoul(), AcquireSoul() 추가 |
| `PlayerAttributeSet.cs` | SwapBaseValues() 추가 |
| `PlayerMovementController.cs` | TransitToIdle() 추가, OnPrevious 입력 핸들러 추가 |
| `Test.cs` | 테스트용 2소울 지급 |
| `CLAUDE.md` | 액션 캔슬 규칙 업데이트 |
| SSC / WSC / SpineController | 변경 없음 (기존 메서드 재사용) |

---

## 테스트 환경

```
// Test.cs
PlayerCharacter.InitPlayer("soul_warrior")  // 메인 1개로 시작
PlayerCharacter.AcquireSoul("soul_mage")    // 서브 획득
// Previous 키 입력으로 스왑 테스트
```
