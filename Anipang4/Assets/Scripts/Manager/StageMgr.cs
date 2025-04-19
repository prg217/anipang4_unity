using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;

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
    int m_MaxMove = 20;
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
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckStage()
    {
        // 아래쪽부터 모든 타일들에게 빈 공간 체크하게 한 뒤 위 타일에서 블록 받아옴(MoveMgr 활용)
        for (int i = m_MaxMatrix.y; i >= 0; i--)
        {
            for (int j = 0; j < m_MaxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(i, j);
                GameObject tile = m_Tiles[matrix];

                // 타일이 비었을 경우 등록된 윗 타일에서 블록을 내려받음
                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    tile.GetComponent<Tile>().EmptyMoving();
                }
            }
        }

        // 그리고 매치 체크


        // 앞으로 매치가 가능한지 체크
    }
}
