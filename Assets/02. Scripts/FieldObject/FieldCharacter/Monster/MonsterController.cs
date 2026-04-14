using Core;
using Cysharp.Threading.Tasks;
using MS.Battle;
using MS.Core.StateMachine;
using MS.Utils;
using UnityEngine;

namespace MS.Field
{
    public enum EMonsterState { Idle, Trace, Attack, Dead }

    public class MonsterController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private SpineController spineController;
        private MonsterCharacter monster;
        private MSStateMachine<MonsterController> stateMachine;

        public Rigidbody2D Rb => rb;

        private float curVelocityX;
        private float elapsedPatrolTime;
        private float patrolMoveTime;
        private float elapsedPatrolWaitTime;
        private bool curPatrolMoving;
        private int patrolDir;


        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spineController = GetComponent<SpineController>();
        }

        public void InitController(MonsterCharacter _character)
        {
            monster = _character;
            rb.gravityScale = Settings.GravityScale;
            rb.linearVelocity = Vector2.zero;
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
            UpdateFallGravity();
            rb.linearVelocity = new Vector2(curVelocityX, rb.linearVelocityY);
        }

        public void SetMonsterState(EMonsterState _state)
        {
            stateMachine.TransitState((int)_state);
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

        private bool IsSameLayerPlayer()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            return Mathf.Abs(player.Position.y - transform.position.y) <= Settings.MonsterLayerThresholdY;
        }

        private bool IsPlayerDetect()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            if (!IsSameLayerPlayer()) return false;
            return Mathf.Abs(player.Position.x - transform.position.x) < Settings.MonsterDetectionRange;
        }

        private bool IsInAttackRange()
        {
            var player = Main.Instance.Player;
            if (player == null) return false;
            return Mathf.Abs(player.Position.x - transform.position.x) < monster.BSC.AttributeSet.GetStatValueByType(EStatType.AttackRange);
        }

        private float GetMoveSpeed()
        {
            return monster.BSC.AttributeSet.GetStatValueByType(EStatType.MoveSpeed) / 100f * Settings.MoveSpeed;
        }


        private void ResetPatrol()
        {
            curPatrolMoving = false;
            elapsedPatrolTime = 0f;
            elapsedPatrolWaitTime = 0f;
            patrolDir = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        #region Idle 패트롤

        private void OnIdleEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            spineController.PlayAnimation("idle", true);
            ResetPatrol();
        }

        private void OnIdleUpdate(float _dt)
        {
            if (IsPlayerDetect())
            {
                stateMachine.TransitState((int)EMonsterState.Trace);
                return;
            }

            if (curPatrolMoving)
            {
                elapsedPatrolTime += _dt;
                if (elapsedPatrolTime >= patrolMoveTime)
                {
                    curVelocityX = 0f;
                    curPatrolMoving = false;
                    elapsedPatrolWaitTime = 0f;
                    spineController.PlayAnimation("idle", true);
                }
                else
                {
                    curVelocityX = patrolDir * GetMoveSpeed();
                    UpdateScaleX(patrolDir);
                }
            }
            else
            {
                elapsedPatrolWaitTime += _dt;
                if (elapsedPatrolWaitTime >= Settings.MonsterPatrolWaitTime)
                {
                    patrolDir *= -1;
                    patrolMoveTime = Random.Range(0.5f, 2f);
                    elapsedPatrolTime = 0f;
                    curPatrolMoving = true;
                    spineController.PlayAnimation("run", true);
                }
            }
        }

        #endregion

        #region Trace — 추적

        private void OnTraceEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            curPatrolMoving = false;
            spineController.PlayAnimation("run", true);
        }

        private void OnTraceUpdate(float _dt)
        {
            var player = Main.Instance.Player;
            if (player == null) return;

            bool inAttackRange = IsInAttackRange();

            if (inAttackRange && IsSameLayerPlayer())
            {
                stateMachine.TransitState((int)EMonsterState.Attack);
                return;
            }

            if (inAttackRange && !IsSameLayerPlayer())
            {
                if (!curPatrolMoving)
                {
                    patrolDir = Random.Range(0, 2) == 0 ? -1 : 1;
                    patrolMoveTime = Random.Range(0.5f, 1.5f);
                    elapsedPatrolTime = 0f;
                    curPatrolMoving = true;
                }

                curVelocityX = patrolDir * GetMoveSpeed();
                UpdateScaleX(patrolDir);

                elapsedPatrolTime += _dt;
                if (elapsedPatrolTime >= patrolMoveTime)
                {
                    patrolDir *= -1;
                    patrolMoveTime = Random.Range(0.5f, 1.5f);
                    elapsedPatrolTime = 0f;
                }
                return;
            }

            curPatrolMoving = false;
            float dirX = player.Position.x - transform.position.x;
            curVelocityX = Mathf.Sign(dirX) * GetMoveSpeed();
            UpdateScaleX(dirX);
        }

        #endregion

        #region Attack — 스킬 실행

        private void OnAttackEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;

            var player = Main.Instance.Player;
            if (player != null)
                UpdateScaleX(player.Position.x - transform.position.x);

            MonsterAttackAsync().Forget();
        }

        private void OnAttackUpdate(float _dt)
        {
            curVelocityX = 0f;
        }

        private async UniTaskVoid MonsterAttackAsync()
        {
            string skillKey = GetUseSkillKey();

            if (skillKey == null)
            {
                stateMachine.TransitState((int)EMonsterState.Trace);
                return;
            }

            try
            {
                await monster.BSC.UseSkill(skillKey);
            }
            catch (System.OperationCanceledException) { }

            if (monster.ObjectLifeState == FieldObject.FieldObjectLifeState.Live)
                stateMachine.TransitState((int)EMonsterState.Trace);
        }

        private string GetUseSkillKey()
        {
            var skillList = monster.SkillList;
            if (skillList == null || skillList.Count == 0) return null;

            int totalRate = 0;
            for (int i = 0; i < skillList.Count; i++)
            {
                if (!monster.BSC.SSC.IsCooltime(skillList[i].SkillKey))
                    totalRate += skillList[i].SkillActivateRate;
            }
            if (totalRate == 0) return null;

            int rand = Random.Range(0, totalRate);
            int sum = 0;
            for (int i = 0; i < skillList.Count; i++)
            {
                if (monster.BSC.SSC.IsCooltime(skillList[i].SkillKey)) continue;
                sum += skillList[i].SkillActivateRate;
                if (rand < sum) return skillList[i].SkillKey;
            }
            return null;
        }

        #endregion

        #region Dead — 사망 처리

        private void OnDeadEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            rb.linearVelocity = Vector2.zero;
            monster.BSC.SSC.CancelAllSkills();
            DeadAsync().Forget();
        }

        private async UniTaskVoid DeadAsync()
        {
            spineController.PlayAnimation("dead", false);

            try { await spineController.WaitForAnimCompleteAsync(); }
            catch (System.OperationCanceledException) { }

            monster.OnDead();
        }

        #endregion
    }
}
