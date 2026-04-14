using Cysharp.Threading.Tasks;
using MS.Field;
using System.Collections.Generic;
using UnityEngine;
using static MS.Field.FieldObject;

namespace Core
{
    public class MonsterManager
    {
        private List<MonsterCharacter> activeMonsterList = new List<MonsterCharacter>();

        public IReadOnlyList<MonsterCharacter> ActiveMonsterList => activeMonsterList;


        public MonsterCharacter SpawnMonster(string _monsterKey, Vector3 _position)
        {
            var go = Main.Instance.ObjectPoolManager.Get(_monsterKey, _position);
            if (go == null) return null;

            var monster = go.GetComponent<MonsterCharacter>();
            monster.InitMonster(_monsterKey);
            activeMonsterList.Add(monster);
            return monster;
        }

        public void OnUpdate(float _deltaTime)
        {
            for (int i = activeMonsterList.Count - 1; i >= 0; i--)
            {
                var monster = activeMonsterList[i];
                if (monster == null || monster.ObjectLifeState == FieldObjectLifeState.Death)
                {
                    activeMonsterList.RemoveAt(i);
                    continue;
                }
                monster.OnUpdate(_deltaTime);
            }
        }

        public void ReleaseMonster(MonsterCharacter _monster)
        {
            activeMonsterList.Remove(_monster);
            Main.Instance.ObjectPoolManager.Return(_monster.MonsterKey, _monster.gameObject);
        }

        public async UniTask LoadAllMonsterAsync()
        {
            var monsterDict = Main.Instance.DataManager.SettingData.MonsterSettingDict;
            if (monsterDict == null || monsterDict.Count == 0)
            {
                Debug.LogWarning("[MonsterManager] MonsterSettingDict가 비어있어 사전 풀 생성을 건너뜁니다.");
                return;
            }

            var tasks = new List<UniTask>(monsterDict.Count);
            foreach (var key in monsterDict.Keys)
            {
                tasks.Add(Main.Instance.ObjectPoolManager.CreatePoolAsync(key, 10));
            }

            await UniTask.WhenAll(tasks);
        }

        public void ClearAll()
        {
            for (int i = activeMonsterList.Count - 1; i >= 0; i--)
            {
                if (activeMonsterList[i] != null)
                    activeMonsterList[i].OnDead();
            }
        }
    }
}
