using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.Video.VideoPlayer;
using System;

using Random = UnityEngine.Random;

public class Tile : MonoBehaviour
{
    #region 변수

    [SerializeField]
    TileType m_tileType = TileType.MOVABLE;

    [SerializeField]
    // 블록이 비었을 때 어떤 타일에서 받아올지?
    GameObject m_upTile;

    #region 타일이 보유 한 자식 오브젝트
    [SerializeField]
    GameObject m_myBlock;
    GameObject m_myFrontObstacle;
    GameObject m_myBackObstacle;
    #endregion

    #region 자신의 위치(행렬)
    [SerializeField]
    Vector2Int m_matrix;
    #endregion

    // 생성 타일 여부
    [SerializeField]
    bool m_createTile = false;

    #endregion 변수 끝

    #region Get함수
    public GameObject GetMyBlock() {  return m_myBlock; }
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
        if (m_myFrontObstacle.GetComponent<Obstacle>().IsPropagationObstacle())
        {
            return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType();
        }
        if (m_myBackObstacle.GetComponent<Obstacle>().IsPropagationObstacle())
        {
            return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType();
        }

        return ObstacleType.NONE;
    }
    public ObstacleType GetMyFrontObstacleType() { return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public ObstacleType GetMyBackObstacleType() { return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    #endregion

    #region Set함수
    public void SetMyBlockType(in BlockType _BlockType)
    {
        m_myBlock.GetComponent<Block>().SetBlockType(_BlockType);
    }
    public void SetMyBlockActiveOutline()
    {
       StartCoroutine(m_myBlock.GetComponent<Block>().ActiveOutline());
    }
    public void SetMyBlockSetOutline(in bool _setting)
    {
        m_myBlock.GetComponent<Block>().SetOutline(_setting);
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

    public void EmptyMoving(GameObject _tile)
    {
        if (_tile != null)
        {
            // 둘 중 하나가 움직일 수 없으면
            if (_tile.GetComponent<Tile>().GetTileType() == TileType.IMMOVABLE || GetTileType() == TileType.IMMOVABLE)
            {
                return;
            }

            MoveMgr.Instance.SetClickedTileAndMoving(transform.gameObject, _tile);

            if (m_createTile)
            {
                StartCoroutine(CreateBlock());
            }
        }
    }

    public void Explode(ObstacleType _addObstacleType, BlockType _newBlockType = BlockType.NONE)
    {
        // StageMgr에 터트린 블록 타입 알려줌
        OnTileExplode?.Invoke(GetMyBlockType());

        // 전달 받은 장애물이 있는 경우
        if (_addObstacleType != ObstacleType.NONE)
        {
            // BackObstacle 인 경우
            if (_addObstacleType > ObstacleType.FRONT_END)
            {
                if (m_tileType == TileType.MOVABLE)
                {
                    m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_addObstacleType);
                }
            }
        }

        // 장애물이 있는 경우
        if (!m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
            // Obstacle이 가지고 있는 자식 장애물 쪽으로 이벤트를 연결해줌
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);

            if (fo.GetLevel() >= 0)
            {
                return;
            }
        }

        BlockType type = GetMyBlockType();

        // 특수 블록인 경우
        if (type >= BlockType.CROSS && type != BlockType.NULL)
        {
            // 이미 MatchMgr에서 타입을 저장했기 때문에 미리 타입을 바꿔 무한루프 예방
            SetMyBlockType(BlockType.NONE);
            MatchMgr.Instance.SpecialExplode();

            return;
        }

        SetMyBlockType(_newBlockType);
    }


}
