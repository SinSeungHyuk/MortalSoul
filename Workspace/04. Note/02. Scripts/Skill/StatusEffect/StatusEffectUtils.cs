using MS.Core;
using MS.Field;
using MS.Manager;
using UnityEngine;


namespace MS.Skill
{
    public static class StatusEffectUtils
    {
        // 능력치 증가/감소 상태이펙트 적용
        public static void ApplyStatEffect(this FieldCharacter _target, string _key, EStatType _statType, float _value, EBonusType _bonusType, float _duration = -1)
        {
            StatusEffect effect = new StatusEffect();
            effect.InitStatusEffect(_duration);

            Stat stat = _target.SSC.AttributeSet.GetStatByType(_statType);
            if (stat == null) return;

            effect.OnStatusStartCallback += () =>
            {
                stat.AddBonusStat(_key, _bonusType, _value);
            };

            effect.OnStatusEndCallback += () =>
            {
                stat.RemoveBonusStat(_key);
            };

            _target.SSC.ApplyStatusEffect(_key, effect);
        }

        // 기절
        public static void ApplyStunEffect(this FieldCharacter _target, string _key, float _duration)
        {
            StatusEffect effect = new StatusEffect();
            effect.InitStatusEffect(_duration);

            MSEffect stunEffectLoop = null;

            effect.OnStatusStartCallback += () =>
            {
                EffectManager.Instance.PlayEffect("Eff_StunBegin", _target.Position, Quaternion.identity);
                stunEffectLoop = EffectManager.Instance.PlayEffect("Eff_StunLoop", _target.Position, Quaternion.identity);
                stunEffectLoop.SetTraceTarget(_target, Vector3.zero);

                _target.ApplyStun(true);
            };
            effect.OnStatusEndCallback += () =>
            {
                stunEffectLoop.StopEffect();

                _target.ApplyStun(false);
            };

            _target.SSC.ApplyStatusEffect(_key, effect);
        }

        // 화상
        public static void ApplyBurnEffect(this FieldCharacter _target, string _key, FieldCharacter _source = null)
        {
            StatusEffect effect = new StatusEffect();
            effect.InitStatusEffect(4.1f);

            float elapsedTime = 0f;
            MSEffect burnEffectLoop = null;

            float damagePerTick = _target.SSC.AttributeSet.GetStatValueByType(EStatType.MaxHealth) * 0.02f;

            effect.OnStatusStartCallback += () =>
            {
                burnEffectLoop = EffectManager.Instance.PlayEffect("Eff_Burn", _target.Position, Quaternion.identity);
                burnEffectLoop.SetTraceTarget(_target, Vector3.up);
            };
            effect.OnStatusUpdateCallback += (_deltaTime) =>
            {
                elapsedTime += _deltaTime;

                if (elapsedTime >= 1f)
                {
                    elapsedTime = 0f;

                    DamageInfo damageInfo = new DamageInfo(
                        _attacker: _source,
                        _target: _target,
                        _attributeType: EDamageAttributeType.Fire,
                        _damage: damagePerTick,
                        _isCritic: false,
                        _knockbackForce: 0f
                    );

                    _target.SSC.TakeDamage(damageInfo);
                }
            };
            effect.OnStatusEndCallback += () =>
            {
                burnEffectLoop.StopEffect();
            };

            _target.SSC.ApplyStatusEffect(_key, effect);
        }

        // 매혹
        public static void ApplyCharmEffect(this FieldCharacter _target, string _key, float _duration, FieldCharacter _charmOwner)
        {
            StatusEffect effect = new StatusEffect();
            effect.InitStatusEffect(_duration);

            Stat moveStat = _target.SSC.AttributeSet.GetStatByType(EStatType.MoveSpeed);
            Stat defStat = _target.SSC.AttributeSet.GetStatByType(EStatType.Defense);
            if (moveStat == null || defStat == null) return;

            MSEffect charmEffectLoop = null;
            effect.OnStatusStartCallback += () =>
            {
                charmEffectLoop = EffectManager.Instance.PlayEffect("Eff_Charm", _target.Position, Quaternion.identity);
                charmEffectLoop.SetTraceTarget(_target, Vector3.up);
                moveStat.AddBonusStat(_key, EBonusType.Percentage, -50);
                defStat.AddBonusStat(_key, EBonusType.Percentage, -50);
            };
            effect.OnStatusEndCallback += () =>
            {
                charmEffectLoop.StopEffect();
                moveStat.RemoveBonusStat(_key);
                defStat.RemoveBonusStat(_key);
            };   

            _target.SSC.ApplyStatusEffect(_key, effect);
        }

        // 동상
        public static void ApplyFrostEffect(this FieldCharacter _target, string _key, float _duration, float _value)
        {
            StatusEffect effect = new StatusEffect();
            effect.InitStatusEffect(_duration);

            Stat moveStat = _target.SSC.AttributeSet.GetStatByType(EStatType.MoveSpeed);

            MSEffect freezeEffectLoop = null;
            effect.OnStatusStartCallback += () =>
            {
                freezeEffectLoop = EffectManager.Instance.PlayEffect("Eff_Frost", _target.Position, Quaternion.identity);
                freezeEffectLoop.SetTraceTarget(_target, Vector3.up);
                moveStat.AddBonusStat(_key, EBonusType.Percentage, _value);
            };
            effect.OnStatusEndCallback += () =>
            {
                freezeEffectLoop.StopEffect();
                moveStat.RemoveBonusStat(_key);
            };

            _target.SSC.ApplyStatusEffect(_key, effect);
        }
    }
}