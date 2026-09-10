using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// 游戏流程管理类
/// - 协调游戏开场、暂停、结算、重生和教程界面。
/// </summary>
public class GameManager : Singleton<GameManager>
{
    #region 配置与运行状态

    [Tooltip("摄像机缩放脚本。")]
    [SerializeField] private CameraScaleBar cameraScale;

    [Tooltip("GUI画布组。")]
    [SerializeField] private CanvasGroup GUICanvasGroup;

    [Tooltip("白场过渡图。")]
    [SerializeField] private Image whiteFade;

    [Tooltip("设置面板。")]
    [SerializeField] private GameObject settingPanel;

    [Tooltip("胜利面板。")]
    [SerializeField] private GameObject winPanel;

    [Tooltip("失败面板。")]
    [SerializeField] private GameObject losePanel;

    [Tooltip("游戏设置面板。")]
    [SerializeField] private GameObject gameSetPanel;

    [Tooltip("所有结算面板的CanvasGroup。")]
    [SerializeField] private CanvasGroup resultPanelCGroup;

    [Tooltip("开始文本。")]
    [SerializeField] private TextMeshProUGUI startText;

    [Tooltip("胜利面板的Avatar。")]
    [SerializeField] private Image winAvatar;

    [Tooltip("胜利图1。")]
    [SerializeField] private Sprite winPic01;

    [Tooltip("胜利图2。")]
    [SerializeField] private Sprite winPic02;

    [Tooltip("胜利图3。")]
    [SerializeField] private Sprite winPic03;

    [Tooltip("胜利文字01。")]
    [SerializeField] private TextMeshProUGUI winText01;

    [Tooltip("胜利面板的第二行评价文本。")]
    [SerializeField] private TextMeshProUGUI winText02;

    [Tooltip("胜利位置的Transform。")]
    [SerializeField] private Transform winPosTransform;

    [Tooltip("教程按钮。")]
    [SerializeField] private Button guideBtn;

    [Tooltip("教程面板01。")]
    [SerializeField] private GameObject guidePanel01;

    [Tooltip("教程面板02。")]
    [SerializeField] private GameObject guidePanel02;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    [Tooltip("游戏流程是否允许玩家操作，由开场、结算及交互入口读取或更新。")]
    public bool isAllowPlayerControl = true;

    #endregion

    #region 生命周期

    /// <summary>
    /// 注册场景单例；仅有效实例缓存桌宠对象。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠对象
    }

    /// <summary>
    /// 读取重生标记，启动开场流程，初始化面板并切换到游戏音乐。
    /// </summary>
    private void Start()
    {
        InitRespawn();      //初始化重生标志
        GameStartInit();    //游戏开场初始
        InitPanel();        //面板初始化
        AudioManager.Instance.AdjustBGMVolume(1);
        AudioManager.Instance.ClearBGM();
        AudioManager.Instance.PlayBGM("GameMusic");
    }

    /// <summary>
    /// 更新道具选择保护状态、处理退出键并检测终点到达。
    /// </summary>
    private void Update()
    {
        SelectedStateHandle();      //选择状态处理(防按钮误触)
        HandleEscape();             //处理ESC
        CheckWin();                 //玩家胜利监测
    }

    #endregion

    #region 输入与界面初始化

    /// <summary>
    /// 允许玩家操作时响应退出键，优先关闭设置或教程，否则切换暂停状态。
    /// </summary>
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

    /// <summary>当前是否有Slot选中。</summary>
    private bool currentSelected = false;

    /// <summary>计时器。</summary>
    private float selectTimer;

    /// <summary>物品栏物品放置后多久可以点击暂停按钮。</summary>
    private float selectInterval = 0.5f;

    /// <summary>
    /// 在拖拽物品期间及结束后的短暂间隔内保留选中标记，供按钮入口判断。
    /// </summary>
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

    /// <summary>
    /// 关闭各功能面板并设置开场过渡所需的透明度及初始激活状态。
    /// </summary>
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

    #endregion

    #region 开场过渡

    [Tooltip("是否是以重新开始游戏的状态进入的。")]
    public bool isRestart = false;

    /// <summary>
    /// 将桌宠放到当前重生位置，再根据重启标记选择首次开场或重生过渡。
    /// </summary>
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

    /// <summary>
    /// 依次播放白场、镜头和文字开场，随后允许操作并让桌宠开始行走。
    /// </summary>
    /// <returns>控制首次开场各等待阶段的协程迭代器。</returns>
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

    /// <summary>
    /// 播放重生白场过渡并显示界面，结束后将桌宠切换为行走状态。
    /// </summary>
    /// <returns>控制重生过渡等待的协程迭代器。</returns>
    IEnumerator RespawnStart()
    {
        whiteFade.DOFade(0, 2f);                //白场过渡
        yield return new WaitForSeconds(1f);
        GUICanvasGroup.DOFade(1, 1f);           //GUI显现
        yield return new WaitForSeconds(1f);
        whiteFade.gameObject.SetActive(false);  //关闭白场过渡GameObject
        vpet.GetComponent<VpetAction>().VpetStateSet(1);   //设置Vpet行走状态
    }

    #endregion

    #region 重生数据与结算

    [Tooltip("当前重生点位置。")]
    public Transform respawnPosition;

    /// <summary>当前重生点优先级。</summary>
    private int currentRespawnOrder = -1;

    /// <summary>
    /// 读取一次性重启标记、重生坐标与优先级，应用后删除对应 PlayerPrefs 键。
    /// </summary>
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

    /// <summary>
    /// 设置当前重生点优先级，不在此处比较新旧优先级。
    /// </summary>
    /// <param name="p">要记录的重生点优先级。</param>
    public void SetCurrentRespawnOrder(int p)
    {
        currentRespawnOrder = p;
    }

    /// <summary>
    /// 读取当前重生点优先级。
    /// </summary>
    /// <returns>当前优先级；尚未激活重生点时默认值为 -1。</returns>
    public int GetCurrentRespawnOrder()
    {
        return currentRespawnOrder;
    }

    /// <summary>
    /// 启动桌宠死亡后的镜头、音乐和失败结算流程。
    /// </summary>
    public void VpetDeadHandle()
    {
        isAllowPlayerControl = false;                   //禁止玩家操作UI
        cameraScale.DieCameraScale();                   //死亡镜头缩放特写
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停当前BGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI渐隐
        StartCoroutine(LoseMusicAndAnimation());        //开始播放失败动画
    }

    /// <summary>
    /// 等待死亡特写后播放失败音乐，并显示失败结算面板。
    /// </summary>
    /// <returns>控制失败结算等待阶段的协程迭代器。</returns>
    IEnumerator LoseMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //等待
        AudioManager.Instance.PlaySound("LoseMusic");   //播放失败音乐
        yield return new WaitForSeconds(1.5f);          //等待
        losePanel.SetActive(true);                      //设置面板可见
        resultPanelCGroup.DOFade(1, 1f);                //结算面板渐出

    }

    /// <summary>是否已触发终点到达判定，用于避免重复检测进入。</summary>
    private bool isWin = false;

    /// <summary>
    /// 首次到达终点横坐标时标记胜利，并调用结算入口与桌宠胜利行为。
    /// </summary>
    private void CheckWin()
    {
        if (vpet.transform.position.x >= winPosTransform.position.x && !isWin)
        {
            isWin = true;
            VpetWinHandle();    //胜利处理
            vpet.GetComponent<VpetAction>().VpetWin();  //Vpet动作处理
        }
    }

    /// <summary>
    /// 启动桌宠胜利后的镜头、音乐和胜利结算流程。
    /// </summary>
    public void VpetWinHandle()
    {
        isAllowPlayerControl = false;                   //禁止玩家操作UI
        cameraScale.WinCameraScale();                   //胜利镜头缩放特写
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停当前BGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI渐隐
        SetWinPanelInfo();                              //设置胜利面板信息
        StartCoroutine(WinMusicAndAnimation());         //开始播放胜利动画
    }

    /// <summary>
    /// 延迟播放胜利音乐，并显示胜利结算面板。
    /// </summary>
    /// <returns>控制胜利结算等待阶段的协程迭代器。</returns>
    IEnumerator WinMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //等待
        AudioManager.Instance.PlaySound("WinMusic");    //播放胜利音乐
        winPanel.SetActive(true);                       //设置面板可见
        resultPanelCGroup.DOFade(1, 1f);                //结算面板渐出
    }

    /// <summary>
    /// 根据当前难度选择胜利头像与评价文本。
    /// </summary>
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

    #endregion

    #region 按钮交互

    /// <summary>由暂停和教程入口维护的暂停标记。</summary>
    private bool isPaused = false;

    /// <summary>
    /// 暂停游戏并打开暂停面板。
    /// </summary>
    public void OnClickBtn_Pause()
    {
        if (currentSelected && !isAllowPlayerControl) return;
        Time.timeScale = 0;     //暂停游戏
        isPaused = true;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(true);
    }

    /// <summary>
    /// 恢复游戏并关闭暂停面板。
    /// </summary>
    public void OnClickBtn_Continue()
    {
        Time.timeScale = 1;     //继续游戏
        isPaused = false;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(false);
    }

    /// <summary>
    /// 打开游戏设置面板。
    /// </summary>
    public void OnClickBtn_GameSetOpen()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(true);   //打开设置面板
    }

    /// <summary>
    /// 关闭游戏设置面板。
    /// </summary>
    public void OnClickBtn_GameSetClose()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(false);   //关闭设置面板
    }

    /// <summary>
    /// 停止当前流程并返回主菜单场景。
    /// </summary>
    public void OnClickBtn_BackToMenu()
    {
        AudioManager.Instance.PlaySound("button_click");                //播放音效
        DOTween.KillAll();
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene("0_MainMenu");
    }

    /// <summary>
    /// 保存当前重生点并重新加载游戏场景。
    /// </summary>
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

    /// <summary>
    /// 停止补间动画及胜利音效后重新加载当前场景，不在此处写入重生数据。
    /// </summary>
    public void OnClickBtn_Restart()
    {
        AudioManager.Instance.PlaySound("button_click");                //播放音效
        DOTween.KillAll();
        //重新加载当前场景
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 打开游戏内教程第一页并暂停游戏。
    /// </summary>
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

    /// <summary>
    /// 从游戏内教程第一页切换到第二页。
    /// </summary>
    public void OnClickBtn_Guide1To2()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel02.SetActive(true);   //打开guide2
        guidePanel01.SetActive(false);  //禁用guide1
        guideBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// 关闭游戏内教程并恢复游戏。
    /// </summary>
    public void OnClickBtn_CloseGuidePanel()
    {

        Time.timeScale = 1;     //继续游戏
        isPaused = false;

        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(false);   //禁用guide1
        guidePanel02.SetActive(false);   //禁用guide2
        guideBtn.gameObject.SetActive(true);
    }

    #endregion
}
