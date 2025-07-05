using NUnit;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System;

public class Block : MonoBehaviour
{
    #region 변수

    [SerializeField]
    BlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    GameObject m_outline;
    GameObject m_effect;

    #region 움직이기 위한 변수
    bool m_moving = false;

    Vector3 m_start;
    Vector3 m_goal;

    GameObject m_goalTile;

    float m_moveDurationTime = 0.5f;
    float m_time = 0f;
    #endregion

    [Header("흔들림 설정")]
    float m_shakeIntensity = 0.02f;  // 흔들림 강도
    float m_shakeSpeed = 10f;       // 흔들림 속도
    bool m_shaking = false;

    #endregion 변수 끝

    #region Get함수
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

    #region Set함수
    public void SetMove(in GameObject _goalTile, in bool _emptyMoving)
    {
        // 타일 오브젝트가 맞는지 확인
        if (_goalTile.GetComponent<Tile>() == null)
        {
            return;
        }

        m_moving = true;
        m_start = transform.position;
        m_goal = _goalTile.transform.position;
        m_goalTile = _goalTile;

        // 빈자리 채우기 일 경우 빠르게
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

        // 특수 블록 여부
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

        #region 블록 타입에 따라 애니메이터 컨트롤러, 스프라이트가 바뀌게 함
        // 애니메이션
        RuntimeAnimatorController controller = null;
        string aniPath = "Animation/";

        // 외곽선
        SpriteRenderer sr = m_outline.GetComponent<SpriteRenderer>();
        string srPath = "BlockOutline/";

        if (m_isSpecial)
        {
            // 특수 블록 전용
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
            // 안 보이게 함
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

        // 외곽선
        Sprite outlineSprite = Resources.Load<Sprite>(srPath);
        sr.sprite = outlineSprite; // 스프라이트 지정
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
                    // 회전
                    spritePath += "FX2_cross_body_effect_0";
                    break;
                case BlockType.SUN:
                    // 원형 파앗...기운...이펙트
                    spritePath += "FX2_cross_All_Combine2_fx_01";
                    break;
                case BlockType.RANDOM:
                    // 겉에 링 생기고 회전(해당하는 블록들 반짝반짝)
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
        // 내 블록 타입으로 한 번 세팅
        SetBlockType(m_blockType);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 교환을 위한 이동
        ChangeMoving();

        if (m_shaking)
        {
            Shake();
        }

    }
    void SpecialComposition(BlockType _Type)
    {
        // 합성일 경우->이펙트+잠깐 멈추기
        // 아닐 경우->터질 때 이펙트+잠깐 멈추기

        // 애니메이션 잠깐 끄기
        GetComponent<Animator>().enabled = false;

        // 타입에 따라 스프라이트 설정
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

    // 교환을 위한 이동
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

            // 정렬
            transform.localPosition = Vector3.zero;

            m_time = 0f;
            m_moving = false;

            // 매니저에 이동 완료 신호 보냄
            MoveMgr.Instance.MoveComplete();
        }
    }

    public void RandomEffect(in bool _active)
    {
        // 흔들리고 터트림(매치 매니저에서 0.1초마다 하나씩 흔들리고, 거기서 완료 사인이 도착하기 까지 흔들림
        m_shaking = _active;
        if (!_active)
        {
            transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    void Shake()
    {
        // 랜덤한 방향으로 흔들림
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
