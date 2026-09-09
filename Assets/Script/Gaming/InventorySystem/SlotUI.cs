using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示单个物品栏格子，并将用户点击转换为拖拽操作。
/// </summary>
public class SlotUI : MonoBehaviour
{
    [Tooltip("该格子对应物品栏列表的索引。")]
    public int index;
    private InventoryManager inv;
    private GameObject previewItem;

    [SerializeField] private Image slotItemImage;
    [SerializeField] private GameObject slotSelectedFrame;

    private void Awake()
    {
        inv = FindObjectOfType<InventoryManager>();
        SetActiveOfSelectedFrame(false);
    }

    // Button 绑定：点击格子
    /// <summary>
    /// 响应物品栏格子点击并开始拖拽当前物品。
    /// </summary>
    public void ClickSlot()
    {
        if (inv == null || GameManager.Instance == null || DragController.Instance == null) return;
        if (!GameManager.Instance.isAllowPlayerControl) return;
        if (index + 1 > inv.slots.Count) return;        // 若没有道具
        if (DragController.Instance.isSelected) return; // 已在拖动中

        BeginDrag(inv.slots[index]);
    }

    private void BeginDrag(ItemData data)
    {
        if (data == null || data.prefab == null || data.entitySprite == null)
            return;

        SpriteRenderer sourceRenderer = data.prefab.GetComponent<SpriteRenderer>();
        PolygonCollider2D sourceCollider = data.prefab.GetComponent<PolygonCollider2D>();
        if (sourceRenderer == null || sourceCollider == null)
            return;

        // 生成一个 preview（和你原来实现一致）
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

        // 用 Tag 忽略某些检测（你原来有这个）
        previewItem.tag = "Ignore";

        // 启动拖动（DragController 订阅输入）
        DragController.Instance.BeginDrag(previewItem, data);
        SetActiveOfSelectedFrame(true);

        // 播放选中音效
        AudioManager.Instance.PlaySound("slot_select");
    }

    // 外部调用：更新 slot 图标
    /// <summary>
    /// 更新格子图标；传入空值时隐藏图标。
    /// </summary>
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
    /// 设置格子选中框的显示状态。
    /// </summary>
    public void SetActiveOfSelectedFrame(bool isActive)
    {
        if (slotSelectedFrame != null)
            slotSelectedFrame.SetActive(isActive);
    }
}
