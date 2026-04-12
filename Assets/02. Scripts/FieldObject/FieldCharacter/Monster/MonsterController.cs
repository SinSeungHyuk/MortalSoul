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
