using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum VpetAvatarState     //����ͷ��״̬
{
    healthy,    //0
    normal,     //1
    bad         //2
}

public class VpetHealthSystem : MonoBehaviour
{
    private float vpetHealth = 30f;        //����������ֵ
    private float _vpetCurrentHealth;      //���赱ǰ����ֵ

    [SerializeField] private Slider _sliderHealthBar;       //Ѫ����
    [SerializeField] private Image _sliderHealthFill;       //Ѫ�������ͼ
    [SerializeField] private Image _stateAvatar;            //״̬ͷ��
    [SerializeField] private Sprite _healthy;               //����״̬����ͼ
    [SerializeField] private Sprite _normal;                //��ͨ״̬����ͼ
    [SerializeField] private Sprite _bad;                   //Σ��״̬����ͼ

    private VpetAvatarState _currentAvatarState;            //��ǰͷ��״̬
    private VpetAvatarState _newAvatarState;                //�µ�ͷ��״̬

    public bool isVpetDead = false;         //�����Ƿ�������

    private Rigidbody2D rb;                 //�������
    private VpetAction vpetAction;          //������Ϊ�ű�

    //��������-----------------------------------------------------------------------------------//

    private void Start()
    {
        InitValueBasedDifficulty();                                         //��ʼ����ֵ
        vpetAction = GetComponent<VpetAction>();                            //��ȡ������Ϊ
        rb = GetComponent<Rigidbody2D>();                                   //��ȡ�������
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //��ȡ����Canvas

        InitHealthBar();                    //��ʼ��Ѫ��

    }

    private void Update()
    {
        UpdateHealthBar();         //����Ѫ��
        UpdateAvatarAndColor();    //����ͷ��״̬��Ѫ����ɫ
    }

    //���ܺ���-----------------------------------------------------------------------------------//

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(��������ֵ|�޵�ʱ��|�����ָ�����)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                vpetHealth = 40f;
                invincibleTime = 1.25f;
                recoverMultiplier = 1.5f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                vpetHealth = 30f;
                invincibleTime = 1f;
                recoverMultiplier = 1f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                vpetHealth = 20f;
                invincibleTime = 0.85f;
                recoverMultiplier = 0.5f;
                break;
        }
    }

    //Ѫ����ʼ��(Start)
    private void InitHealthBar()
    {
        _vpetCurrentHealth = vpetHealth;                //���µ�ǰ����ֵ
        _sliderHealthBar.maxValue = vpetHealth;         //Ѫ��Value���ֵ����ΪVpet����ֵ
        _sliderHealthBar.value = _vpetCurrentHealth;    //��Vpet��ǰ����ֵ���µ�Ѫ��Value
        _sliderHealthFill.color = SetColor("#83ff58");  //��ʼ��Ѫ����ɫ


        //��ʼ������ͷ��״̬
        _currentAvatarState = VpetAvatarState.healthy;  //��ʼΪ����״̬
        _newAvatarState = _currentAvatarState;          //���ֳ�ʼ��״̬һ����
        _stateAvatar.sprite = _healthy;                 //��ʼ��״̬ͷ��
    }

    //Ѫ��ʵʱ����(Update)
    private void UpdateHealthBar()
    {
        //Ϊ�ռ��&&����currentHealthֵ�����ı�ʱִ��
        if (_sliderHealthBar && _vpetCurrentHealth != _sliderHealthBar.value)
        {
            //Ѫ����0������ʱ��������UI
            if (_vpetCurrentHealth >= 0)    
                _sliderHealthBar.value = _vpetCurrentHealth;    //����Ѫ����Value
            //Ѫ����0����ʱĬ������ValueΪ0
            else
                _sliderHealthBar.value = 0;

            //����Ѫ���ٷֱ���ֵ(**[0%~30%),[30%~70%),[70%~N%)**)
            float healthPercent = _vpetCurrentHealth / vpetHealth;  //��ǰ�����ٷֱ�
            if (healthPercent >= 0.7f)
                _newAvatarState = VpetAvatarState.healthy;      //״̬Ϊ����
            else if (healthPercent >= 0.3f)
                _newAvatarState = VpetAvatarState.normal;       //״̬Ϊ��ͨ
            else
                _newAvatarState = VpetAvatarState.bad;          //״̬ΪΣ��
        }
    }

    //����״̬ͷ��&Ѫ����ɫ(Update)
    private void UpdateAvatarAndColor()
    {
        //״̬�����ı�ʱִ��
        if(_currentAvatarState != _newAvatarState)
        {
            _currentAvatarState = _newAvatarState;      //����״̬

            switch (_currentAvatarState)
            {
                //����״̬ʱ
                case VpetAvatarState.healthy:
                    _stateAvatar.sprite = _healthy;
                    _sliderHealthFill.color = SetColor("#83ff58");   //��ɫѪ��
                    break;

                //��ͨ״̬ʱ
                case VpetAvatarState.normal:
                    _stateAvatar.sprite = _normal;
                    _sliderHealthFill.color = SetColor("#fff958");   //��ɫѪ��
                    break;

                //Σ��״̬ʱ
                case VpetAvatarState.bad:
                    _stateAvatar.sprite = _bad;
                    _sliderHealthFill.color = SetColor("#ff7058");   //��ɫѪ��
                    break;

                default:
                    Debug.Log("δ֪״̬");
                    break;
            }
        }
    }

    [SerializeField] private SpriteRenderer vpetSpriteRenderer;   //������SpriteRenderer

    public bool isVpetInvincible = false;   //�����Ƿ��޵У�
    private float invincibleTime = 1f;      //�����޵�ʱ��

    //����Ч��
    IEnumerator VpetHurtEffect()
    {
        vpetSpriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        vpetSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    //�޵�״̬����(Э��)
    IEnumerator VpetInvincibleSet()
    {
        isVpetInvincible = true;    //�����޵�
        yield return new WaitForSeconds(invincibleTime);    //�޵�ʱ��ȴ�
        isVpetInvincible = false;   //�ر��޵�
    }

    //��������-----------------------------------------------------------------------------------//

    //ʹ��ʮ��������ת����ɫֵ(�ڲ���������)
    private Color SetColor(string hexColor)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString(hexColor, out newColor);
        return newColor;
    }

    private float knockBackFactor = 1f; //����ϵ��

    //�����Ƿ�ɻ���
    public void SetKnockBack(bool isAllowKnockBack)
    {
        if (isAllowKnockBack)
            knockBackFactor = 1f;
        else
            knockBackFactor = 0f;
    }

    //���˷���(�ⲿ����)
    public void VpetGethurt(float damage,Vector2 force)
    {
        if (isVpetInvincible || isVpetDead) return;

        float newHealth = _vpetCurrentHealth - damage;  //���˺������ֵ
        //�����˺�����ֵ����0
        if (newHealth <= 0)
        {
            _vpetCurrentHealth = 0;     //����ֵ�̶�Ϊ0
            isVpetDead = true;          //��������
            vpetAction.VpetDead();      //ִ��������Ϊ
        }

        //������������
        else
            _vpetCurrentHealth = newHealth;

        rb.velocity = Vector2.zero;
        rb.AddForce(force * knockBackFactor);

        AudioManager.Instance.PlaySound("getHurt");  //������Ч

        StartCoroutine(VpetHurtEffect());       //����Ч��
        StartCoroutine(VpetInvincibleSet());    //�����޵�״̬����
        ShowFigure(damage, true);
    }

    private float recoverMultiplier = 1f;       //�����ָ�����

    //�����ָ�����(�ⲿ����)
    public void VpetRecover(float recoverHealth)
    {
        float recover = Mathf.Ceil(recoverHealth * recoverMultiplier);  

        if (isVpetDead) return;

        float newHealth = _vpetCurrentHealth + recover;   //�ָ������������ֵ
        //���ָ�������ֵ�����������ֵ
        if (newHealth >= vpetHealth)
            _vpetCurrentHealth = vpetHealth;    //���ָ����������
        //���������ָ�����
        else
            _vpetCurrentHealth = newHealth;     //�ָ�����

        ShowFigure(recover, false);   //�ָ�����
    }

    [SerializeField] private GameObject figureTextPrefab;   //�����ı�Ԥ����
    private GameObject figureCanvas;                        //FigureCanvas���ڵ�

    //��ʾUI����
    private void ShowFigure(float num,bool isRed)
    {
        Transform parent = figureCanvas.transform;
        //����TMP�˺�����ʵ��
        GameObject figureText = Instantiate(figureTextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //��ȡTMP

        tmp.SetText(num.ToString());

        //�����ı���ɫ
        if (isRed)
            tmp.color = new Color(1, 0.4f, 0.4f, 1);
        else
            tmp.color = new Color(0.4f, 1, 0.5f, 1);

    }
}