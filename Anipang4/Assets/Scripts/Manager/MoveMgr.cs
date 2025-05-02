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
    bool m_isClickMoving = false;

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

                // 특수 블록인 경우 마우스 한 번 클릭에도 매치(Random 제외)
                /* 코드 추가 예정 */

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

                    m_isClickMoving = true;
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
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                // 둘 다 특수 블록인 경우->특수 블록 합성
                /* 코드 추가 예정 */

                // 매치 시도 후 매치가 안 되면 원상복구
                bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                // 둘 다 매치가 되지 않았다면
                if (match1 == false && match2 == false)
                {
                    Debug.LogWarning("원상복구");
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

                // 매치 후 빈 공간 채우기 실행
                StartCoroutine(CheckEmpty());
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;
            m_isClickMoving = false;

            m_completeCount = 0;
        }
    }

    public IEnumerator CheckEmpty()
    {
        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // 위에부터 밑에 타일이 빈 블록인지 확인 후 그쪽으로 보냄
        /*
         * 1. 본인의 아래 블록이 있다->거기로 보냄
         * 2. 아래 블록이 있는데 대각선 아래가 비었다->그 아래의 위가 움직일 수 없는 타일이라면 그쪽으로 보냄
        */
        for (int i = 0; i <= maxMatrix.y; i++)
        {
            bool isEmpty = false;
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                // 본인
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                // 밑 타일
                Vector2Int downMatrix = new Vector2Int(j, i + 1);
                GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                if (downTile != null)
                {
                    if (downTile.GetComponent<Tile>().IsBlockEmpty())
                    {
                        // 바로 밑 블록이 비어있는 경우 자신의 블록을 밑으로 보냄
                        tile.GetComponent<Tile>().EmptyMoving(downTile);
                        isEmpty = true;
                    }
                    else
                    {
                        #region 왼쪽, 오른쪽 타일이 움직일 수 없는 경우 대각선 검사까지 함
                        Vector2Int leftMatrix = new Vector2Int(j - 1, i);
                        GameObject leftTile = StageMgr.Instance.GetTile(leftMatrix);
                        Vector2Int rightMatrix = new Vector2Int(j + 1, i);
                        GameObject rightTile = StageMgr.Instance.GetTile(rightMatrix);

                        if (DiagonalTest(matrix, leftMatrix))
                        {
                            isEmpty = true;
                        }
                        else
                        {
                            if (DiagonalTest(matrix, rightMatrix))
                            {
                                Debug.Log("오른쪽 아래 갔음");
                                isEmpty = true;
                            }
                        }

                        #endregion
                    }
                }
            }
            // 코루틴으로 시간 지난 뒤 진행되게 함
            if (isEmpty)
            {
                yield return new WaitForSeconds(0.25f);
            }
        }

        // 그리고 매치 체크
        for (int length = 0; length <= maxMatrix.y; length++)
        {
            for (int i = maxMatrix.y; i >= 0; i--)
            {
                for (int j = 0; j <= maxMatrix.x; j++)
                {
                    Vector2Int matrix = new Vector2Int(j, i);
                    GameObject tile = StageMgr.Instance.GetTile(matrix);
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // 특수 블록이 아닐 경우에만 매치 판정을 함
                    if (type < BlockType.CROSS)
                    {
                        MatchMgr.Instance.CheckMatch(tile);
                    }
                }
            }
        }

        // 빈 공간 없나 체크(재귀함수로 빈 공간 없을 때 까지 하기)
        // 만약 빈 공간이 있으나, 거기로 블록을 보낼 수 없는 경우(왼쪽 위, 위, 오른쪽 위가 모두 움직일 수 없는 타일) 패스
        for (int i = 0; i <= maxMatrix.y; i++)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    TileType tileType = tile.GetComponent<Tile>().GetTileType();
                    if (tileType == TileType.MOVABLE)
                    {
                        #region 왼쪽 위, 위, 오른쪽 위가 모두 움직일 수 없나 체크
                        Vector2Int leftUpMatrix = new Vector2Int(j - 1, i - 1);
                        Vector2Int upMatrix = new Vector2Int(j, i - 1);
                        Vector2Int rightUpMatrix = new Vector2Int(j + 1, i - 1);
                        if (EmptySpaceTest(rightUpMatrix) && EmptySpaceTest(upMatrix) && EmptySpaceTest(leftUpMatrix))
                        {
                            break;
                        }
                        #endregion

                        StartCoroutine(CheckEmpty());
                        yield break;
                    }
                }
            }
        }

        // 앞으로 매치가 가능한지 체크
        StageMgr.Instance.CheckPossibleMatch();

        m_emptyMoving = false;
    }

    // CheckEmpty에서 대각선 검사할 때 사용
    bool DiagonalTest(in Vector2Int _originalMatrix, in Vector2Int _checkMatrix)
    {
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);
        GameObject checkTile = StageMgr.Instance.GetTile(_checkMatrix);

        if (originalTile != null && checkTile != null)
        {
            TileType type = checkTile.GetComponent<Tile>().GetTileType();
            if (type == TileType.IMMOVABLE)
            {
                // 움직일 수 없는 경우 밑 검사
                Vector2Int downMatrix = new Vector2Int(_checkMatrix.x, _checkMatrix.y + 1);
                GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                if (downTile != null)
                {
                    if (downTile.GetComponent<Tile>().IsBlockEmpty())
                    {
                        originalTile.GetComponent<Tile>().EmptyMoving(downTile);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // CheckEmpty에서 빈 공간 검사 : 타일이 움직일 수 없나 검사
    bool EmptySpaceTest(in Vector2Int _matrix)
    {
        GameObject tile = StageMgr.Instance.GetTile(_matrix);
        if (tile != null)
        {
            TileType type = tile.GetComponent<Tile>().GetTileType();
            if (type == TileType.IMMOVABLE)
            {
                return true;
            }
        }

        return false;
    }
}
