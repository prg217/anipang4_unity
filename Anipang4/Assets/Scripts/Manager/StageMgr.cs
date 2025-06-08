using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;
using System.Collections;
using Unity.Burst.CompilerServices;
using System;

using Random = UnityEngine.Random;
using System.Runtime.ConstrainedExecution;
using System.Reflection;

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
    StageClearConditions m_stageClearConditions;
    #endregion

    #region Hint 관련 변수
    // 매치가 가능한 블록들 저장
    List<GameObject> m_matchOK = new List<GameObject>();
    List<List<GameObject>> m_matchOKs = new List<List<GameObject>>();
    bool m_hint = false;
    bool m_hintStart = false;

    Coroutine m_hintCoroutine; // 힌트 코루틴

    bool m_waitingMoveComplete = false; // 스테이지 클리어, 게임 오버 시 움직임 완료
    #endregion

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
    public Dictionary<ObstacleType, bool> GetClearObstacleTypes()
    {
        if (m_stageClearConditions.obstacleTypes != null)
        {
            Dictionary<ObstacleType, bool> types = new Dictionary<ObstacleType, bool>();

            for (int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
            {
                ObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
                bool clear = m_stageClearConditions.obstacleTypes[i].clear;
                types.Add(obstacleType, clear);
            }

            return types;
        }
        return null;
    }
    public Dictionary<BlockType, bool> GetClearBlockTypes()
    {
        if (m_stageClearConditions.blockTypes != null)
        {
            Dictionary<BlockType, bool> types = new Dictionary<BlockType, bool>();

            for (int i = 0; i < m_stageClearConditions.blockTypes.Count; i++)
            {
                BlockType blockType = m_stageClearConditions.blockTypes[i].type;
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
            Vector2Int randomMatrix = new Vector2Int(Random.Range(0, m_maxMatrix.x), Random.Range(0, m_maxMatrix.y));
            GameObject tile = GetTile(randomMatrix);
            // 유효 타일일 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() != BlockType.NULL)
            {
                return tile;
            }
        }
    }
    #endregion

    #region Set함수
    public void SetMatchOK(in List<GameObject> _matchOK)
    {
        m_matchOK = new List<GameObject>(_matchOK);
    }
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
    void HandleTileExplode(BlockType _type)
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
        // 블록 종류 등록
        foreach (BlockType type in Enum.GetValues(typeof(BlockType)))
        {
            //m_stageInfo.blockCounts.Add((BlockType)type, 0);
        }

        // 모든 타일의 이벤트 구독
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            Tile tileScript = tile.Value.GetComponent<Tile>();
            tileScript.OnTileExplode += HandleTileExplode;
        }

        // 장애물 종류 등록
        foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
        {
            //m_stageInfo.obstacleCounts.Add((ObstacleType)type, 0);
        }

        // 시작 시 맵 체크(스테이지 블록 구성을 랜덤으로 했을 경우 대비)
        CheckPossibleMatch();

        // UI에 클리어 조건 넘겨줌
        UIMgr.Instance.UpdateStageClearConditions(m_stageClearConditions);

        // 스테이지 클리어 체크
        CheckStageClear();
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

    // 움직여서 매치가 될 수 있나 확인
    public void CheckPossibleMatch()
    {
        m_matchOKs.Clear();

        // 움직일 수 있는 타일의 블록을 상하좌우로 움직인(임시로) 다음 매치가 되는지 체크
        // 매치가 하나라도 된다면 바로 return
        // 모두 매치가 안 된다면 움직일 수 없는 타일을 제외하고 블록 타입 개수를 수집한 뒤, 랜덤으로 배분하고 블록을 바꿈
        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            m_matchOK.Clear();

            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (MatchMgr.Instance.SimulateBlockMove(tile))
                {
                    m_matchOK.Add(tile);
                    m_hint = true;
                }
            }

            if (m_matchOK.Count >= 3/* 또는 특수 블록 */)
            {
                m_matchOKs.Add(new List<GameObject>(m_matchOK));
            }
        }

        // 매치 불가능
        if (!m_hint)
        {
            Debug.Log("매치 불가능");
            RandomPlacement();
        }
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
                BlockType blockType = m_stageClearConditions.blockTypes[i].type;
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
                ObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
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
        MoveMgr.Instance.OnMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr에서 빈 공간 채우기 함수 완료까지 대기
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // 움직임이 있다면 움직임이 다 할 때까지 기다림

        // UI쪽에 클리어 UI 띄움

        // 다시 화면으로 돌아오고

        // 남은 moveCount 횟수에 따라 랜덤한 블록(특수 블록 제외)을 특수블록으로 만든 다음

        // 특수블록들을 터트림
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
        MoveMgr.Instance.OnMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr에서 빈 공간 채우기 함수 완료까지 대기
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI쪽에 게임오버 UI 띄움
        Debug.Log("게임오버");
    }

    // MoveMgr에서 빈 공간 채우기 함수 완료
    void OnMoveCompleted()
    {
        m_waitingMoveComplete = false;
        // 구독 해제
        MoveMgr.Instance.OnMoveCompleteFunction -= OnMoveCompleted;
    }

    // 스테이지 정보 갱신
    void StageInfoUpdate()
    {
        // 장애물 개수 초기화
        foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
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
                ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();
                StageInfo.AddObstacle(frontObstacleType, 1);
                ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                StageInfo.AddObstacle(backObstacleType, 1);
            }
        }
    }

    // 조건을 입력하고 해당하는 타일들을 반환
    public List<GameObject> SearchTiles(BlockType _blockType = BlockType.NONE, ObstacleType _obstacleType = ObstacleType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                // 블록 타입이 조건에 맞는지
                if (_blockType != BlockType.NONE)
                {
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // 장애물 타입이 조건에 맞는지
                if (_obstacleType != ObstacleType.NONE)
                {
                    ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

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
    public List<GameObject> SearchTiles(BlockType _blockType = BlockType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                // 블록 타입이 조건에 맞는지
                if (_blockType != BlockType.NONE)
                {
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

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
    public List<GameObject> SearchTiles(ObstacleType _obstacleType)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                // 장애물 타입이 조건에 맞는지
                if (_obstacleType != ObstacleType.NONE)
                {
                    ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

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
