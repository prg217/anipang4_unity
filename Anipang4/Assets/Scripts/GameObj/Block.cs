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
    BlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    GameObject m_outline;
    GameObject m_effect;

    #region �����̱� ���� ����
    bool m_moving = false;

    Vector3 m_start;
    Vector3 m_goal;

    GameObject m_goalTile;

    float m_moveDurationTime = 0.5f;
    float m_time = 0f;
    #endregion

    [Header("��鸲 ����")]
    float m_shakeIntensity = 0.02f;  // ��鸲 ����
    float m_shakeSpeed = 10f;       // ��鸲 �ӵ�
    bool m_shaking = false;

    #endregion ���� ��

    #region Get�Լ�
    public bool GetIsEmpty()
    {
        if (m_blockType == BlockType.NONE)
        {
            return true;
        }
        return false;
    }
    public BlockType GetBlockType() { return m_blockType; }
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
        m_start = transform.position;
        m_goal = _goalTile.transform.position;
        m_goalTile = _goalTile;

        // ���ڸ� ä��� �� ��� ������
        if (_emptyMoving)
        {
            m_moveDurationTime = 0.3f;
        }
        else
        {
            m_moveDurationTime = 0.5f;
        }

        transform.SetParent(m_goalTile.transform);
    }

    public void SetBlockType(in BlockType _Type)
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Animator>().enabled = true;
        m_blockType = _Type;

        // Ư�� ��� ����
        if ((int)m_blockType >= (int)BlockType.DOUBLE_CROSS)
        {
            SpecialComposition(_Type);
            return;
        }
        else if ((int)m_blockType >= (int)BlockType.CROSS)
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
                case BlockType.CROSS:
                    aniPath += "cross";
                    break;
                case BlockType.SUN:
                    aniPath += "sun";
                    break;
                case BlockType.RANDOM:
                    aniPath += "random";
                    break;
                case BlockType.COSMIC:
                    aniPath += "cosmic";
                    break;
                case BlockType.MOON:
                    aniPath += "moon";
                    break;
                default:
                    break;
            }
        }
        else
        {
            // �� ���̰� ��
            if (m_blockType == BlockType.NONE)
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
                case BlockType.CROSS:
                    // ȸ��
                    spritePath += "FX2_cross_body_effect_0";
                    break;
                case BlockType.SUN:
                    // ���� �ľ�...���...����Ʈ
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case BlockType.RANDOM:
                    // �ѿ� �� ����� ȸ��(�ش��ϴ� ��ϵ� ��¦��¦)
                    spritePath += "FX2_cosmic_glow";
                    break;
                case BlockType.COSMIC:
                    spritePath += "FX2_cosmic_glow";
                    break;
                case BlockType.MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case BlockType.DOUBLE_CROSS:
                    spritePath += "FX2_cross_body_effect_0";
                    break;
                case BlockType.CROSS_SUN:
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case BlockType.CROSS_MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case BlockType.DOUBLE_SUN:
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case BlockType.SUN_MOON:
                    spritePath += "FX2_moon_effect";
                    break;
                case BlockType.DOUBLE_MOON:
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
    void SpecialComposition(BlockType _Type)
    {
        // �ռ��� ���->����Ʈ+��� ���߱�
        // �ƴ� ���->���� �� ����Ʈ+��� ���߱�

        // �ִϸ��̼� ��� ����
        GetComponent<Animator>().enabled = false;

        // Ÿ�Կ� ���� ��������Ʈ ����
        string spritePath = "";

        switch (m_blockType)
        {
            case BlockType.DOUBLE_CROSS:
                spritePath += "Block/FX2_cross_All_Combine1";
                break;
            case BlockType.CROSS_SUN:
                spritePath += "Block/FX2_cross_All_Combine2";
                break;
            case BlockType.CROSS_MOON:
                spritePath += "Moon/moonCross_01";
                break;
            case BlockType.DOUBLE_SUN:
                spritePath += "Block/sun_01";
                break;
            case BlockType.SUN_MOON:
                spritePath += "Moon/moonSun_01";
                break;
            case BlockType.DOUBLE_MOON:
                spritePath += "Moon/FX2_moon_body_double";
                break;
            case BlockType.DOUBLE_RANDOM:
            case BlockType.RANDOM_CROSS:
            case BlockType.RANDOM_SUN:
            case BlockType.RANDOM_MOON:
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
            if (m_time < m_moveDurationTime)
            {
                m_time += Time.deltaTime;
                float normalizedTime = m_time / m_moveDurationTime;
                transform.position = Vector3.Lerp(m_start, m_goal, normalizedTime);

                return;
            }

            // ����
            transform.localPosition = Vector3.zero;

            m_time = 0f;
            m_moving = false;

            // �Ŵ����� �̵� �Ϸ� ��ȣ ����
            MoveMgr.Instance.MoveComplete();
        }
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
        transform.SetParent(_goalTile.transform);
    }
}
