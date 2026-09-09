using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
/// <summary>
/// 定义物品栏道具的图标、场景预制体、缩放和分类数据。
/// </summary>
public class ItemData : ScriptableObject
{
    [Tooltip("物品的唯一编号，用于分类、存档或逻辑判断。")]
    public int itemID;

    [Tooltip("物品栏中显示的图标。")]
    public Sprite icon;

    [Tooltip("物品放置或生成时使用的场景预制体。")]
    public GameObject prefab;

    [Tooltip("拖拽预览和场景实体使用的精灵图。")]
    public Sprite entitySprite;

    [Tooltip("生成场景实体时使用的缩放值。")]
    public float entityScale = 1.0f;
}
