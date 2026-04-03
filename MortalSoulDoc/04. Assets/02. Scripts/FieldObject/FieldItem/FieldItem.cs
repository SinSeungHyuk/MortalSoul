using MS.Data;
using MS.Field;
using MS.Manager;
using UnityEngine;


namespace MS.Field
{
    public abstract class FieldItem : FieldObject
    {
        private string fieldItemKey;
        private string gameplayCueKey;
        protected EItemType itemType;

        public string FieldItemKey => fieldItemKey;


        public virtual void InitFieldItem(string _itemKey, ItemSettingData _itemData)
        {
            fieldItemKey = _itemKey;
            gameplayCueKey = _itemData.GameplayCueKey;
            itemType = _itemData.ItemType;

            // GPU Instancing으로 렌더링하므로 MeshRenderer 비활성화 (풀 확장으로 새 인스턴스 생성 시에도 처리)
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            ObjectLifeState = FieldObjectLifeState.Live;
            ObjectType = FieldObjectType.FieldItem;
        }

        protected abstract void OnAcquire(PlayerCharacter _player);

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerCharacter>(out PlayerCharacter player))
            {
                // ���� ȹ�� ������ ����
                GameplayCueManager.Instance.PlayCue(gameplayCueKey, player);
                OnAcquire(player);
                ObjectPoolManager.Instance.Return(fieldItemKey, this.gameObject);
            }
        }
    }
}