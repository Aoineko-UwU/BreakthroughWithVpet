using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Item_Gacha : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPool; //�����
    [SerializeField] private GameObject canPickItem;     //��ʰȡ��ƷԤ�Ƽ�
    [SerializeField] private GameObject bomb;            //ը��
    [SerializeField] private GameObject spring;          //����
    [SerializeField] private GameObject stoneCone;       //ʯ׶
    [SerializeField] private GameObject openParticle;    //Ť����������Ч��

    private SpriteRenderer sprite;


    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();    //��ȡ�������
    }

    private void Start()
    {
        StartCoroutine(GachaAction());
    }

    Color shiningColor = new Color(0.3f, 0.3f, 0.3f, 1f);   //��˸��ɫ

    IEnumerator GachaAction()
    {
        //������˸Ч��
        var tween =  sprite.DOColor(shiningColor, 0.5f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
        //�ȴ���˸
        yield return new WaitForSeconds(2f);

        OpenRandomItemEvent();  //����¼�
        //��������
        Instantiate(openParticle, transform.position, Quaternion.identity);
        tween.Kill();           //��ֹ����
        Destroy(gameObject);
    }

    //���������Ʒ�¼�
    private void OpenRandomItemEvent()
    {
        //��ȡ������¼����
        int eventIndex = RandomSelector.Instance.EventRandomSelector(2);

        switch (eventIndex)
        {
            //����������ʰȡ��Ʒ
            case 1:
                int randItemAmount = Random.Range(1, 3);    //�������
                for(int i = 0; i < randItemAmount; i++)
                {
                    ItemData item = InventoryManager.Instance.GetRandomItem();  //��ȡ�������Ʒ����
                    //�������λ�ò�����
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    var itemCanPick = Instantiate(canPickItem, randPos, Quaternion.identity);
                    itemCanPick.GetComponent<CanPickItem>().SetItemData(item);      //������Ʒ����
                    AudioManager.Instance.PlaySound3D("Gacha_item", transform.position);
                }
                break;

            //���������������
            case 2:
                int randEnemyAmount = Random.Range(1, 4);    //�������
                AudioManager.Instance.PlaySound3D("Gacha_bad", transform.position);
                for (int i = 0; i < randEnemyAmount; i++)
                {
                    GameObject enemy = enemyPool[Random.Range(0, enemyPool.Count)]; //ѡ��һ���������
                    //�������λ�ò�����
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    var newEnemy = Instantiate(enemy, randPos, Quaternion.identity);
                    newEnemy.GetComponent<EnemyHealthSystem>().health = 5f;         //���ٹ��������
                }
                break;

            //��������ը��
            case 3:
                int randBombAmount = Random.Range(2, 8);    //�������
                for (int i = 0; i < randBombAmount; i++)
                {
                    //�������λ�ò�����
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    Instantiate(bomb, randPos, Quaternion.identity);
                }
                break;

            //������������
            case 4:
                AudioManager.Instance.PlaySound3D("spring_active", transform.position);
                int randSpringAmount = Random.Range(2, 6);    //�������
                for (int i = 0; i < randSpringAmount; i++)
                {
                    //�������λ�ò�����
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-2f, 2f), transform.position.y + Random.Range(-2f, 2f));
                    Instantiate(spring, randPos, Quaternion.identity);
                }
                break;

            //��������ʯ׶
            case 5:
                AudioManager.Instance.PlaySound3D("Gacha_bad", transform.position);
                int randStoneconeAmount = Random.Range(3, 7);    //�������
                for (int i = 0; i < randStoneconeAmount; i++)
                {
                    //�������λ�ò�����
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-3f, 3f), transform.position.y + Random.Range(-2f, 2f));
                    var stone = Instantiate(stoneCone, randPos, Quaternion.identity);
                    stone.GetComponent<StoneCone>().isFalling = true;
                    stone.GetComponent<StoneCone>().StartCheck();
                }
                break;


            default:
                Debug.Log("����ʧ�ܣ�δ֪��Ť���¼�");
                break;
        }

    }


}
