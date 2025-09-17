using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public int index;                 // 插槽索引
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
    public void ClickSlot()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        if (index + 1 > inv.slots.Count) return;        // 若没有道具
        if (DragController.Instance != null && DragController.Instance.isSelected) return; // 已在拖动中

        BeginDrag(inv.slots[index]);
    }

    private void BeginDrag(ItemData data)
    {
        if (data == null) return;

        // 生成一个 preview（和你原来实现一致）
        previewItem = new GameObject("PreviewItem");
        SpriteRenderer previewRenderer = previewItem.AddComponent<SpriteRenderer>();
        previewRenderer.sortingLayerName = "TextUI";
        previewRenderer.sprite = data.entitySprite;
        previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);

        if (data.prefab.GetComponent<SpriteRenderer>().drawMode == SpriteDrawMode.Tiled)
        {
            previewRenderer.drawMode = SpriteDrawMode.Tiled;
            previewRenderer.size = data.prefab.GetComponent<SpriteRenderer>().size;
        }

        // 复制碰撞箱
        PolygonCollider2D sourceCollider = data.prefab.GetComponent<PolygonCollider2D>();
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
    public void SetItemImage(ItemData data)
    {
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

    public void SetActiveOfSelectedFrame(bool isActive)
    {
        slotSelectedFrame.SetActive(isActive);
    }
}
