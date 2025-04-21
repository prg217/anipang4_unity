using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class MoveMgr : MonoBehaviour
{
    #region 싱글톤
    static MoveMgr instance;

    public static MoveMgr Instance
    {
        get
        {
            if (instance == null) instance = new MoveMgr();
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
    }

    #region 변수

    // ====== 클릭 된 타일 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ===============================
    // 움직이는 중
    bool m_moving = false;
    int m_completeCount = 0;

    // 되돌리기
    bool m_reMoving = false;
    // 빈 블럭 채우기 중
    bool m_emptyMoving = false;

    #endregion 변수 끝

    #region Set함수
    public void SetClickedTileAndMoving(in GameObject _tile1, in GameObject _tile2)
    {
        m_pClickedTile1 = _tile1;
        m_pClickedTile2 = _tile2;

        Moving();
    }
    #endregion 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckClick();
    }

    void CheckClick()
    {
        // 마우스 왼쪽 클릭
        if (Input.GetMouseButton(0) && !m_moving)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;
                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;
                }
                else
                {
                    // 이미 저장된 타일을 누르고 있다면 return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
                    }

                    m_pClickedTile2 = clickedTransform.gameObject;

                    Moving();
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && !m_moving)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;
                m_pClickedTile1 = null;
                m_pClickedTile2 = null;
            }
        }
    }

    void Moving()
    {
        if (m_pClickedTile1 == null || m_pClickedTile2 == null)
        {
            return;
        }

        m_moving = true;

        #region 둘 중 하나라도 움직일 수 없는 상태인지 확인
        //Debug.Log(m_pClickedTile1);
        //Debug.Log(m_pClickedTile2);
        TileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        TileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // 둘 중 하나라도 움직일 수 없는 상태라면 움직이지 않음
        if (tileType1 == TileType.IMMOVABLE || tileType2 == TileType.IMMOVABLE)
        {
            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            return;
        }
        #endregion

        #region 블록 움직이기
        // 타일이 가지고 있는 블록에게 상대 타일쪽으로 움직이라고 함
        GameObject block1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlock();
        GameObject block2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlock();

        block1.GetComponent<Block>().SetMove(m_pClickedTile2);
        block2.GetComponent<Block>().SetMove(m_pClickedTile1);
        #endregion

        // 타일들 정보 새로고침
        m_pClickedTile1.GetComponent<Tile>().Refresh();
        m_pClickedTile2.GetComponent<Tile>().Refresh();
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // 블록 교환이 둘 다 완료가 되었다면
        if (m_completeCount >= 2)
        {
            if (!m_reMoving)
            {
                // 매치 시도 후 매치가 안 되면 원상복구
                bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                if (!m_emptyMoving)
                {
                    // 둘 다 매치가 되지 않았다면
                    if (match1 == false && match2 == false)
                    {
                        // 원상복구
                        m_reMoving = true;
                        GameObject tempTile = m_pClickedTile1;
                        m_pClickedTile1 = m_pClickedTile2;
                        m_pClickedTile2 = tempTile;
                        Moving();

                        m_moving = false;
                        m_completeCount = 0;
                        return;
                    }
                }
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;

            m_completeCount = 0;

            // 빈 공간 채우기
            if (!m_emptyMoving)
            {
                //StartCoroutine(CheckEmpty());
            }
            StartCoroutine(CheckEmpty());
            // 스테이지 매니저에 스테이지 검사 요구
            StageMgr.Instance.CheckStage();
        }
    }

    IEnumerator CheckEmpty()
    {
        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        // 아래쪽부터 모든 타일들에게 빈 공간 체크하게 한 뒤 위 타일에서 블록 받아옴
        for (int i = maxMatrix.y; i >= 0; i--)
        {
            bool isEmpty = false;
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);
                // 타일이 비었을 경우 등록된 윗 타일에서 블록을 내려받음
                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    tile.GetComponent<Tile>().EmptyMoving();
                    isEmpty = true;
                }
            }
            // 코루틴으로 시간 지난 뒤 진행되게 함
            if (isEmpty)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        // 그리고 매치 체크
        for (int i = maxMatrix.y; i >= 0; i--)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);
                bool match = MatchMgr.Instance.CheckMatch(tile);
                if (match)
                {
                    //m_emptyMoving = false;
                    //yield break;
                }
            }
        }

        // 앞으로 매치가 가능한지 체크

        m_emptyMoving = false;
    }
}
