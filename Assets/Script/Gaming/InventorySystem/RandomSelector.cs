using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSelector : MonoBehaviour
{
    // ����һ���ṹ�����洢Ч�����ƺ����ĸ���
    [System.Serializable]
    private class RandomEvent
    {
        public int eventIndex;     //Ч�����
        public float probability;  //��Ӧ�ĸ���
    }

    public static RandomSelector Instance;      //��̬����

    private void Awake()
    {
        Instance = this;    //������
    }

    //��ʳЧ������¼���(�¼���IDΪ1)
    private List<RandomEvent> eatEffect = new List<RandomEvent>()
    {
        //�¼�1�� �ظ�����
        new RandomEvent() { eventIndex = 1, probability = 25f },
        //�¼�2�� ˲������
        new RandomEvent() { eventIndex = 2, probability = 5f },
        //�¼�3�� �ƶ�����
        new RandomEvent() { eventIndex = 3, probability = 15f },
        //�¼�4�� ��ͨ�����˺�����
        new RandomEvent() { eventIndex = 4, probability = 15f },
        //�¼�5�� �۳�����
        new RandomEvent() { eventIndex = 5, probability = 10f },
        //�¼�6�� ˲��
        new RandomEvent() { eventIndex = 6, probability = 15f },
        //�¼�7�� ˲�䱬ը
        new RandomEvent() { eventIndex = 7, probability = 15f }
    };

    //Ť������¼���(�¼���IDΪ2)
    private List<RandomEvent> gachaEvent = new List<RandomEvent>()
    {
        //�¼�1�� ����������ʰȡ�������Ʒ
        new RandomEvent() { eventIndex = 1, probability = 30f },
        //�¼�2�� ���������������
        new RandomEvent() { eventIndex = 2, probability = 15f },
        //�¼�3�� ��������ը��
        new RandomEvent() { eventIndex = 3, probability = 15f },
        //�¼�3�� ������������
        new RandomEvent() { eventIndex = 4, probability = 20f },
        //�¼�4�� ��������ʯ׶
        new RandomEvent() { eventIndex = 5, probability = 20f },
    };

    //���ʳ�ȡ����
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
                Debug.Log("δ֪���¼�����");
                break;
        }

        //����ѡȡ���¼���
        if (eventGroups.Count >0)
        {
            // ��������Ч�����ܸ���
            float totalProbability = 0f;
            foreach (var effect in eventGroups)
            {
                totalProbability += effect.probability;
            }

            // ����һ�����������Χ�� 0 �� totalProbability
            float randomValue = Random.Range(0f, totalProbability);

            // �����������ѡ��Ч��
            foreach (var effect in eventGroups)
            {
                randomValue -= effect.probability;
                if (randomValue <= 0f)
                {
                    // ���ض�Ӧ��Ч������
                    return effect.eventIndex;
                }
            }
        }

        // ���û�д������򷵻�0ֵ
        return 0;

    }

}
