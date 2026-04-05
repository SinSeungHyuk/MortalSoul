using MS.Field;
using UnityEngine;

namespace MS.Battle
{
    public abstract class BaseSkill
    {
        protected SkillSystemComponent ownerSSC;
        protected FieldCharacter owner;
        protected BaseAttributeSet attributeSet;

        private float cooltime;
        private float elapsedCooltime;

        public bool IsCooltime => elapsedCooltime < cooltime;
        public float CooltimeRatio => cooltime > 0 ? Mathf.Clamp01(elapsedCooltime / cooltime) : 1f;


        public void InitSkill(SkillSystemComponent _ownerSSC, float _cooltime)
        {
            ownerSSC = _ownerSSC;
            owner = _ownerSSC.Owner;
            attributeSet = _ownerSSC.AttributeSet;
            cooltime = _cooltime;
            elapsedCooltime = _cooltime; // 초기: 쿨타임 완료 상태 (즉시 사용 가능)
        }

        public abstract void ActivateSkill();

        public virtual bool CanActivateSkill() => true;

        public void SetCooltime()
        {
            elapsedCooltime = 0f;
        }

        public void OnUpdate(float _deltaTime)
        {
            if (elapsedCooltime < cooltime)
                elapsedCooltime += _deltaTime;
        }
    }
}
