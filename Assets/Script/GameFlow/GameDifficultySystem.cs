using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏难度枚举
/// - 定义简单、普通与困难三个难度等级。
/// </summary>
public enum GameDifficultyLevel
{
    /// <summary>简单难度，编号为 0。</summary>
    Easy,

    /// <summary>普通难度，编号为 1。</summary>
    Normal,

    /// <summary>困难难度，编号为 2。</summary>
    Hard
}

/// <summary>
/// 游戏难度管理类
/// - 跨场景保存当前难度，供各系统初始化参数时读取。
/// </summary>
public class GameDifficultySystem : Singleton<GameDifficultySystem>
{
    #region 难度状态与设置

    [Tooltip("简单难度的战斗、道具和环境数值配置。")]
    [SerializeField] private GameDifficultyConfig easyConfig;

    [Tooltip("普通难度的战斗、道具和环境数值配置。")]
    [SerializeField] private GameDifficultyConfig normalConfig;

    [Tooltip("困难难度的战斗、道具和环境数值配置。")]
    [SerializeField] private GameDifficultyConfig hardConfig;

    /// <summary>当前游戏难度，仅允许通过设置方法修改，默认使用普通难度。</summary>
    public GameDifficultyLevel CurrentDifficulty { get; private set; } = GameDifficultyLevel.Normal; //(外部只可读)

    /// <summary>当前难度对应的数值配置；场景未绑定配置时抛出明确错误。</summary>
    public GameDifficultyConfig CurrentConfig
    {
        get
        {
            GameDifficultyConfig config;
            switch (CurrentDifficulty)
            {
                case GameDifficultyLevel.Easy:
                    config = easyConfig;
                    break;
                case GameDifficultyLevel.Normal:
                    config = normalConfig;
                    break;
                case GameDifficultyLevel.Hard:
                    config = hardConfig;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(CurrentDifficulty), CurrentDifficulty, "未知的游戏难度。");
            }

            if (config == null)
                throw new System.InvalidOperationException($"难度 {CurrentDifficulty} 未绑定 GameDifficultyConfig。");

            return config;
        }
    }

    /// <summary>是否在切换场景时保留当前单例对象。</summary>
    protected override bool PersistAcrossScenes => true;

    /// <summary>
    /// 记录当前难度，供后续初始化读取；不会主动重新计算已生成对象的属性。
    /// </summary>
    /// <param name="level">要采用的游戏难度等级。</param>
    public void SetDifficulty(GameDifficultyLevel level)
    {
        CurrentDifficulty = level;
    }

    #endregion
}
