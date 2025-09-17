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
    public static GameDifficultySystem Instance { get; private set; }  //�൥��(�ⲿֻ�ɶ�)

    public GameDifficultyLevel CurrentDifficulty { get; private set; } = GameDifficultyLevel.Normal; //(�ⲿֻ�ɶ�)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ��֤�л�����������
    }

    //��Ϸ�Ѷ�����
    public void SetDifficulty(GameDifficultyLevel level)
    {
        CurrentDifficulty = level;
    }
}
