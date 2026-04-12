# Monster System 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 단일 FSM AI 기반 몬스터 시스템 구현 — MonsterCharacter, MonsterController, MonsterManager + Main 연결.

**Architecture:** `FieldCharacter` → `MonsterCharacter`(전투 데이터/생명주기) + `MonsterController`(AI 상태머신/물리 이동) 분리. `MonsterManager`는 스폰/풀/활성 추적만 담당. 플레이어 참조는 `Main.Instance.Player`로 전역 접근.

**Tech Stack:** Unity 6, C#, UniTask, Rigidbody2D, Spine, MSStateMachine

---

### Task 1: Settings 상수 추가 + Main.Player 등록

**Files:**
- Modify: `Assets/02. Scripts/Utils/Settings.cs`
- Modify: `Assets/02. Scripts/Core/Main.cs`
- Modify: `Assets/02. Scripts/FieldObject/FieldCharacter/Player/PlayerCharacter.cs`

- [ ] **Step 1: Settings.cs에 몬스터 관련 상수 추가**

`Settings.cs`의 `BATTLE SETTING` region 뒤에 새 region 추가:

```csharp
#region MONSTER SETTING
public static float MonsterDetectionRange = 8f;
public static float MonsterLayerThresholdY = 2f;
public static float MonsterPatrolWaitTime = 1f;
public static float MonsterPatrolSpeedRatio = 0.5f;
public static float MonsterFidgetDistance = 1.5f;
#endregion
```

- [ ] **Step 2: Main.cs에 Player 프로퍼티 추가**

`Main.cs`의 프로퍼티 영역에 추가:

```csharp
public PlayerCharacter Player { get; set; }
```

`Main.cs` 상단에 using 추가:

```csharp
using MS.Field;
```

- [ ] **Step 3: PlayerCharacter.InitPlayer() 끝에 자기 등록 추가**

`PlayerCharacter.InitPlayer()` 메서드 마지막 줄(`pmc.InitController(this, BSC.WSC);` 뒤)에 추가:

```csharp
Main.Instance.Player = this;
```

- [ ] **Step 4: 커밋**

```
feat: Settings 몬스터 상수 추가 + Main.Player 전역 참조
```

---

### Task 2: MonsterCharacter 작성

**Files:**
- Create: `Assets/02. Scripts/FieldObject/FieldCharacter/Monster/MonsterCharacter.cs`

- [ ] **Step 1: MonsterCharacter.cs 작성**

```csharp
using Core;
using MS.Battle;
using MS.Data;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class MonsterCharacter : FieldCharacter
    {
        private string monsterKey;
        private MonsterController controller;
        private List<MonsterSkillSettingData> skillList;

        public string MonsterKey => monsterKey;
        public List<MonsterSkillSettingData> SkillList => skillList;


        public void InitMonster(string _monsterKey)
        {
            ObjectType = FieldObjectType.Monster;
            ObjectLifeState = FieldObjectLifeState.Live;
            monsterKey = _monsterKey;

            var monsterDict = Main.Instance.DataManager.SettingData.MonsterSettingDict;
            if (!monsterDict.TryGetValue(_monsterKey, out MonsterSettingData monsterData))
            {
                Debug.LogError($"[MonsterCharacter] MonsterSettingData 없음: {_monsterKey}");
                return;
            }

            var attributeSet = new AttributeSet();
            attributeSet.Init(monsterData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, attributeSet);
            BSC.OnDead += OnDeadCallback;

            skillList = monsterData.SkillList;
            if (skillList != null)
            {
                foreach (var skillInfo in skillList)
                    BSC.SSC.GiveSkill(skillInfo.SkillKey);
            }

            controller = GetComponent<MonsterController>();
            controller.InitController(this);
        }

        protected override void Update()
        {
            if (ObjectLifeState != FieldObjectLifeState.Live) return;
            base.Update();
            controller?.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (controller != null) controller.OnFixedUpdate();
        }

        public void ApplyKnockback(Vector2 _dir, float _force)
        {
            if (ObjectLifeState != FieldObjectLifeState.Live) return;
            controller.Rb.linearVelocity = _dir * _force;
        }

        private void OnDeadCallback()
        {
            ObjectLifeState = FieldObjectLifeState.Dying;
            controller.TransitToDead();
        }

        public void OnDespawn()
        {
            BSC.OnDead -= OnDeadCallback;
            BSC.ClearBSC();
            ObjectLifeState = FieldObjectLifeState.Death;
        }

        protected override void OnDestroy()
        {
            if (BSC != null) BSC.OnDead -= OnDeadCallback;
            base.OnDestroy();
        }
    }
}
```

- [ ] **Step 2: 커밋**

```
feat: MonsterCharacter 작성 — BSC/SSC 초기화, 사망/넉백 처리
```

---

### Task 3: MonsterController 작성

**Files:**
- Create: `Assets/02. Scripts/FieldObject/FieldCharacter/Monster/MonsterController.cs`

- [ ] **Step 1: MonsterController.cs 작성**

```csharp
using Core;
using Cysharp.Threading.Tasks;
using MS.Core.StateMachine;
using MS.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MS.Field
{
    public enum EMonsterState { Idle, Trace, Attack, Dead }

    public class MonsterController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private BoxCollider2D col;
        private SpineController spineController;
        private MonsterCharacter character;
        private MSStateMachine<MonsterController> stateMachine;

        public Rigidbody2D Rb => rb;

        private bool isGrounded;
        private float curVelocityX;
        private bool isDetected;


        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            spineController = GetComponent<SpineController>();
        }

        public void InitController(MonsterCharacter _character)
        {
            character = _character;
            rb.gravityScale = Settings.GravityScale;
            isDetected = false;
            curVelocityX = 0f;

            stateMachine = new MSStateMachine<MonsterController>(this);
            stateMachine.RegisterState((int)EMonsterState.Idle, OnIdleEnter, OnIdleUpdate, null);
            stateMachine.RegisterState((int)EMonsterState.Trace, OnTraceEnter, OnTraceUpdate, null);
            stateMachine.RegisterState((int)EMonsterState.Attack, OnAttackEnter, OnAttackUpdate, null);
            stateMachine.RegisterState((int)EMonsterState.Dead, OnDeadEnter, null, null);
            stateMachine.TransitState((int)EMonsterState.Idle);
        }

        public void OnUpdate(float _dt)
        {
            stateMachine.OnUpdate(_dt);
        }

        public void OnFixedUpdate()
        {
            UpdateGroundCheck();
            UpdateFallGravity();
            rb.linearVelocity = new Vector2(curVelocityX, rb.linearVelocityY);
        }

        public void TransitToDead()
        {
            stateMachine.TransitState((int)EMonsterState.Dead);
        }

        private void UpdateGroundCheck()
        {
            Vector2 origin = (Vector2)transform.position + col.offset + Vector2.down * (col.size.y * 0.5f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, Settings.GroundCheckSize, 0f, Vector2.down,
                Settings.GroundCheckDistance, Settings.GroundLayer);
            isGrounded = hit.collider != null;
        }

        private void UpdateFallGravity()
        {
            if (rb.linearVelocityY < 0f)
                rb.gravityScale = Settings.GravityScale * Settings.FallMultiple;
            else
                rb.gravityScale = Settings.GravityScale;
        }

        private void UpdateScaleX(float _dirX)
        {
            if (_dirX > 0.01f && !spineController.IsScaleXRight)
                spineController.SetScaleX(true);
            else if (_dirX < -0.01f && spineController.IsScaleXRight)
                spineController.SetScaleX(false);
        }

        private bool IsSameLayer()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            return Mathf.Abs(player.Position.y - transform.position.y) <= Settings.MonsterLayerThresholdY;
        }

        private bool CheckDetection()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            if (!IsSameLayer()) return false;
            return Mathf.Abs(player.Position.x - transform.position.x) < Settings.MonsterDetectionRange;
        }

        private bool IsInAttackRange()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            float attackRange = character.BSC.AttributeSet.AttackRange.Value;
            return Mathf.Abs(player.Position.x - transform.position.x) < attackRange;
        }

        private float GetMoveSpeed()
        {
            return character.BSC.AttributeSet.MoveSpeed.Value / 100f * Settings.MoveSpeed;
        }

        #region Idle — 비전투 패트롤

        private float patrolMoveTimer;
        private float patrolMoveTime;
        private float patrolWaitTimer;
        private bool patrolMoving;
        private int patrolDir;

        private void OnIdleEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            spineController.PlayAnimation(Settings.AnimIdle, true);
            patrolMoving = false;
            patrolWaitTimer = 0f;
            patrolDir = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        private void OnIdleUpdate(float _dt)
        {
            if (CheckDetection())
            {
                isDetected = true;
                stateMachine.TransitState((int)EMonsterState.Trace);
                return;
            }

            if (patrolMoving)
            {
                patrolMoveTimer += _dt;
                if (patrolMoveTimer >= patrolMoveTime)
                {
                    curVelocityX = 0f;
                    patrolMoving = false;
                    patrolWaitTimer = 0f;
                    spineController.PlayAnimation(Settings.AnimIdle, true);
                }
                else
                {
                    curVelocityX = patrolDir * GetMoveSpeed() * Settings.MonsterPatrolSpeedRatio;
                    UpdateScaleX(patrolDir);
                }
            }
            else
            {
                patrolWaitTimer += _dt;
                if (patrolWaitTimer >= Settings.MonsterPatrolWaitTime)
                {
                    patrolDir *= -1;
                    patrolMoveTime = Random.Range(1f, 3f);
                    patrolMoveTimer = 0f;
                    patrolMoving = true;
                    spineController.PlayAnimation(Settings.AnimRun, true);
                }
            }
        }

        #endregion

        #region Trace — 영구 추적

        private float fidgetTimer;
        private int fidgetDir;
        private bool fidgetInitialized;

        private void OnTraceEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            fidgetInitialized = false;
            spineController.PlayAnimation(Settings.AnimRun, true);
        }

        private void OnTraceUpdate(float _dt)
        {
            var player = Main.Instance.Player;
            if (player == null) return;

            if (!IsSameLayer())
            {
                UpdateFidgetPatrol(_dt);
                return;
            }

            fidgetInitialized = false;

            if (IsInAttackRange())
            {
                stateMachine.TransitState((int)EMonsterState.Attack);
                return;
            }

            float dirX = player.Position.x - transform.position.x;
            curVelocityX = Mathf.Sign(dirX) * GetMoveSpeed();
            UpdateScaleX(dirX);
        }

        private void UpdateFidgetPatrol(float _dt)
        {
            if (!fidgetInitialized)
            {
                fidgetDir = Random.Range(0, 2) == 0 ? -1 : 1;
                fidgetTimer = 0f;
                fidgetInitialized = true;
            }

            float fidgetSpeed = GetMoveSpeed() * Settings.MonsterPatrolSpeedRatio;
            curVelocityX = fidgetDir * fidgetSpeed;
            UpdateScaleX(fidgetDir);

            fidgetTimer += _dt;
            if (fidgetTimer >= Settings.MonsterFidgetDistance / fidgetSpeed)
            {
                fidgetDir *= -1;
                fidgetTimer = 0f;
            }
        }

        #endregion

        #region Attack — 스킬 1회 실행

        private void OnAttackEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;

            var player = Main.Instance.Player;
            if (player != null)
                UpdateScaleX(player.Position.x - transform.position.x);

            ExecuteAttackAsync().Forget();
        }

        private void OnAttackUpdate(float _dt)
        {
            curVelocityX = 0f;
        }

        private async UniTaskVoid ExecuteAttackAsync()
        {
            string skillKey = PickSkillByRate();

            if (skillKey == null)
            {
                stateMachine.TransitState((int)EMonsterState.Trace);
                return;
            }

            try
            {
                await character.BSC.UseSkill(skillKey);
            }
            catch (System.OperationCanceledException) { }

            if (character.ObjectLifeState == FieldObject.FieldObjectLifeState.Live)
                stateMachine.TransitState((int)EMonsterState.Trace);
        }

        private string PickSkillByRate()
        {
            var skillList = character.SkillList;
            if (skillList == null || skillList.Count == 0) return null;

            var available = skillList.Where(s => !character.BSC.SSC.IsCooltime(s.SkillKey)).ToList();
            if (available.Count == 0) return null;

            int totalRate = available.Sum(x => x.SkillActivateRate);
            int rand = Random.Range(0, totalRate);
            int sum = 0;
            foreach (var s in available)
            {
                sum += s.SkillActivateRate;
                if (rand < sum) return s.SkillKey;
            }
            return available[available.Count - 1].SkillKey;
        }

        #endregion

        #region Dead — 사망 처리

        private void OnDeadEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            rb.linearVelocity = Vector2.zero;
            character.BSC.SSC.CancelAllSkills();
            ExecuteDeadAsync().Forget();
        }

        private async UniTaskVoid ExecuteDeadAsync()
        {
            spineController.PlayAnimation("Dead", false);

            try { await spineController.WaitForAnimCompleteAsync(); }
            catch (System.OperationCanceledException) { }

            character.OnDespawn();
            Main.Instance.MonsterManager.DespawnMonster(character);
        }

        #endregion
    }
}
```

- [ ] **Step 2: 커밋**

```
feat: MonsterController 작성 — FSM(Idle/Trace/Attack/Dead) + 물리 이동
```

---

### Task 4: MonsterManager 구현

**Files:**
- Modify: `Assets/02. Scripts/Core/Manager/MonsterManager.cs`

- [ ] **Step 1: MonsterManager.cs 전체 재작성**

```csharp
using MS.Field;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class MonsterManager
    {
        private List<MonsterCharacter> activeMonsters = new List<MonsterCharacter>();

        public IReadOnlyList<MonsterCharacter> ActiveMonsters => activeMonsters;


        public MonsterCharacter SpawnMonster(string _monsterKey, Vector3 _position)
        {
            var go = Main.Instance.ObjectPoolManager.Get(_monsterKey, _position);
            if (go == null) return null;

            var monster = go.GetComponent<MonsterCharacter>();
            monster.InitMonster(_monsterKey);
            activeMonsters.Add(monster);
            return monster;
        }

        public void DespawnMonster(MonsterCharacter _monster)
        {
            activeMonsters.Remove(_monster);
            Main.Instance.ObjectPoolManager.Return(_monster.MonsterKey, _monster.gameObject);
        }

        public void ClearAll()
        {
            for (int i = activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = activeMonsters[i];
                if (monster != null)
                {
                    monster.OnDespawn();
                    Main.Instance.ObjectPoolManager.Return(monster.MonsterKey, monster.gameObject);
                }
            }
            activeMonsters.Clear();
        }
    }
}
```

- [ ] **Step 2: 커밋**

```
feat: MonsterManager 구현 — Spawn/Despawn/ClearAll
```

---

### Task 5: Main.cs 연결 + SettingData 로딩 활성화

**Files:**
- Modify: `Assets/02. Scripts/Core/Main.cs`
- Modify: `Assets/02. Scripts/Data/SettingData/SettingData.cs`

- [ ] **Step 1: Main.cs OnDestroy에 MonsterManager 정리 추가**

`BattleObjectManager?.ClearBattleObject();` 줄 바로 위에 추가:

```csharp
MonsterManager?.ClearAll();
```

- [ ] **Step 2: SettingData.cs에서 MonsterSettingDict 로딩 주석 해제**

25~26번째 줄의 주석을 해제하여 활성화:

```csharp
TextAsset monsterJson = await Main.Instance.AddressableManager.LoadResourceAsync<TextAsset>("MonsterSettingData");
MonsterSettingDict = JsonConvert.DeserializeObject<Dictionary<string, MonsterSettingData>>(monsterJson.text);
```

- [ ] **Step 3: 커밋**

```
feat: Main에 MonsterManager 정리 연결 + MonsterSettingData 로딩 활성화
```

---

### Task 6: MonsterSettingData JSON 작성 (테스트용)

**Files:**
- Create: `Assets/04. Settings/MonsterSettingData.json`

- [ ] **Step 1: 테스트용 몬스터 JSON 데이터 작성**

Addressable 키 `MonsterSettingData`로 등록할 JSON. 테스트용 슬라임 1종:

```json
{
  "TestSlime": {
    "AttributeSetSettingData": {
      "MaxHealth": 50,
      "BaseAttackPower": 10,
      "SkillAttackPower": 10,
      "Defense": 5,
      "MoveSpeed": 80,
      "CriticChance": 0,
      "CriticMultiple": 150,
      "Evasion": 0,
      "LifeSteal": 0,
      "CooltimeAccel": 0,
      "AttackSpeed": 100,
      "AttackRange": 2,
      "WeaknessAttributeType": 0
    },
    "DropItemKey": "",
    "SkillList": []
  }
}
```

> SkillList는 비워둠 — 스킬 구현체가 아직 없으므로. 추후 몬스터 스킬 작성 시 추가.
> Unity에서 Addressable로 등록 필요 (키: `MonsterSettingData`).

- [ ] **Step 2: 커밋**

```
feat: 테스트용 MonsterSettingData JSON 추가 (TestSlime)
```

---

## 구현 순서 요약

| Task | 내용 | 의존성 |
|------|------|--------|
| 1 | Settings 상수 + Main.Player 등록 | 없음 |
| 2 | MonsterCharacter | Task 1 |
| 3 | MonsterController | Task 2 |
| 4 | MonsterManager | Task 2 |
| 5 | Main 연결 + SettingData 로딩 | Task 4 |
| 6 | 테스트 JSON 데이터 | Task 5 |
