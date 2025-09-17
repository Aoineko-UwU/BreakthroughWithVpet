using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID;                  //唯一编号，用于存档或对比
    public Sprite icon;                 //UI显示用贴图
    public GameObject prefab;           //预制体
    public Sprite entitySprite;         //实体精灵图
    public float entityScale = 1.0f;    //实体缩放值(生成的Prefab)
}