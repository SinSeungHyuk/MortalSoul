# DataManager 아키텍처 설계 — SettingData + GameData

## Context

이전 프로젝트에서는 DataManager가 정적인 SettingData만 관리했다. 이번 MortalSoul에서는 던전 진행 중 실시간으로 변하는 런타임 데이터도 DataManager에서 함께 관리하되, 프로젝트 규모가 작으므로 과도한 세분화를 피하고 실용적인 구조를 채택한다.

레퍼런스: WheelWindRPG의 `DataManager → GameDataSet(정적) + PlayerDataSet(런타임)` 패턴을 참고하되, 규모에 맞게 경량화.

## 전체 구조

```
DataManager (Main.Instance.DataManager)
├── SettingData (SettingData 클래스) — 정적, JSON 로드, 게임 내내 불변
│   ├── CharacterSettingDict   Dictionary<string, CharacterSettingData>  — 소울별 설정
│   ├── MonsterSettingDict     Dictionary<string, MonsterSettingData>    — 몬스터 설정
│   ├── SkillSettingDict       Dictionary<string, SkillSettingData>      — 스킬 설정
│   └── SoundSettingDict       Dictionary<string, SoundSettingData>      — 사운드 설정
│
└── GameData (GameData 클래스) — 런타임, 던전 입장 시 생성 / 종료 시 null
    ├── Soul    (SoulGameData)     — 메인/서브 소울 슬롯, 소울별 체력
    ├── Level   (LevelGameData)    — 공용 레벨, 경험치, 레벨업 누적
    ├── Dungeon (DungeonGameData)  — 현재 구역, 휴무 상태
    └── Battle  (BattleGameData)   — 킬수, 골드, 스킬DPS 통계
```

## 접근 패턴

```csharp
// DataManager 자체는 2개의 래퍼 프로퍼티만 보유
public class DataManager
{
    public SettingData SettingData { get; private set; }
    public GameData GameData { get; private set; }

    public DataManager()
    {
        SettingData = new SettingData();
    }

    public void CreateGameData()  => GameData = new GameData();
    public void ReleaseGameData() => GameData = null;
}

// 정적 데이터 접근
var soulSetting = Main.Instance.DataManager.SettingData.CharacterSettingDict["warrior_soul"];

// 런타임 데이터 접근
var mainSoul = Main.Instance.DataManager.GameData.Soul.MainSoulKey;
var level = Main.Instance.DataManager.GameData.Level.CurrentLevel;
```

## SettingData 상세

게임 시작 시 Addressables + Newtonsoft.Json으로 JSON 파일을 로드하여 불변 데이터를 보관한다.

```csharp
public class SettingData
{
    public Dictionary<string, CharacterSettingData> CharacterSettingDict { get; private set; }
    public Dictionary<string, MonsterSettingData> MonsterSettingDict { get; private set; }
    public Dictionary<string, SkillSettingData> SkillSettingDict { get; private set; }
    public Dictionary<string, SoundSettingData> SoundSettingDict { get; private set; }

    public async UniTask LoadAllAsync() { /* Addressables로 각 JSON 로드 */ }
}
```

### 개별 SettingData 클래스

| 클래스 | 핵심 필드 | 비고 |
|--------|----------|------|
| `CharacterSettingData` | BaseStat(AttributeSetSettingData), SkinKeys(부위별 Spine 스킨), WeaponType, SkillKeys[2], SwitchingEffectKey, SubPassiveKey | 소울 단위 설정. Dict 키 = 소울 ID |
| `MonsterSettingData` | 6개 스탯(MonsterAttributeSetSettingData), WeaknessAttribute, SkillList(키+확률+애니키+지속시간) | 이전 프로젝트와 동일 구조 |
| `SkillSettingData` | OwnerType, DamageAttribute, Cooltime, SkillValueDict(ESkillValueType→float) | 이전 프로젝트와 동일 구조 |
| `SoundSettingData` | VolumeMin, VolumeMax, Loop | 이전 프로젝트와 동일 구조 |

**향후 확장 예정**: StageSettingData(던전 구역), ItemSettingData(아이템), StatRewardSettingData(레벨업 보상) — 해당 시스템 구현 시점에 추가

## GameData 상세

던전 1회 진행의 모든 런타임 데이터를 보관한다. 던전 입장 시 `CreateGameData()`로 생성, 종료 시 `ReleaseGameData()`로 null 처리.

```csharp
public class GameData
{
    public SoulGameData Soul { get; private set; }
    public LevelGameData Level { get; private set; }
    public DungeonGameData Dungeon { get; private set; }
    public BattleGameData Battle { get; private set; }

    public GameData()
    {
        Soul = new SoulGameData();
        Level = new LevelGameData();
        Dungeon = new DungeonGameData();
        Battle = new BattleGameData();
    }
}
```

### SoulGameData — 소울 슬롯 관리

```csharp
public class SoulGameData
{
    public string MainSoulKey { get; set; }                    // 현재 메인 소울 ID
    public string SubSoulKey { get; set; }                     // 서브 소울 ID (null 가능)
    public Dictionary<string, float> SoulHealthDict { get; set; } // 소울별 마지막 체력 보존
}
```

### LevelGameData — 공용 레벨/경험치

```csharp
public class LevelGameData
{
    public MSReactProp<int> CurrentLevel { get; set; }             // UI 바인딩용 반응형
    public MSReactProp<float> CurrentExp { get; set; }             // UI 바인딩용 반응형
    public int PendingLevelUpCount { get; set; }                   // 대기 중 레벨업 횟수
    public Dictionary<EStatType, float> LevelUpGrowth { get; set; } // 레벨업 누적 스탯
}
```

### DungeonGameData — 던전 진행 상태

```csharp
public class DungeonGameData
{
    public int CurrentZoneIndex { get; set; }      // 현재 구역 번호
    public EZoneType CurrentZoneType { get; set; }  // 전투/상점/이벤트/보스
    public bool IsResting { get; set; }             // 휴무 상태 여부 (레벨업 보상 타이밍)
}
```

### BattleGameData — 전투 통계 (결과 팝업용)

```csharp
public class BattleGameData
{
    public int KillCount { get; set; }                          // 총 킬수
    public int GoldEarned { get; set; }                         // 획득 골드
    public Dictionary<string, float> SkillDpsDict { get; set; } // 스킬별 DPS 추적
}
```

### MSReactProp 사용 기준

- UI에 실시간 바인딩이 필요한 필드(레벨, 경험치)만 `MSReactProp<T>` 사용
- 나머지는 일반 프로퍼티로 관리 (불필요한 옵저버 오버헤드 방지)

## 파일 구조

```
Assets/02. Scripts/Data/
├── SettingData/
│   ├── SettingData.cs              — 래퍼 클래스 + LoadAllAsync()
│   ├── CharacterSettingData.cs     — 소울별 설정 데이터
│   ├── MonsterSettingData.cs       — 몬스터 설정 데이터
│   ├── SkillSettingData.cs         — 스킬 설정 데이터
│   └── SoundSettingData.cs         — 사운드 설정 데이터
└── GameData/
    ├── GameData.cs                 — 래퍼 클래스
    ├── SoulGameData.cs             — 소울 슬롯 런타임 데이터
    ├── LevelGameData.cs            — 레벨/경험치 런타임 데이터
    ├── DungeonGameData.cs          — 던전 진행 런타임 데이터
    └── BattleGameData.cs           — 전투 통계 런타임 데이터
```

## 네임스페이스

- SettingData 계열: `MS.Data`
- GameData 계열: `MS.Data`
- DataManager: `MS.Core` (기존 매니저들과 동일)

## 설계 원칙

1. **DataManager는 경량 진입점**: SettingData/GameData 두 래퍼만 보유, 로직은 각 래퍼에 위임
2. **던전 휘발성과 일치**: GameData는 던전 생명주기와 동일 — 입장 시 new, 종료 시 null
3. **과도한 세분화 금지**: GameData 내부 카테고리는 4개로 제한, 필드 수가 적으므로 추가 분리 불필요
4. **확장 용이**: 새 SettingData 추가 시 SettingData 래퍼에 Dict 추가 + 개별 클래스 파일 생성
5. **[SerializeField] 미사용**: CLAUDE.md 규칙에 따라 인스펙터 연결 자제

## 검증 방법

1. DataManager 생성 후 `SettingData.LoadAllAsync()` 호출 → 각 Dict에 데이터가 정상 로드되는지 로그 확인
2. 던전 입장 시 `CreateGameData()` → `GameData != null` 확인
3. GameData 내 각 카테고리 필드 읽기/쓰기 테스트
4. 던전 종료 시 `ReleaseGameData()` → `GameData == null` 확인
5. MSReactProp 바인딩 필드(Level, Exp)의 UI 연동 테스트
