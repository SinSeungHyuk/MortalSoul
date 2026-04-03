using MS.Data;
using MS.Manager;
using MS.Utils;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Field
{
    public class PlayerArtifact : MonoBehaviour
    {
        private PlayerCharacter owner;
        private Dictionary<ArtifactTriggerType, List<ArtifactSettingData>> ownedArtifactDict = new();

        public IReadOnlyDictionary<ArtifactTriggerType, List<ArtifactSettingData>> OwnedArtifactList => ownedArtifactDict;


        public void InitPlayerArtifact(PlayerCharacter _player)
        {
            owner = _player;
        }

        public void AddArtifact(string _key)
        {     
            if (DataManager.Instance.ArtifactSettingDataDict.TryGetValue(_key, out ArtifactSettingData _data))
            {
                if (!ownedArtifactDict.ContainsKey(_data.TriggerType))
                    ownedArtifactDict[_data.TriggerType] = new List<ArtifactSettingData>();
                ownedArtifactDict[_data.TriggerType].Add(_data);

                switch (_data.TriggerType)
                {
                    case ArtifactTriggerType.OnAcquire:
                        ActivateArtifact(_data, new ArtifactContext());
                        break;

                    case ArtifactTriggerType.OnSkillUse:
                        owner.SSC.OnSkillUsed += (skillName) =>
                        {
                            var context = new ArtifactContext { StringVal = skillName };
                            ActivateArtifact(_data, context);
                        };
                        break;

                    case ArtifactTriggerType.OnHit:
                        owner.SSC.OnHitCallback += (_,_) =>
                        {
                            ActivateArtifact(_data, new ArtifactContext());
                        };
                        break;
                }
            }
        }

        public void OnTriggerArtifact(ArtifactTriggerType _triggerType, ArtifactContext _context)
        {
            if (ownedArtifactDict.TryGetValue(_triggerType, out List<ArtifactSettingData> artifactList))
            {
                foreach (var artifact in artifactList)
                {
                    ActivateArtifact(artifact, _context);
                }
            }
        }

        private void ActivateArtifact(ArtifactSettingData _data, ArtifactContext _context)
        {
            if (ArtifactUtil.CheckAllConditions(_data.ConditionList, _context))
            {
                owner.ExecuteAllActions(_data.ActionList, _context);
            }
        }
    }
}