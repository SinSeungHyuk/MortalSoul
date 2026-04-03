using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace MS.Skill
{
    public class Teleport : BaseSkill
    {
        private PlayerController playerController;
        private CharacterController cc;


        public override void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            base.InitSkill(_owner, _skillData);

            playerController = owner.GetComponent<PlayerCharacter>().PlayerController;
            cc = playerController.CC;
        }

        public override async UniTask ActivateSkill(CancellationToken token)
        {
            Vector3 moveDir = playerController.MoveDir;
            if (moveDir == Vector3.zero) moveDir = owner.transform.forward;

            float teleportRange = skillData.GetValue(ESkillValueType.Move);
            Vector3 currentPos = owner.Position;
            Vector3 targetPos = currentPos + (moveDir.normalized * teleportRange);

            NavMeshHit hit;
            if (NavMesh.Raycast(currentPos, targetPos, out hit, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }

            cc.enabled = false;
            EffectManager.Instance.PlayEffect("Eff_Teleport", owner.Position, owner.Rotation);
            owner.transform.position = targetPos;
            cc.enabled = true;

            await UniTask.CompletedTask;
        }
    }
}