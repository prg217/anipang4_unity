using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

public class MoveMgr : BaseMgr<MoveMgr>
{
    #region 변수

    [SerializeField]
    GameObject m_moveEffect;

    // ====== 드래그 클릭 된 타일 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ======================================


    int m_completeCount = 0;


    // ====== 상태 ======
    bool m_isClickMoving = false;
    // 움직임 완료
    bool m_moveComplete = false;
    // 움직이는 중
    bool m_moving = false;
    bool m_specialClicked = false;
    // 되돌리기
    bool m_reMoving = false;
    // 클릭 가능한 상태
    bool m_clickOK = true;
    // ==================

    // ====== 빈공간 채우기 ======
    // 빈공간 채우기 중
    bool m_emptyMoving = false;
    // 빈공간 채우기 중에 클릭
    bool m_clickDuringEmptyMoving = false;

    List<Coroutine> checkEmptyCoroutines = new List<Coroutine>();
    // ==========================

    #endregion 변수 끝

    #region Set함수
    public void SetClickedTileAndMoving(in GameObject _tile1, in GameObject _tile2)
    {
        m_pClickedTile1 = _tile1;
        m_pClickedTile2 = _tile2;

        Moving();
    }
    #endregion

    public event System.Action OnEmptyMoveCompleteFunction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_clickOK)
        {
            CheckClick();
        }
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

                // 만약 빈자리 채우기 중이라면 빈자리 채우기 움직이던거 멈추고 이쪽 먼저 처리
                if (m_emptyMoving)
                {
                    // 코루틴 중지
                    foreach (Coroutine coroutine in checkEmptyCoroutines)
                    {
                        if (coroutine != null)
                        {
                            StopCoroutine(coroutine);
                        }
                    }
                    checkEmptyCoroutines.Clear();

                    m_emptyMoving = false;
                    m_pClickedTile1 = null;
                    m_pClickedTile2 = null;

                    m_clickDuringEmptyMoving = true;
                }

                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;

                    // 특수 블록인 경우 마우스 한 번 클릭에도 매치(Random 제외)
                    BlockType type = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                    if (type >= BlockType.CROSS && type != BlockType.RANDOM)
                    {
                        m_specialClicked = true;
                    }
                }
                else
                {
                    // 이미 저장된 타일을 누르고 있다면 return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
                    }

                    m_pClickedTile2 = clickedTransform.gameObject;

                    m_specialClicked = false;
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
                // 특수 블록을 한 번 클릭했을 경우
                if (m_specialClicked)
                {
                    // 직접 움직였을 때만 moveCount차감
                    ConsumeMove();

                    m_specialClicked = false;
                    m_isClickMoving = true;

                    ObstacleType obType = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                    MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    // 매치 후 빈 공간 채우기 실행
                    if (!m_emptyMoving)
                    {
                        StartCheckEmpty();
                    }
                }

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

        // 직접 클릭해서 할 때만 체크
        if (!m_emptyMoving)
        {
            m_moving = true;
        }

        // 둘 중 하나라도 움직일 수 없는 상태인지 확인
        if (IsMovementImpossible())
        {
            return;
        }
        
        // 빈자리 채우기가 아닐 때에만 적용
        if (!m_emptyMoving)
        {
            // 상하좌우에 있는 타일인지 확인
            if (!CheckAdjacentTile())
            {
                return;
            }
        }

        #region 미리 매치 시도 후 reMoving 대상인지 판단
        if (!m_reMoving && m_isClickMoving)
        {
            bool match1 = MatchMgr.Instance.SimulationMatch(m_pClickedTile1, m_pClickedTile2);
            bool match2 = MatchMgr.Instance.SimulationMatch(m_pClickedTile2, m_pClickedTile1);

            if (match1 == false && match2 == false)
            {
                // 원상복구
                m_reMoving = true;
            }
        }
        #endregion

        // 직접 움직였을 때만 moveCount차감
        ConsumeMove();

        #region 블록 움직이기
        // 타일이 가지고 있는 블록에게 상대 타일쪽으로 움직이라고 함
        m_pClickedTile1.GetComponent<Tile>().SetBlockMove(m_pClickedTile2, m_emptyMoving);
        m_pClickedTile2.GetComponent<Tile>().SetBlockMove(m_pClickedTile1, m_emptyMoving);

        // 블록 움직이는 이펙트
        // 직접 움직일 때만 적용
        if (!m_emptyMoving)
        {
            StartCoroutine(ActiveMoveEffect());
        }
        #endregion
        m_moveComplete = false;

        // 타일들 정보 새로고침
        m_pClickedTile1.GetComponent<Tile>().Refresh();
        m_pClickedTile2.GetComponent<Tile>().Refresh();
    }

    IEnumerator ActiveMoveEffect()
    {
        Vector3 midPos = (m_pClickedTile1.transform.position + m_pClickedTile2.transform.position) / 2f;
        m_moveEffect.transform.position = midPos;

        // 상하좌우에 따라 각도 돌리기
        Vector2Int matrix1 = m_pClickedTile1.GetComponent<Tile>().GetMatrix();
        Vector2Int matrix2 = m_pClickedTile2.GetComponent<Tile>().GetMatrix();
        // 가로
        if (matrix1.x - matrix2.x != 0)
        {
            m_moveEffect.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // 세로
        else
        {
            m_moveEffect.transform.rotation = Quaternion.Euler(0, 0, 90f);
        }

        m_moveEffect.SetActive(true);

        // 움직이는 시간이 0.5초
        yield return new WaitForSeconds(0.5f);

        m_moveEffect.SetActive(false);
    }

    bool CheckAdjacentTile()
    {
        // 상하좌우 체크
        Vector2Int matrix1 = m_pClickedTile1.GetComponent<Tile>().GetMatrix();
        Vector2Int matrix2 = m_pClickedTile2.GetComponent<Tile>().GetMatrix();

        // 맨해튼 거리(거리 최소 값)
        int distance = Mathf.Abs(matrix1.x - matrix2.x) + Mathf.Abs(matrix1.y - matrix2.y);

        if (distance == 1)
        {
            return true;
        }

        return false;
    }

    bool IsMovementImpossible()
    {
        TileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        TileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // 둘 중 하나라도 움직일 수 없는 상태라면 움직이지 않음
        if (tileType1 == TileType.IMMOVABLE || tileType2 == TileType.IMMOVABLE)
        {
            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            return true;
        }

        return false;
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // 블록 교환이 둘 다 완료가 되었다면
        if (m_completeCount >= 2)
        {
            m_moveComplete = true;
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                BlockType type1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                BlockType type2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlockType();

                #region 장애물 설정
                ObstacleType obType1 = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                ObstacleType obType2 = m_pClickedTile2.GetComponent<Tile>().GetPropagationObstacle();
                ObstacleType obType = obType1;

                if (obType1 != ObstacleType.NONE)
                {
                    obType = obType1;
                }
                else if (obType2 != ObstacleType.NONE)
                {
                    obType = obType2;
                }
                #endregion

                // 둘 다 특수 블록인 경우->특수 블록 합성
                if (type1 >= BlockType.CROSS && type2 >= BlockType.CROSS)
                {
                    MatchMgr.Instance.SpecialCompositionExplode(m_pClickedTile1, m_pClickedTile2, obType);

                    if (type1 == BlockType.RANDOM || type2 == BlockType.RANDOM)
                    {
                        return;
                    }
                }
                // 아닌경우
                else
                {
                    #region 랜덤+일반

                    // 랜덤과 일반 블록인 경우->랜덤 Explode 실행
                    if (type1 == BlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile1, type2, obType);
                    }
                    else if (type2 == BlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile2, type1, obType);
                    }

                    #endregion

                    // 매치 시도 후 매치가 안 되면 원상복구
                    bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                    if (type1 == BlockType.RANDOM || type2 == BlockType.RANDOM)
                    {
                        return;
                    }
                }
            }
            else if (m_reMoving)
            {
                // 둘 다 매치가 되지 않았다면
                // 원상복구
                GameObject tempTile = m_pClickedTile1;
                m_pClickedTile1 = m_pClickedTile2;
                m_pClickedTile2 = tempTile;
                Moving();
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;
            m_isClickMoving = false;

            m_completeCount = 0;

            // 빈공간 채우기 실행
            if (!m_emptyMoving)
            {
                StartCheckEmpty();
            }

            if (m_clickDuringEmptyMoving)
            {
                m_emptyMoving = true;
            }
        }
    }

    public void StartCheckEmpty()
    {
        Coroutine coroutine = StartCoroutine(CheckEmpty());
        checkEmptyCoroutines.Add(coroutine);
    }

    void ConsumeMove()
    {
        if (m_isClickMoving && !m_reMoving)
        {
            StageInfo.MoveCount--;
            StageMgr.Instance.CheckGameOver();
        }
    }

    // 외부에서 움직임 잠깐 멈춰달라고 요청
    public void WaitMove()
    { 
        // 빈공간 채우기 코루틴 멈추기
    }

    public IEnumerator CheckEmpty()
    {
        // 빈 공간 체크하는 중에는 스테이지 매니저에 힌트를 못 하게 전달
        StageMgr.Instance.SetHint(false);

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
                                isEmpty = true;
                            }
                        }

                        #endregion
                    }
                }
            }
            // 움직인 다음에 진행되게 함
            if (isEmpty)
            {
                yield return new WaitUntil(() => m_moveComplete);
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

                        // 빈 공간이 있을 때 다시 처음부터
                        Coroutine coroutine = StartCoroutine(CheckEmpty());
                        checkEmptyCoroutines.Add(coroutine);
                        yield return coroutine;
                        yield break;
                    }
                }
            }
        }

        // 클리어 확인
        StageMgr.Instance.CheckStageClear();

        // 스테이지 클리어, 게임 오버 시 완료 신호 보냄
        OnEmptyMoveCompleteFunction?.Invoke();

        // 앞으로 매치가 가능한지 체크
        StageMgr.Instance.CheckPossibleMatch();

        // 다시 힌트를 줄 수 있게 설정
        StageMgr.Instance.SetHint(true);
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

    // CheckEmpty에서 빈 공간 검사 : 타일이 움직일 수 없나 검사(움직일 수 없을 때 true)
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
            else
            {
                return false;
            }
        }

        return true;
    }

    public void StopClickMoving()
    {
        // 클릭해서 움직이는 행동을 못하게 함
        m_clickOK = false;
    }
}
