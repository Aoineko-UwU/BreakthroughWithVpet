using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 随机事件选择类
/// - 按预置权重抽取食物和扭蛋事件编号。
/// </summary>
public class RandomSelector : Singleton<RandomSelector>
{
    #region 事件配置与权重表

    /// <summary>
    /// 随机事件配置类
    /// - 存储事件编号和相对抽取权重。
    /// </summary>
    [System.Serializable]
    private class RandomEvent
    {

        /// <summary>抽取成功时返回的事件编号。</summary>
        public int eventIndex;

        /// <summary>事件的相对抽取权重，不要求总和为一或一百。</summary>
        public float probability;
    }

    /// <summary>食物随机事件组，组编号为 1；每个条目使用相对权重。</summary>
    private List<RandomEvent> eatEffect = new List<RandomEvent>()
    {
        //事件1： 回复生命
        new RandomEvent() { eventIndex = 1, probability = 25f },
        //事件2： 瞬间死亡
        new RandomEvent() { eventIndex = 2, probability = 5f },
        //事件3： 移动加速
        new RandomEvent() { eventIndex = 3, probability = 15f },
        //事件4： 普通攻击伤害增加
        new RandomEvent() { eventIndex = 4, probability = 15f },
        //事件5： 扣除生命
        new RandomEvent() { eventIndex = 5, probability = 10f },
        //事件6： 瞬移
        new RandomEvent() { eventIndex = 6, probability = 15f },
        //事件7： 瞬间爆炸
        new RandomEvent() { eventIndex = 7, probability = 15f }
    };

    /// <summary>扭蛋随机事件组，组编号为 2；每个条目使用相对权重。</summary>
    private List<RandomEvent> gachaEvent = new List<RandomEvent>()
    {
        //事件1： 生成数个可拾取的随机物品
        new RandomEvent() { eventIndex = 1, probability = 30f },
        //事件2： 生成数个随机怪物
        new RandomEvent() { eventIndex = 2, probability = 15f },
        //事件3： 生成数个炸弹
        new RandomEvent() { eventIndex = 3, probability = 15f },
        //事件4： 生成数个弹簧
        new RandomEvent() { eventIndex = 4, probability = 20f },
        //事件5： 生成数个石锥
        new RandomEvent() { eventIndex = 5, probability = 20f },
    };

    #endregion

    #region 按权重抽取

    /// <summary>
    /// 按事件组的相对权重抽取一个事件编号。
    /// </summary>
    /// <param name="eventGroupIndex">事件组编号：1 为进食事件，2 为扭蛋事件。</param>
    /// <returns>抽中的事件编号；组编号未知或未命中任何条目时返回 0。</returns>
    public int EventRandomSelector(int eventGroupIndex)
    {
        List<RandomEvent> eventGroups = new List<RandomEvent>();
        switch (eventGroupIndex)
        {
            case 1:
                eventGroups = eatEffect;
                break;

            case 2:
                eventGroups = gachaEvent;
                break;

            default:
                Debug.Log("未知的事件参数");
                break;
        }

        //若已选取到事件组
        if (eventGroups.Count >0)
        {
            // 累加所有事件的相对权重，无需将总和固定为一百。
            float totalProbability = 0f;
            foreach (var effect in eventGroups)
            {
                totalProbability += effect.probability;
            }

            // 生成一个随机数，范围从 0 到 totalProbability
            float randomValue = Random.Range(0f, totalProbability);

            // 根据随机数来选择效果
            foreach (var effect in eventGroups)
            {
                randomValue -= effect.probability;
                if (randomValue <= 0f)
                {
                    // 返回对应的效果参数
                    return effect.eventIndex;
                }
            }
        }

        // 如果没有触发，则返回0值
        return 0;

    }

    #endregion
}
