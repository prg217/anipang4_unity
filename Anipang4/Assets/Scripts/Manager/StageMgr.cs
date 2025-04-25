using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;
using System.Collections;

public class StageMgr : MonoBehaviour
{
    #region 싱글톤
    static StageMgr instance;

    public static StageMgr Instance
    {
        get
        {
            if (instance == null) instance = new StageMgr();
            return instance;
        }
    }
    #endregion


    void Awake()
    {
        #region 싱글톤
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion

        #region 타일 정보 등록
        // 보드의 자식인 라인들 불러오기
        for (int i = 0; i < m_Board.transform.childCount; i++)
        {
            Transform line = m_Board.transform.GetChild(i);

            // 라인의 자식인 타일들 등록
            for (int j = 0; j < line.transform.childCount; j++)
            {
                Transform tile = line.transform.GetChild(j);

                if (tile.CompareTag("Tile"))
                {
                    Vector2Int matrix = tile.GetComponent<Tile>().GetMatrix();
                    m_Tiles.Add(matrix, tile.gameObject);

                    // 최대 행렬 세팅
                    if (m_MaxMatrix.x < matrix.x)
                    {
                        m_MaxMatrix.x = matrix.x;
                    }
                    if (m_MaxMatrix.y < matrix.y)
                    {
                        m_MaxMatrix.y = matrix.y;
                    }
                }
            }
        }
        #endregion
    }

    #region 변수

    [SerializeField]
    GameObject m_Board;
    // 타일 최대 행렬
    Vector2Int m_MaxMatrix = new Vector2Int(0, 0);
    // 맵에 있는 타일들
    Dictionary<Vector2Int, GameObject> m_Tiles = new Dictionary<Vector2Int, GameObject>();

    #region Stage 설정 변수
    [SerializeField]
    int m_MaxBlockType = 5;
    [SerializeField]
    //int m_MaxMove = 20;
    #endregion

    #endregion 변수 끝

    #region Get함수
    public GameObject GetTile(in Vector2Int _Matrix)
    { 
        if (m_Tiles.ContainsKey(_Matrix))
        {
            return m_Tiles[_Matrix];
        }

        return null;
    }
    public int GetMaxBlockType() { return m_MaxBlockType; }
    public Vector2Int GetMaxMatrix() { return m_MaxMatrix; }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 움직여서 매치가 될 수 있나 확인
    public void CheckPossibleMatch()
    {
        // 움직일 수 있는 타일의 블록을 상하좌우로 움직인(임시로) 다음 매치가 되는지 체크
        // 매치가 하나라도 된다면 바로 return
        // 모두 매치가 안 된다면 움직일 수 없는 타일을 제외하고 블록 타입 개수를 수집한 뒤, 랜덤으로 배분하고 블록을 바꿈

        for (int i = 0; i <= m_MaxMatrix.y; i++)
        {
            for (int j = 0; j <= m_MaxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (MatchMgr.Instance.SimulationMatch(tile))
                {
                    return;
                }
            }
        }

        RandomPlacement();
    }

    // 무작위 배치
    void RandomPlacement()
    {
        List<GameObject> tiles = new List<GameObject>();
        List<BlockType> blockTypes = new List<BlockType>();
        List<BlockType> saveBlockTypes = new List<BlockType>();

        for (int i = 0; i <= m_MaxMatrix.y; i++)
        {
            for (int j = 0; j <= m_MaxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);
                TileType tileType = tile.GetComponent<Tile>().GetTileType(); 

                // 타일 타입이 움직일 수 있는 경우에만 저장
                if (tileType == TileType.MOVABLE)
                {
                    // 움직일 수 있는 타일 저장
                    tiles.Add(tile);

                    // 블록 타입 저장
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();
                    blockTypes.Add(blockType);
                }
            }
        }
        saveBlockTypes = new List<BlockType>(blockTypes);

        // 무작위 배치 실행
        foreach (GameObject tile in tiles)
        {
            int random = Random.Range(0, blockTypes.Count);
            tile.GetComponent<Tile>().SetMyBlockType(blockTypes[random]);
            blockTypes.RemoveAt(random);

            // 만약 바로 매치가 된다면 무작위 배치 재실행
            if (MatchMgr.Instance.CheckMatch(tile, false))
            {
                blockTypes = new List<BlockType>(saveBlockTypes);
                RandomPlacement();
                return;
            }
        }

        // 다시 빈 타일 없나 확인
        CheckPossibleMatch();
    }

    // 클리어 조건 확인
    public void CheckStage()
    {

    }
}
