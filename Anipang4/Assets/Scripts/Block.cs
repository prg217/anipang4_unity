using NUnit;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Block : MonoBehaviour
{
    #region 변수

    enum BlockType
    {
        NONE,

        // 기본 블록
        MOUSE,
        DOG,
        RABBIT,
        MONGKEY,
        CHICK,
        CAT,

        // 특수 블록
        CROSS, // 십자 모양으로 블록 제거
        SUN, // 폭탄 블록
        RANDOM, // 무지개 블록, 겹친 블록들을 제거
        COSMIC, // 모두 다 제거
        MOON, // 클리어 조건 블록을 추적해서 제거
    }

    [SerializeField]
    BlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    #region 움직이기 위한 변수
    bool m_moving = false;

    Vector3 m_start;
    Vector3 m_goal;

    GameObject m_goalTile;

    float m_moveDurationTime = 0.5f;
    float m_time = 0f;
    #endregion

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
    #endregion

    #region Set함수
    public void SetMove(GameObject _goalTile)
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
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 교환을 위한 이동
        ChangeMoving();
    }

    void SetBlockType(BlockType _type)
    {
        m_blockType = _type;

        // 특수 블록 여부
        if ((int)m_blockType >= (int)BlockType.CROSS)
        {
            m_isSpecial = true;
        }
        else
        {
            m_isSpecial = false;
        }

        #region 블록 타입에 따라 애니메이터 컨트롤러가 바뀌게 함
        RuntimeAnimatorController controller = null;
        string path = "Animation/";

        if (m_isSpecial)
        {
            // 특수 블록 전용
            switch (m_blockType)
            {
                case BlockType.CROSS:
                    path += "cross";
                    break;
                case BlockType.SUN:
                    path += "sun";
                    break;
                case BlockType.RANDOM:
                    path += "random";
                    break;
                case BlockType.COSMIC:
                    path += "cosmic";
                    break;
                case BlockType.MOON:
                    path += "moon";
                    break;
                default:
                    break;
            }
        }
        else
        {
            path += "block";
            int number = (int)m_blockType;
            path += number.ToString();
            controller = Resources.Load<RuntimeAnimatorController>(path);
        }
        path += "_aniCtrl.controller";
        GetComponent<Animator>().runtimeAnimatorController = controller;
        #endregion
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

            // 위치 이동이 끝난 후에 부모 변경
            transform.SetParent(m_goalTile.transform);
            transform.localPosition = Vector3.zero; // 새 부모 기준 정렬

            m_moving = false;

            // 매니저에 이동 완료 신호 보냄
            MoveMgr.Instance.MoveComplete();
        }
    }
}
