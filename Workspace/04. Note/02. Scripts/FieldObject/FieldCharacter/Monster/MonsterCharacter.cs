using Cysharp.Threading.Tasks;
using DG.Tweening;
using MS.Core.StateMachine;
using MS.Data;
using MS.Manager;
using MS.Skill;
using MS.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace MS.Field
{
    public class MonsterCharacter : FieldCharacter
    {
        private string monsterKey;
        private string dropItemKey;
        private bool isBoss;
        private MSStateMachine<MonsterCharacter> monsterStateMachine;
        private List<MonsterSkillSettingData> skillList = new List<MonsterSkillSettingData>();
        private NavMeshAgent navMeshAgent;

        public bool IsBoss => isBoss;

        public enum MonsterState
        {
            Idle,
            Trace,
            Attack,
            Dead,
        }


        protected override void Awake()
        {
            base.Awake();

            navMeshAgent = GetComponent<NavMeshAgent>();
            monsterStateMachine = new MSStateMachine<MonsterCharacter>(this);
            monsterStateMachine.RegisterState((int)MonsterState.Idle, OnIdleEnter, OnIdleUpdate, OnIdleExit);
            monsterStateMachine.RegisterState((int)MonsterState.Trace, OnTraceEnter, OnTraceUpdate, OnTraceExit);
            monsterStateMachine.RegisterState((int)MonsterState.Attack, OnAttackEnter, OnAttackUpdate, OnAttackExit);
            monsterStateMachine.RegisterState((int)MonsterState.Dead, OnDeadEnter, OnDeadUpdate, OnDeadExit);
        }

        private void OnEnable()
        {
            if (SSC != null)
            {
                SSC.OnDeadCallback += OnDeadCallback;
                SSC.OnHitCallback += OnHitCallback; 
            }
        }
        private void OnDisable()
        {
            if (SSC != null)
            {
                SSC.ClearSSC();
            }
        }

        public void InitMonster(string _monsterKey)
        {
            ObjectType = FieldObjectType.Monster;
            ObjectLifeState = FieldObjectLifeState.Live;
            monsterKey = _monsterKey;
            isBoss = false;

            if (!DataManager.Instance.MonsterSettingDataDict.TryGetValue(_monsterKey, out MonsterSettingData _monsterData))
            {
                Debug.LogError($"InitMonster Key Missing : {_monsterKey}");
                return;
            }

            MonsterAttributeSet monsterAttributeSet = new MonsterAttributeSet();
            monsterAttributeSet.InitAttributeSet(_monsterData.AttributeSetSettingData);
            monsterAttributeSet.MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
            SSC.InitSSC(this, monsterAttributeSet);

            foreach (var skillInfo in _monsterData.SkillList)
            {
                SSC.GiveSkill(skillInfo.SkillKey);
            }
            skillList = _monsterData.SkillList;
            dropItemKey = _monsterData.DropItemKey;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(Position, out hit, 5.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
                navMeshAgent.enabled = true;
            }
            navMeshAgent.stoppingDistance = _monsterData.AttributeSetSettingData.AttackRange - 1f;
            navMeshAgent.speed = _monsterData.AttributeSetSettingData.MoveSpeed;
            monsterStateMachine.TransitState((int)MonsterState.Idle);
        }

        public void OnUpdate(float _deltaTime)
        {
            if (IsStunned && ObjectLifeState == FieldObjectLifeState.Live) 
                return; 

            monsterStateMachine.OnUpdate(_deltaTime);
        }

        public void SetBossMonster()
        {
            transform.localScale *= Settings.BossScaleMultiple;
            this.ApplyStatEffect("BossMaxHealth", EStatType.MaxHealth, 250, EBonusType.Percentage);
            this.ApplyStatEffect("BossAttackPower", EStatType.AttackPower, 200, EBonusType.Percentage);
            SSC.AttributeSet.Health = SSC.AttributeSet.GetStatValueByType(EStatType.MaxHealth);
            dropItemKey = "BossChest"; // 보스는 보스상자를 떨구도록 드랍키 변경
            isBoss = true;
        }


        #region Override
        public override void ApplyKnockback(Vector3 _dir, float _force)
        {
            if (ObjectLifeState != FieldObjectLifeState.Live) return;

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();

            DOVirtual.Float(_force, 0, 0.2f, (currentForce) =>
            {
                if (ObjectLifeState != FieldObjectLifeState.Live) return;

                Vector3 moveVec = _dir * currentForce * Time.deltaTime;
                navMeshAgent.Move(moveVec);
            })
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                if (ObjectLifeState == FieldObjectLifeState.Live)
                {
                    navMeshAgent.isStopped = false;
                }
            });
        }

        public override void ApplyStun(bool _isStunned)
        {
            if (ObjectLifeState != FieldObjectLifeState.Live) return;

            IsStunned = _isStunned;

            if (IsStunned)
            {
                navMeshAgent.isStopped = true;
                SSC.CancelAllSkills();
                monsterStateMachine.TransitState((int)MonsterState.Idle);
                Animator.SetTrigger(Settings.AnimHashIdle);
            }
            else
            {
                navMeshAgent.isStopped = false;
            }
        }
        #endregion


        #region Callback
        private void OnHitCallback(int _damage, bool _isCritic)
        {
            GameplayCueManager.Instance.PlayCue("GC_MonsterHit", this);
            UIManager.Instance.ShowDamageText(Position,_damage, _isCritic);
        }

        private void OnDeadCallback()
        {
            ObjectLifeState = FieldObjectLifeState.Dying;
            monsterStateMachine.TransitState((int)MonsterState.Dead);
        }

        private void OnMoveSpeedChanged(float _newSpeed)
        {
            navMeshAgent.speed = _newSpeed;
        }
        #endregion


        #region Idle
        private float elapsedIdleTime = 0f;
        private void OnIdleEnter(int _prev, object[] _params)
        {
            navMeshAgent.ResetPath();
            Animator.SetTrigger(Settings.AnimHashIdle);
            attackRange = SSC.AttributeSet.GetStatValueByType(EStatType.AttackRange);
            elapsedIdleTime = 0f;
        }
        private void OnIdleUpdate(float _dt) // Idle => Attack or Trace
        {
            elapsedIdleTime += _dt;
            if (elapsedIdleTime > 0.2f)
            {
                if ((PlayerManager.Instance.Player.Position - Position).sqrMagnitude < (attackRange * attackRange))
                    monsterStateMachine.TransitState((int)MonsterState.Attack);
                else
                    monsterStateMachine.TransitState((int)MonsterState.Trace);
            }
        }
        private void OnIdleExit(int _next)
        {
            
        }
        #endregion

        #region Trace
        private float attackRange = 0f;
        private void OnTraceEnter(int _prev, object[] _params)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = PlayerManager.Instance.Player.Position;
            Animator.SetTrigger(Settings.AnimHashRun);
        }
        private void OnTraceUpdate(float _dt) // Trace => Attack
        {
            navMeshAgent.destination = PlayerManager.Instance.Player.Position;
            
            if ((PlayerManager.Instance.Player.Position - Position).sqrMagnitude < (attackRange* attackRange))
            {
                monsterStateMachine.TransitState((int)MonsterState.Attack);
            }
        }
        private void OnTraceExit(int _next)
        {
            
        }
        #endregion

        #region Attack
        private MonsterSkillSettingData currentSkillData;
        private bool isSkillUsed = false;
        private float elapsedTime = 0f;
        private void OnAttackEnter(int _prev, object[] _params)
        {
            navMeshAgent.isStopped = true;
            isSkillUsed = false;

            // 소유한 스킬리스트에서 랜덤으로 사용할 스킬 선택
            MonsterSkillSettingData skillData = null;
            int totalRatio = skillList.Sum(x => x.SkillActivateRate);
            int ratioSum = 0;
            int randomRate = Random.Range(0, totalRatio);
            foreach (var skillInfo in skillList)
            {
                ratioSum += skillInfo.SkillActivateRate;
                if (randomRate < ratioSum)
                {
                    skillData = skillInfo;
                    break;
                }
            }
            currentSkillData = skillData;
        }
        private void OnAttackUpdate(float _dt) // Attack => Idle
        {
            if (currentSkillData == null) return;

            if (!isSkillUsed)
            {
                Vector3 direction = PlayerManager.Instance.Player.Position - transform.position;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction); // 플레이어를 향해 회전
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _dt * 5f);
                }
                if (!SSC.IsCooltime(currentSkillData.SkillKey))
                {
                    SSC.UseSkill(currentSkillData.SkillKey).Forget();
                    Animator.SetTrigger(currentSkillData.AnimTriggerKey);
                    isSkillUsed = true;
                }
                return;
            }

            elapsedTime += _dt;
            if (elapsedTime >= currentSkillData.SkillDuration)
            {
                monsterStateMachine.TransitState((int)MonsterState.Idle);
                elapsedTime = 0f;
            }
        }
        private void OnAttackExit(int _next)
        {
           
        }
        #endregion

        #region Dead
        private float elapsedDeadTime = 0f;
        private void OnDeadEnter(int _prev, object[] _params)
        {
            elapsedDeadTime = 0f;
            IsStunned = false;
            navMeshAgent.ResetPath();
            Animator.SetTrigger(Settings.AnimHashDead);
            SSC.CancelAllSkills();

            FieldItemManager.Instance.SpawnFieldItem(dropItemKey, Position);
        }
        private void OnDeadUpdate(float _dt)
        {
            elapsedDeadTime += _dt;
            if (elapsedDeadTime > 2f)
            {
                ObjectLifeState = FieldObjectLifeState.Death;
                ObjectPoolManager.Instance.Return(monsterKey, this.gameObject);
            }
        }
        private void OnDeadExit(int _next)
        {

        }
        #endregion
    }
}