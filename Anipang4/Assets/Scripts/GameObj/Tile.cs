using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.Video.VideoPlayer;
using System;

using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Drawing;

public class Tile : MonoBehaviour
{
    #region 변수

    [SerializeField]
    ETileType m_tileType = ETileType.MOVABLE;

    [SerializeField]
    // 블록이 비었을 때 어떤 타일에서 받아올지
    GameObject m_upTile;

    #region 타일이 보유 한 자식 오브젝트
    [Header("타일이 보유 한 자식 오브젝트")]
    [SerializeField]
    GameObject m_myBlock;
    GameObject m_myFrontObstacle;
    GameObject m_myBackObstacle;
    #endregion

    #region 자신의 위치(행렬)
    [Header("자신의 위치(행렬)")]
    [SerializeField]
    Vector2Int m_matrix;
    #endregion

    // 생성 타일 여부
    [Header("생성 타일 여부")]
    [SerializeField]
    bool m_createTile = false;

    // 타일 상태
    STileState m_tileState;

    GameObject m_myExplodeEffect;

    #endregion 변수 끝

    #region Get함수
    // -1 : 블록 없음, 0 : 움직일 수 없음, 1 : 움직일 수 있음
    public ETileType GetTileType() { return m_tileType; }
    public Vector2Int GetMatrix() { return m_matrix; }
    public EBlockType GetMyBlockType() { return m_myBlock.GetComponent<Block>().GetBlockType(); }
    public bool IsBlockEmpty()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == EBlockType.NONE) { return true; }
        return false;
    }
    public bool IsEmptyCreateTile()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == EBlockType.NONE && m_createTile) { return true; }
        return false;
    }
    // 전파되는 장애물
    public EObstacleType GetPropagationObstacle()
    {
        if (m_myFrontObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType();
        }
        if (m_myBackObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType();
        }

        return EObstacleType.NONE;
    }
    public bool GetFrontObstacleEmpty() { return m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty(); }
    public EObstacleType GetMyFrontObstacleType() { return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public EObstacleType GetMyBackObstacleType() { return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public bool GetIsTargeted() { return m_tileState.isTargeted; }
    #endregion

    #region Set함수
    void SetTileType(in ETileType _tileType) { m_tileType = _tileType; }
    public void SetMyBlockType(in EBlockType _BlockType)
    {
        m_myBlock.GetComponent<Block>().SetBlockType(_BlockType);
    }
    public void SetBlockMove(in GameObject _goalTile, in bool _emptyMoving)
    {
        m_myBlock.GetComponent<Block>().SetMove(_goalTile, _emptyMoving);
    }
    public void SetMyBlockActiveOutline()
    {
       StartCoroutine(m_myBlock.GetComponent<Block>().ActiveOutline());
    }
    public void SetMyBlockSetOutline(in bool _setting)
    {
        m_myBlock.GetComponent<Block>().SetOutline(_setting);
    }
    public void SetIsTargeted(in bool _setting)
    {
        m_tileState.isTargeted = _setting;

        if (_setting)
        {
            SetTileType(ETileType.IMMOVABLE);
        }
        else
        {
            SetTileType(ETileType.MOVABLE);
        }
    }
    public void SetRandomComplete(in bool _setting)
    {
        m_tileState.randomComplete = _setting;
    }
    public void SetRandomExplode(in bool _setting)
    {
        m_tileState.randomExplode = _setting;
    }
    public void SetRandomExecute(in bool _setting)
    {
        m_tileState.randomExecute = _setting;
    }
    #endregion

    #region 이벤트
    public event Action<EBlockType> OnTileExplode;

    void HandleSetTileTypeExecution(ETileType _type)
    {
        m_tileType = _type;
    }
    #endregion

    void Awake()
    {
        // 구조체 안 데이터 초기화
        m_tileState.isTargeted = false;
        m_tileState.isExplodeWaiting = false;
        m_tileState.randomComplete = false;
        m_tileState.randomExplode = false;
        m_tileState.randomExecute = false;

        Refresh();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 블록이 비었는지 체크 후 MoveMgr에 신호?
    }

    // 자신의 정보 새로고침
    public void Refresh()
    {
        #region 자식 오브젝트를 변수에 넣기
        Transform child = transform.Find("Block");
        if (child != null)
        {
            m_myBlock = child.gameObject;
        }
        else
        {
            // Block이 없으면 null 타일이므로 초기 설정을 하지 않는다.
            return;
        }
        child = transform.Find("Front_Obstacle");
        if (child != null)
        {
            m_myFrontObstacle = child.gameObject;
        }
        child = transform.Find("Back_Obstacle");
        if (child != null)
        {
            m_myBackObstacle = child.gameObject;
        }
        child = transform.Find("ExplodeEffect");
        if (child != null)
        {
            m_myExplodeEffect = child.gameObject;
        }
        #endregion

        #region 타일 안의 블록이 움직일 수 있는 상태인가
        if (CheckMove())
        {
            m_tileType = ETileType.MOVABLE;
        }
        else
        {
            m_tileType = ETileType.IMMOVABLE;
        }
        #endregion
    }

    // 블록이 이동할 수 있는지에 대해 반환
    bool CheckMove()
    {
        if (m_myBlock == null)
        {
            return false;
        }

        // 움직일 수 없는 장애물이 있나 판단
        bool isEmpty = m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty();
        if (!isEmpty)
        {
            return false;
        }

        // 블록이 NULL인 경우
        EBlockType type = m_myBlock.GetComponent<Block>().GetBlockType();
        if (type == EBlockType.NULL)
        {
            return false;
        }

        return true;
    }

    // 생성 타일일 때 블록을 랜덤 생성
    public void CreateBlock()
    {
        if (m_myBlock == null || !m_createTile)
        {
            return;
        }

        if (!IsBlockEmpty())
        {
            return;
        }

        // StageMgr에서 설정된 블록 값으로 랜덤한 값
        int maxRandom = StageMgr.Instance.GetMaxBlockType();
        int random = Random.Range(0, maxRandom);
        m_myBlock.GetComponent<Block>().SetBlockType((EBlockType)random);
        // 위에서 내려오는 연출
        m_myBlock.GetComponent<Block>().CreatTileBlockMove(transform.gameObject);
    }

    public void BlockTeleport(in GameObject _goalTile)
    {
        m_myBlock.GetComponent<Block>().BlockTeleport(_goalTile);
    }

    public void EmptyMoving(in Vector2Int _point)
    {
        MoveMgr.Instance.EmptyMoving(transform.gameObject, _point);
    }

    IEnumerator ExplodeEffect()
    {
        m_myExplodeEffect.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        m_myExplodeEffect.SetActive(false);
    }

    public void StartExplodeEffect()
    {
        StartCoroutine(ExplodeEffect());
    }

    public void Explode(in EObstacleType _contagiousObstacleType, in EBlockType _newBlockType = EBlockType.NONE)
    {
        if (m_tileState.isExplodeWaiting)
        {
            return;
        }

        m_tileState.isExplodeWaiting = true;

        // StageMgr에 터트린 블록 타입 알려줌
        OnTileExplode?.Invoke(GetMyBlockType());

        // 장애물이 있는 경우
        if (!m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();

            // 이전에 구독한 이벤트 해제
            fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
            // Obstacle이 가지고 있는 자식 장애물 쪽으로 이벤트를 연결해줌
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);

            if (fo.GetLevel() >= 0)
            {
                if (m_tileState.randomExplode)
                {
                    SetRandomExplode(false);
                    MatchMgr.Instance.RandomExplodeComplete();
                }
                m_tileState.isExplodeWaiting = false;
                return;
            }
        }

        // 전달 받은 장애물이 있는 경우
        if (_contagiousObstacleType != EObstacleType.NONE)
        {
            // BackObstacle 인 경우
            if (_contagiousObstacleType > EObstacleType.FRONT_END)
            {
                if (m_tileType == ETileType.MOVABLE)
                {
                    m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                }
            }
        }

        EBlockType type = GetMyBlockType();

        // 특수 블록인 경우
        if (type >= EBlockType.CROSS && type != EBlockType.NULL)
        {
            // 딜레이 후 터트림
            StartCoroutine(SpecialExplode());

            if (m_tileState.randomExplode)
            {
                SetRandomExplode(false);
                MatchMgr.Instance.RandomExplodeComplete();
            }
            return;
        }

        if (m_tileState.randomExplode)
        {
            SetRandomExplode(false);
            MatchMgr.Instance.RandomExplodeComplete();
        }

        m_tileState.isExplodeWaiting = false;
        SetMyBlockType(_newBlockType);
    }

    public void ChasingMoonExplode(in EObstacleType _contagiousObstacleType, in EBlockType _explodeType = EBlockType.NONE)
    {
        // StageMgr에 터트린 블록 타입 알려줌
        OnTileExplode?.Invoke(GetMyBlockType());

        // 장애물이 있는 경우
        if (!GetFrontObstacleEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();

            // 이전에 구독한 이벤트 해제
            fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
            // Obstacle이 가지고 있는 자식 장애물 쪽으로 이벤트를 연결해줌
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);
        }
        else
        {
            // 전달 받은 장애물이 있는 경우
            if (_contagiousObstacleType != EObstacleType.NONE)
            {
                // BackObstacle 인 경우
                if (_contagiousObstacleType > EObstacleType.FRONT_END)
                {
                    if (m_tileType == ETileType.MOVABLE)
                    {
                        m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                    }
                }
            }
        }

        EBlockType type = _explodeType;

        // 특수 블록인 경우
        if (type >= EBlockType.CROSS && type != EBlockType.NULL)
        {
            MatchMgr.Instance.SpecialExplode(transform.gameObject, _explodeType);

            return;
        }
    }

    public void RandomEffect(in bool _active)
    {
        m_myBlock.GetComponent<Block>().RandomEffect(_active);
    }

    public IEnumerator SpecialExplode()
    {
        // 블록 타입에 따라 이펙트 실행
        m_myBlock.GetComponent<Block>().SetEffect(true);
        SetRandomComplete(false);

        switch (GetMyBlockType())
        {
            case EBlockType.RANDOM:
            case EBlockType.DOUBLE_RANDOM:
            case EBlockType.RANDOM_CROSS:
            case EBlockType.RANDOM_SUN:
            case EBlockType.RANDOM_MOON:
                if (!m_tileState.randomExecute)
                {
                    MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                }

                yield return new WaitUntil(() => m_tileState.randomComplete);
                m_myBlock.GetComponent<Block>().SetEffect(false);
                SetMyBlockType(EBlockType.NONE);
                SetRandomExecute(false);
                break;
            default:
                SetTileType(ETileType.IMMOVABLE);
                yield return new WaitForSeconds(0.3f);
                m_myBlock.GetComponent<Block>().SetEffect(false);

                MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                SetTileType(ETileType.MOVABLE);
                break;
        }

        m_tileState.isExplodeWaiting = false;

        // 랜덤으로 인해 실행 중이라면 랜덤 쪽에서 StartCheckEmpty를 함
        if (!m_tileState.randomExplode && !m_tileState.randomComplete)
        {
            MoveMgr.Instance.StartCheckEmpty();
        }
        if (m_tileState.randomExplode)
        {
            SetRandomExplode(false);

            MatchMgr.Instance.RandomExplodeComplete();
        }
    }

    void OnDestroy()
    {
        Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
        // 구독한 이벤트 해제
        fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
    }
}
