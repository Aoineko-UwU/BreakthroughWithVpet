using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraScaleBar cameraScale;    //摄像机缩放脚本
    [SerializeField] private CanvasGroup GUICanvasGroup;    //GUI画布组
    [SerializeField] private Image whiteFade;               //白场过渡图

    [SerializeField] private GameObject settingPanel;       //设置面板
    [SerializeField] private GameObject winPanel;           //胜利面板
    [SerializeField] private GameObject losePanel;          //失败面板
    [SerializeField] private GameObject gameSetPanel;       //游戏设置面板
    [SerializeField] private CanvasGroup resultPanelCGroup; //所有结算面板的CanvasGroup
    [SerializeField] private TextMeshProUGUI startText;     //开始文本

    [SerializeField] private Image winAvatar;               //胜利面板的Avatar
    [SerializeField] private Sprite winPic01;               //胜利图1
    [SerializeField] private Sprite winPic02;               //胜利图2
    [SerializeField] private Sprite winPic03;               //胜利图3
    [SerializeField] private TextMeshProUGUI winText01;     //胜利文字01
    [SerializeField] private TextMeshProUGUI winText02;     //胜利文字02s


    [SerializeField] private Transform winPosTransform;     //胜利位置的Transform

    [SerializeField] private Button guideBtn;            //教程按钮
    [SerializeField] private GameObject guidePanel01;    //教程面板01
    [SerializeField] private GameObject guidePanel02;    //教程面板02

    private GameObject vpet; //桌宠游戏对象

    public static GameManager Instance;       //游戏管理器脚本单例 

    public bool isAllowPlayerControl = true;  //是否允许角色操作

    //生命周期--------------------------------------------------------------------------------------------//

    private void Awake()
    {
        Instance = this;    //单例指向本身
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠对象
    }

    private void Start()
    {
        InitRespawn();      //初始化重生标志
        GameStartInit();    //游戏开场初始
        InitPanel();        //面板初始化
        AudioManager.Instance.AdjustBGMVolume(1);
        AudioManager.Instance.ClearBGM();   
        AudioManager.Instance.PlayBGM("GameMusic");
    }

    private void Update()
    {
        SelectedStateHandle();      //选择状态处理(防按钮误触)
        HandleEscape();             //处理ESC
        CheckWin();                 //玩家胜利监测
    }

    //功能函数--------------------------------------------------------------------------------------------//

    //处理ESC事件
    private void HandleEscape()
    {
        if (!isAllowPlayerControl) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //若是暂停状态
            if (isPaused)
            {
                //若此时已打开设置面板
                if (gameSetPanel.activeSelf) OnClickBtn_GameSetClose(); //优先关闭设置面板 
                                                                        //若此时打开的是教程面板
                else if (guidePanel01.activeSelf || guidePanel02.activeSelf) OnClickBtn_CloseGuidePanel();
                //否则就是暂停面板
                else OnClickBtn_Continue();
            }
            else
            {
                OnClickBtn_Pause(); //暂停游戏
            }
        }
    }

    private bool currentSelected = false;   //当前是否有Slot选中
    private float selectTimer;              //计时器
    private float selectInterval = 0.5f;    //物品栏物品放置后多久可以点击暂停按钮

    //物品选中状态处理(针对暂停按钮，防止误触)
    private void SelectedStateHandle()
    {
        //同步选中状态
        if (DragController.Instance.isSelected)
        {
            currentSelected = true;
            selectTimer = selectInterval;       //赋予时间间隔
        }
        
        if (currentSelected)
            if (!DragController.Instance.isSelected)
            {
                if (selectTimer > 0)
                    selectTimer -= Time.deltaTime;
                else
                    currentSelected = false;
            }
    }

    //面板初始化(Start)
    private void InitPanel()
    {
        //面板默认关闭
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        settingPanel.SetActive(false);
        gameSetPanel.SetActive(false);
        guidePanel01.SetActive(false);
        guidePanel02.SetActive(false);

        //UI默认透明
        resultPanelCGroup.alpha = 0;         
        GUICanvasGroup.alpha = 0;

        resultPanelCGroup.gameObject.SetActive(true);   //结算面板游戏对象默认启动
        whiteFade.gameObject.SetActive(true);           //白场默认启动
        startText.gameObject.SetActive(false);          //开场文字默认关闭
    }


    public bool isRestart = false;      //是否是以重新开始游戏的状态进入的？

    //游戏开始初始化
    private void GameStartInit()
    {
        vpet.transform.position = respawnPosition.position;  //设置桌宠的重生位置
        //若不是重生状态
        if (!isRestart)
            StartCoroutine(FirstStart());
        //若是重生状态
        else
            StartCoroutine(RespawnStart());

    }

    IEnumerator FirstStart()
    {   
        isAllowPlayerControl = false;           //禁止玩家操作
        whiteFade.DOFade(0, 2f);                //白场过渡
        yield return new WaitForSeconds(1f);    //等待过渡
        cameraScale.StartCameraScale();         //开始镜头缩放效果
        yield return new WaitForSeconds(2.5f);  //等待镜头效果

        //开场文字
        startText.gameObject.SetActive(true);
        startText.SetText("保护");
        AudioManager.Instance.PlaySound("cat01");
        yield return new WaitForSeconds(0.7f);
        startText.SetText("保护 萝莉丝");
        AudioManager.Instance.PlaySound("cat01");
        yield return new WaitForSeconds(0.7f);
        startText.SetText("保护 萝莉丝 出发！");
        AudioManager.Instance.PlaySound("cat02");
        yield return new WaitForSeconds(0.7f);

        startText.DOFade(0, 1f);
        isAllowPlayerControl = true;            //允许玩家操作
        whiteFade.gameObject.SetActive(false);  //关闭白场过渡GameObject
        GUICanvasGroup.DOFade(1, 1f);           //GUI显现
        vpet.GetComponent<VpetAction>().VpetStateSet(1);   //设置Vpet行走状态

        yield return new WaitForSeconds(1f);
        startText.gameObject.SetActive(false);
    }

    IEnumerator RespawnStart()
    {
        whiteFade.DOFade(0, 2f);                //白场过渡
        yield return new WaitForSeconds(1f);    
        GUICanvasGroup.DOFade(1, 1f);           //GUI显现
        yield return new WaitForSeconds(1f);
        whiteFade.gameObject.SetActive(false);  //关闭白场过渡GameObject
        vpet.GetComponent<VpetAction>().VpetStateSet(1);   //设置Vpet行走状态
    }

    //桌宠相关进程函数--------------------------------------------------------------------------------------------//

    public Transform respawnPosition;            //当前重生点位置
    private int currentRespawnOrder = -1;        //当前重生点优先级

    private void InitRespawn()
    {
        //读取PlayerPrefs中的复活标志
        isRestart = PlayerPrefs.GetInt("isRestart", 0) == 1;
        //若读取成功
        if (isRestart)
        {
            // 读取并应用重生点
            float x = PlayerPrefs.GetFloat("respawnX", 0f);
            float y = PlayerPrefs.GetFloat("respawnY", 0f);
            respawnPosition.position = new Vector2(x, y);
            // 读取优先级
            currentRespawnOrder = PlayerPrefs.GetInt("respawnOrder", -1);
            // 用完清除
            PlayerPrefs.DeleteKey("isRestart");
            PlayerPrefs.DeleteKey("respawnX");
            PlayerPrefs.DeleteKey("respawnY");
            PlayerPrefs.DeleteKey("respawnOrder");
        }
    }

    //设置重生点优先级
    public void SetCurrentRespawnOrder(int p)
    {
        currentRespawnOrder = p;
    }
    //获取当前的重生点优先级
    public int GetCurrentRespawnOrder()
    {
        return currentRespawnOrder;
    }


    //桌宠死亡处理
    public void VpetDeadHandle()
    {
        isAllowPlayerControl = false;                   //禁止玩家操作UI
        cameraScale.DieCameraScale();                   //死亡镜头缩放特写
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停当前BGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI渐隐
        StartCoroutine(LoseMusicAndAnimation());        //开始播放失败动画
    }

    IEnumerator LoseMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //等待
        AudioManager.Instance.PlaySound("LoseMusic");   //播放失败音乐
        yield return new WaitForSeconds(1.5f);          //等待
        losePanel.SetActive(true);                      //设置面板可见
        resultPanelCGroup.DOFade(1, 1f);                //结算面板渐出

    }

    private bool isWin = false;

    //检查是否获胜
    private void CheckWin()
    {
        if (vpet.transform.position.x >= winPosTransform.position.x && !isWin)
        {
            isWin = true;
            VpetWinHandle();    //胜利处理
            vpet.GetComponent<VpetAction>().VpetWin();  //Vpet动作处理
        }
    }

    //桌宠胜利处理
    public void VpetWinHandle()
    {
        isAllowPlayerControl = false;                   //禁止玩家操作UI
        cameraScale.WinCameraScale();                   //胜利镜头缩放特写
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停当前BGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI渐隐
        SetWinPanelInfo();                              //设置胜利面板信息
        StartCoroutine(WinMusicAndAnimation());         //开始播放胜利动画
    }

    IEnumerator WinMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //等待
        AudioManager.Instance.PlaySound("WinMusic");    //播放胜利音乐
        winPanel.SetActive(true);                       //设置面板可见
        resultPanelCGroup.DOFade(1, 1f);                //结算面板渐出
    }

    //设置胜利面板的信息
    private void SetWinPanelInfo()
    {
        //根据游戏难度设置评价
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //若是简单难度
            case GameDifficultyLevel.Easy:
                winAvatar.sprite = winPic01;
                winText01.SetText("还算合格喵~");
                winText02.SetText("(好感度+50)");
                break;

            //若是普通难度
            case GameDifficultyLevel.Normal:
                winAvatar.sprite = winPic02;
                winText01.SetText("不愧是我的主人喵~");
                winText02.SetText("(好感度+100)");
                break;

            //若是困难难度
            case GameDifficultyLevel.Hard:
                winAvatar.sprite = winPic03;
                winText01.SetText("主人这么强吗喵！！");
                winText02.SetText("(好感度+114514)");
                break;
        }
    }

    //按钮---------------------------------------------------------------------------------------------------//

    private bool isPaused = false;

    //暂停按钮(外部绑定)
    public void OnClickBtn_Pause()
    {
        if (currentSelected && !isAllowPlayerControl) return;
        Time.timeScale = 0;     //暂停游戏
        isPaused = true;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(true);
    }

    //继续游戏按钮(外部绑定)
    public void OnClickBtn_Continue()
    {
        Time.timeScale = 1;     //继续游戏
        isPaused = false;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(false);
    }

    //游戏设置按钮(外部绑定)
    public void OnClickBtn_GameSetOpen()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(true);   //打开设置面板
    }

    //关闭游戏设置按钮(外部绑定)
    public void OnClickBtn_GameSetClose()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(false);   //关闭设置面板
    }

    //点击返回主菜单按钮
    public void OnClickBtn_BackToMenu()
    {
        AudioManager.Instance.PlaySound("button_click");                //播放音效
        DOTween.KillAll();
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene("0_MainMenu");
    }


    //点击继续游戏按钮(外部绑定)(从最近的重生点复活)
    public void OnClickBtn_Respawn()
    {
        //将重启数据写入并保存
        PlayerPrefs.SetInt("isRestart", 1);                             //标记为isRestart = true;
        PlayerPrefs.SetFloat("respawnX", respawnPosition.position.x);   //保存重生坐标X
        PlayerPrefs.SetFloat("respawnY", respawnPosition.position.y);   //保存重生坐标Y
        PlayerPrefs.SetInt("respawnOrder", currentRespawnOrder);        //保存当前重生点优先级

        AudioManager.Instance.PlaySound("button_click");                //播放音效
        DOTween.KillAll();
        //重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //点击重头开始按钮(外部绑定)(重启场景)
    public void OnClickBtn_Restart()
    {
        AudioManager.Instance.PlaySound("button_click");                //播放音效
        DOTween.KillAll();
        //重新加载当前场景
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //打开教程面板(2跳转1)
    public void OnClickBtn_OpenGuidePanel()
    {
        if (currentSelected && !isAllowPlayerControl) return;

        Time.timeScale = 0;     //暂停游戏
        isPaused = true;
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(true);   //打开guide1
        guidePanel02.SetActive(false);  //禁用guide2
        guideBtn.gameObject.SetActive(false);
    }

    //教程面板1跳转2
    public void OnClickBtn_Guide1To2()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel02.SetActive(true);   //打开guide2
        guidePanel01.SetActive(false);  //禁用guide1
        guideBtn.gameObject.SetActive(false);
    }

    //关闭教程面板
    public void OnClickBtn_CloseGuidePanel()
    {

        Time.timeScale = 1;     //继续游戏
        isPaused = false;

        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(false);   //禁用guide1
        guidePanel02.SetActive(false);   //禁用guide2
        guideBtn.gameObject.SetActive(true);
    }


}
