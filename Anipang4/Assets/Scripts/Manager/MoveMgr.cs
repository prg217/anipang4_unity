using UnityEngine;
using System.Collections;
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
    // 빈공간 채우기 활성 여부
    bool m_checkEmptyEnabled = true;

    List<Coroutine> checkEmptyCoroutines = new List<Coroutine>();
    // ==========================

    #endregion 변수 끝

    #region Set함수
    public void SetCheckEmptyEnabled(in bool _setting)
    {
        m_checkEmptyEnabled = _setting;

        if (!m_checkEmptyEnabled)
        {
            StopCheckEmpty();
        }
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
        Vector2 inputPosition = Vector2.zero;
        bool hasInput = false;
        bool isReleased = false;

        if (Input.GetMouseButton(0) && !m_moving)
        {
            inputPosition = Input.mousePosition;
            hasInput = true;
        }
        else if ((Input.touchCount > 0) && !m_moving)
        {
            inputPosition = Input.GetTouch(0).position;
            hasInput = true;
        }

        if (hasInput)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;

                // 만약 빈자리 채우기 중이라면 빈자리 채우기 움직이던거 멈추고 이쪽 먼저 처리
                if (m_emptyMoving)
                {
                    StopCheckEmpty();

                    m_emptyMoving = false;
                    m_pClickedTile1 = null;
                    m_pClickedTile2 = null;

                    m_clickDuringEmptyMoving = true;
                }

                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;

                    // 특수 블록인 경우 마우스 한 번 클릭에도 매치(Random 제외)
                    EBlockType type = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                    if (type >= EBlockType.CROSS && type != EBlockType.RANDOM)
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
            inputPosition = Input.mousePosition;
            isReleased = true;      
        }
        else if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) && !m_moving)
        {
            inputPosition = Input.GetTouch(0).position;
            isReleased = true;
        }

        if (isReleased)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                // 특수 블록을 한 번 클릭했을 경우
                if (m_specialClicked)
                {
                    // 앞 장애물이 있는 경우 터트리지 않음
                    if (m_pClickedTile1.GetComponent<Tile>().GetFrontObstacleEmpty())
                    {
                        // 직접 움직였을 때만 moveCount차감
                        ConsumeMove();

                        m_specialClicked = false;
                        m_isClickMoving = true;

                        MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                        // 매치 후 빈 공간 채우기 실행
                        if (!m_emptyMoving)
                        {
                            StartCheckEmpty();
                        }
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

        SoundMgr.Instance.PlaySFX(ESFX.BLOCK_SWAP);

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
        ETileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        ETileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // 둘 중 하나라도 움직일 수 없는 상태라면 움직이지 않음
        if (tileType1 == ETileType.IMMOVABLE || tileType2 == ETileType.IMMOVABLE)
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
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                if (m_pClickedTile1 == null)
                {
                    return;
                }

                EBlockType type1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                EBlockType type2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlockType();

                #region 장애물 설정
                EObstacleType obType1 = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                EObstacleType obType2 = m_pClickedTile2.GetComponent<Tile>().GetPropagationObstacle();
                EObstacleType obType = obType1;

                if (obType1 != EObstacleType.NONE)
                {
                    obType = obType1;
                }
                else if (obType2 != EObstacleType.NONE)
                {
                    obType = obType2;
                }
                #endregion

                // 둘 다 특수 블록인 경우->특수 블록 합성
                if (type1 >= EBlockType.CROSS && type2 >= EBlockType.CROSS)
                {
                    MatchMgr.Instance.SpecialCompositionExplode(m_pClickedTile1, m_pClickedTile2, obType);

                    if (type1 == EBlockType.RANDOM || type2 == EBlockType.RANDOM)
                    {
                        return;
                    }
                }
                // 아닌경우
                else
                {
                    #region 랜덤+일반

                    // 랜덤과 일반 블록인 경우->랜덤 Explode 실행
                    if (type1 == EBlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile1, type2, obType);
                    }
                    else if (type2 == EBlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile2, type1, obType);
                    }

                    #endregion

                    // 매치 시도 후 매치가 안 되면 원상복구
                    bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                    if (type1 == EBlockType.RANDOM || type2 == EBlockType.RANDOM)
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
        if (!m_checkEmptyEnabled)
        {
            return;
        }

        // 기존에 이미 작동하고 있는 CheckEmpty가 있다면 멈추고 다시 하게 함
        foreach (Coroutine checkEmptyCoroutine in checkEmptyCoroutines)
        {
            if (checkEmptyCoroutine != null)
            {
                StopCoroutine(checkEmptyCoroutine);
            }
        }
        checkEmptyCoroutines.Clear();

        Coroutine coroutine = StartCoroutine(CheckEmpty());
        checkEmptyCoroutines.Add(coroutine);
    }

    void StopCheckEmpty()
    {
        // 기존에 이미 작동하고 있는 CheckEmpty가 있다면 멈춤
        foreach (Coroutine checkEmptyCoroutine in checkEmptyCoroutines)
        {
            if (checkEmptyCoroutine != null)
            {
                StopCoroutine(checkEmptyCoroutine);
            }
        }
        checkEmptyCoroutines.Clear();

        // 다시 힌트를 줄 수 있게 설정
        StageMgr.Instance.SetHint(true);
        m_emptyMoving = false;
    }

    public void ActiveCheckEmpty()
    {
        SetCheckEmptyEnabled(true);
        StartCheckEmpty();
    }

    void ConsumeMove()
    {
        if (m_isClickMoving && !m_reMoving)
        {
            StageInfo.MoveCount--;
            StageMgr.Instance.CheckGameOver();
        }
    }

    // 제일 아래에 있는 빈 공간 찾기
    Vector2Int SearchLastDownEmptyTile(in Vector2Int _matrix)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        Vector2Int lastEmptyMatrix = new Vector2Int(-1, -1);

        for (int i = _matrix.y + 1; i <= maxMatrix.y; i++)
        {
            // 최대 y와 같다면, 블록이 있어도 더 떨어질 곳이 없음
            if (_matrix.y >= maxMatrix.y)
            {
                return(new Vector2Int(-1, -1));
            }
            Vector2Int downMatrix = new Vector2Int(_matrix.x, i);
            GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

            if (downTile.GetComponent<Tile>().IsBlockEmpty())
            {
                lastEmptyMatrix = downMatrix;
            }
            // 빈 공간이 아니라면 바로 빠져나오기
            else
            {
                break;
            }
        }

        return lastEmptyMatrix;
    }

    // 제일 대각선 아래에 있는 빈 공간 찾기
    Vector2Int SearchLastDiagonalEmptyTile(in Vector2Int _matrix)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        Vector2Int lastEmptyMatrix = new Vector2Int(-1, -1);
        Vector2Int testMatrix = _matrix;

        while (true)
        {
            Vector2Int leftMatrix = new Vector2Int(testMatrix.x - 1, testMatrix.y);
            Vector2Int rightMatrix = new Vector2Int(testMatrix.x + 1, testMatrix.y);

            if (DiagonalTest(testMatrix, leftMatrix))
            {
                lastEmptyMatrix = new Vector2Int(leftMatrix.x, leftMatrix.y + 1);
            }
            else if (DiagonalTest(testMatrix, rightMatrix))
            {
                lastEmptyMatrix = new Vector2Int(rightMatrix.x, rightMatrix.y + 1);
            }
            // 대각선 아래에 빈 공간이 없으면 빠져나옴
            else
            {
                break;
            }

            testMatrix = lastEmptyMatrix;
        }

        return lastEmptyMatrix;
    }

    public void EmptyMoving(in GameObject _startTile, in Vector2Int _point)
    {
        // _point를 향해 쭉 이동(빈 공간은 바로 순간이동 시키기)
        GameObject pointTile = StageMgr.Instance.GetTile(_point);

        _startTile.GetComponent<Tile>().SetBlockMove(pointTile, true);
        pointTile.GetComponent<Tile>().BlockTeleport(_startTile);

        _startTile.GetComponent<Tile>().Refresh();
        pointTile.GetComponent<Tile>().Refresh();

        if (_startTile.GetComponent<Tile>().IsEmptyCreateTile())
        {
            _startTile.GetComponent<Tile>().CreateBlock();
        }
    }

    public IEnumerator CheckEmpty()
    {
        // 빈 공간 체크하는 중에는 스테이지 매니저에 힌트를 못 하게 전달
        StageMgr.Instance.SetHint(false);

        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // SearchEmptyTile를 이용해 가장 밑 빈 공간 타일 서치(연속으로)
        // 대각선이 있다면 가장 밑->대각선->가장 밑(이 있다면)or대각선 ``` 반복 순으로 이동
        bool isEmpty = true;

        for (int i = maxMatrix.y; i >= 0; i--)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                // 본인
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                // 이동 불가 타일이라면 패스
                if (tile.GetComponent<Tile>().GetTileType() == ETileType.IMMOVABLE)
                {
                    continue;
                }

                // 빈 공간인데, 생성 블록이라면
                if (tile.GetComponent<Tile>().IsEmptyCreateTile())
                {
                    tile.GetComponent<Tile>().CreateBlock();
                    continue;
                }

                Vector2Int lastEmpty = new Vector2Int(-1, -1);

                while (true)
                {
                    lastEmpty = SearchLastDownEmptyTile(matrix);
                    if (lastEmpty == new Vector2Int(-1, -1))
                    {
                        lastEmpty = SearchLastDiagonalEmptyTile(matrix);

                        // 빈 공간이 없을 때 다음으로 넘어감
                        if (lastEmpty == new Vector2Int(-1, -1))
                        {
                            break;
                        }
                    }
                    break;
                }

                // 빈 공간이 없을 때 다음으로 넘어감
                if (lastEmpty == new Vector2Int(-1, -1))
                {
                    continue;
                }

                // 빈 공간을 향해 이동
                tile.GetComponent<Tile>().EmptyMoving(lastEmpty);
            }

            yield return new WaitForSeconds(0.05f);
        }

        // 빈 공간 없나 체크
        // 만약 빈 공간이 있으나, 거기로 블록을 보낼 수 없는 경우(왼쪽 위, 위, 오른쪽 위가 모두 움직일 수 없는 타일) 패스
        isEmpty = DoubleCheckEmpty();

        if (isEmpty)
        {
            StartCheckEmpty();
            yield break;
        }


        // 매치 체크
        for (int length = 0; length <= maxMatrix.y; length++)
        {
            for (int i = maxMatrix.y; i >= 0; i--)
            {
                for (int j = 0; j <= maxMatrix.x; j++)
                {
                    Vector2Int matrix = new Vector2Int(j, i);
                    GameObject tile = StageMgr.Instance.GetTile(matrix);
                    EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // 특수 블록이 아닐 경우에만 매치 판정을 함
                    if (type < EBlockType.CROSS)
                    {
                        var(result, matchCount, matchTiles) = MatchMgr.Instance.CheckMatchWithStatus(tile, false);

                        if (result)
                        {
                            // 매치 가능하면 그 매치 가능한 타일들 각자를 또 검사
                            // ->제일 많은 매치 카운트를 가진 타일로 터트림
                            GameObject mostMatchCountTile = tile;

                            foreach (GameObject matchTile in matchTiles)
                            {
                                var (result2, matchCount2, _) = MatchMgr.Instance.CheckMatchWithStatus(tile, false);

                                if (result2)
                                {
                                    if (matchCount < matchCount2)
                                    {
                                        matchCount = matchCount2;
                                        mostMatchCountTile = matchTile;
                                    }
                                }
                            }

                            MatchMgr.Instance.CheckMatch(mostMatchCountTile);
                            // 매치가 일어났다는 뜻은 빈 공간이 생겼다는 뜻
                            isEmpty = true;
                        }
                    }
                }
            }
        }

        // 매치 체크 후 다시 빈 공간 검사
        if (isEmpty)
        {
            StartCheckEmpty();
            yield break;
        }

        // 로그 갱신
        LogMgr.Instance.UpdateLog();

        // 클리어 확인
        StageMgr.Instance.CheckStageClear();

        // 스테이지 클리어, 게임 오버 시 완료 신호 보냄
        OnEmptyMoveCompleteFunction?.Invoke();

        // 앞으로 매치가 가능한지 체크
        StageMgr.Instance.StartCheckPossibleMatch();

        // 다시 힌트를 줄 수 있게 설정
        StageMgr.Instance.SetHint(true);
        m_emptyMoving = false;
    }

    bool DoubleCheckEmpty()
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        for (int i = 0; i <= maxMatrix.y; i++)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    ETileType tileType = tile.GetComponent<Tile>().GetTileType();
                    if (tileType == ETileType.MOVABLE)
                    {
                        // 빈 생성 타일일 경우
                        if (tile.GetComponent<Tile>().IsEmptyCreateTile())
                        {
                            return true;
                        }

                        #region 왼쪽 위, 위, 오른쪽 위가 모두 움직일 수 없나 체크
                        Vector2Int leftUpMatrix = new Vector2Int(j - 1, i - 1);
                        Vector2Int upMatrix = new Vector2Int(j, i - 1);
                        Vector2Int rightUpMatrix = new Vector2Int(j + 1, i - 1);
                        if (EmptySpaceTest(rightUpMatrix) && EmptySpaceTest(upMatrix) && EmptySpaceTest(leftUpMatrix))
                        {
                            break;
                        }
                        #endregion

                        return true;
                    }
                }
            }
        }
        return false;
    }

    // CheckEmpty에서 대각선 검사할 때 사용
    bool DiagonalTest(in Vector2Int _originalMatrix, in Vector2Int _checkMatrix)
    {
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);

        if (originalTile == null)
        {
            return false;
        }

        // y축 위를 쭉 검사해서 위쪽에 그냥 블록이 없고(다 빈공간이고)맨 위가 이동 불가라면 대각선 이동
        for (int y = _checkMatrix.y; y >= 0; y--)
        {
            GameObject checkUpTile = StageMgr.Instance.GetTile(new Vector2Int(_checkMatrix.x, y));

            if (checkUpTile != null)
            {
                ETileType type = checkUpTile.GetComponent<Tile>().GetTileType();

                // 만약 막혀있는 맨 위가 이동 불가라면 대각선 이동 테스트 시도
                if (type == ETileType.IMMOVABLE)
                {
                    // 움직일 수 없는 경우 밑 검사
                    Vector2Int downMatrix = new Vector2Int(_checkMatrix.x, _checkMatrix.y + 1);
                    GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                    if (downTile != null)
                    {
                        if (downTile.GetComponent<Tile>().IsBlockEmpty())
                        {
                            return true;
                        }
                    }
                    break;
                }

                // 내려갈 수 있는 블록이 있다면 빠져나옴
                if (!checkUpTile.GetComponent<Tile>().IsBlockEmpty())
                {
                    break;
                }

                // 생성 블록이라면 빠져나옴
                if (checkUpTile.GetComponent<Tile>().IsEmptyCreateTile())
                {
                    break;
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
            ETileType type = tile.GetComponent<Tile>().GetTileType();
            if (type == ETileType.IMMOVABLE)
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
        Debug.Log("못 움직임");
        // 클릭해서 움직이는 행동을 못하게 함
        m_clickOK = false;
    }
}
