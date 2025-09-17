using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickItem : MonoBehaviour
{
    [SerializeField] SpriteRenderer icoSprite;      //��Ʒ��ͼ��Ⱦ��
    [SerializeField] private ItemData itemData;     //��Ʒ����
    public bool isAllowSelfDestroy = true;          //�Ƿ������Դݻ�(Ĭ������)

    //������Ʒ����(�ⲿ����)
    public void SetItemData(ItemData data)
    {
        itemData = data;                //������Ʒ����
        if(icoSprite!= null)
            icoSprite.sprite = data.icon;   //������ͼ
    }

    private void Start()
    {    
        if (isAllowSelfDestroy)
        {
            //���ʱ�̺��Զ��ݻ�
            float randDestroyTime = Random.Range(7f, 8f);
            Destroy(gameObject, randDestroyTime);
        }

        if (icoSprite.sprite == null && itemData != null)
        {
            icoSprite.sprite = itemData.icon;
        }
    }

    //ʰȡ��Ʒ�߼�
    public void ItemPickUpLogic()
    {
        if (InventoryManager.Instance.TryAddSpecificItem(itemData))
        {
            Destroy(gameObject);
            AudioManager.Instance.PlaySound("pickItem");
        }

    }
}
