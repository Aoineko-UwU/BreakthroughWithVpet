using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Item_Gacha : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPool; //怪物池
    [SerializeField] private GameObject canPickItem;     //可拾取物品预制件
    [SerializeField] private GameObject bomb;            //炸弹
    [SerializeField] private GameObject spring;          //弹簧
    [SerializeField] private GameObject stoneCone;       //石锥
    [SerializeField] private GameObject openParticle;    //扭蛋开启粒子效果

    private SpriteRenderer sprite;


    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();    //获取精灵组件
    }

    private void Start()
    {
        StartCoroutine(GachaAction());
    }

    Color shiningColor = new Color(0.3f, 0.3f, 0.3f, 1f);   //闪烁颜色

    IEnumerator GachaAction()
    {
        //设置闪烁效果
        var tween =  sprite.DOColor(shiningColor, 0.5f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
        //等待闪烁
        yield return new WaitForSeconds(2f);

        OpenRandomItemEvent();  //随机事件
        //生成粒子
        Instantiate(openParticle, transform.position, Quaternion.identity);
        tween.Kill();           //终止动画
        Destroy(gameObject);
    }

    //开到随机物品事件
    private void OpenRandomItemEvent()
    {
        //获取随机的事件编号
        int eventIndex = RandomSelector.Instance.EventRandomSelector(2);

        switch (eventIndex)
        {
            //生成数个可拾取物品
            case 1:
                int randItemAmount = Random.Range(1, 3);    //随机数量
                for(int i = 0; i < randItemAmount; i++)
                {
                    ItemData item = InventoryManager.Instance.GetRandomItem();  //获取随机的物品数据
                    //设置随机位置并生成
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    var itemCanPick = Instantiate(canPickItem, randPos, Quaternion.identity);
                    itemCanPick.GetComponent<CanPickItem>().SetItemData(item);      //设置物品数据
                    AudioManager.Instance.PlaySound3D("Gacha_item", transform.position);
                }
                break;

            //生成数个随机怪物
            case 2:
                int randEnemyAmount = Random.Range(1, 4);    //随机数量
                AudioManager.Instance.PlaySound3D("Gacha_bad", transform.position);
                for (int i = 0; i < randEnemyAmount; i++)
                {
                    GameObject enemy = enemyPool[Random.Range(0, enemyPool.Count)]; //选择一个随机怪物
                    //设置随机位置并生成
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    var newEnemy = Instantiate(enemy, randPos, Quaternion.identity);
                    newEnemy.GetComponent<EnemyHealthSystem>().health = 5f;         //减少怪物的生命
                }
                break;

            //生成数个炸弹
            case 3:
                int randBombAmount = Random.Range(2, 8);    //随机数量
                for (int i = 0; i < randBombAmount; i++)
                {
                    //设置随机位置并生成
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f));
                    Instantiate(bomb, randPos, Quaternion.identity);
                }
                break;

            //生成数个弹簧
            case 4:
                AudioManager.Instance.PlaySound3D("spring_active", transform.position);
                int randSpringAmount = Random.Range(2, 6);    //随机数量
                for (int i = 0; i < randSpringAmount; i++)
                {
                    //设置随机位置并生成
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-2f, 2f), transform.position.y + Random.Range(-2f, 2f));
                    Instantiate(spring, randPos, Quaternion.identity);
                }
                break;

            //生成数个石锥
            case 5:
                AudioManager.Instance.PlaySound3D("Gacha_bad", transform.position);
                int randStoneconeAmount = Random.Range(3, 7);    //随机数量
                for (int i = 0; i < randStoneconeAmount; i++)
                {
                    //设置随机位置并生成
                    Vector2 randPos = new Vector2(transform.position.x + Random.Range(-3f, 3f), transform.position.y + Random.Range(-2f, 2f));
                    var stone = Instantiate(stoneCone, randPos, Quaternion.identity);
                    stone.GetComponent<StoneCone>().isFalling = true;
                    stone.GetComponent<StoneCone>().StartCheck();
                }
                break;


            default:
                Debug.Log("生成失败：未知的扭蛋事件");
                break;
        }

    }


}
