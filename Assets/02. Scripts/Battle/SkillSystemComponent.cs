using MS.Field;
using System;
using System.Collections.Generic;

namespace MS.Battle
{
    public class SkillSystemComponent
    {
        public FieldCharacter Owner { get; private set; }
        public BaseAttributeSet AttributeSet { get; private set; }

        private Dictionary<string, BaseSkill> ownedSkillDict;

        public event Action<string, BaseSkill> OnSkillAdded;
        public event Action<string> OnSkillUsed;


        public void InitSSC(FieldCharacter _owner, BaseAttributeSet _attributeSet)
        {
            Owner = _owner;
            AttributeSet = _attributeSet;
            ownedSkillDict = new Dictionary<string, BaseSkill>();
        }

        public void GiveSkill(string _key, BaseSkill _skill, float _cooltime)
        {
            if (ownedSkillDict.ContainsKey(_key))
                return;

            _skill.InitSkill(this, _cooltime);
            ownedSkillDict.Add(_key, _skill);
            OnSkillAdded?.Invoke(_key, _skill);
        }

        public bool UseSkill(string _key)
        {
            if (!ownedSkillDict.TryGetValue(_key, out BaseSkill skill))
                return false;

            if (skill.IsCooltime)
                return false;

            if (!skill.CanActivateSkill())
                return false;

            skill.ActivateSkill();
            skill.SetCooltime();
            OnSkillUsed?.Invoke(_key);
            return true;
        }

        public bool IsCooltime(string _key)
        {
            if (ownedSkillDict.TryGetValue(_key, out BaseSkill skill))
                return skill.IsCooltime;
            return false;
        }

        public bool HasSkill(string _key)
        {
            return ownedSkillDict.ContainsKey(_key);
        }

        public void ClearSSC()
        {
            ownedSkillDict.Clear();
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (var skill in ownedSkillDict.Values)
            {
                skill.OnUpdate(_deltaTime);
            }
        }
    }
}
