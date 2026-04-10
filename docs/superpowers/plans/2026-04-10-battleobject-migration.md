# BattleObject 계층 이주 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 레퍼런스의 SkillObject 계층을 BattleObject로 이름 변경하고 2D 전환하여 현재 프로젝트에 이주한다.

**Architecture:** FieldObject를 상속하는 BattleObject(abstract) → ProjectileObject / AreaObject 계층. BattleObjectManager가 수명/풀 관리를 담당하며 Main 패턴으로 통합.

**Tech Stack:** Unity 6, C#, UniTask, Physics2D

---

### Task 1: BattleObject 기반 클래스

**Files:**
- Create: `Assets/02. Scripts/FieldObject/BattleObject/BattleObject.cs`

- [ ] **Step 1: BattleObject.cs 작성**

```csharp
using MS.Battle;
using System;
using UnityEngine;

namespace MS.Field
{
    public abstract class BattleObject : FieldObject
    {
        private const int INFINITE_ATTACK = int.MaxValue;
        private const float INFINITE_DURATION = float.MaxValue;

        protected Action<BattleObject, BattleSystemComponent> onHitCallback;

        protected FieldCharacter owner;
        protected LayerMask targetLayer;
        protected int hitCountPerAttack;
        protected int maxAttackCount;
        protected FieldObject traceTarget;
        protected Vector3 targetOffset;

        private string battleObjectKey;
        private float duration;
        private float elapsedTime;

        public string BattleObjectKey => battleObjectKey;


        public virtual void OnUpdate(float _deltaTime)
        {
            elapsedTime += _deltaTime;
            if (elapsedTime >= duration)
            {
                ObjectLifeState = FieldObjectLifeState.Death;
            }
        }

        public void InitBattleObject(string _battleObjectKey, FieldCharacter _owner, LayerMask _targetLayer)
        {
            ObjectLifeState = FieldObjectLifeState.Live;
            ObjectType = FieldObjectType.SkillObject;

            battleObjectKey = _battleObjectKey;
            owner = _owner;
            targetLayer = _targetLayer;

            traceTarget = null;
            elapsedTime = 0;
            duration = INFINITE_DURATION;
            maxAttackCount = INFINITE_ATTACK;
            hitCountPerAttack = 1;
        }

        public void SetHitCountPerAttack(int _hitCountPerAttack)
            => hitCountPerAttack = _hitCountPerAttack;
        public void SetMaxHitCount(int _maxHitCount)
            => maxAttackCount = _maxHitCount;
        public void SetDuration(float _duration)
            => duration = _duration;
        public void SetHitCallback(Action<BattleObject, BattleSystemComponent> _onHitCallback)
            => onHitCallback = _onHitCallback;

        public void SetTraceTarget(FieldObject _target, Vector3 _offset = default)
        {
            traceTarget = _target;
            targetOffset = _offset;
        }
        public void ClearTraceTarget()
            => traceTarget = null;

        protected bool IsValidTarget(Collider2D _other, out BattleSystemComponent _bsc)
        {
            _bsc = null;
            if (((1 << _other.gameObject.layer) & targetLayer) == 0)
                return false;
            if (_other.TryGetComponent(out FieldCharacter fieldChar))
            {
                _bsc = fieldChar.BSC;
                return _bsc != null;
            }
            return false;
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/BattleObject/BattleObject.cs"
git commit -m "feat: BattleObject 기반 클래스 추가"
```

---

### Task 2: ProjectileObject

**Files:**
- Create: `Assets/02. Scripts/FieldObject/BattleObject/ProjectileObject.cs`

- [ ] **Step 1: ProjectileObject.cs 작성**

```csharp
using MS.Battle;
using UnityEngine;

namespace MS.Field
{
    public class ProjectileObject : BattleObject
    {
        private Vector2 moveDir;
        private float moveSpeed;


        public void InitProjectile(Vector2 _moveDir, float _moveSpeed)
        {
            moveDir = _moveDir.normalized;
            moveSpeed = _moveSpeed;
        }

        public void SetMoveSpeed(float _speed)
        {
            moveSpeed = _speed;
        }

        public void SetMoveDir(Vector2 _dir)
        {
            moveDir = _dir.normalized;
        }


        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (IsValidTarget(_other, out BattleSystemComponent _bsc))
            {
                for (int i = 0; i < hitCountPerAttack; i++)
                {
                    onHitCallback?.Invoke(this, _bsc);
                }
                maxAttackCount--;
            }

            if (maxAttackCount <= 0)
                ObjectLifeState = FieldObjectLifeState.Death;
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            if (traceTarget != null && traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
            {
                Vector2 targetPos = (Vector2)traceTarget.Position + (Vector2)targetOffset;
                Vector2 curPos = transform.position;
                transform.position = Vector2.MoveTowards(curPos, targetPos, moveSpeed * _deltaTime);

                Vector2 dir = targetPos - curPos;
                if (dir.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
            else
            {
                transform.position += (Vector3)(moveDir * moveSpeed * _deltaTime);
            }
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/BattleObject/ProjectileObject.cs"
git commit -m "feat: ProjectileObject 2D 투사체 클래스 추가"
```

---

### Task 3: AreaObject

**Files:**
- Create: `Assets/02. Scripts/FieldObject/BattleObject/AreaObject.cs`

- [ ] **Step 1: AreaObject.cs 작성**

```csharp
using MS.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class AreaObject : BattleObject
    {
        private List<FieldCharacter> attackTargetList = new List<FieldCharacter>();
        private float attackInterval;
        private float elapsedAttackTime;
        private float delayTime;
        private float elapsedDelayTime;


        public void InitArea()
        {
            attackTargetList.Clear();
            attackInterval = 0f;
            elapsedAttackTime = 0f;
            delayTime = 0f;
            elapsedDelayTime = 0f;
        }

        public void SetDelay(float _delay)
            => delayTime = _delay;
        public void SetAttackInterval(float _attackInterval)
        {
            attackInterval = _attackInterval;
            elapsedAttackTime = attackInterval;
        }


        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.TryGetComponent(out FieldCharacter fieldChar))
            {
                if (((1 << _other.gameObject.layer) & targetLayer) != 0)
                    attackTargetList.Add(fieldChar);
            }
        }

        private void OnTriggerExit2D(Collider2D _other)
        {
            if (_other.TryGetComponent(out FieldCharacter fieldChar))
                attackTargetList.Remove(fieldChar);
        }

        public override void OnUpdate(float _deltaTime)
        {
            base.OnUpdate(_deltaTime);

            if (traceTarget != null)
            {
                if (traceTarget.ObjectLifeState == FieldObjectLifeState.Live)
                    transform.position = traceTarget.Position + targetOffset;
                else
                    ClearTraceTarget();
            }

            if (delayTime > 0f && elapsedDelayTime < delayTime)
            {
                elapsedDelayTime += _deltaTime;
                return;
            }
            else if (elapsedDelayTime >= delayTime)
            {
                delayTime = 0f;
                elapsedDelayTime = 0f;
            }

            elapsedAttackTime += _deltaTime;
            if (elapsedAttackTime < attackInterval)
                return;

            if (maxAttackCount <= 0) return;
            maxAttackCount--;

            for (int i = attackTargetList.Count - 1; i >= 0; i--)
            {
                var attackTarget = attackTargetList[i];
                if (!attackTarget || attackTarget.ObjectLifeState != FieldObjectLifeState.Live)
                {
                    attackTargetList.RemoveAt(i);
                    continue;
                }

                for (int hitCount = 0; hitCount < hitCountPerAttack; hitCount++)
                {
                    onHitCallback?.Invoke(this, attackTarget.BSC);
                }
            }
            elapsedAttackTime = 0f;
        }
    }
}
```

- [ ] **Step 2: Unity 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add "Assets/02. Scripts/FieldObject/BattleObject/AreaObject.cs"
git commit -m "feat: AreaObject 2D 장판 클래스 추가"
```

---

### Task 4: BattleObjectManager + Main 통합

**Files:**
- Create: `Assets/02. Scripts/Core/Manager/BattleObjectManager.cs`
- Modify: `Assets/02. Scripts/Core/Main.cs`

- [ ] **Step 1: BattleObjectManager.cs 작성**

```csharp
using Cysharp.Threading.Tasks;
using MS.Field;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class BattleObjectManager
    {
        private List<BattleObject> battleObjectList = new List<BattleObject>();
        private List<BattleObject> releaseBattleObjectList = new List<BattleObject>();


        public T SpawnBattleObject<T>(string _key, FieldCharacter _owner, LayerMask _targetLayer) where T : BattleObject
        {
            T battleObject = Main.Instance.ObjectPoolManager
                .Get(_key, _owner.transform).GetComponent<T>();

            if (battleObject)
            {
                battleObjectList.Add(battleObject);
                battleObject.InitBattleObject(_key, _owner, _targetLayer);
            }
            return battleObject;
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (BattleObject battleObject in battleObjectList)
            {
                battleObject.OnUpdate(_deltaTime);
                if (battleObject.ObjectLifeState == FieldObject.FieldObjectLifeState.Death)
                    releaseBattleObjectList.Add(battleObject);
            }

            foreach (BattleObject releaseObject in releaseBattleObjectList)
            {
                Main.Instance.ObjectPoolManager.Return(
                    releaseObject.BattleObjectKey, releaseObject.gameObject);
                battleObjectList.Remove(releaseObject);
            }
            releaseBattleObjectList.Clear();
        }

        public void ClearBattleObject()
        {
            battleObjectList.Clear();
        }

        public async UniTask LoadAllBattleObjectAsync()
        {
            try
            {
                var tasks = new List<UniTask>
                {
                };

                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
```

- [ ] **Step 2: Main.cs 수정 — BattleObjectManager 프로퍼티 추가**

`Main.cs`에서 `MonsterManager` 프로퍼티 아래에 추가:

```csharp
public BattleObjectManager BattleObjectManager { get; private set; }
```

`Awake()` 메서드 끝에 추가:

```csharp
BattleObjectManager = new BattleObjectManager();
```

- [ ] **Step 3: Unity 컴파일 확인**

Unity 에디터에서 컴파일 에러가 없는지 확인한다.

- [ ] **Step 4: 커밋**

```bash
git add "Assets/02. Scripts/Core/Manager/BattleObjectManager.cs"
git add "Assets/02. Scripts/Core/Main.cs"
git commit -m "feat: BattleObjectManager 추가 및 Main 통합"
```

---

### Task 5: 작업요청 문서 이동

- [ ] **Step 1: 작업요청 문서를 InProgress로 이동**

```bash
mv "MortalSoulDoc/01. WorkReq/BattleObject 계층 이주.md" "MortalSoulDoc/02. InProgress/BattleObject 계층 이주.md"
```

이 단계는 구현 시작 시점에 수행한다.
