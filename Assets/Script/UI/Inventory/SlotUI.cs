using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品栏格子类
/// - 显示单个格子的图标与选中框，并创建道具拖拽预览。
/// </summary>
public class SlotUI : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("该格子对应物品栏列表的索引。")]
    public int index;

    /// <summary>场景中的物品栏管理器。</summary>
    private InventoryManager inv;

    /// <summary>当前创建或持有的半透明拖拽预览对象。</summary>
    private GameObject previewItem;

    [Tooltip("显示当前格子物品图标的 Image。")]
    [SerializeField] private Image slotItemImage;

    [Tooltip("当前格子的选中框对象。")]
    [SerializeField] private GameObject slotSelectedFrame;

    #endregion

    #region 初始化与预览创建

    /// <summary>
    /// 缓存物品栏管理器，并隐藏选中框。
    /// </summary>
    private void Awake()
    {
        inv = FindObjectOfType<InventoryManager>();
        SetActiveOfSelectedFrame(false);
    }

    /// <summary>
    /// 在允许操作、格子有物品且尚未拖拽时，尝试为当前条目创建预览。
    /// </summary>
    public void ClickSlot()
    {
        if (inv == null || GameManager.Instance == null || DragController.Instance == null) return;
        if (!GameManager.Instance.isAllowPlayerControl) return;
        if (index + 1 > inv.slots.Count) return;        // 若没有道具
        if (DragController.Instance.isSelected) return; // 已在拖动中

        BeginDrag(inv.slots[index]);
    }

    /// <summary>
    /// 验证物品资源，复制精灵和多边形碰撞路径作为预览，并交给拖拽控制器。
    /// </summary>
    /// <param name="data">要预览的物品数据；数据或必需资源缺失时不创建预览。</param>
    private void BeginDrag(ItemData data)
    {
        if (data == null || data.prefab == null || data.entitySprite == null)
            return;

        SpriteRenderer sourceRenderer = data.prefab.GetComponent<SpriteRenderer>();
        PolygonCollider2D sourceCollider = data.prefab.GetComponent<PolygonCollider2D>();
        if (sourceRenderer == null || sourceCollider == null)
            return;

        // 创建独立的半透明预览，避免修改物品预制体。
        previewItem = new GameObject("PreviewItem");
        SpriteRenderer previewRenderer = previewItem.AddComponent<SpriteRenderer>();
        previewRenderer.sortingLayerName = "TextUI";
        previewRenderer.sprite = data.entitySprite;
        previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);

        if (sourceRenderer.drawMode == SpriteDrawMode.Tiled)
        {
            previewRenderer.drawMode = SpriteDrawMode.Tiled;
            previewRenderer.size = sourceRenderer.size;
        }

        // 复制碰撞箱
        PolygonCollider2D previewCollider = previewItem.AddComponent<PolygonCollider2D>();

        previewCollider.pathCount = sourceCollider.pathCount;
        for (int i = 0; i < sourceCollider.pathCount; i++)
            previewCollider.SetPath(i, sourceCollider.GetPath(i));
        previewCollider.isTrigger = true;

        // 将 preview 放在鼠标位置（或触摸位置）
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        previewItem.transform.position = pos;

        // 缩放
        previewItem.transform.localScale = new Vector2(data.entityScale, data.entityScale);

        // 标记为 Ignore，供现有交互检测排除预览对象。
        previewItem.tag = "Ignore";

        // 将预览交给拖拽控制器，由其逐帧处理输入。
        DragController.Instance.BeginDrag(previewItem, data);
        SetActiveOfSelectedFrame(true);

        // 播放选中音效
        AudioManager.Instance.PlaySound("slot_select");
    }

    #endregion

    #region 图标与选中反馈

    /// <summary>
    /// 设置格子图标；数据为空时清空并隐藏图标对象。
    /// </summary>
    /// <param name="data">格子对应的物品数据，传入 null 表示空格子。</param>
    public void SetItemImage(ItemData data)
    {
        if (slotItemImage == null)
            return;

        if (data != null)
        {
            slotItemImage.sprite = data.icon;
            slotItemImage.gameObject.SetActive(true);
        }
        else
        {
            slotItemImage.sprite = null;
            slotItemImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 在选中框引用存在时更新其显示状态。
    /// </summary>
    /// <param name="isActive">是否显示选中框。</param>
    public void SetActiveOfSelectedFrame(bool isActive)
    {
        if (slotSelectedFrame != null)
            slotSelectedFrame.SetActive(isActive);
    }

    #endregion
}
