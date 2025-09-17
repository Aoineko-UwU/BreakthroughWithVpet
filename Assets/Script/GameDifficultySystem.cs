using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameDifficultyLevel
{
    Easy,       //0
    Normal,     //1
    Hard    
}

public class GameDifficultySystem : MonoBehaviour
{
    public static GameDifficultySystem Instance { get; private set; }  //类单例(外部只可读)

    public GameDifficultyLevel CurrentDifficulty { get; private set; } = GameDifficultyLevel.Normal; //(外部只可读)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 保证切换场景不销毁
    }

    //游戏难度设置
    public void SetDifficulty(GameDifficultyLevel level)
    {
        CurrentDifficulty = level;
    }
}
