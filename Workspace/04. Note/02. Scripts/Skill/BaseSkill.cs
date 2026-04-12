using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Skill;
using MS.Utils;
using System.Threading;
using UnityEngine;


namespace MS.Skill
{
    public abstract class BaseSkill
    {
        protected SkillSystemComponent ownerSSC;
        protected BaseAttributeSet attributeSet;
        protected FieldCharacter owner;
        protected SkillSettingData skillData;

        private float curCooltime;
        private float elapsedCooltime;
        private int curSkillLevel;

        private float totalDamageDealt; // 스킬로 누적된 총 데미지
        private float acquiredTime; // 스킬이 획득된 시간


        public SkillSettingData SkillData => skillData;
        public bool IsCooltime => elapsedCooltime > 0;
        public bool IsPostUseCooltime => skillData.IsPostUseCooltime;
        public int CurSkillLevel => curSkillLevel;
        public float CooltimeRatio => elapsedCooltime / curCooltime;

        public float TotalDamageDealt => totalDamageDealt;
        public float DPS
        {
            get
            {
                float duration = Time.time - acquiredTime;
                if (duration <= 0) return 0;
                return totalDamageDealt / duration;
            }
        }


        public virtual void InitSkill(SkillSystemComponent _owner, SkillSettingData _skillData)
        {
            ownerSSC = _owner;
            attributeSet = ownerSSC.AttributeSet;
            owner = ownerSSC.Owner;
            skillData = _skillData;
            curCooltime = 0;
            elapsedCooltime = 0;
            curSkillLevel = 1;
            totalDamageDealt = 0f;
            acquiredTime = Time.time;
        }

        public abstract UniTask ActivateSkill(CancellationToken token);

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


        #region Util
        public virtual bool CanActivateSkill()
            => true;

        // 스킬 캐스팅 세팅 (플레이어만 호출할 함수)
        public async UniTask SetSkillCasting(CancellationToken token)
        {
            owner.Animator.SetBool(Settings.AnimHashCasting, true);
            await UniTask.WaitForSeconds(skillData.SkillValueDict[ESkillValueType.Casting]);
            owner.Animator.SetBool(Settings.AnimHashCasting, false);
        }

        public void AddTotalDamageDealt(float _damage)
        {
            totalDamageDealt += _damage;
        }
        #endregion


        public void OnUpdate(float _deltaTime)
        {
            if (elapsedCooltime > 0)
            {
                elapsedCooltime -= _deltaTime;
            }
        }
    }
}