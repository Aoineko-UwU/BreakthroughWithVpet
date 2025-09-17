using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickItem : MonoBehaviour
{
    [SerializeField] SpriteRenderer icoSprite;      //物品贴图渲染器
    [SerializeField] private ItemData itemData;     //物品数据
    public bool isAllowSelfDestroy = true;          //是否允许自摧毁(默认允许)

    //设置物品数据(外部调用)
    public void SetItemData(ItemData data)
    {
        itemData = data;                //传递物品数据
        if(icoSprite!= null)
            icoSprite.sprite = data.icon;   //设置贴图
    }

    private void Start()
    {    
        if (isAllowSelfDestroy)
        {
            //随机时刻后自动摧毁
            float randDestroyTime = Random.Range(7f, 8f);
            Destroy(gameObject, randDestroyTime);
        }

        if (icoSprite.sprite == null && itemData != null)
        {
            icoSprite.sprite = itemData.icon;
        }
    }

    //拾取物品逻辑
    public void ItemPickUpLogic()
    {
        if (InventoryManager.Instance.TryAddSpecificItem(itemData))
        {
            Destroy(gameObject);
            AudioManager.Instance.PlaySound("pickItem");
        }

    }
}
