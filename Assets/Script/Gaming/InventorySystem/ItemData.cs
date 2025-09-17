using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID;                  //Ψһ��ţ����ڴ浵��Ա�
    public Sprite icon;                 //UI��ʾ����ͼ
    public GameObject prefab;           //Ԥ����
    public Sprite entitySprite;         //ʵ�徫��ͼ
    public float entityScale = 1.0f;    //ʵ������ֵ(���ɵ�Prefab)
}