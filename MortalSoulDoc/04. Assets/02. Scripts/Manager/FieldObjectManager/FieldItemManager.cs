using Core;
using Cysharp.Threading.Tasks;
using MS.Data;
using MS.Field;
using MS.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MS.Manager
{
    public class FieldItemManager : Singleton<FieldItemManager>
    {
        private struct FieldItemRenderData
        {
            public Mesh mesh;
            public Material material;
        }

        private List<FieldItem> fieldItemList = new List<FieldItem>();
        private Dictionary<string, FieldItemRenderData> renderDataDict = new Dictionary<string, FieldItemRenderData>();
        // DrawMeshInstanced 최대 인스턴스 수 제한
        private static readonly Matrix4x4[] matrixBuffer = new Matrix4x4[1023];


        public FieldItem SpawnFieldItem(string _key, Vector3 _spawnPos)
        {
            FieldItem fieldItem = ObjectPoolManager.Instance.Get(_key, _spawnPos, Quaternion.identity).GetComponent<FieldItem>();

            if (fieldItem != null)
            {
                if (!DataManager.Instance.ItemSettingDataDict.TryGetValue(_key, out ItemSettingData _itemData))
                {
                    Debug.LogError($"SpawnFieldItem Key Missing : {_key}");
                    return null;
                }

                fieldItem.InitFieldItem(_key, _itemData);
                fieldItemList.Add(fieldItem);
            }

            return fieldItem;
        }

        public void SpawnRandomFieldItem(Vector3 _spawnPos)
        {
            int minIndex = (int)EItemType.RedCrystal;
            int maxIndex = (int)EItemType.BlueCrystal;

            int randomIndex = UnityEngine.Random.Range(minIndex, maxIndex + 1);
            string randomKey = ((EItemType)randomIndex).ToString();

            SpawnFieldItem(randomKey, _spawnPos);
        }

        public void ClearFieldItem()
        {
            fieldItemList.Clear();
            // 다음 스테이지에서 재로드 시 새로운 레퍼런스를 사용하도록 초기화
            renderDataDict.Clear();
        }

        public async UniTask LoadAllFieldItemAsync()
        {
            try
            {
                var poolConfig = new (string key, int count)[]
                {
                    ("Coin", 100),
                    ("BlueCrystal", 50),
                    ("GreenCrystal", 50),
                    ("RedCrystal", 50),
                    ("BossChest", 5),
                    ("Artifact", 5),
                };

                // 풀 생성
                var tasks = new List<UniTask>();
                foreach (var (key, count) in poolConfig)
                {
                    tasks.Add(ObjectPoolManager.Instance.CreatePoolAsync(key, count));
                }
                await UniTask.WhenAll(tasks);

                // 풀 인스턴스에서 렌더 데이터 추출 및 MeshRenderer 비활성화
                foreach (var (key, count) in poolConfig)
                {
                    ExtractRenderDataAndDisableRenderers(key, count);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // 풀에서 인스턴스를 꺼내 Mesh/Material을 추출하고 MeshRenderer를 비활성화한 뒤 반환
        private void ExtractRenderDataAndDisableRenderers(string _key, int _count)
        {
            var instances = new List<GameObject>(_count);
            for (int i = 0; i < _count; i++)
            {
                GameObject obj = ObjectPoolManager.Instance.Get(_key);
                if (obj != null) instances.Add(obj);
            }

            if (instances.Count > 0)
            {
                MeshFilter meshFilter = instances[0].GetComponentInChildren<MeshFilter>();
                MeshRenderer meshRenderer = instances[0].GetComponentInChildren<MeshRenderer>();

                if (meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null)
                {
                    renderDataDict[_key] = new FieldItemRenderData
                    {
                        mesh = meshFilter.sharedMesh,
                        material = meshRenderer.sharedMaterial
                    };
                }
            }

            foreach (var obj in instances)
            {
                MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
                if (renderer != null) renderer.enabled = false;
                ObjectPoolManager.Instance.Return(_key, obj);
            }
        }

        // GPU Instancing으로 활성 아이템을 타입별로 일괄 렌더링
        public void OnUpdate()
        {
            foreach (var renderData in renderDataDict)
            {
                FieldItemRenderData data = renderData.Value;
                if (data.mesh == null || data.material == null) continue;

                string key = renderData.Key;
                int count = 0;

                for (int i = 0; i < fieldItemList.Count; i++)
                {
                    FieldItem item = fieldItemList[i];
                    if (item == null || !item.gameObject.activeInHierarchy) continue;
                    if (item.FieldItemKey != key) continue;

                    matrixBuffer[count++] = item.transform.localToWorldMatrix;

                    // 1023개 초과 시 분할 호출
                    if (count == 1023)
                    {
                        Graphics.DrawMeshInstanced(data.mesh, 0, data.material, matrixBuffer, count);
                        count = 0;
                    }
                }

                if (count > 0)
                {
                    Graphics.DrawMeshInstanced(data.mesh, 0, data.material, matrixBuffer, count);
                }
            }
        }
    }
}
