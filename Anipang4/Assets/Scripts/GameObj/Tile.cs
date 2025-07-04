using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.Video.VideoPlayer;
using System;

using Random = UnityEngine.Random;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    #region 변수

    [SerializeField]
    TileType m_tileType = TileType.MOVABLE;

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

    // ====== 타일 상태 ======(구조체로 만들어야 하나?)
    // 타겟이 되었는지
    bool m_isTargeted = false;
    // 터지기 대기 중인가(대기 중이라면 또 터짐 신호를 받으면 안 됨)
    bool m_isExplodeWaiting = false;
    // 랜덤이 끝날 때까지 대기하기 위한 용도
    bool m_randomComplete = false;
    // 랜덤으로 인해 터지는 상태인가?
    [SerializeField]
    bool m_randomExplode = false;
    // =======================

    GameObject m_myExplodeEffect;

    #endregion 변수 끝

    #region Get함수
    //public GameObject GetMyBlock() {  return m_myBlock; }
    // -1 : 블록 없음, 0 : 움직일 수 없음, 1 : 움직일 수 있음
    public TileType GetTileType() { return m_tileType; }
    public Vector2Int GetMatrix() { return m_matrix; }
    public BlockType GetMyBlockType() { return m_myBlock.GetComponent<Block>().GetBlockType(); }
    public bool IsBlockEmpty()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == BlockType.NONE) { return true; }
        return false;
    }
    // 전파되는 장애물
    public ObstacleType GetPropagationObstacle()
    {
        if (m_myFrontObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType();
        }
        if (m_myBackObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType();
        }

        return ObstacleType.NONE;
    }
    public bool GetFrontObstacleEmpty() { return m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty(); }
    public ObstacleType GetMyFrontObstacleType() { return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public ObstacleType GetMyBackObstacleType() { return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public bool GetIsTargeted() { return m_isTargeted; }
    #endregion

    #region Set함수
    public void SetMyBlockType(in BlockType _BlockType)
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
        m_isTargeted = _setting;
    }
    public void SetRandomComplete(in bool _setting)
    {
        m_randomComplete = _setting;
    }
    public void SetRandomExplode(in bool _setting)
    {
        m_randomExplode = _setting;
    }
    #endregion

    #region 이벤트
    public event Action<BlockType> OnTileExplode;

    void HandleSetTileTypeExecution(TileType _type)
    {
        m_tileType = _type;
    }
    #endregion

    void Awake()
    {
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
            m_tileType = TileType.MOVABLE;
        }
        else
        {
            m_tileType = TileType.IMMOVABLE;
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
        BlockType type = m_myBlock.GetComponent<Block>().GetBlockType();
        if (type == BlockType.NULL)
        {
            return false;
        }

        return true;
    }

    // 생성 타일일 때 블록을 랜덤 생성
    IEnumerator CreateBlock()
    {
        if (m_myBlock == null || !m_createTile)
        {
            yield break;
        }

        if (!IsBlockEmpty())
        {
            yield break;
        }

        yield return new WaitForSeconds(0.3f);
        // StageMgr에서 설정된 블록 값으로 랜덤한 값
        int maxRandom = StageMgr.Instance.GetMaxBlockType();
        int random = Random.Range(0, maxRandom);
        m_myBlock.GetComponent<Block>().SetBlockType((BlockType)random);
    }

    public void EmptyMoving(in List<Vector2Int> _points)
    {
        //if (_tile != null)
        //{
        //    // 둘 중 하나가 움직일 수 없으면
        //    if (_tile.GetComponent<Tile>().GetTileType() == TileType.IMMOVABLE || GetTileType() == TileType.IMMOVABLE)
        //    {
        //        return;
        //    }
        //
        //    MoveMgr.Instance.SetClickedTileAndMoving(transform.gameObject, _tile);
        //
        //    if (m_createTile)
        //    {
        //        StartCoroutine(CreateBlock());
        //    }
        //}

        foreach (Vector2Int point in _points)
        {
            

            //if ()
        }

        if (m_createTile)
        {
            StartCoroutine(CreateBlock());
        }
    }

    IEnumerator ExplodeEffect()
    {
        m_myExplodeEffect.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        m_myExplodeEffect.SetActive(false);
    }

    public void Explode(in ObstacleType _contagiousObstacleType, in BlockType _newBlockType = BlockType.NONE)
    {
        if (m_isExplodeWaiting)
        {
            return;
        }

        m_isExplodeWaiting = true;

        // StageMgr에 터트린 블록 타입 알려줌
        OnTileExplode?.Invoke(GetMyBlockType());

        StartCoroutine(ExplodeEffect());

        // 장애물이 있는 경우
        if (!m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
            // Obstacle이 가지고 있는 자식 장애물 쪽으로 이벤트를 연결해줌
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);

            if (fo.GetLevel() >= 0)
            {
                if (m_randomExplode)
                {
                    SetRandomExplode(false);

                    MatchMgr.Instance.RandomExplodeComplete();
                }
                return;
            }
        }

        // 전달 받은 장애물이 있는 경우
        if (_contagiousObstacleType != ObstacleType.NONE)
        {
            // BackObstacle 인 경우
            if (_contagiousObstacleType > ObstacleType.FRONT_END)
            {
                if (m_tileType == TileType.MOVABLE)
                {
                    m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                }
            }
        }

        BlockType type = GetMyBlockType();

        // 특수 블록인 경우
        if (type >= BlockType.CROSS && type != BlockType.NULL)
        {
            // 딜레이 후 터트림
            StartCoroutine(SpecialExplode());
            return;
        }

        m_isExplodeWaiting = false;
        SetMyBlockType(_newBlockType);
    }

    public void ChasingMoonExplode(in ObstacleType _contagiousObstacleType, in BlockType _explodeType = BlockType.NONE)
    {
        // StageMgr에 터트린 블록 타입 알려줌
        OnTileExplode?.Invoke(GetMyBlockType());

        // 장애물이 있는 경우
        if (!GetFrontObstacleEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
            // Obstacle이 가지고 있는 자식 장애물 쪽으로 이벤트를 연결해줌
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);
        }
        else
        {
            // 전달 받은 장애물이 있는 경우
            if (_contagiousObstacleType != ObstacleType.NONE)
            {
                // BackObstacle 인 경우
                if (_contagiousObstacleType > ObstacleType.FRONT_END)
                {
                    if (m_tileType == TileType.MOVABLE)
                    {
                        m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                    }
                }
            }
        }

        BlockType type = _explodeType;

        // 특수 블록인 경우
        if (type >= BlockType.CROSS && type != BlockType.NULL)
        {
            MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());

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
            case BlockType.RANDOM:
                yield return new WaitUntil(() => m_randomComplete);
                m_myBlock.GetComponent<Block>().SetEffect(false);
                SetMyBlockType(BlockType.NONE);
                break;
            case BlockType.DOUBLE_RANDOM:
            case BlockType.RANDOM_CROSS:
            case BlockType.RANDOM_SUN:
            case BlockType.RANDOM_MOON:
                MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                yield return new WaitUntil(() => m_randomComplete);

                m_myBlock.GetComponent<Block>().SetEffect(false);
                SetMyBlockType(BlockType.NONE);
                break;
            default:
                yield return new WaitForSeconds(0.3f);
                m_myBlock.GetComponent<Block>().SetEffect(false);

                MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                break;
        }

        m_isExplodeWaiting = false;

        // 랜덤으로 인해 실행 중이라면 랜덤 쪽에서 StartCheckEmpty를 함
        if (!m_randomExplode && !m_randomComplete)
        {
            Debug.Log(GetMatrix());
            MoveMgr.Instance.StartCheckEmpty();
        }
        if (m_randomExplode)
        {
            SetRandomExplode(false);

            MatchMgr.Instance.RandomExplodeComplete();
        }
    }
}
