using Cysharp.Threading.Tasks;
using MS.Field;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class BattleObjectManager
    {
        private List<BattleObject> battleObjectList = new List<BattleObject>();
        private List<BattleObject> releaseBattleObjectList = new List<BattleObject>();


        public T SpawnBattleObject<T>(string _key, FieldCharacter _owner, LayerMask _targetLayer) where T : BattleObject
        {
            T battleObject = Main.Instance.ObjectPoolManager
                .Get(_key, _owner.transform).GetComponent<T>();

            if (battleObject)
            {
                battleObjectList.Add(battleObject);
                battleObject.InitBattleObject(_key, _owner, _targetLayer);
            }
            return battleObject;
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (BattleObject battleObject in battleObjectList)
            {
                battleObject.OnUpdate(_deltaTime);
                if (battleObject.ObjectLifeState == FieldObject.FieldObjectLifeState.Death)
                    releaseBattleObjectList.Add(battleObject);
            }

            foreach (BattleObject releaseObject in releaseBattleObjectList)
            {
                Main.Instance.ObjectPoolManager.Return(
                    releaseObject.BattleObjectKey, releaseObject.gameObject);
                battleObjectList.Remove(releaseObject);
            }
            releaseBattleObjectList.Clear();
        }

        public void ClearBattleObject()
        {
            foreach (BattleObject battleObject in battleObjectList)
            {
                Main.Instance.ObjectPoolManager.Return(
                    battleObject.BattleObjectKey, battleObject.gameObject);
            }
            battleObjectList.Clear();
            releaseBattleObjectList.Clear();
        }

        public async UniTask LoadAllBattleObjectAsync()
        {
            try
            {
                var tasks = new List<UniTask>
                {
                };

                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
