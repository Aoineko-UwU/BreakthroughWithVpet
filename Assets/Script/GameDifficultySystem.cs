using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 定义游戏内可选的难度等级。
/// </summary>
public enum GameDifficultyLevel
{
    Easy,       //0
    Normal,     //1
    Hard
}

/// <summary>
/// 跨场景保存当前难度，供战斗、生成和物品栏系统读取难度参数。
/// </summary>
public class GameDifficultySystem : Singleton<GameDifficultySystem>
{
    public GameDifficultyLevel CurrentDifficulty { get; private set; } = GameDifficultyLevel.Normal; //(外部只可读)

    protected override bool PersistAcrossScenes => true;

    //游戏难度设置
    /// <summary>
    /// 设置当前难度，后续生成和属性初始化会读取该值。
    /// </summary>
    public void SetDifficulty(GameDifficultyLevel level)
    {
        CurrentDifficulty = level;
    }
}
