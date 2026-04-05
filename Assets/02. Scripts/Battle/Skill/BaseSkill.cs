using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Utils;
using System.Threading;
using UnityEngine;

namespace MS.Battle
{
    public abstract class BaseSkill
    {
        protected SkillSystemComponent ownerSSC;
        protected FieldCharacter owner;
        protected BaseAttributeSet attributeSet;
        protected SkillSettingData skillData;

        private float curCooltime;
        private float elapsedCooltime;

        public bool IsCooltime => elapsedCooltime > 0;
        public float CooltimeRatio => curCooltime > 0 ? elapsedCooltime / curCooltime : 0f;
        public bool IsPostUseCooltime => skillData.IsPostUseCooltime;


        public virtual void InitSkill(SkillSystemComponent _ownerSSC, SkillSettingData _skillData)
        {
            ownerSSC = _ownerSSC;
            owner = _ownerSSC.Owner;
            attributeSet = _ownerSSC.AttributeSet;
            skillData = _skillData;

            curCooltime = _skillData.Cooltime;
            elapsedCooltime = 0;
        }

        public abstract UniTask ActivateSkill(CancellationToken token);

        public virtual bool CanActivateSkill() => true;

        public void SetCooltime()
        {
            float cooltime = skillData.Cooltime;
            float cooltimeAccel = attributeSet.GetStatValueByType(EStatType.CooltimeAccel);
            if (cooltimeAccel > 0)
            {
                float cooltimePercent = MathUtils.BattleScaling(cooltimeAccel);
                cooltime = MathUtils.DecreaseByPercent(cooltime, cooltimePercent);
            }

            curCooltime = cooltime;
            elapsedCooltime = cooltime;
        }

        public async UniTask SetSkillCasting(CancellationToken token)
        {
            // TODO: FieldCharacter에 Animator/Spine 애니메이션 인터페이스 연결 시 활성화
            await UniTask.WaitForSeconds(skillData.GetValue(ESkillValueType.Casting), cancellationToken: token);
        }

        public void OnUpdate(float _deltaTime)
        {
            if (elapsedCooltime > 0)
                elapsedCooltime -= _deltaTime;
        }
    }
}
