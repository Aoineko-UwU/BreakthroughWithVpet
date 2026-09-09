using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameDifficultyLevel
{
    Easy,       //0
    Normal,     //1
    Hard    
}

public class GameDifficultySystem : Singleton<GameDifficultySystem>
{
    public GameDifficultyLevel CurrentDifficulty { get; private set; } = GameDifficultyLevel.Normal; //(外部只可读)

    protected override bool PersistAcrossScenes => true;

    //游戏难度设置
    public void SetDifficulty(GameDifficultyLevel level)
    {
        CurrentDifficulty = level;
    }
}
