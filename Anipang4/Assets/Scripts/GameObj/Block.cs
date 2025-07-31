using NUnit;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System;

public class Block : MonoBehaviour
{
    #region ����

    [SerializeField]
    EBlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    GameObject m_outline;
    GameObject m_effect;

    #region �����̱� ���� ����
    bool m_moving = false;

    Vector3 m_goal;

    GameObject m_goalTile;

    float m_moveSpeed = 1.5f;
    float m_changeSpeed = 1.5f;
    float m_emptyMovingSpeed = 2f;
    #endregion

    [Header("��鸲 ����")]
    float m_shakeIntensity = 0.02f;  // ��鸲 ����
    bool m_shaking = false;

    #endregion ���� ��

    #region Get�Լ�
    public bool GetIsEmpty()
    {
        if (m_blockType == EBlockType.NONE)
        {
            return true;
        }
        return false;
    }
    public EBlockType GetBlockType() { return m_blockType; }
    #endregion

    #region Set�Լ�
    public void SetMove(in GameObject _goalTile, in bool _emptyMoving)
    {
        // Ÿ�� ������Ʈ�� �´��� Ȯ��
        if (_goalTile.GetComponent<Tile>() == null)
        {
            return;
        }

        m_moving = true;
        m_goal = _goalTile.transform.position;
        m_goalTile = _goalTile;

        // ���ڸ� ä��� �� ��� ������
        if (_emptyMoving)
        {
            m_moveSpeed = m_emptyMovingSpeed;
        }
        else
        {
            m_moveSpeed = m_changeSpeed;
        }

        transform.SetParent(m_goalTile.transform);
    }

    public void SetBlockType(in EBlockType _Type)
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Animator>().enabled = true;
        m_blockType = _Type;

        // Ư�� ��� ����
        if ((int)m_blockType >= (int)EBlockType.DOUBLE_CROSS)
        {
            SpecialComposition(_Type);
            return;
        }
        else if ((int)m_blockType >= (int)EBlockType.CROSS)
        {
            m_isSpecial = true;
        }
        else
        {
            m_isSpecial = false;
        }

        #region ��� Ÿ�Կ� ���� �ִϸ����� ��Ʈ�ѷ�, ��������Ʈ�� �ٲ�� ��
        // �ִϸ��̼�
        RuntimeAnimatorController controller = null;
        string aniPath = "Animation/";

        // �ܰ���
        SpriteRenderer sr = m_outline.GetComponent<SpriteRenderer>();
        string srPath = "BlockOutline/";

        if (m_isSpecial)
        {
            // Ư�� ��� ����
            switch (m_blockType)
            {
                case EBlockType.CROSS:
                    aniPath += "cross";
                    break;
                case EBlockType.SUN:
                    aniPath += "sun";
                    break;
                case EBlockType.RANDOM:
                    aniPath += "random";
                    break;
                case EBlockType.COSMIC:
                    aniPath += "cosmic";
                    break;
                case EBlockType.MOON:
                    aniPath += "moon";
                    break;
                default:
                    break;
            }
        }
        else
        {
            // �� ���̰� ��
            if (m_blockType == EBlockType.NONE)
            {
                GetComponent<Renderer>().enabled = false;
            }

            int number = (int)m_blockType + 1;

            aniPath += "block";
            aniPath += number.ToString();

            srPath += "FX1_block_0";
            srPath += number.ToString();
        }

        aniPath += "_aniCtrl";
        controller = Resources.Load<RuntimeAnimatorController>(aniPath);
        GetComponent<Animator>().runtimeAnimatorController = controller;

        // �ܰ���
        Sprite outlineSprite = Resources.Load<Sprite>(srPath);
        sr.sprite = outlineSprite; // ��������Ʈ ����
        #endregion
    }

    public void SetOutline(in bool _setting)
    {
        m_outline.SetActive(_setting);
    }
    public void SetEffect(in bool _active)
    {
        m_effect.SetActive(_active);

        if (_active)
        {
            string spritePath = "Effect/";

            switch (m_blockType)
            {
                case EBlockType.CROSS:
                    // ȸ��
                    spritePath += "FX2_cross_body_effect_0";
                    break;
                case EBlockType.SUN:
                    // ���� �ľ�...���...����Ʈ
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case EBlockType.RANDOM:
                    // �ѿ� �� ����� ȸ��(�ش��ϴ� ��ϵ� ��¦��¦)
                    spritePath += "FX2_cosmic_glow";
                    break;
                case EBlockType.COSMIC:
                    spritePath += "FX2_cosmic_glow";
                    break;
                case EBlockType.MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case EBlockType.DOUBLE_CROSS:
                    spritePath += "FX2_cross_body_effect_0";
                    break;
                case EBlockType.CROSS_SUN:
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case EBlockType.CROSS_MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case EBlockType.DOUBLE_SUN:
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case EBlockType.SUN_MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case EBlockType.DOUBLE_MOON:
                    spritePath += "FX2_moon_double_effect";
                    break;
                default:
                    break;
            }

            Sprite newSprite = Resources.Load<Sprite>(spritePath);
            if (newSprite != null)
            {
                m_effect.GetComponent<SpriteRenderer>().sprite = newSprite;
            }

            
        }
    }
    #endregion

    private void Awake()
    {
        m_outline = transform.Find("Outline").GameObject();
        m_outline.SetActive(false);
        m_effect = transform.Find("Effect").GameObject();
        m_effect.SetActive(false);
        // �� ��� Ÿ������ �� �� ����
        SetBlockType(m_blockType);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ��ȯ�� ���� �̵�
        ChangeMoving();

        if (m_shaking)
        {
            Shake();
        }

    }
    void SpecialComposition(EBlockType _Type)
    {
        // �ռ��� ���->����Ʈ+��� ���߱�
        // �ƴ� ���->���� �� ����Ʈ+��� ���߱�

        // �ִϸ��̼� ��� ����
        GetComponent<Animator>().enabled = false;

        // Ÿ�Կ� ���� ��������Ʈ ����
        string spritePath = "";

        switch (m_blockType)
        {
            case EBlockType.DOUBLE_CROSS:
                spritePath += "Block/FX2_cross_All_Combine1";
                break;
            case EBlockType.CROSS_SUN:
                spritePath += "Block/FX2_cross_All_Combine2";
                break;
            case EBlockType.CROSS_MOON:
                spritePath += "Moon/moonCross_01";
                break;
            case EBlockType.DOUBLE_SUN:
                spritePath += "Block/sun_01";
                break;
            case EBlockType.SUN_MOON:
                spritePath += "Moon/moonSun_01";
                break;
            case EBlockType.DOUBLE_MOON:
                spritePath += "Moon/FX2_moon_body_double";
                break;
            case EBlockType.DOUBLE_RANDOM:
            case EBlockType.RANDOM_CROSS:
            case EBlockType.RANDOM_SUN:
            case EBlockType.RANDOM_MOON:
                spritePath += "Block/FX2_random_02";
                break;
            default:
                break;
        }

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }

    public IEnumerator ActiveOutline()
    {
        m_outline.SetActive(true);
        yield return new WaitForSeconds(2f);
        m_outline.SetActive(false);
    }

    // ��ȯ�� ���� �̵�
    void ChangeMoving()
    {
        if (m_moving)
        {
            // ��ǥ �������� ������ �ӵ��� �̵�
            transform.position = Vector3.MoveTowards(transform.position, m_goal, m_moveSpeed * Time.deltaTime);

            // ��ǥ ������ �����ߴ��� Ȯ��
            if (Vector3.Distance(transform.position, m_goal) < 0.01f)
            {
                // ����
                transform.localPosition = Vector3.zero;
                m_moving = false;
                // �Ŵ����� �̵� �Ϸ� ��ȣ ����
                MoveMgr.Instance.MoveComplete();
            }
        }
    }

    public void CreatTileBlockMove(in GameObject _myTile)
    {
        // ���� �Ǿ��� ��� �� �� ������ �������� ȿ��

        // Ÿ�� ������Ʈ�� �´��� Ȯ��
        if (_myTile.GetComponent<Tile>() == null)
        {
            return;
        }

        transform.position = transform.position + new Vector3(0, _myTile.transform.localScale.y, 0);

        m_moving = true;
        m_goal = _myTile.transform.position;
        m_goalTile = _myTile;

        // ���ڸ� ä��� �� ��� ������
        m_moveSpeed = m_emptyMovingSpeed;
    }

    public void RandomEffect(in bool _active)
    {
        // ��鸮�� ��Ʈ��(��ġ �Ŵ������� 0.1�ʸ��� �ϳ��� ��鸮��, �ű⼭ �Ϸ� ������ �����ϱ� ���� ��鸲
        m_shaking = _active;
        if (!_active)
        {
            transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    void Shake()
    {
        // ������ �������� ��鸲
        Vector3 shakeOffset =
        new Vector3(
            UnityEngine.Random.Range(-m_shakeIntensity, m_shakeIntensity),
            UnityEngine.Random.Range(-m_shakeIntensity, m_shakeIntensity),
            0f
        );

        transform.localPosition = shakeOffset;
    }

    public void BlockTeleport(in GameObject _goalTile)
    {
        m_moving = false;
        transform.SetParent(_goalTile.transform);
        transform.position = _goalTile.transform.position;
    }
}
