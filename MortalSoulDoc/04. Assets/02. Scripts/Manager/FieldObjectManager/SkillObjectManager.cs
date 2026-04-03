using Core;
using Cysharp.Threading.Tasks;
using MS.Field;
using MS.Skill;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.GridLayoutGroup;


namespace MS.Manager
{
    public class SkillObjectManager : Singleton<SkillObjectManager>
    {
        private List<SkillObject> skillObjectList = new List<SkillObject>();
        private List<SkillObject> releaseSkillObjectList = new List<SkillObject>();


        public T SpawnSkillObject<T>(string _key, FieldCharacter _owner, LayerMask _targetLayer) where T : SkillObject
        {
            T skillObject = ObjectPoolManager.Instance.Get(_key, _owner.transform).GetComponent<T>();

            if (skillObject)
            {
                skillObjectList.Add(skillObject);
                skillObject.InitSkillObject(_key, _owner , _targetLayer);
            }
            return skillObject;
        }

        public void SpawnIndicator(Vector3 _spawnPos, float _range, float _duration, Action<Vector3> _callback)
        {
            IndicatorObject indicator = ObjectPoolManager.Instance.Get("Indicator", _spawnPos, Quaternion.identity).GetComponent<IndicatorObject>();
            indicator.InitIndicator(_range, _duration, _callback);
        }

        public void OnUpdate(float _deltaTime)
        {
            foreach (SkillObject skillObject in skillObjectList)
            {
                skillObject.OnUpdate(_deltaTime);
                if (skillObject.ObjectLifeState == FieldObject.FieldObjectLifeState.Death)
                    releaseSkillObjectList.Add(skillObject);
            }

            foreach (SkillObject releaseObject in releaseSkillObjectList)
            {
                ObjectPoolManager.Instance.Return(releaseObject.SkillObjectKey, releaseObject.gameObject);
                skillObjectList.Remove(releaseObject);
            }
            releaseSkillObjectList.Clear();
        }

        public void ClearSkillObject()
        {
            skillObjectList.Clear();
        }

        public async UniTask LoadAllSkillObjectAsync()
        {
            try
            {
                var tasks = new List<UniTask>
                {
                    // Area Objects
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_FOBS", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_Blizzard", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_Meteor", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_BigCrystal", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_CrystalFront", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_FastCrystal", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_Plexus", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_RedExplosion", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_SlashBlue", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_SlashGreen", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_SlashOrange", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_FrostCircle", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_GravitationalField", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Area_LightningAura", 10),

                    // Projectile Objects
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_MonsterArrow", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_StunBall", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_FireBall", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_Charm", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_IceBall", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_GravitationalField", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_BloodBall", 10),
                    ObjectPoolManager.Instance.CreatePoolAsync("Projec_Star", 10),

                    // ETC
                    ObjectPoolManager.Instance.CreatePoolAsync("Indicator", 10),
                    // ...
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