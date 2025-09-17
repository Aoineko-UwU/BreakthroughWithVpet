using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSelector : MonoBehaviour
{
    // 定义一个结构体来存储效果名称和它的概率
    [System.Serializable]
    private class RandomEvent
    {
        public int eventIndex;     //效果编号
        public float probability;  //对应的概率
    }

    public static RandomSelector Instance;      //静态单例

    private void Awake()
    {
        Instance = this;    //单例化
    }

    //进食效果随机事件组(事件组ID为1)
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

    //扭蛋随机事件组(事件组ID为2)
    private List<RandomEvent> gachaEvent = new List<RandomEvent>()
    {
        //事件1： 生成数个可拾取的随机物品
        new RandomEvent() { eventIndex = 1, probability = 30f },
        //事件2： 生成数个随机怪物
        new RandomEvent() { eventIndex = 2, probability = 15f },
        //事件3： 生成数个炸弹
        new RandomEvent() { eventIndex = 3, probability = 15f },
        //事件3： 生成数个弹簧
        new RandomEvent() { eventIndex = 4, probability = 20f },
        //事件4： 生成数个石锥
        new RandomEvent() { eventIndex = 5, probability = 20f },
    };

    //概率抽取方法
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
            // 计算所有效果的总概率
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

}
