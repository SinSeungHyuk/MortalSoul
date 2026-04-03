using DG.Tweening;
using MS.Data;
using MS.Skill;
using MS.Utils;
using UnityEngine;


namespace MS.Field
{
    public class ResourceFieldItem : FieldItem
    {
        private float itemValue;
        private Tween rotationTween;


        public override void InitFieldItem(string _key, ItemSettingData _data)
        {
            base.InitFieldItem(_key, _data);

            itemValue = _data.ItemValue;

            transform.position = new Vector3(Position.x, Position.y + 1f, Position.z);

            if (_data.ItemType == EItemType.Coin)
            {
                rotationTween = transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
        }

        protected override void OnAcquire(PlayerCharacter _player)
        {
            if (_player == null)
            {
                Debug.Log("OnAcquire :: Player null");
                return;
            }

            _player.PlayerArtifact.OnTriggerArtifact(ArtifactTriggerType.OnItemAcquire, new ArtifactContext()
            {
                StringVal = FieldItemKey,
                FloatVal = itemValue
            });

            switch (itemType)
            {
                case EItemType.Coin:
                    float CoinMultiple = _player.SSC.AttributeSet.GetStatValueByType(EStatType.CoinMultiple);
                    if (CoinMultiple > 0)
                    {
                        itemValue *= CoinMultiple;
                    }
                    _player.LevelSystem.AddExp(itemValue);
                    break;
                case EItemType.RedCrystal: // 공격력 버프
                    _player.ApplyStatEffect("RedCrystal", EStatType.AttackPower, itemValue, EBonusType.Flat, 5f);
                    break;
                case EItemType.GreenCrystal: // 체력회복
                    _player.SSC.AttributeSet.Health += itemValue;
                    break;
                case EItemType.BlueCrystal: // 이동속도 증가
                    _player.ApplyStatEffect("BlueCrystal", EStatType.MoveSpeed, itemValue, EBonusType.Percentage, 5f);
                    break;
            }

            if (rotationTween != null)
            {
                rotationTween.Kill();
                rotationTween = null;
            }
        }
    }
}