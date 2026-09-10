using UnityEngine;

/// <summary>
/// 物品数据类
/// - 配置道具编号、图标、实体预制体、预览精灵与实体缩放。
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    #region 物品配置

    [Tooltip("物品编号，用于现有食物效果和放置、投掷逻辑分类。")]
    public int itemID;

    [Tooltip("物品栏中显示的图标。")]
    public Sprite icon;

    [Tooltip("物品放置或生成时使用的场景预制体。")]
    public GameObject prefab;

    [Tooltip("创建拖拽预览时显示的精灵图。")]
    public Sprite entitySprite;

    [Tooltip("生成场景实体时使用的缩放值。")]
    public float entityScale = 1.0f;

    #endregion
}
