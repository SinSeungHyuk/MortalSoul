# DataManager 아키텍처 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** DataManager에 SettingData(정적 JSON 데이터)와 GameData(던전 런타임 데이터) 두 래퍼 클래스를 구현하여 데이터 접근을 통일한다.

**Architecture:** DataManager는 SettingData/GameData 두 프로퍼티만 보유하는 경량 진입점. SettingData는 게임 시작 시 JSON 로드, GameData는 던전 입장 시 생성/종료 시 null 처리. GameData 내부는 Soul/Level/Dungeon/Battle 4개 카테고리 클래스로 구분.

**Tech Stack:** Unity 6, C#, Newtonsoft.Json, Cysharp.Threading.Tasks (UniTask), Addressables

**Spec:** `docs/superpowers/specs/2026-04-05-datamanager-architecture-design.md`

---

## File Map

| Action | Path | Responsibility |
|--------|------|---------------|
| Modify | `Assets/02. Scripts/Data/GlobalDefine.cs` | EGrade, EWeaponType, EZoneType, ESkillValueType enum 정의 |
| Create | `Assets/02. Scripts/Data/SettingData/SettingData.cs` | 래퍼: 4개 Dict 프로퍼티 + LoadAllAsync() |
| Create | `Assets/02. Scripts/Data/SettingData/CharacterSettingData.cs` | 소울별 설정 (스탯, 스킨, 무기, 스킬) |
| Create | `Assets/02. Scripts/Data/SettingData/MonsterSettingData.cs` | 몬스터 설정 (스탯, 약점, 스킬리스트) |
| Create | `Assets/02. Scripts/Data/SettingData/SkillSettingData.cs` | 스킬 설정 (쿨타임, 데미지, 타입) |
| Create | `Assets/02. Scripts/Data/SettingData/SoundSettingData.cs` | 사운드 설정 (볼륨, 루프) |
| Create | `Assets/02. Scripts/Data/GameData/GameData.cs` | 래퍼: 4개 카테고리 프로퍼티 |
| Create | `Assets/02. Scripts/Data/GameData/SoulGameData.cs` | 소울 슬롯 런타임 |
| Create | `Assets/02. Scripts/Data/GameData/LevelGameData.cs` | 레벨/경험치 런타임 |
| Create | `Assets/02. Scripts/Data/GameData/DungeonGameData.cs` | 던전 진행 런타임 |
| Create | `Assets/02. Scripts/Data/GameData/BattleGameData.cs` | 전투 통계 런타임 |
| Modify | `Assets/02. Scripts/Core/Manager/DataManager.cs` | SettingData/GameData 프로퍼티 + CreateGameData/ReleaseGameData |

---

### Task 1: GlobalDefine — 공용 enum 정의

**Files:**
- Modify: `Assets/02. Scripts/Data/GlobalDefine.cs`

- [ ] **Step 1: GlobalDefine.cs에 enum 작성**

현재 빈 파일. 프로젝트 전역에서 사용할 enum들을 정의한다.

```csharp
using System;

namespace MS.Data
{
    // 소울/아이템 등급
    public enum EGrade
    {
        Normal,
        Rare,
        Unique,
        Legendary
    }

    // 소울별 무기 타입
    public enum EWeaponType
    {
        GreatSword,     // 대검
        OneHandSword,   // 한손검
        Dagger,         // 단도
        Bow,            // 활
        Staff           // 지팡이
    }

    // 던전 구역 타입
    public enum EZoneType
    {
        Battle,
        Shop,
        Event,
        Boss
    }

    // 스킬 수치 타입 (SkillSettingData에서 사용)
    public enum ESkillValueType
    {
        Default,        // 기본 수치
        Damage,         // 데미지율
        Knockback,      // 넉백 수치
        Move,           // 이동 거리
        Buff,           // 버프 효과 수치
        Duration,       // 지속 시간
        Casting         // 캐스팅 시간
    }
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Data/GlobalDefine.cs"
git commit -m "feat: GlobalDefine에 EGrade, EWeaponType, EZoneType, ESkillValueType enum 추가"
```

---

### Task 2: SettingData 개별 클래스 4개 생성

**Files:**
- Create: `Assets/02. Scripts/Data/SettingData/CharacterSettingData.cs`
- Create: `Assets/02. Scripts/Data/SettingData/MonsterSettingData.cs`
- Create: `Assets/02. Scripts/Data/SettingData/SkillSettingData.cs`
- Create: `Assets/02. Scripts/Data/SettingData/SoundSettingData.cs`

- [ ] **Step 1: CharacterSettingData.cs 작성**

이전 프로젝트의 CharacterSettingData를 소울 시스템에 맞게 확장한다. Dict 키가 소울 ID가 된다.

```csharp
using System;
using System.Collections.Generic;

namespace MS.Data
{
    // 게임 전체 캐릭터(소울) 설정 — JSON 최상위 객체
    [Serializable]
    public class GameCharacterSettingData
    {
        public LevelSettingData LevelSettingData { get; set; }
        public Dictionary<string, CharacterSettingData> CharacterSettingDataDict { get; set; }
    }

    // 레벨 경험치 테이블 설정
    [Serializable]
    public class LevelSettingData
    {
        public float BaseExp { get; set; }
        public float IncreaseExpPerLevel { get; set; }
    }

    // 소울 1개의 설정 데이터
    [Serializable]
    public class CharacterSettingData
    {
        public EGrade Grade { get; set; }
        public AttributeSetSettingData AttributeSetSettingData { get; set; }

        // Spine 스킨 키값 (부위별)
        public Dictionary<string, string> SkinKeys { get; set; }

        // 무기 타입
        public EWeaponType WeaponType { get; set; }

        // 기본공격 스킬 키
        public string BasicAttackKey { get; set; }

        // 고유 스킬 2개 키
        public string[] SkillKeys { get; set; }

        // 스위칭 효과 키
        public string SwitchingEffectKey { get; set; }

        // 서브슬롯 패시브 효과 키
        public string SubPassiveKey { get; set; }
    }

    // 캐릭터(소울) 기본 스탯 설정 — 13개 스탯
    [Serializable]
    public class AttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float AttackPower { get; set; }
        public float Defense { get; set; }
        public float Evasion { get; set; }
        public float MoveSpeed { get; set; }
        public float CriticChance { get; set; }
        public float CriticMultiple { get; set; }
        public float LifeSteal { get; set; }
        public float CooltimeAccel { get; set; }
        public float ProjectileCount { get; set; }
        public float AreaRangeMultiple { get; set; }
        public float KnockbackMultiple { get; set; }
        public float CoinMultiple { get; set; }
    }
}
```

- [ ] **Step 2: MonsterSettingData.cs 작성**

```csharp
using MS.Battle;
using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class MonsterSettingData
    {
        public MonsterAttributeSetSettingData AttributeSetSettingData { get; set; }
        public string DropItemKey { get; set; }
        public List<MonsterSkillSettingData> SkillList { get; set; }
    }

    // 몬스터 전용 스탯 설정 — 6개 스탯 + 약점 속성
    [Serializable]
    public class MonsterAttributeSetSettingData
    {
        public float MaxHealth { get; set; }
        public float AttackPower { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackRange { get; set; }
        public EDamageAttributeType WeaknessAttributeType { get; set; }
    }

    // 몬스터 스킬 설정
    [Serializable]
    public class MonsterSkillSettingData
    {
        public string SkillKey { get; set; }
        public int SkillActivateRate { get; set; }
        public string AnimTriggerKey { get; set; }
        public float SkillDuration { get; set; }
    }
}
```

- [ ] **Step 3: SkillSettingData.cs 작성**

```csharp
using MS.Battle;
using System;
using System.Collections.Generic;

namespace MS.Data
{
    [Serializable]
    public class SkillSettingData
    {
        public string OwnerType { get; set; }
        public string IconKey { get; set; }
        public List<string> CategoryKeyList { get; set; }
        public EDamageAttributeType AttributeType { get; set; }
        public float Cooltime { get; set; }
        public bool IsPostUseCooltime { get; set; }
        public Dictionary<ESkillValueType, float> SkillValueDict { get; set; }

        public float GetValue(ESkillValueType _valueType)
        {
            if (SkillValueDict != null && SkillValueDict.TryGetValue(_valueType, out float value))
                return value;
            return 0f;
        }
    }
}
```

- [ ] **Step 4: SoundSettingData.cs 작성**

```csharp
using System;

namespace MS.Data
{
    [Serializable]
    public class SoundSettingData
    {
        public float MinVolume { get; set; }
        public float MaxVolume { get; set; }
        public bool Loop { get; set; }
    }
}
```

- [ ] **Step 5: 커밋**

```bash
git add "Assets/02. Scripts/Data/SettingData/"
git commit -m "feat: SettingData 개별 클래스 4개 생성 (Character/Monster/Skill/Sound)"
```

---

### Task 3: SettingData 래퍼 클래스

**Files:**
- Create: `Assets/02. Scripts/Data/SettingData/SettingData.cs`

- [ ] **Step 1: SettingData.cs 래퍼 작성**

4개 Dict를 보유하고 Addressables + Newtonsoft.Json으로 JSON 일괄 로드하는 래퍼 클래스.

```csharp
using Core;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Data
{
    public class SettingData
    {
        public GameCharacterSettingData CharacterSettingData { get; private set; }
        public Dictionary<string, MonsterSettingData> MonsterSettingDict { get; private set; }
        public Dictionary<string, SkillSettingData> SkillSettingDict { get; private set; }
        public Dictionary<string, SoundSettingData> SoundSettingDict { get; private set; }

        public async UniTask LoadAllSettingDataAsync()
        {
            try
            {
                TextAsset characterJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("CharacterSettingData");
                CharacterSettingData = JsonConvert.DeserializeObject<GameCharacterSettingData>(characterJson.text);

                TextAsset monsterJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("MonsterSettingData");
                MonsterSettingDict = JsonConvert.DeserializeObject<Dictionary<string, MonsterSettingData>>(monsterJson.text);

                TextAsset skillJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("SkillSettingData");
                SkillSettingDict = JsonConvert.DeserializeObject<Dictionary<string, SkillSettingData>>(skillJson.text);

                TextAsset soundJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("SoundSettingData");
                SoundSettingDict = JsonConvert.DeserializeObject<Dictionary<string, SoundSettingData>>(soundJson.text);

                Debug.Log("[SettingData] 모든 세팅 데이터 로드 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingData] 데이터 로딩 실패: {e.Message}");
            }
        }
    }
}
```

**참고**: `CharacterSettingData`는 Dict가 아닌 `GameCharacterSettingData` 객체로 관리한다. 이 안에 `LevelSettingData`와 `CharacterSettingDataDict`가 함께 들어있기 때문이다. 소울별 설정 접근 시 `SettingData.CharacterSettingData.CharacterSettingDataDict["soul_id"]`를 사용한다.

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Data/SettingData/SettingData.cs"
git commit -m "feat: SettingData 래퍼 클래스 구현 (LoadAllAsync)"
```

---

### Task 4: GameData 개별 카테고리 클래스 4개 생성

**Files:**
- Create: `Assets/02. Scripts/Data/GameData/SoulGameData.cs`
- Create: `Assets/02. Scripts/Data/GameData/LevelGameData.cs`
- Create: `Assets/02. Scripts/Data/GameData/DungeonGameData.cs`
- Create: `Assets/02. Scripts/Data/GameData/BattleGameData.cs`

- [ ] **Step 1: SoulGameData.cs 작성**

```csharp
using System.Collections.Generic;

namespace MS.Data
{
    public class SoulGameData
    {
        // 현재 메인 소울 ID
        public string MainSoulKey { get; set; }

        // 서브 소울 ID (null = 빈 슬롯)
        public string SubSoulKey { get; set; }

        // 소울별 마지막 체력 보존 (소울키 → 체력값)
        public Dictionary<string, float> SoulHealthDict { get; set; }

        public SoulGameData()
        {
            SoulHealthDict = new Dictionary<string, float>();
        }
    }
}
```

- [ ] **Step 2: LevelGameData.cs 작성**

```csharp
using MS.Battle;
using MS.Core;
using System.Collections.Generic;

namespace MS.Data
{
    public class LevelGameData
    {
        // UI 바인딩용 반응형 프로퍼티
        public MSReactProp<int> CurrentLevel { get; private set; }
        public MSReactProp<float> CurrentExp { get; private set; }

        // 방 클리어 후 한꺼번에 처리할 대기 레벨업 횟수
        public int PendingLevelUpCount { get; set; }

        // 레벨업으로 누적된 스탯 증가량 (bonusStat "levelup" 키에 반영)
        public Dictionary<EStatType, float> LevelUpGrowth { get; set; }

        public LevelGameData()
        {
            CurrentLevel = new MSReactProp<int>(1);
            CurrentExp = new MSReactProp<float>(0f);
            PendingLevelUpCount = 0;
            LevelUpGrowth = new Dictionary<EStatType, float>();
        }
    }
}
```

- [ ] **Step 3: DungeonGameData.cs 작성**

```csharp
namespace MS.Data
{
    public class DungeonGameData
    {
        // 현재 구역 번호 (0부터 시작)
        public int CurrentZoneIndex { get; set; }

        // 현재 구역 타입 (전투/상점/이벤트/보스)
        public EZoneType CurrentZoneType { get; set; }

        // 휴무 상태 여부 (방 클리어 후 레벨업 보상 타이밍)
        public bool IsResting { get; set; }

        public DungeonGameData()
        {
            CurrentZoneIndex = 0;
            CurrentZoneType = EZoneType.Battle;
            IsResting = false;
        }
    }
}
```

- [ ] **Step 4: BattleGameData.cs 작성**

```csharp
using System.Collections.Generic;

namespace MS.Data
{
    public class BattleGameData
    {
        // 총 킬수
        public int KillCount { get; set; }

        // 획득 골드
        public int GoldEarned { get; set; }

        // 스킬별 누적 DPS 추적 (스킬키 → 누적 데미지)
        public Dictionary<string, float> SkillDpsDict { get; set; }

        public BattleGameData()
        {
            KillCount = 0;
            GoldEarned = 0;
            SkillDpsDict = new Dictionary<string, float>();
        }
    }
}
```

- [ ] **Step 5: 커밋**

```bash
git add "Assets/02. Scripts/Data/GameData/"
git commit -m "feat: GameData 카테고리 클래스 4개 생성 (Soul/Level/Dungeon/Battle)"
```

---

### Task 5: GameData 래퍼 클래스

**Files:**
- Create: `Assets/02. Scripts/Data/GameData/GameData.cs`

- [ ] **Step 1: GameData.cs 래퍼 작성**

```csharp
namespace MS.Data
{
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
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Data/GameData/GameData.cs"
git commit -m "feat: GameData 래퍼 클래스 구현"
```

---

### Task 6: DataManager 리팩터링

**Files:**
- Modify: `Assets/02. Scripts/Core/Manager/DataManager.cs`

- [ ] **Step 1: DataManager에 SettingData/GameData 프로퍼티 추가**

기존 빈 DataManager를 스펙에 맞게 구현한다.

```csharp
using MS.Data;

namespace Core
{
    public class DataManager
    {
        public SettingData SettingData { get; private set; }
        public GameData GameData { get; private set; }

        public DataManager()
        {
            SettingData = new SettingData();
        }

        // 던전 입장 시 호출 — 런타임 데이터 초기화
        public void CreateGameData()
        {
            GameData = new GameData();
        }

        // 던전 종료 시 호출 — 런타임 데이터 해제
        public void ReleaseGameData()
        {
            GameData = null;
        }
    }
}
```

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Core/Manager/DataManager.cs"
git commit -m "feat: DataManager에 SettingData/GameData 래퍼 연결"
```

---

### Task 7: 컴파일 검증

- [ ] **Step 1: Unity 프로젝트 컴파일 확인**

Unity Editor에서 컴파일 에러가 없는지 확인한다. 특히:
- `MS.Data` 네임스페이스의 모든 클래스가 정상 참조되는지
- `Core.DataManager`에서 `MS.Data.SettingData`/`MS.Data.GameData` 접근이 정상인지
- `MS.Battle.EStatType`, `MS.Battle.EDamageAttributeType` 크로스 네임스페이스 참조가 정상인지
- `MS.Core.MSReactProp<T>` 참조가 LevelGameData에서 정상인지

에러 발생 시 해당 파일을 수정하고 다시 확인한다.

- [ ] **Step 2: 최종 커밋**

모든 컴파일 에러가 해결된 후 최종 커밋.

```bash
git add -A
git commit -m "fix: DataManager 아키텍처 컴파일 에러 수정 (있을 경우)"
```