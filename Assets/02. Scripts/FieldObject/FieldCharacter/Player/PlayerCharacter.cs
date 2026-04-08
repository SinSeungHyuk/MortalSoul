using Core;
using Cysharp.Threading.Tasks;
using MS.Battle;
using MS.Data;
using System.Collections.Generic;

namespace MS.Field
{
    public class PlayerCharacter : FieldCharacter
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InitTestAsync().Forget();
        }

        private async UniTaskVoid InitTestAsync()
        {
            await UniTask.WaitUntil(() => Main.Instance.IsBootCompleted);
            InitPlayer("test");
        }

        protected override void Update()
        {
            base.Update();
        }

        public void InitPlayer(string _characKey)
        {
            // TODO: _characKey로 CharacterSettingData 조회하여 attrSet/weapon 구성
            var attrSet = new PlayerAttributeSet();
            attrSet.InitBaseAttributeSet(new Dictionary<EStatType, float>
            {
                { EStatType.MaxHealth, 100f },
                { EStatType.AttackPower, 10f },
                { EStatType.Defense, 5f },
                { EStatType.MoveSpeed, 5f }
            });

            BSC = new BattleSystemComponent();
            BSC.InitBSC(this, attrSet, EWeaponType.OneHandSword);

            GetComponent<PlayerController>().InitController(BSC.WSC);
        }
    }
}