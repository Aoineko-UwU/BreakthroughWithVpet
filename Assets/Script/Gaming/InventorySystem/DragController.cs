using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    public static DragController Instance;  //��̬�൥��

    private GameObject previewItem;         //��͸����ƷԤ��
    private SpriteRenderer preItemSprite;   //��͸����Ʒ����
    private ItemData currentItemData;   //��ǰ�϶�����Ʒ����
    private InventoryManager inv;       //��������Ʒ��������
    private GameObject vpet;

    //�������ں���--------------------------------------------------------------------------------//

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //��ȡ�������Ϸ����
        Instance = this;    //ȷ������ָ���౾��
        inv = FindObjectOfType<InventoryManager>(); //��ȡ�����ڵ���Ʒ��������
    }

    void Update()
    {
        PreItemAndCheckKey();   //����Ԥ������λ�ò���������
        CheckPlaceItem();       //����Ƿ��ܹ���������
    }
    //��Ϊ��������--------------------------------------------------------------------------------//

    public bool isSelected = false;    //�Ƿ�����Ʒ�ѱ�ѡ�У�

    //��ʼ��ק����(�ⲿ����)
    public void BeginDrag(GameObject preview, ItemData data)
    {
        previewItem = preview;                                      //��ȡԤ������GameObject
        currentItemData = data;                                     //��ȡԤ�����ߵĵ�������
        preItemSprite = previewItem.GetComponent<SpriteRenderer>(); //��ȡԤ�����ߵľ�����Ⱦ
        isSelected = true;                                          //��־Ϊ������Ʒ��ѡ��
    }

    //Ԥ����������밴�����(Update)
    private void PreItemAndCheckKey()
    {
        //Ϊ�ռ��
        if (previewItem != null)
        {
            // �������
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            previewItem.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f);

            // �ж��Ƿ����������
            if (Input.GetMouseButtonDown(0)) // ����������
            {
                if (GameManager.Instance.isAllowPlayerControl)
                    TryPlaceItem();
                else
                    CancelDrag();
            }

            // �ж��Ƿ����Ҽ�ȡ��
            if (Input.GetMouseButtonDown(1)) // �Ҽ����ȡ��
            {
                CancelDrag();
                if (GameManager.Instance.isAllowPlayerControl)
                    AudioManager.Instance.PlaySound("slot_cancel");
            }

            //��������֣���תԤ����Ʒ
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");  //��ȡ����������
            if (scrollInput != 0 && previewItem != null && GameManager.Instance.isAllowPlayerControl)  // ����������ƶ�
            {
                float rotationAmount = scrollInput > 0 ? 15f : -15f;            //���Ϲ���˳ʱ����ת�����¹�����ʱ����ת
                previewItem.transform.Rotate(Vector3.forward, rotationAmount);  //��Z����ת��Ʒ
            }
        }
    }

    private bool isAllowPlaceItem = false;      //�Ƿ�������õ��ߣ�

    // �ж��Ƿ���Է�����Ʒ(Update)
    private void CheckPlaceItem()
    {
        //Ϊ���ж�
        if (previewItem != null && currentItemData != null)
        {
            //������ƷΪ��������Ʒʱִ�з��������ж�
            if (currentItemData.itemID <= 49)
            {
                PolygonCollider2D collider = previewItem.GetComponent<PolygonCollider2D>();     //��ȡpreviewItem��ײ��

                //Ϊ�ռ��
                if (collider != null)
                {
                    ContactFilter2D filter = new ContactFilter2D();     //׼��һ�� ContactFilter2D
                    filter.useTriggers = false;                         //������� trigger ����ײ��             
                    Collider2D[] results = new Collider2D[10];          //�洢��⵽����ײ��

                    //ִ���ص����
                    int hitCount = collider.OverlapCollider(filter, results);

                    //�������ײ�壬����Ϊ���ܷ��ã���������Ⱦ��ɫ
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
            //����Ĭ���������
            else
            {
                isAllowPlaceItem = true;
            }
        }
    }

    private float throwForce = 6f; //���鷢������

    //������Ʒ(�ڲ�����)
    private void TryPlaceItem()
    {
        if (isAllowPlaceItem)
        {
            if (currentItemData.itemID <= 49)
            {
                // ������Ʒ�ĳ���ʵ��
                Quaternion itemRotation = previewItem.transform.rotation;   //��ȡ��ǰԤ����Ʒ����תֵ
                var prefab = Instantiate(currentItemData.prefab, previewItem.transform.position, itemRotation);         //ʵ������
                prefab.transform.localScale = new Vector2(currentItemData.entityScale, currentItemData.entityScale);    //��������ֵ
                                                                                                                        //ִ����Ʒ��ɾ�������Ԥ��ʵ��
                inv.RemoveAt(inv.slots.IndexOf(currentItemData));
                AudioManager.Instance.PlaySound("place_confirm");
                CancelDrag();
            }
            //��Ϊ��������
            else if (currentItemData.itemID == 50)
            {
                //������Ͷ��Ч��
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);   //��ȡ�������
                mouseWorldPosition.z = 0;  // ��֤z��Ϊ0

                Vector2 dir = (mouseWorldPosition - vpet.transform.position).normalized;    // �����Vpet�����ķ���
                GameObject thrownItem = Instantiate(currentItemData.prefab, vpet.transform.position, Quaternion.identity);  //����
                thrownItem.transform.localScale = new Vector2(currentItemData.entityScale, currentItemData.entityScale);    //��������ֵ
                thrownItem.GetComponent<Rigidbody2D>().AddForce(dir * throwForce, ForceMode2D.Impulse);     //Ͷ��������                  

                //��������
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

    //ȡ���϶�(�ڲ�����)
    private void CancelDrag()
    {
        Destroy(previewItem);   //����Ԥ����Ʒ
        previewItem = null;     //���Ԥ����������
        preItemSprite = null;   //�����������
        isSelected = false;     //��־Ϊ����Ʒѡ��

        //��Slot��ѡ�п�����
        SlotUI[] slotUI = FindObjectsOfType<SlotUI>();
        foreach (SlotUI slot in slotUI)
        {
            slot.SetActiveOfSelectedFrame(false);
        }

    }

}
