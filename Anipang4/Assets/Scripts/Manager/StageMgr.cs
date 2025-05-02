using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;
using System.Collections;
using Unity.Burst.CompilerServices;

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
        for (int i = 0; i < m_board.transform.childCount; i++)
        {
            Transform line = m_board.transform.GetChild(i);

            // 라인의 자식인 타일들 등록
            for (int j = 0; j < line.transform.childCount; j++)
            {
                Transform tile = line.transform.GetChild(j);

                if (tile.CompareTag("Tile"))
                {
                    Vector2Int matrix = tile.GetComponent<Tile>().GetMatrix();
                    m_tiles.Add(matrix, tile.gameObject);

                    // 최대 행렬 세팅
                    if (m_maxMatrix.x < matrix.x)
                    {
                        m_maxMatrix.x = matrix.x;
                    }
                    if (m_maxMatrix.y < matrix.y)
                    {
                        m_maxMatrix.y = matrix.y;
                    }
                }
            }
        }
        #endregion
    }

    #region 변수

    [SerializeField]
    GameObject m_board;
    // 타일 최대 행렬
    Vector2Int m_maxMatrix = new Vector2Int(0, 0);
    // 맵에 있는 타일들
    Dictionary<Vector2Int, GameObject> m_tiles = new Dictionary<Vector2Int, GameObject>();

    #region Stage 설정 변수
    [SerializeField]
    int m_maxBlockType = 5;
    [SerializeField]
    //int m_maxMove = 20;
    #endregion

    // 매치가 가능한 블록들 저장
    List<GameObject> m_matchOK = new List<GameObject>();
    bool m_hint = false;
    bool m_hintStart = false;

    #endregion 변수 끝

    #region Get함수
    public GameObject GetTile(in Vector2Int _Matrix)
    { 
        if (m_tiles.ContainsKey(_Matrix))
        {
            return m_tiles[_Matrix];
        }

        return null;
    }
    public int GetMaxBlockType() { return m_maxBlockType; }
    public Vector2Int GetMaxMatrix() { return m_maxMatrix; }
    #endregion

    #region Set함수
    public void SetMatchOK(in List<GameObject> _matchOK)
    {
        m_matchOK.Clear();
        m_matchOK = new List<GameObject>(_matchOK);
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 시작 시 맵 체크(스테이지 블록 구성을 랜덤으로 했을 경우 대비)
        CheckPossibleMatch();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_hint && !m_hintStart)
        {
            StartCoroutine(Hint());
        }
    }

    IEnumerator Hint()
    {
        m_hintStart = true;
        yield return new WaitForSeconds(10f); // 10초 대기 

        // 매치가 된다면 어디가 매치 되는지 저장했다가 몇 초 마다 알려주기
        if (m_matchOK.Count >= 3)
        {
            // 외곽선 실행
            foreach (GameObject tile in m_matchOK)
            {
                tile.GetComponent<Tile>().SetMyBlockActiveOutline();
            }
        }
        yield return new WaitForSeconds(5f); // 5초 대기
        m_hintStart = false;
    }

    // 움직여서 매치가 될 수 있나 확인
    public void CheckPossibleMatch()
    {
        // 움직일 수 있는 타일의 블록을 상하좌우로 움직인(임시로) 다음 매치가 되는지 체크
        // 매치가 하나라도 된다면 바로 return
        // 모두 매치가 안 된다면 움직일 수 없는 타일을 제외하고 블록 타입 개수를 수집한 뒤, 랜덤으로 배분하고 블록을 바꿈
        m_hint = false;

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (MatchMgr.Instance.SimulationMatch(tile))
                {
                    Debug.Log("매치 가능");
                    m_matchOK.Add(tile);
                    m_hint = true;
                    return;
                }
            }
        }
        Debug.Log("매치 불가능");
        RandomPlacement();
    }

    // 무작위 배치
    void RandomPlacement()
    {
        List<GameObject> tiles = new List<GameObject>();
        List<BlockType> blockTypes = new List<BlockType>();
        List<BlockType> saveBlockTypes = new List<BlockType>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);
                if (tile == null)
                {
                    break;
                }

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

        bool loof = false;
        do
        {
            // 무작위 배치 실행
            foreach (GameObject tile in tiles)
            {
                int random = Random.Range(0, blockTypes.Count);
                tile.GetComponent<Tile>().SetMyBlockType(blockTypes[random]);
                blockTypes.RemoveAt(random);

                // 만약 바로 매치가 된다면 재실행
                if (MatchMgr.Instance.CheckMatch(tile, false))
                {
                    blockTypes = new List<BlockType>(saveBlockTypes);
                    // 만약 시도해도 안 된다면 무작위 배치를 재실행(은 이 방법이 안 되면 시도할 것)
                    //RandomPlacement();
                    //return;
                    loof = true;
                    break;
                }
            }
            loof = false;
        } while (loof);

        // 다시 움직여서 매치가 될 수 있는지 확인
        CheckPossibleMatch();
    }

    // 클리어 조건 확인
    public void CheckStage()
    {

    }
}
