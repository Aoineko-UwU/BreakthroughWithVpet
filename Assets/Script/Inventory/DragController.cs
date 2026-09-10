using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具拖拽控制类
/// - 处理预览跟随、旋转、放置校验、珍珠投掷和取消拖拽。
/// </summary>
public class DragController : Singleton<DragController>
{
    #region 配置与运行状态

    /// <summary>当前创建或持有的半透明拖拽预览对象。</summary>
    private GameObject previewItem;

    /// <summary>半透明物品精灵。</summary>
    private SpriteRenderer preItemSprite;

    /// <summary>当前拖动的物品数据。</summary>
    private ItemData currentItemData;

    /// <summary>场景中的物品栏管理器。</summary>
    private InventoryManager inv;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    /// <summary>场景中的物品栏格子缓存。</summary>
    private SlotUI[] slotUIs;

    #endregion

    #region 生命周期

    /// <summary>
    /// 注册单例；仅有效实例缓存桌宠、物品栏管理器和格子组件。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
        inv = FindObjectOfType<InventoryManager>(); //获取场景内的物品栏管理器
        slotUIs = FindObjectsOfType<SlotUI>();
    }

    /// <summary>
    /// 先更新预览与处理输入，再计算当前帧的放置许可。
    /// </summary>
    void Update()
    {
        PreItemAndCheckKey();   //更新预览物体位置并监听按键
        CheckPlaceItem();       //监测是否能够放置物体
    }

    #endregion

    #region 拖拽预览与放置校验

    [Tooltip("是否有物品已被选中。")]
    public bool isSelected = false;

    /// <summary>
    /// 登记新的预览对象和物品数据，并标记为正在拖拽。
    /// </summary>
    /// <param name="preview">已创建的预览对象；为空时忽略请求。</param>
    /// <param name="data">预览对应的物品数据；为空时忽略请求。</param>
    public void BeginDrag(GameObject preview, ItemData data)
    {
        if (preview == null || data == null)
            return;

        previewItem = preview;                                      //获取预览道具GameObject
        currentItemData = data;                                     //获取预览道具的道具数据
        preItemSprite = previewItem.GetComponent<SpriteRenderer>(); //获取预览道具的精灵渲染
        isSelected = true;                                          //标志为已有物品被选中
    }

    /// <summary>
    /// 让预览跟随鼠标，并处理左键放置、右键取消与滚轮旋转。
    /// </summary>
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
                float rotationAmount = scrollInput > 0 ? 15f : -15f;            // 正向滚动绕 Z 轴增加角度，反向滚动减少角度。
                previewItem.transform.Rotate(Vector3.forward, rotationAmount);  //按Z轴旋转物品
            }
        }
    }

    /// <summary>是否允许放置道具。</summary>
    private bool isAllowPlaceItem = false;

    /// <summary>
    /// 对编号不大于 49 的预览进行非触发碰撞体重叠检测并着色，其他编号默认允许放置。
    /// </summary>
    private void CheckPlaceItem()
    {
        isAllowPlaceItem = false;

        //为空判断
        if (previewItem != null && currentItemData != null)
        {
            //仅当物品为放置类物品时执行放置允许判断
            if (currentItemData.itemID <= 49)
            {
                PolygonCollider2D collider = previewItem.GetComponent<PolygonCollider2D>();     //获取previewItem碰撞箱

                //为空检查
                if (collider != null && preItemSprite != null)
                {
                    ContactFilter2D filter = new ContactFilter2D();     //准备一个 ContactFilter2D
                    filter.useTriggers = false;                         // 排除其他触发器，只检查实体碰撞体。
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

    #endregion

    #region 实体放置与拖拽清理

    /// <summary>珍珠发射力度。</summary>
    private float throwForce = 6f;

    /// <summary>
    /// 验证预览和物品栏来源后，按缓存的许可放置物品或投掷珍珠，成功后移除条目并清理预览。
    /// </summary>
    private void TryPlaceItem()
    {
        if (inv == null || currentItemData == null || previewItem == null)
        {
            CancelDrag();
            return;
        }

        int itemIndex = inv.slots.IndexOf(currentItemData);
        if (itemIndex < 0)
        {
            CancelDrag();
            return;
        }

        if (isAllowPlaceItem)
        {
            if (currentItemData.itemID <= 49)
            {
                // 创建物品的场景实例
                Quaternion itemRotation = previewItem.transform.rotation;   //获取当前预览物品的旋转值
                var prefab = Instantiate(currentItemData.prefab, previewItem.transform.position, itemRotation);         //实例创建
                prefab.transform.localScale = new Vector2(currentItemData.entityScale, currentItemData.entityScale);    //更新缩放值
                                                                                                                        //执行物品栏删除并清除预览实例
                inv.RemoveAt(itemIndex);
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
                inv.RemoveAt(itemIndex);
                AudioManager.Instance.PlaySound("throw");
                CancelDrag();
            }

        }
        else
        {
            AudioManager.Instance.PlaySound("place_forbid");
        }
    }

    /// <summary>
    /// 销毁预览，清除拖拽引用及许可标记，并隐藏所有格子的选中框。
    /// </summary>
    private void CancelDrag()
    {
        Destroy(previewItem);   //销毁预览物品
        previewItem = null;     //清除预览物体引用
        preItemSprite = null;   //清除精灵引用
        currentItemData = null; //清除当前物品数据
        isSelected = false;     //标志为无物品选中
        isAllowPlaceItem = false;

        //将Slot的选中框隐藏
        foreach (SlotUI slot in slotUIs)
        {
            slot.SetActiveOfSelectedFrame(false);
        }

    }

    #endregion
}
