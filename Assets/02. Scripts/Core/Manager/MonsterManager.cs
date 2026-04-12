using MS.Field;
using System.Collections.Generic;
using UnityEngine;

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

        public void ReleaseMonster(MonsterCharacter _monster)
        {
            activeMonsterList.Remove(_monster);
            Main.Instance.ObjectPoolManager.Return(_monster.MonsterKey, _monster.gameObject);
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
