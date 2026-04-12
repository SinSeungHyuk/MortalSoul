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
        protected AttributeSet attributeSet;
        protected SkillSettingData skillData;

        private float curCooltime;
        private float remainCooltime;

        public bool IsCooltime => remainCooltime > 0;
        public float CooltimeRemainRatio => curCooltime > 0 ? remainCooltime / curCooltime : 0f;
        public bool IsPostUseCooltime => skillData.IsPostUseCooltime;


        public virtual void InitSkill(SkillSystemComponent _ownerSSC, SkillSettingData _skillData)
        {
            ownerSSC = _ownerSSC;
            owner = _ownerSSC.Owner;
            attributeSet = _ownerSSC.AttributeSet;
            skillData = _skillData;

            curCooltime = _skillData.Cooltime;
            remainCooltime = 0;
        }

        public abstract UniTask ActivateSkillAsync(CancellationToken _token);

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
            remainCooltime = cooltime;
        }

        public void ResetCooltime()
        {
            remainCooltime = 0f;
        }

        public async UniTask SkillCastingAsync(CancellationToken _token)
        {
            // TODO: FieldCharacter에 Animator/Spine 애니메이션 인터페이스 연결 시 활성화
            await UniTask.WaitForSeconds(skillData.GetValue(ESkillValueType.Casting), cancellationToken: _token);
        }

        public void OnUpdate(float _deltaTime)
        {
            if (remainCooltime > 0)
                remainCooltime -= _deltaTime;
        }
    }
}
