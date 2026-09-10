using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// 主菜单类
/// - 管理菜单入场、面板切换、难度选择及进入游戏的过渡流程。
/// </summary>
public class MenuManager : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("摄像机Transform。")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("过渡背景图。")]
    [SerializeField] private Image fadeBackgroundImage;

    [Tooltip("标题Image。")]
    [SerializeField] private Image titleImage;

    [Tooltip("版本文字。")]
    [SerializeField] private TextMeshProUGUI versionText;

    [Tooltip("开始按钮。")]
    [SerializeField] private Button _startBtn01;

    [Tooltip("设置按钮。")]
    [SerializeField] private Button _settingBtn02;

    [Tooltip("关于按钮。")]
    [SerializeField] private Button _aboutBtn03;

    [Tooltip("退出按钮。")]
    [SerializeField] private Button _quitBtn04;

    [Tooltip("教程按钮。")]
    [SerializeField] private Button _guideBtn05;

    [Tooltip("关于面板。")]
    [SerializeField] private GameObject aboutPanel;

    [Tooltip("设置面板。")]
    [SerializeField] private GameObject settingPanel;

    [Tooltip("教程面板01。")]
    [SerializeField] private GameObject guidePanel01;

    [Tooltip("教程面板02。")]
    [SerializeField] private GameObject guidePanel02;

    [Tooltip("难度选择面板。")]
    [SerializeField] private GameObject difficultyPanel;

    [Tooltip("主菜单中用于播放出发动画与移动过渡的桌宠对象。")]
    [SerializeField] private GameObject vpet;

    #endregion

    #region 初始化与入场表现

    /// <summary>
    /// 恢复时间缩放，初始化按钮和界面，并启动菜单入场动画。
    /// </summary>
    private void Start()
    {
        Time.timeScale = 1f;
        InitButton();   //按钮初始化
        InitUI();       //UI初始化

        //启用动画协程
        StartCoroutine(InitAnimation());
    }

    /// <summary>
    /// 将主菜单按钮设为透明并禁止点击交互。
    /// </summary>
    private void InitButton()
    {
        //初始化按钮透明度
        _startBtn01.GetComponent<CanvasGroup>().alpha = 0;
        _settingBtn02.GetComponent<CanvasGroup>().alpha = 0;
        _aboutBtn03.GetComponent<CanvasGroup>().alpha = 0;
        _quitBtn04.GetComponent<CanvasGroup>().alpha = 0;
        _guideBtn05.GetComponent<CanvasGroup>().alpha = 0;

        SetBtnInteractable(false);   //禁止按钮交互
    }

    /// <summary>
    /// 统一设置五个主菜单按钮的点击交互开关。
    /// </summary>
    /// <param name="isAllow">是否允许按钮响应点击。</param>
    private void SetBtnInteractable(bool isAllow)
    {
        _startBtn01.interactable = isAllow;
        _settingBtn02.interactable = isAllow;
        _aboutBtn03.interactable = isAllow;
        _quitBtn04.interactable = isAllow;
        _guideBtn05.interactable = isAllow;
    }

    /// <summary>
    /// 统一设置五个主菜单按钮的悬停反馈开关。
    /// </summary>
    /// <param name="isAllow">是否允许按钮显示悬停反馈及播放音效。</param>
    private void SetBtnHoverActable(bool isAllow)
    {
        _startBtn01.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _settingBtn02.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _aboutBtn03.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _quitBtn04.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _guideBtn05.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
    }

    /// <summary>
    /// 关闭功能面板，并将版本文本设为透明。
    /// </summary>
    private void InitUI()
    {
        aboutPanel.SetActive(false);
        settingPanel.SetActive(false);
        guidePanel01.SetActive(false);
        guidePanel02.SetActive(false);
        difficultyPanel.SetActive(false);
        versionText.alpha = 0f;
    }

    /// <summary>
    /// 播放菜单镜头、标题与按钮入场动画，切换菜单音乐并在结束阶段启用交互。
    /// </summary>
    /// <returns>串联菜单入场等待阶段的协程迭代器。</returns>
    IEnumerator InitAnimation()
    {
        //初始化过渡背景
        fadeBackgroundImage.gameObject.SetActive(true);
        fadeBackgroundImage.color = Color.white;
        fadeBackgroundImage.DOFade(0, 1.5f);            //过渡效果
        //初始化摄像机
        cameraTransform.position = new Vector3(cameraTransform.position.x, 7f, cameraTransform.position.z);
        cameraTransform.DOMove(new Vector3(cameraTransform.position.x, 0f, cameraTransform.position.z),5f);     //摄像机移动效果
        //初始化标题UI
        titleImage.color = new Color(1f, 1f, 1f, 0f);
        titleImage.fillAmount = 0f;

        AudioManager.Instance.AdjustBGMVolume(1);       //设置BGM音源音量
        AudioManager.Instance.ClearBGM();
        AudioManager.Instance.PlayBGM("MenuMusic");     //播放音乐
        yield return new WaitForSeconds(3f);
        titleImage.DOFade(1f, 2f);
        yield return new WaitForSeconds(0.5f);
        titleImage.DOFillAmount(1f, 1f);
        yield return new WaitForSeconds(1.3f);
        titleImage.rectTransform.DORotate(new Vector3(0, 0, 3f), 1.8f)
              .SetEase(Ease.InOutSine)
              .SetLoops(-1, LoopType.Yoyo);

        //按钮渐入效果
        _startBtn01.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _settingBtn02.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _aboutBtn03.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _quitBtn04.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _guideBtn05.GetComponent<CanvasGroup>().DOFade(1, 2f);

        versionText.DOFade(1, 2f);

        yield return new WaitForSeconds(1.4f);
        //设置按钮的可交互性
        SetBtnInteractable(true);
        SetBtnHoverActable(true);
    }

    #endregion

    #region 开始游戏与难度选择

    /// <summary>
    /// 打开难度选择面板。
    /// </summary>
    public void OnClickBtn_Start()
    {
        AudioManager.Instance.PlaySound("button_click");        //按钮音效
        difficultyPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏菜单，播放桌宠移动及白场过渡，淡出音乐后异步加载游戏场景。
    /// </summary>
    /// <returns>串联离开菜单过渡阶段的协程迭代器。</returns>
    IEnumerator StartAnimation()
    {
        //禁止按钮交互&按钮渐隐
        SetBtnInteractable(false);
        SetBtnHoverActable(false);
        _startBtn01.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _settingBtn02.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _aboutBtn03.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _quitBtn04.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _guideBtn05.GetComponent<CanvasGroup>().DOFade(0, 1f);
        //标题Image渐隐
        titleImage.DOFade(0, 1f);
        versionText.DOFade(0, 1f);

        yield return new WaitForSeconds(1.2f);

        fadeBackgroundImage.DOFade(1f, 4f);     //白场过渡
        vpet.transform.DOLocalMoveX(vpet.transform.position.x + 15f, 7f);  //桌宠移动

        yield return new WaitForSeconds(2f);

        float duration = 2f;
        float elapsed = 0f;
        float startVolume = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);
            AudioManager.Instance.AdjustBGMVolume(currentVolume);
            yield return null;
        }
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停BGM
        AudioManager.Instance.AdjustBGMVolume(1);       //恢复BGM音源音量

        // 等待淡出动画完成
        yield return new WaitForSeconds(2.2f);

        DOTween.KillAll();          //清理所有Tween动画
        // 开始加载下一场景（异步）
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("1_GameScene");
        asyncLoad.allowSceneActivation = true;
    }

    /// <summary>
    /// 选择简单难度并开始游戏。
    /// </summary>
    public void OnClickBtn_SelectDif_Eazy()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Easy);  //难度设置为简单
        GameStart();    //开始游戏
    }

    /// <summary>
    /// 选择普通难度并开始游戏。
    /// </summary>
    public void OnClickBtn_SelectDif_Normal()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Normal);  //难度设置为普通
        GameStart();    //开始游戏
    }

    /// <summary>
    /// 选择困难难度并开始游戏。
    /// </summary>
    public void OnClickBtn_SelectDif_Hard()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Hard);  //难度设置为困难
        GameStart();    //开始游戏
    }

    /// <summary>
    /// 关闭难度选择面板。
    /// </summary>
    public void OnClickBtn_SelectDif_Close()
    {
        AudioManager.Instance.PlaySound("button_click");
        difficultyPanel.SetActive(false);
    }

    /// <summary>
    /// 关闭难度面板，触发桌宠出发动画并启动进入游戏的过渡。
    /// </summary>
    private void GameStart()
    {
        difficultyPanel.SetActive(false);
        vpet.GetComponent<Animator>().SetTrigger("start");      //桌宠动画
        StartCoroutine(StartAnimation());   //开始动画(协程)
    }

    #endregion

    #region 菜单面板与退出

    /// <summary>
    /// 打开设置面板。
    /// </summary>
    public void OnClickBtn_OpenSettingPanel()
    {
        settingPanel.SetActive(true);
        AudioManager.Instance.PlaySound("button_click");
    }

    /// <summary>
    /// 关闭设置面板。
    /// </summary>
    public void OnClickBtn_CloseSettingPanel()
    {
        settingPanel.SetActive(false);
        AudioManager.Instance.PlaySound("button_click");
    }

    /// <summary>
    /// 打开关于面板。
    /// </summary>
    public void OnClickBtn_OpenAboutPanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        aboutPanel.SetActive(true);
    }

    /// <summary>
    /// 关闭关于面板。
    /// </summary>
    public void OnClickBtn_CloseAboutPanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        aboutPanel.SetActive(false);
    }

    /// <summary>
    /// 打开教程第一页。
    /// </summary>
    public void OnClickBtn_OpenGuidePanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(true);   //打开guide1
        guidePanel02.SetActive(false);  //禁用guide2
        _guideBtn05.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从教程第一页切换到第二页。
    /// </summary>
    public void OnClickBtn_Guide1To2()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel02.SetActive(true);   //打开guide2
        guidePanel01.SetActive(false);  //禁用guide1
        _guideBtn05.gameObject.SetActive(false);
    }

    /// <summary>
    /// 关闭教程面板并恢复菜单按钮。
    /// </summary>
    public void OnClickBtn_CloseGuidePanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(false);   //禁用guide1
        guidePanel02.SetActive(false);   //禁用guide2
        _guideBtn05.gameObject.SetActive(true);
    }

    /// <summary>
    /// 退出应用程序。
    /// </summary>
    public void OnClickBtn_Quit()
    {
        AudioManager.Instance.PlaySound("button_click");
        Application.Quit();     //关闭应用
    }

    #endregion
}
