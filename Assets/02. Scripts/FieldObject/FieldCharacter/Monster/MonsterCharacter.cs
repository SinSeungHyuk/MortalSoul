using Core;
using MS.Battle;
using MS.Data;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class MonsterCharacter : FieldCharacter
    {
        private string monsterKey;
        private MonsterController controller;
        private List<MonsterSkillSettingData> skillList;

        public string MonsterKey => monsterKey;
        public List<MonsterSkillSettingData> SkillList => skillList;


        public void InitMonster(string _monsterKey)
        {
            ObjectType = FieldObjectType.Monster;
            ObjectLifeState = FieldObjectLifeState.Live;
            monsterKey = _monsterKey;

            var monsterDict = Main.Instance.DataManager.SettingData.MonsterSettingDict;
            if (!monsterDict.TryGetValue(_monsterKey, out MonsterSettingData monsterData))
            {
                Debug.LogError($"[MonsterCharacter] MonsterSettingData 없음: {_monsterKey}");
                return;
            }

            var attributeSet = new AttributeSet();
            attributeSet.Init(monsterData.AttributeSetSettingData);

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, attributeSet);
            BSC.OnDead += OnDeadCallback;

            skillList = monsterData.SkillList;
            if (skillList != null)
            {
                foreach (var skillInfo in skillList)
                    BSC.SSC.GiveSkill(skillInfo.SkillKey);
            }

            controller = GetComponent<MonsterController>();
            controller.InitController(this);
        }

        protected override void Update()
        {
            if (ObjectLifeState == FieldObjectLifeState.Death) return;
            base.Update();
            controller?.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (ObjectLifeState == FieldObjectLifeState.Death) return;
            if (controller != null) controller.OnFixedUpdate();
        }

        public void ApplyKnockback(Vector2 _dir, float _force)
        {
            if (ObjectLifeState != FieldObjectLifeState.Live) return;
            controller.Rb.linearVelocity = _dir * _force;
        }

        private void OnDeadCallback()
        {
            ObjectLifeState = FieldObjectLifeState.Dying;
            controller.TransitToDead();
        }

        public void OnDespawn()
        {
            BSC.OnDead -= OnDeadCallback;
            BSC.ClearBSC();
            ObjectLifeState = FieldObjectLifeState.Death;
        }

        protected override void OnDestroy()
        {
            if (BSC != null) BSC.OnDead -= OnDeadCallback;
            base.OnDestroy();
        }
    }
}
