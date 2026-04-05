using Cysharp.Threading.Tasks;
using MS.Field;
using System.Collections.Generic;

namespace MS.Battle
{
    public class BattleSystemComponent
    {
        public SkillSystemComponent SSC { get; private set; }
        public BaseAttributeSet AttributeSet { get; private set; }

        private Dictionary<string, StatusEffect> statusEffectDict;


        public void InitBSC(FieldCharacter _owner, BaseAttributeSet _attributeSet)
        {
            AttributeSet = _attributeSet;
            statusEffectDict = new Dictionary<string, StatusEffect>();

            SSC = new SkillSystemComponent();
            SSC.InitSSC(_owner, _attributeSet);
        }

        public async UniTask UseSkill(string _key)
        {
            // 상태이상 체크 (기절 등으로 스킬 사용 불가 시 차단)
            // 현재는 항상 통과 — 추후 구현
            await SSC.UseSkill(_key);
        }

        public void ApplyStatusEffect(string _key, StatusEffect _effect)
        {
            // 기존 키가 있으면 End 후 교체
            if (statusEffectDict.TryGetValue(_key, out StatusEffect existing))
            {
                existing.End();
                statusEffectDict.Remove(_key);
            }

            statusEffectDict.Add(_key, _effect);
            _effect.Start();
        }

        public void OnUpdate(float _deltaTime)
        {
            SSC.OnUpdate(_deltaTime);
            UpdateStatusEffects(_deltaTime);
        }

        private void UpdateStatusEffects(float _deltaTime)
        {
            var removeEffectKeyList = new List<string>();

            foreach (var pair in statusEffectDict)
            {
                pair.Value.Update(_deltaTime);

                if (pair.Value.IsFinished)
                {
                    pair.Value.End();
                    removeEffectKeyList.Add(pair.Key);
                }
            }

            foreach (var key in removeEffectKeyList)
                statusEffectDict.Remove(key);
        }
    }
}
