using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    public static DragController Instance;  //静态类单例

    private GameObject previewItem;         //半透明物品预览
    private SpriteRenderer preItemSprite;   //半透明物品精灵
    private ItemData currentItemData;   //当前拖动的物品数据
    private InventoryManager inv;       //场景内物品栏管理器
    private GameObject vpet;

    //生命周期函数--------------------------------------------------------------------------------//

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
        Instance = this;    //确保单例指向类本身
        inv = FindObjectOfType<InventoryManager>(); //获取场景内的物品栏管理器
    }

    void Update()
    {
        PreItemAndCheckKey();   //更新预览物体位置并监听按键
        CheckPlaceItem();       //监测是否能够放置物体
    }
    //行为方法函数--------------------------------------------------------------------------------//

    public bool isSelected = false;    //是否有物品已被选中？

    //开始拖拽方法(外部调用)
    public void BeginDrag(GameObject preview, ItemData data)
    {
        previewItem = preview;                                      //获取预览道具GameObject
        currentItemData = data;                                     //获取预览道具的道具数据
        preItemSprite = previewItem.GetComponent<SpriteRenderer>(); //获取预览道具的精灵渲染
        isSelected = true;                                          //标志为已有物品被选中
    }

    //预览物体跟随与按键监测(Update)
    private void PreItemAndCheckKey()
    {
        //为空检查
        if (previewItem != null)
        {
            // 跟随鼠标
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            previewItem.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f);

            // 判断是否点击左键放置
            if (Input.GetMouseButtonDown(0)) // 左键点击放置
            {
                if (GameManager.Instance.isAllowPlayerControl)
                    TryPlaceItem();
                else
                    CancelDrag();
            }

            // 判断是否点击右键取消
            if (Input.GetMouseButtonDown(1)) // 右键点击取消
            {
                CancelDrag();
                if (GameManager.Instance.isAllowPlayerControl)
                    AudioManager.Instance.PlaySound("slot_cancel");
            }

            //检测鼠标滚轮，旋转预览物品
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");  //获取鼠标滚轮输入
            if (scrollInput != 0 && previewItem != null && GameManager.Instance.isAllowPlayerControl)  // 如果滚轮有移动
            {
                float rotationAmount = scrollInput > 0 ? 15f : -15f;            //向上滚动顺时针旋转，向下滚动逆时针旋转
                previewItem.transform.Rotate(Vector3.forward, rotationAmount);  //按Z轴旋转物品
            }
        }
    }

    private bool isAllowPlaceItem = false;      //是否允许放置道具？

    // 判断是否可以放置物品(Update)
    private void CheckPlaceItem()
    {
        //为空判断
        if (previewItem != null && currentItemData != null)
        {
            //仅当物品为放置类物品时执行放置允许判断
            if (currentItemData.itemID <= 49)
            {
                PolygonCollider2D collider = previewItem.GetComponent<PolygonCollider2D>();     //获取previewItem碰撞箱

                //为空检查
                if (collider != null)
                {
                    ContactFilter2D filter = new ContactFilter2D();     //准备一个 ContactFilter2D
                    filter.useTriggers = false;                         //检测其他 trigger 的碰撞体             
                    Collider2D[] results = new Collider2D[10];          //存储检测到的碰撞体

                    //执行重叠检测
                    int hitCount = collider.OverlapCollider(filter, results);

                    //如果有碰撞体，设置为不能放置，并更改渲染颜色
                    if (hitCount > 0)
                    {
                        isAllowPlaceItem = false;
                        preItemSprite.color = new Color(1, 0, 0, 0.8f);
                    }
                    else
                    {
                        isAllowPlaceItem = true;
                        preItemSprite.color = new Color(0, 1, 0, 0.5f);
                    }
                }
            }
            //否则默认允许放置
            else
            {
                isAllowPlaceItem = true;
            }
        }
    }

    private float throwForce = 6f; //珍珠发射力度

    //放置物品(内部调用)
    private void TryPlaceItem()
    {
        if (isAllowPlaceItem)
        {
            if (currentItemData.itemID <= 49)
            {
                // 创建物品的场景实例
                Quaternion itemRotation = previewItem.transform.rotation;   //获取当前预览物品的旋转值
                var prefab = Instantiate(currentItemData.prefab, previewItem.transform.position, itemRotation);         //实例创建
                prefab.transform.localScale = new Vector2(currentItemData.entityScale, currentItemData.entityScale);    //更新缩放值
                                                                                                                        //执行物品栏删除并清除预览实例
                inv.RemoveAt(inv.slots.IndexOf(currentItemData));
                AudioManager.Instance.PlaySound("place_confirm");
                CancelDrag();
            }
            //若为传送珍珠
            else if (currentItemData.itemID == 50)
            {
                //处理点击投掷效果
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);   //获取鼠标坐标
                mouseWorldPosition.z = 0;  // 保证z轴为0

                Vector2 dir = (mouseWorldPosition - vpet.transform.position).normalized;    // 计算从Vpet到鼠标的方向
                GameObject thrownItem = Instantiate(currentItemData.prefab, vpet.transform.position, Quaternion.identity);  //生成
                thrownItem.transform.localScale = new Vector2(currentItemData.entityScale, currentItemData.entityScale);    //更新缩放值
                thrownItem.GetComponent<Rigidbody2D>().AddForce(dir * throwForce, ForceMode2D.Impulse);     //投掷力给予                  

                //其他处理
                inv.RemoveAt(inv.slots.IndexOf(currentItemData));
                AudioManager.Instance.PlaySound("throw");
                CancelDrag();
            }

        }
        else
        {
            AudioManager.Instance.PlaySound("place_forbid");
        }
    }

    //取消拖动(内部调用)
    private void CancelDrag()
    {
        Destroy(previewItem);   //销毁预览物品
        previewItem = null;     //清除预览物体引用
        preItemSprite = null;   //清除精灵引用
        isSelected = false;     //标志为无物品选中

        //将Slot的选中框隐藏
        SlotUI[] slotUI = FindObjectsOfType<SlotUI>();
        foreach (SlotUI slot in slotUI)
        {
            slot.SetActiveOfSelectedFrame(false);
        }

    }

}
