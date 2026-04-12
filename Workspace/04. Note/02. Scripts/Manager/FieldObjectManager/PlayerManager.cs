using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Utils;
using System;
using UnityEngine;


namespace MS.Manager
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        private PlayerCharacter player;

        public PlayerCharacter Player => player;


        public async UniTask<PlayerCharacter> SpawnPlayerCharacter(string _key)
        {
            GameObject playerResource = await AddressableManager.Instance.LoadResourceAsync<GameObject>("PlayerCharacter");
            player = PlayerCharacter.Instantiate(
                playerResource,
                new Vector3(0,8,0),
                Quaternion.identity
            ).GetComponent< PlayerCharacter>();

            if (!DataManager.Instance.CharacterSettingData.CharacterSettingDataDict.TryGetValue(_key, out CharacterSettingData _characterData))
            {
                return null;
            }

            Mesh weaponMesh = await AddressableManager.Instance.LoadResourceAsync<Mesh>(_characterData.DefaultWeaponKey);
            MeshFilter weaponMeshFilter = player.gameObject.transform.FindChildComponentDeep<MeshFilter>("Weapon");
            if (weaponMeshFilter) weaponMeshFilter.mesh = weaponMesh;

            CameraManager.Instance.InitMainCamera(player.transform);

            return player;
        }

        public void ClearPlayerCharacter()
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        }
    }
}