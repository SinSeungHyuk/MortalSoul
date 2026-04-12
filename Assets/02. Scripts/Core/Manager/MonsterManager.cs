using MS.Field;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class MonsterManager
    {
        private List<MonsterCharacter> activeMonsters = new List<MonsterCharacter>();

        public IReadOnlyList<MonsterCharacter> ActiveMonsters => activeMonsters;


        public MonsterCharacter SpawnMonster(string _monsterKey, Vector3 _position)
        {
            var go = Main.Instance.ObjectPoolManager.Get(_monsterKey, _position);
            if (go == null) return null;

            var monster = go.GetComponent<MonsterCharacter>();
            monster.InitMonster(_monsterKey);
            activeMonsters.Add(monster);
            return monster;
        }

        public void DespawnMonster(MonsterCharacter _monster)
        {
            activeMonsters.Remove(_monster);
            Main.Instance.ObjectPoolManager.Return(_monster.MonsterKey, _monster.gameObject);
        }

        public void ClearAll()
        {
            for (int i = activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = activeMonsters[i];
                if (monster != null)
                {
                    monster.OnDespawn();
                    Main.Instance.ObjectPoolManager.Return(monster.MonsterKey, monster.gameObject);
                }
            }
            activeMonsters.Clear();
        }
    }
}
