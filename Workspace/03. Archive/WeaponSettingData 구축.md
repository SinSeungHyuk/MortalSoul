
작성일 2026-04-06

### 작업 요청
1. 기본공격(노멀 어택) 데이터를 관리할 신규 설정 데이터 클래스 `WeaponSettingData`를 생성한다. 이는 스킬과 성격이 다르므로 **`SkillSettingData`에 포함시키지 않는다.**
2. `WeaponSettingData`는 **무기 종류별(EWeaponType) 콤보 리스트**를 담는 컨테이너다. 키는 `EWeaponType` — "같은 무기 = 같은 기본공격" 원칙에 따라 SoulKey가 아닌 WeaponType을 사용한다.
3. 본 작업은 **데이터 구조 + JSON 로딩까지만** 범위로 삼는다. WSC/BSC 쪽에서 이 데이터를 실제로 소비하는 로직(히트 판정, 콤보 진행 등)은 이후 별도 작업에서 진행한다.

---

### 확정 디자인

#### 1. 클래스 정의 — `Assets/02. Scripts/Data/SettingData/WeaponSettingData.cs`

`EWeaponType` enum은 이미 해당 파일에 존재한다. 동일 파일에 `WeaponSettingData`, `AttackComboData` 클래스를 추가한다. 네임스페이스는 `MS.Data`.

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

**필드 의미**
- `AnimKey`: Spine 애니메이션 이름 (예: `"Attack_OneHand1"`)
- `DamageMultiplier`: 공격력 대비 배율 (1.0 = 100%)
- `HitRange`: 원형 판정 반경 (Unity world unit)
- `HitOffset`: 캐릭터 전방 기준 판정 중심 오프셋
- `Knockback`: 히트 시 넉백 세기

**의도적 누락**
- 타이밍 필드 없음 (Spine 이벤트가 담당)
- 속성 필드 없음 (기본공격은 Void 고정, WSC 단계에서 코드로 지정)
- `ComboResetTime` 없음 (Complete 시점에 예약 없으면 즉시 리셋)
- 헬퍼 메서드 없음 (YAGNI — 필요해지면 WSC 구현 단계에서 추가)
- `[Serializable]`은 Newtonsoft에 필수 아니지만 기존 `SkillSettingData` 컨벤션 통일 목적

#### 2. `SettingData.cs` 수정

`WeaponSettingDict` 프로퍼티 추가 + `LoadAllSettingDataAsync()`에 로드 블록 추가.

```csharp
public Dictionary<EWeaponType, WeaponSettingData> WeaponSettingDict { get; private set; }

// LoadAllSettingDataAsync() 내부 try 블록 끝에 추가
TextAsset weaponJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("WeaponSettingData");
WeaponSettingDict = JsonConvert.DeserializeObject<Dictionary<EWeaponType, WeaponSettingData>>(weaponJson.text);
```

`Dictionary<EnumType, T>`의 키는 Newtonsoft 기본 동작으로 **enum 이름 문자열**("OneHandSword")로 직/역직렬화되므로 별도의 `StringEnumConverter` 어트리뷰트는 **불필요**하다.

#### 3. JSON 파일 — `Assets/04. Settings/SettingData/WeaponSettingData.json`

OneHandSword 하나만 등록한다. 콤보는 리소스 한계로 **최대 2개**.

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

수치 의도: 1콤보는 견본 스타터, 2콤보는 피니시 — 데미지·범위·넉백이 모두 증가하여 "마무리 일격" 감각을 줌. `AnimKey`는 실제 Spine 세팅 전 **플레이스홀더**.

#### 4. Addressables 등록 (Unity 에디터 수동 작업)

`Assets/04. Settings/SettingData/WeaponSettingData.json`을 기존 SettingData가 속한 Addressables 그룹에 추가하고 키를 `"WeaponSettingData"`로 지정한다. 기존 `SkillSettingData.json`과 동일한 그룹·네이밍 컨벤션.

---

### 결정된 사항 요약

| 항목 | 결정 |
|---|---|
| 구조 | 래퍼 클래스 `WeaponSettingData { ComboList }` + `AttackComboData` (플랫) |
| 딕셔너리 | `Dictionary<EWeaponType, WeaponSettingData>` (enum 키 자동 이름 직렬화) |
| 파일 경로 | `Assets/04. Settings/SettingData/WeaponSettingData.json` |
| Addressable 키 | `"WeaponSettingData"` |
| 초기 데이터 | OneHandSword 2콤보만 |
| 검증 방식 | Unity 런타임 로드 + Dict 조회 수동 확인 |

### 범위 외 (향후 작업)
- WSC가 이 데이터를 소비하는 로직 (히트 판정, 콤보 진행)
- 활/스태프 등 원거리 무기의 폴리모픽 확장 (투사체/AOE Executor)
- 나머지 4종 무기 데이터 (GreatSword, Dagger, Bow, Staff)
- Spine 실제 애니메이션 이름 확정 및 `AnimKey` 교체


### 작업 내용
1. `Assets/02. Scripts/Data/SettingData/WeaponSettingData.cs`에 `WeaponSettingData`(ComboList 래퍼) 및 `AttackComboData`(AnimKey/DamageMultiplier/HitRange/HitOffset/Knockback) 클래스 추가. `EWeaponType` enum과 동일 파일/네임스페이스(`MS.Data`)에 배치.
2. `Assets/04. Settings/SettingData/WeaponSettingData.json` 생성 — OneHandSword 2콤보 초기 데이터(Attack_OneHand1/2 플레이스홀더) 등록.
3. `SettingData.cs`에 `WeaponSettingDict` 프로퍼티 추가 및 `LoadAllSettingDataAsync()`에서 Addressables 키 `"WeaponSettingData"`로 비동기 로드 후 `Dictionary<EWeaponType, WeaponSettingData>` 역직렬화.
4. Addressables 그룹에 JSON 등록(에디터 수동 작업 완료).

### 특이사항
1. Newtonsoft 기본 동작으로 enum 키가 이름 문자열("OneHandSword")로 직/역직렬화되므로 `StringEnumConverter` 어트리뷰트 불필요.
2. 타이밍/속성/ComboResetTime 필드는 의도적으로 누락 — 타이밍은 Spine 이벤트(hit/combo_ready/Complete), 속성은 Void 고정(WSC 단계), 콤보 리셋은 Complete 시점 예약 유무로 처리 예정.
3. WSC가 이 데이터를 실제로 소비하는 로직(히트 판정, 콤보 진행)은 본 작업 범위 외 — 후속 작업에서 진행.
4. 나머지 4종 무기(GreatSword/Dagger/Bow/Staff) 데이터 및 실제 Spine `AnimKey` 확정도 후속 작업.

---
태그 : #데이터 #기본공격 #WeaponSettingData #JSON #Addressables
