# GameManager 게임 흐름 관리 설계

## 개요

게임 진행 흐름을 관리하는 GameManager 도입. Main은 순수 Hub 역할만 유지하고, 실제 게임 국면(Title/Village/Dungeon) 전환 및 상태별 Update 위임은 GameManager가 담당한다.

## 아키텍처

### Update 체인

```
Main.Update()
  └─ GameManager.OnUpdate(dt)
       └─ stateMachine.OnUpdate(dt)
            └─ 현재 상태의 OnStateUpdate(dt)
                 ├─ BattleObjectManager.OnUpdate(dt)  // Dungeon, Village
                 └─ (추후) MonsterManager, EffectManager 등
```

- Main은 `GameManager.OnUpdate(dt)` 한 줄만 호출
- 기존 Main.Update()의 BattleObjectManager.OnUpdate() 직접 호출은 제거

### GameManager 클래스

```
GameManager (일반 클래스, 싱글톤 아님)
├─ MSStateMachine<GameManager> stateMachine
├─ InitGameManager() → 3상태 등록, 초기 상태 Title로 전환
├─ OnUpdate(float dt) → stateMachine.OnUpdate(dt)
└─ TransitState(EGameState) → stateMachine.TransitState()
```

- Main.Awake()에서 `new GameManager()` + `InitGameManager()` 호출
- `Main.Instance.GameManager`로 접근

### 상태 열거형

`GlobalDefine.cs`에 추가:

```csharp
enum EGameState { Title, Village, Dungeon }
```

### 상태 전환 흐름

```
[Title] ──(로드 완료)──→ [Village] ──(던전 입구)──→ [Dungeon]
                            ↑                          │
                            └──(클리어/실패)────────────┘
```

## 각 상태별 책임

### Title

| 구분 | 내용 |
|------|------|
| Enter | SettingData.LoadAllSettingDataAsync() 호출. 로드 완료 후 Village로 전환 |
| Update | 없음 |
| Exit | (추후 타이틀 UI 닫기) |

기존 Main.Start()의 SettingData 로드 로직이 여기로 이동.

### Village

| 구분 | 내용 |
|------|------|
| Enter | (추후) 마을 맵/플레이어 스폰, 인게임 UI. 던전 복귀 시 던전 데이터 정리 확인 |
| Update | BattleObjectManager.OnUpdate(dt). (추후) 필요 매니저 추가 |
| Exit | (추후) 인게임 UI 정리 |

### Dungeon

| 구분 | 내용 |
|------|------|
| Enter | DataManager.InitGameData(). (추후) 던전 맵 생성, 몬스터 스폰 |
| Update | BattleObjectManager.OnUpdate(dt). (추후) MonsterManager, EffectManager |
| Exit | DataManager.ReleaseGameData(). BattleObjectManager.ClearBattleObject() 등 정리 |

## 초기화 순서

```
Main.Awake()
  1. DataManager 생성
  2. AddressableManager 생성
  3. UIManager 생성 + InitUIManager
  4. SoundManager 생성
  5. ObjectPoolManager 생성 + Init
  6. MonsterManager 생성
  7. BattleObjectManager 생성
  8. GameManager 생성 + InitGameManager()  ← 마지막 (다른 매니저 접근 가능해야 하므로)

Main.Update()
  GameManager.OnUpdate(Time.deltaTime)  ← 유일한 호출
```

GameManager는 다른 매니저들이 모두 생성된 후 초기화되어야 한다. InitGameManager() 내부에서 상태 등록 후 Title로 즉시 전환하지 않고, Main.Start()에서 Title 전환을 트리거한다 (Awake 시점에는 아직 프레임 루프가 시작되지 않았으므로).

## 앱 종료 정리

Main.OnDestroy()의 기존 정리 코드는 그대로 유지. GameManager 상태 Exit가 앱 종료 시 호출되지 않을 수 있으므로, 최종 안전망은 Main이 담당.

## 뼈대 구현 범위

### 만드는 것

- GameManager.cs 신규 생성
- EGameState enum 추가 (GlobalDefine.cs)
- Main.cs 수정: GameManager 생성/호출, 기존 테스트 코드 정리

### 각 상태 뼈대

- Title.Enter: SettingData 로드 → 완료 시 Village 전환
- Village.Enter/Update: 로그 + BattleObjectManager.OnUpdate
- Dungeon.Enter: InitGameData + 로그
- Dungeon.Exit: ReleaseGameData + 정리
- Dungeon.Update: BattleObjectManager.OnUpdate

### 안 만드는 것 (추후)

- 타이틀 UI / 인게임 UI 전환
- 마을 맵 스폰, 던전 맵 생성
- Village ↔ Dungeon 실제 전환 트리거
- MonsterManager, EffectManager의 실제 Update 로직
