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
        private float patrolTimer;
        private float patrolDuration;
        private int patrolDir;
        private bool curPatrolMoving;
        private bool isAttacking;
        private bool wasTracing;


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
            patrolTimer = 0f;
            patrolDuration = Settings.MonsterPatrolWaitTime;
            patrolDir = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        private void UpdatePatrol(float _dt)
        {
            patrolTimer += _dt;

            if (patrolTimer < patrolDuration)
            {
                if (curPatrolMoving)
                {
                    curVelocityX = patrolDir * GetMoveSpeed();
                    UpdateScaleX(patrolDir);
                }
                return;
            }

            patrolTimer = 0f;
            curPatrolMoving = !curPatrolMoving;

            if (curPatrolMoving)
            {
                patrolDir *= -1;
                patrolDuration = Random.Range(0.5f, 1.5f);
                spineController.PlayAnimation("run", true);
            }
            else
            {
                patrolDuration = Settings.MonsterPatrolWaitTime;
                curVelocityX = 0f;
                spineController.PlayAnimation("idle", true);
            }
        }

        #region Idle — 플레이어 미인지 시 패트롤

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

            UpdatePatrol(_dt);
        }

        #endregion

        #region Trace — 같은 층이면 추적, 아니면 패트롤

        private void OnTraceEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            ResetPatrol();
            spineController.PlayAnimation("run", true);
            wasTracing = true;
        }

        private void OnTraceUpdate(float _dt)
        {
            var player = Main.Instance.Player;
            if (player == null) return;

            if (!IsSameLayerPlayer())
            {
                if (wasTracing)
                {
                    wasTracing = false;
                    ResetPatrol();
                }
                UpdatePatrol(_dt);
                return;
            }

            if (!wasTracing)
            {
                wasTracing = true;
                spineController.PlayAnimation("run", true);
            }

            if (IsInAttackRange())
            {
                stateMachine.TransitState((int)EMonsterState.Attack);
                return;
            }

            float dirX = player.Position.x - transform.position.x;
            curVelocityX = Mathf.Sign(dirX) * GetMoveSpeed();
            UpdateScaleX(dirX);
        }

        #endregion

        #region Attack — 스킬 실행

        private void OnAttackEnter(int _prev, object[] _params)
        {
            curVelocityX = 0f;
            isAttacking = false;
            spineController.PlayAnimation("idle", true);

            var player = Main.Instance.Player;
            if (player != null)
                UpdateScaleX(player.Position.x - transform.position.x);
        }

        private void OnAttackUpdate(float _dt)
        {
            curVelocityX = 0f;

            if (isAttacking) return;

            if (!IsInAttackRange() || !IsSameLayerPlayer())
            {
                stateMachine.TransitState((int)EMonsterState.Trace);
                return;
            }

            string skillKey = GetUseSkillKey();
            if (skillKey != null)
                MonsterAttackAsync(skillKey).Forget();
        }

        private async UniTaskVoid MonsterAttackAsync(string _skillKey)
        {
            isAttacking = true;
            try
            {
                await monster.BSC.UseSkill(_skillKey);
            }
            catch (System.OperationCanceledException) { }
            finally
            {
                isAttacking = false;
                if (monster.ObjectLifeState == FieldObject.FieldObjectLifeState.Live
                    && stateMachine.IsCurState((int)EMonsterState.Attack))
                    spineController.PlayAnimation("idle", true);
            }
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
