# GameManager 게임 흐름 관리 뼈대 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Main의 Hub 역할을 유지하면서, 게임 흐름(Title → Village → Dungeon)을 상태머신으로 관리하는 GameManager 뼈대를 구현한다.

**Architecture:** GameManager는 일반 클래스로 MSStateMachine<GameManager>를 보유하며 3상태(Title/Village/Dungeon)를 등록한다. Main.Update()는 GameManager.OnUpdate()만 호출하고, 각 상태의 Update에서 필요한 하위 매니저의 OnUpdate를 위임 호출한다.

**Tech Stack:** Unity 6, C#, MSStateMachine, UniTask

---

### Task 1: EGameState enum 추가

**Files:**
- Modify: `Assets/02. Scripts/Data/GlobalDefine.cs`

- [ ] **Step 1: GlobalDefine.cs에 EGameState enum 추가**

`GlobalDefine.cs`의 `MS.Data` 네임스페이스 끝에 추가:

```csharp
public enum EGameState
{
    Title,
    Village,
    Dungeon
}
```

기존 `EWeaponType` enum 아래에 배치.

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Data/GlobalDefine.cs"
git commit -m "feat: EGameState enum 추가 (Title/Village/Dungeon)"
```

---

### Task 2: GameManager.cs 생성

**Files:**
- Create: `Assets/02. Scripts/Core/Manager/GameManager.cs`

- [ ] **Step 1: GameManager 클래스 작성**

`Assets/02. Scripts/Core/Manager/GameManager.cs` 생성:

```csharp
using Cysharp.Threading.Tasks;
using MS.Core.StateMachine;
using MS.Data;
using UnityEngine;

namespace Core
{
    public class GameManager
    {
        private MSStateMachine<GameManager> stateMachine;

        public void InitGameManager()
        {
            stateMachine = new MSStateMachine<GameManager>(this);

            stateMachine.RegisterState((int)EGameState.Title,
                OnTitleEnter, null, null);

            stateMachine.RegisterState((int)EGameState.Village,
                OnVillageEnter, OnVillageUpdate, null);

            stateMachine.RegisterState((int)EGameState.Dungeon,
                OnDungeonEnter, OnDungeonUpdate, OnDungeonExit);
        }

        public void StartGame()
        {
            stateMachine.TransitState((int)EGameState.Title);
        }

        public void OnUpdate(float _deltaTime)
        {
            stateMachine.OnUpdate(_deltaTime);
        }

        public void TransitState(EGameState _state)
        {
            stateMachine.TransitState((int)_state);
        }

        public EGameState GetCurState()
        {
            return (EGameState)stateMachine.GetCurrentStateId();
        }

        // ===== Title =====
        private void OnTitleEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Title 진입");
            BootAsync().Forget();
        }

        private async UniTaskVoid BootAsync()
        {
            await Main.Instance.DataManager.SettingData.LoadAllSettingDataAsync();

            Debug.Log("[GameManager] SettingData 로드 완료 → Village 전환");
            TransitState(EGameState.Village);
        }

        // ===== Village =====
        private void OnVillageEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Village 진입");
        }

        private void OnVillageUpdate(float _deltaTime)
        {
            Main.Instance.EffectManager.OnUpdate(_deltaTime);
            Main.Instance.BattleObjectManager.OnUpdate(_deltaTime);
        }

        // ===== Dungeon =====
        private void OnDungeonEnter(int _prevStateId, object[] _params)
        {
            Debug.Log("[GameManager] Dungeon 진입");
            Main.Instance.DataManager.InitGameData();
        }

        private void OnDungeonUpdate(float _deltaTime)
        {
            Main.Instance.EffectManager.OnUpdate(_deltaTime);
            Main.Instance.BattleObjectManager.OnUpdate(_deltaTime);
        }

        private void OnDungeonExit(int _nextStateId)
        {
            Debug.Log("[GameManager] Dungeon 퇴장");
            Main.Instance.BattleObjectManager.ClearBattleObject();
            Main.Instance.EffectManager.ClearEffect();
            Main.Instance.DataManager.ReleaseGameData();
        }
    }
}
```

핵심 포인트:
- Title의 Update는 `null` 등록 (이벤트 대기만, 매 프레임 처리 없음)
- Title의 Exit도 `null` (추후 타이틀 UI 닫기 시 추가)
- Village의 Exit도 `null` (추후 필요 시 추가)
- BootAsync에서 SettingData 로드 후 자동으로 Village 전환
- Village/Dungeon Update에서 EffectManager, BattleObjectManager OnUpdate 위임

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Core/Manager/GameManager.cs"
git commit -m "feat: GameManager 뼈대 구현 (Title/Village/Dungeon 상태머신)"
```

---

### Task 3: Main.cs 수정

**Files:**
- Modify: `Assets/02. Scripts/Core/Main.cs`

- [ ] **Step 1: GameManager 프로퍼티 추가 및 초기화 순서 변경**

Main.cs를 아래와 같이 수정:

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class Main : MonoSingleton<Main>
    {
        public DataManager DataManager { get; private set; }
        public AddressableManager AddressableManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public SoundManager SoundManager { get; private set; }
        public ObjectPoolManager ObjectPoolManager { get; private set; }
        public EffectManager EffectManager { get; private set; }
        public MonsterManager MonsterManager { get; private set; }
        public BattleObjectManager BattleObjectManager { get; private set; }
        public GameManager GameManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            DataManager = new DataManager();
            AddressableManager = new AddressableManager();
            UIManager = new UIManager();
            UIManager.InitUIManager(transform);
            SoundManager = new SoundManager();

            Transform poolContainer = new GameObject("ObjectPool").transform;
            poolContainer.SetParent(transform);

            ObjectPoolManager = new ObjectPoolManager();
            ObjectPoolManager.InitObjectPoolManager(poolContainer);
            EffectManager = new EffectManager();
            MonsterManager = new MonsterManager();
            BattleObjectManager = new BattleObjectManager();

            GameManager = new GameManager();
            GameManager.InitGameManager();
        }

        private void Start()
        {
            GameManager.StartGame();
        }

        private void Update()
        {
            GameManager.OnUpdate(Time.deltaTime);
        }

        protected override void OnDestroy()
        {
            BattleObjectManager?.ClearBattleObject();
            EffectManager?.ClearEffect();
            ObjectPoolManager?.ClearAllPools();
            SoundManager?.ClearAllSounds();
            AddressableManager?.ReleaseAll();
            DataManager?.ReleaseGameData();

            base.OnDestroy();
        }
    }
}
```

변경 사항 정리:
1. `GameManager` 프로퍼티 추가
2. `Awake()` 끝에 GameManager 생성 + InitGameManager() (모든 매니저 뒤)
3. `Start()` → `GameManager.StartGame()` (기존 TESTBootAsync 제거)
4. `Update()` → `GameManager.OnUpdate(Time.deltaTime)` 한 줄만 (기존 EffectManager/BattleObjectManager 직접 호출 제거)
5. `IsBootCompleted` 프로퍼티 제거
6. `TESTBootAsync()` 메서드 제거
7. `OnDestroy()` 안전망은 그대로 유지

- [ ] **Step 2: 커밋**

```bash
git add "Assets/02. Scripts/Core/Main.cs"
git commit -m "refactor: Main에서 GameManager로 게임 흐름 위임"
```

---

### Task 4: 최종 확인 및 커밋

- [ ] **Step 1: Unity 컴파일 확인**

Unity Editor에서 컴파일 에러가 없는지 확인. 콘솔에 에러 없이 빌드 성공해야 함.

- [ ] **Step 2: 플레이 테스트**

Play 모드 진입 후 콘솔 로그 확인:
```
[GameManager] Title 진입
[GameManager] SettingData 로드 완료 → Village 전환
[GameManager] Village 진입
```

이 순서로 로그가 출력되면 정상.

- [ ] **Step 3: (에러 시) 수정 후 커밋**

문제가 있으면 수정 후:
```bash
git add -A
git commit -m "fix: GameManager 컴파일/런타임 수정"
```
