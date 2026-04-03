using MS.Data;
using MS.Manager;
using MS.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;



namespace MS.Field
{
    public class InteractionItem : FieldItem
    {
        protected override void OnAcquire(PlayerCharacter _player)
        {
            if (_player == null)
            {
                Debug.Log("OnAcquire :: Player null");
                return;
            }

            switch (itemType)
            {
                case EItemType.BossChest:
                    var SkillDict = DataManager.Instance.SkillSettingDataDict;
                    var rewards = new List<string>();

                    var playerSkillKeys = SkillDict
                        .Where(x => x.Value.OwnerType == FieldObjectType.Player && !_player.SSC.HasSkill(x.Key))
                        .Select(x => x.Key)
                        .ToList();

                    while (rewards.Count < 4 && playerSkillKeys.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, playerSkillKeys.Count);
                        rewards.Add(playerSkillKeys[randomIndex]);
                        playerSkillKeys.RemoveAt(randomIndex);
                    }

                    var popup = UIManager.Instance.ShowPopup<SkillRewardPopup>("SkillRewardPopup");
                    popup.InitSkillRewardPopup(rewards, _player);

                    break;

                case EItemType.Artifact:
                    var ownedSet = _player.PlayerArtifact.OwnedArtifactList.Values
                                            .SelectMany(list => list)
                                            .ToHashSet();

                    string artifactKey = DataManager.Instance.ArtifactSettingDataDict
                        .Where(pair => !ownedSet.Contains(pair.Value))
                        .Select(pair => pair.Key)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(artifactKey))
                    {
                        var artifactPopup = UIManager.Instance.ShowPopup<ArtifactPopup>("ArtifactPopup");
                        artifactPopup.InitArtifactPopup(artifactKey);

                        _player.PlayerArtifact.AddArtifact(artifactKey);
                    }
                    break;
            }
        }
    }
}