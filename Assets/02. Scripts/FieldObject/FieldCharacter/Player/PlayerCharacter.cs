using MS.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        public TestSpineComponent SpineComponent { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            SpineComponent = GetComponent<TestSpineComponent>();

            // 테스트용 AttributeSet + BSC 초기화
            var attrSet = new PlayerAttributeSet();
            attrSet.InitBaseAttributeSet(new Dictionary<EStatType, float>
            {
                { EStatType.MaxHealth, 100f },
                { EStatType.AttackPower, 10f },
                { EStatType.Defense, 5f },
                { EStatType.MoveSpeed, 5f }
            });
            InitBSC(attrSet);

            // 테스트 스킬 등록 (쿨타임 1초)
            BSC.SSC.GiveSkill("TestOneHandAttack", new TestOneHandAttack(), 1f);
        }

        protected override void Update()
        {
            base.Update();
        }

        public void InitPlayer(string _characKey)
        {

        }
    }
}