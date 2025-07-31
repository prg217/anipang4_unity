using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using Random = UnityEngine.Random;

public class StageMgr : BaseMgr<StageMgr>
{
    #region 변수

    bool m_gameEnd =false;

    [Header("보드 등록")]
    [SerializeField]
    GameObject m_board;
    // 타일 최대 행렬
    Vector2Int m_maxMatrix = new Vector2Int(0, 0);
    // 맵에 있는 타일들
    Dictionary<Vector2Int, GameObject> m_tiles = new Dictionary<Vector2Int, GameObject>();

    #region Stage 설정 변수
    [Header("Stage 설정 변수")]
    [SerializeField]
    int m_maxBlockType = 5;
    [SerializeField]
    int m_maxMoveCount = 20;

    [SerializeField]
    SStageClearConditions m_stageClearConditions;
    #endregion

    #region Hint 관련 변수
    // 매치가 가능한 블록들 저장
    List<List<GameObject>> m_matchOKs = new List<List<GameObject>>();
    bool m_hint = false;
    bool m_hintStart = false;

    Coroutine m_hintCoroutine; // 힌트 코루틴

    bool m_waitingMoveComplete = false; // 스테이지 클리어, 게임 오버 시 움직임 완료
    #endregion

    #endregion 변수 끝

    #region Get함수
    public GameObject GetTile(in Vector2Int _matrix)
    { 
        if (m_tiles.ContainsKey(_matrix))
        {
            return m_tiles[_matrix];
        }

        return null;
    }
    public int GetMaxBlockType() { return m_maxBlockType; }
    public Vector2Int GetMaxMatrix() { return m_maxMatrix; }
    public Dictionary<EObstacleType, bool> GetClearObstacleTypes()
    {
        if (m_stageClearConditions.obstacleTypes != null)
        {
            Dictionary<EObstacleType, bool> types = new Dictionary<EObstacleType, bool>();

            for (int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
            {
                EObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
                bool clear = m_stageClearConditions.obstacleTypes[i].clear;
                types.Add(obstacleType, clear);
            }

            return types;
        }
        return null;
    }
    public Dictionary<EBlockType, bool> GetClearBlockTypes()
    {
        if (m_stageClearConditions.blockTypes != null)
        {
            Dictionary<EBlockType, bool> types = new Dictionary<EBlockType, bool>();

            for (int i = 0; i < m_stageClearConditions.blockTypes.Count; i++)
            {
                EBlockType blockType = m_stageClearConditions.blockTypes[i].type;
                bool clear = m_stageClearConditions.blockTypes[i].clear;
                types.Add(blockType, clear);
            }

            return types;
        }
        return null;
    }
    public GameObject GetRandomTile()
    {
        while (true)
        {
            Vector2Int randomMatrix = new Vector2Int(Random.Range(0, m_maxMatrix.x + 1), Random.Range(0, m_maxMatrix.y + 1));
            GameObject tile = GetTile(randomMatrix);
            // 유효 타일일 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() != EBlockType.NULL)
            {
                return tile;
            }
        }
    }

    public EBlockType GetMostNormalBlockType()
    {
        EBlockType mostType = EBlockType.NULL;
        int mostCount = 0;
        // 일반 블록 타입 별로 카운트
        for (int i = 0; i < (int)EBlockType.CROSS; i++)
        {
            int count = SearchTiles((EBlockType)i).Count;
            if (mostCount < count)
            {
                mostCount = count;
                mostType = (EBlockType)i;
            }
        }
        
        return mostType;
    }
    #endregion

    #region Set함수
    public void SetHint(in bool _hint)
    {
        m_hint = _hint;
        if (m_hint == false)
        {
            // 기존에 저장되어 있던 m_matchOKs 타일들에게 외곽선을 끄라고 한 뒤 초기화
            if (m_matchOKs.Count > 0)
            {
                foreach (List<GameObject> tiles in m_matchOKs)
                {
                    foreach (GameObject tile in tiles)
                    {
                        tile.GetComponent<Tile>().SetMyBlockSetOutline(false);
                    }
                }
            }
            m_matchOKs.Clear();
        }
    }
    #endregion

    #region 이벤트
    // 타일의 Explode가 실행될 때마다 어떤 타일이 터졌는지 누적
    void HandleTileExplode(EBlockType _type)
    {
        StageInfo.AddBlock(_type, 1);
    }
    #endregion

    protected override void OnAwake()
    {
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

        // m_stageInfo 변수 초기화
        StageInfo.Initialize(m_maxMoveCount);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 모든 타일의 이벤트 구독
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            Tile tileScript = tile.Value.GetComponent<Tile>();
            tileScript.OnTileExplode += HandleTileExplode;
        }

        // 시작 시 맵 체크(스테이지 블록 구성을 랜덤으로 했을 경우 대비)
        StartCheckPossibleMatch();

        // UI에 클리어 조건 넘겨줌
        UIMgr.Instance.UpdateStageClearConditions(m_stageClearConditions);

        // 스테이지 클리어 체크
        CheckStageClear();

        // BGM 재생
        // 추후 스테이지 추가 시 스테이지에 따라 할 수 있게 변경할 것
        SoundMgr.Instance.PlayBGM(EBGM.STAGE);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameEnd && m_hintCoroutine != null)
        {
            StopCoroutine(m_hintCoroutine);
            m_hintCoroutine = null;
            m_hintStart = false;
        }

        if (m_hint && !m_hintStart)
        {
            m_hintCoroutine = StartCoroutine(Hint());
        }
        else if (!m_hint && m_hintCoroutine != null)
        {
            // 힌트가 꺼지면 코루틴 중단
            StopCoroutine(m_hintCoroutine);
            m_hintCoroutine = null;
            m_hintStart = false;
        }
    }

    IEnumerator Hint()
    {
        m_hintStart = true;
        yield return new WaitForSeconds(8f);

        // 매치가 된다면 어디가 매치 되는지 저장했다가 몇 초 마다 알려주기
        if (m_matchOKs.Count > 0)
        {
            // 외곽선 실행
            int random = Random.Range(0, m_matchOKs.Count);

            foreach (GameObject tile in m_matchOKs[random])
            {
                tile.GetComponent<Tile>().SetMyBlockActiveOutline();
            }
        }
        yield return new WaitForSeconds(5f);
        m_hintStart = false;
    }

    public void StartCheckPossibleMatch()
    {
        StartCoroutine(CheckPossibleMatch());
    }

    // 움직여서 매치가 될 수 있나 확인
    IEnumerator CheckPossibleMatch()
    {
        m_matchOKs.Clear();

        // 움직일 수 있는 타일의 블록을 상하좌우로 움직인(임시로) 다음 매치가 되는지 체크
        // 모두 매치가 안 된다면 움직일 수 없는 타일을 제외하고 블록 타입 개수를 수집한 뒤, 랜덤으로 배분하고 블록을 바꿈
        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                var(result, matchOK) = MatchMgr.Instance.SimulateBlockMove(tile);

                if (result)
                {
                    if (matchOK != null)
                    {
                        matchOK.Add(tile);
                        m_hint = true;

                        // 힌트를 위해 매치 되는 블록의 정보를 저장함
                        if (matchOK.Count >= 3)
                        {
                            m_matchOKs.Add(new List<GameObject>(matchOK));
                        }
                    }
                }
            }
        }

        // 매치 불가능
        if (!m_hint)
        {
            UIMgr.Instance.RandomPlacementUI(true);
            yield return new WaitForSeconds(0.5f);
            RandomPlacement();
            yield return new WaitForSeconds(0.5f);
            UIMgr.Instance.RandomPlacementUI(false);
        }
    }

    // 무작위 배치
    void RandomPlacement()
    {
        List<GameObject> tiles = new List<GameObject>();
        List<EBlockType> blockTypes = new List<EBlockType>();
        List<EBlockType> saveBlockTypes = new List<EBlockType>();

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

                ETileType tileType = tile.GetComponent<Tile>().GetTileType(); 

                // 타일 타입이 움직일 수 있는 경우에만 저장
                if (tileType == ETileType.MOVABLE)
                {
                    // 움직일 수 있는 타일 저장
                    tiles.Add(tile);

                    // 블록 타입 저장
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();
                    blockTypes.Add(blockType);
                }
            }
        }
        saveBlockTypes = new List<EBlockType>(blockTypes);

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
                    blockTypes = new List<EBlockType>(saveBlockTypes);
                    loof = true;
                    break;
                }
            }
            loof = false;
        } while (loof);

        // 다시 움직여서 매치가 될 수 있는지 확인
        StartCheckPossibleMatch();
    }

    // 클리어 조건 확인
    public bool CheckStageClear()
    {
        // 하나라도 false로 설정되면 클리어 못함
        bool clear = true;

        // 현재 스테이지 내부의 정보를 갱신
        StageInfoUpdate();

        if (m_stageClearConditions.blockTypes != null)
        {
            for (int i = 0; i < m_stageClearConditions.blockTypes.Count; i++)
            {
                EBlockType blockType = m_stageClearConditions.blockTypes[i].type;
                int blockCount = m_stageClearConditions.blockTypes[i].count;

                if (StageInfo.GetBlockCount(blockType) < blockCount)
                {
                    clear = false;
                }
                else
                {
                    // 개별 클리어 조건 달성했는지 확인
                    var temp = m_stageClearConditions.blockTypes[i];
                    temp.clear = true;
                    m_stageClearConditions.blockTypes[i] = temp;
                }
            }
        }

        if (m_stageClearConditions.obstacleTypes != null)
        {
            for(int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
            {
                EObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
                int obstacleCount = m_stageClearConditions.obstacleTypes[i].count;

                if (StageInfo.GetObstacleCount(obstacleType) != obstacleCount)
                {
                    clear = false;
                }
                else
                {
                    // 개별 클리어 조건 달성했는지 확인
                    var temp = m_stageClearConditions.obstacleTypes[i];
                    temp.clear = true;
                    m_stageClearConditions.obstacleTypes[i] = temp;
                }
            }
        }

        // UI정보 갱신
        UIMgr.Instance.UpdateStageUI();

        // 클리어하면
        if (clear)
        {
            StartCoroutine(StageClear());
        }

        return clear;
    }

    IEnumerator StageClear()
    {
        if (m_gameEnd)
        { yield break; }

        m_gameEnd = true;

        // MoveMgr에서 클릭 판정 중단 요청
        MoveMgr.Instance.StopClickMoving();

        // 움직임이 있다면 움직임이 다 할 때까지 기다림
        m_waitingMoveComplete = true;
        // 이벤트 구독
        MoveMgr.Instance.OnEmptyMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr에서 빈 공간 채우기 함수 완료까지 대기
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI쪽에 클리어 UI 띄움
        UIMgr.Instance.StageClear(true);
        yield return new WaitForSeconds(1f);
        // 다시 화면으로 돌아오고
        UIMgr.Instance.StageClear(false);

        // 남은 moveCount 횟수에 따라 랜덤한 블록(특수 블록 제외)을 특수블록으로 만든 다음
        // 랜덤, 코즈믹 제외 특수 블록
        EBlockType[] selectableBlockTypes = { EBlockType.CROSS, EBlockType.SUN, EBlockType.MOON };
        // 중복 방지
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
        while (StageInfo.MoveCount > 0)
        {
            StageInfo.MoveCount--;

            while (true)
            {
                // 만약 모든 타일이 특수 블록or바꿀 수 없는 블록일 경우
                if (usedPositions.Count >= (m_maxMatrix.x * m_maxMatrix.y))
                {
                    // 바로 MoveCount 0으로 만들고 빠져나감
                    StageInfo.MoveCount = 0;
                    break;
                }

                // 랜덤한 타일 하나를 뽑음
                int x = Random.Range(0, m_maxMatrix.x + 1);
                int y = Random.Range(0, m_maxMatrix.y + 1);
                Vector2Int randomPos = new Vector2Int(x, y);

                // 이전과 중복되면 패스
                if (usedPositions.Contains(randomPos))
                {
                    continue;
                }
                usedPositions.Add(randomPos);

                GameObject tile = m_tiles[randomPos];
                EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();

                // 타일이 일반 블록인지 확인
                if (type > EBlockType.NONE && type < EBlockType.CROSS)
                {
                    // 랜덤한 특수 블록으로 변환(랜덤, 코즈믹 제외)
                    EBlockType randomBlockType = selectableBlockTypes[Random.Range(0, selectableBlockTypes.Length)];
                    tile.GetComponent<Tile>().SetMyBlockType(randomBlockType);
                    break;
                }
            }

            yield return new WaitForSeconds(0.15f);
        }

        // 특수블록들을 터트림
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            EBlockType type = tile.Value.GetComponent<Tile>().GetMyBlockType();

            if (type != EBlockType.NULL && type >= EBlockType.CROSS)
            {
                tile.Value.GetComponent<Tile>().Explode(EObstacleType.NONE);
                MoveMgr.Instance.ActiveCheckEmpty();
                yield return new WaitForSeconds(0.5f);
            }
        }

        // 다 터트리고 스테이지 결과
        UIMgr.Instance.ClearResult();
        Debug.Log("스테이지 클리어");
    }

    IEnumerator GameOver()
    {
        if (m_gameEnd)
        { yield break; }

        m_gameEnd = true;

        // MoveMgr에서 클릭 판정 중단 요청
        MoveMgr.Instance.StopClickMoving();

        // 움직임이 있다면 움직임이 다 할 때까지 기다림
        m_waitingMoveComplete = true;
        // 이벤트 구독
        MoveMgr.Instance.OnEmptyMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr에서 빈 공간 채우기 함수 완료까지 대기
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI쪽에 게임오버 UI 띄움
        UIMgr.Instance.GameOver();
    }

    // MoveMgr에서 빈 공간 채우기 함수 완료
    void OnMoveCompleted()
    {
        m_waitingMoveComplete = false;
        // 구독 해제
        MoveMgr.Instance.OnEmptyMoveCompleteFunction -= OnMoveCompleted;
    }

    // 스테이지 정보 갱신
    void StageInfoUpdate()
    {
        // 장애물 개수 초기화
        foreach (EObstacleType type in Enum.GetValues(typeof(EObstacleType)))
        {
            StageInfo.ResetObstacle();
        }

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                // 장애물 개수
                EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();
                StageInfo.AddObstacle(frontObstacleType, 1);
                EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                StageInfo.AddObstacle(backObstacleType, 1);
            }
        }
    }

    // 조건을 입력하고 해당하는 타일들을 반환
    public List<GameObject> SearchTiles(EBlockType _blockType = EBlockType.NONE, EObstacleType _obstacleType = EObstacleType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // 블록 타입이 조건에 맞는지
                if (_blockType != EBlockType.NONE)
                {
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // 장애물 타입이 조건에 맞는지
                if (_obstacleType != EObstacleType.NONE)
                {
                    EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                    if (backObstacleType != _obstacleType && frontObstacleType != _obstacleType)
                    {
                        continue;
                    }
                }

                // 다 조건에 맞는다면 저장
                tiles.Add(tile);
            }
        }

        return tiles;
    }
    public List<GameObject> SearchTiles(EBlockType _blockType = EBlockType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // 블록 타입이 조건에 맞는지
                if (_blockType != EBlockType.NONE)
                {
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // 다 조건에 맞는다면 저장
                tiles.Add(tile);
            }
        }

        return tiles;
    }
    public List<GameObject> SearchTiles(EObstacleType _obstacleType)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // 장애물 타입이 조건에 맞는지
                if (_obstacleType != EObstacleType.NONE)
                {
                    EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                    if (backObstacleType != _obstacleType && frontObstacleType != _obstacleType)
                    {
                        continue;
                    }
                }

                // 다 조건에 맞는다면 저장
                tiles.Add(tile);
            }
        }

        return tiles;
    }
    // 제외하고 검색
    public List<GameObject> SearchTilesExcept(EObstacleType _obstacleType)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // 장애물 타입이 조건에 맞는지
                if (_obstacleType != EObstacleType.NONE)
                {
                    if (_obstacleType < EObstacleType.FRONT_END)
                    {
                        EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                        if (frontObstacleType != _obstacleType)
                        {
                            tiles.Add(tile);
                        }
                    }
                    else
                    {
                        EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();

                        if (backObstacleType != _obstacleType)
                        {
                            tiles.Add(tile);
                        }
                    }
                }
            }
        }

        return tiles;
    }

    public void CheckGameOver()
    {
        // 만약 moveCount가 0이 되었는데, 클리어 조건을 만족하지 못하면 게임 오버
        if (StageInfo.MoveCount <= 0)
        {
            StageInfo.MoveCount = 0;

            // 클리어 체크
            if (CheckStageClear() == false)
            {
                StartCoroutine(GameOver());
            }
        }
    }

    protected override void OnDestroyed()
    {
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            if (tile.Value != null)
            {
                Tile tileScript = tile.Value.GetComponent<Tile>();
                tileScript.OnTileExplode -= HandleTileExplode;
            }
        }
    }
}
