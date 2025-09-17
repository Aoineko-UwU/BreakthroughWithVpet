using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public int index;                 // �������
    private InventoryManager inv;
    private GameObject previewItem;

    [SerializeField] private Image slotItemImage;
    [SerializeField] private GameObject slotSelectedFrame;

    private void Awake()
    {
        inv = FindObjectOfType<InventoryManager>();
        SetActiveOfSelectedFrame(false);
    }

    // Button �󶨣��������
    public void ClickSlot()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        if (index + 1 > inv.slots.Count) return;        // ��û�е���
        if (DragController.Instance != null && DragController.Instance.isSelected) return; // �����϶���

        BeginDrag(inv.slots[index]);
    }

    private void BeginDrag(ItemData data)
    {
        if (data == null) return;

        // ����һ�� preview������ԭ��ʵ��һ�£�
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

        // ������ײ��
        PolygonCollider2D sourceCollider = data.prefab.GetComponent<PolygonCollider2D>();
        PolygonCollider2D previewCollider = previewItem.AddComponent<PolygonCollider2D>();

        previewCollider.pathCount = sourceCollider.pathCount;
        for (int i = 0; i < sourceCollider.pathCount; i++)
            previewCollider.SetPath(i, sourceCollider.GetPath(i));
        previewCollider.isTrigger = true;

        // �� preview �������λ�ã�����λ�ã�
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        previewItem.transform.position = pos;

        // ����
        previewItem.transform.localScale = new Vector2(data.entityScale, data.entityScale);

        // �� Tag ����ĳЩ��⣨��ԭ���������
        previewItem.tag = "Ignore";

        // �����϶���DragController �������룩
        DragController.Instance.BeginDrag(previewItem, data);
        SetActiveOfSelectedFrame(true);

        // ����ѡ����Ч
        AudioManager.Instance.PlaySound("slot_select");
    }

    // �ⲿ���ã����� slot ͼ��
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
