using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MatchMgr : BaseMgr<MatchMgr>
{
    #region 변수

    [Header("프리팹 등록")]
    [SerializeField]
    GameObject m_chasingMoonPrefab; // 추격하는 달 프리팹 등록

    Vector2Int m_targetMatrix;
    BlockType m_targetType;
    GameObject m_targetTile;

    int m_matchCount = 1; // 본인 포함으로 계산
    int m_saveMatchCount = 1;
    List<GameObject> m_matchTiles = new List<GameObject>(); // 매치가 되는 타일들 저장(후에 터트림)
    List<GameObject> m_saveMatchTiles = new List<GameObject>();

    BlockType m_newBlock = BlockType.NONE; // 특수 블록의 조건에 맞을 경우 생성될 블럭
    ObstacleType m_contagiousObstacle = ObstacleType.NONE; // 타일에 추가 될 장애물

    int m_randomExplodeCompleteCount = 0;

    #endregion 변수 끝

    public int GetMatchCount() { return m_matchCount; }
    public List<GameObject> GetMatchTiles() { return m_matchTiles; }

    void SetTargetTile(in GameObject _targetTile)
    {
        m_targetTile = _targetTile;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CheckMatch(in GameObject _tile, in bool _explode = true)
    {
        if (_tile == null)
        {
            return false;
        }

        /* 
        ! 다른 타입의 블록이 나올 경우 바로 검사를 빠져나올 것    

            우선순위 별로 처리
        ~상하좌우 2블록씩 검사~(본인 포함)
        1. 총 7개 이상->COSMIC
        2. 총 5개 이상(상하, 좌우 둘 중 하나)->RANDOM
        3. 총 5개 이상(위에 포함이 안 되는 경우)->SUN
        4. 총 4개 이상(상하, 좌우 둘 중 하나)->CROSS
        5. 총 4개 이상(대각선 검사)->MOON
        6. 총 3개 이상->기본
        */

        #region 초기화
        m_matchTiles.Clear();
        m_saveMatchTiles.Clear();
        m_matchCount = 1;
        m_saveMatchCount = 1;
        m_newBlock = BlockType.NONE;
        #endregion

        #region 타겟 타일 변수 세팅
        if (_explode)
        {
            SetTargetTile(_tile);
            m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
            m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();
        }
        #endregion

        // 타입이 비어 있는 경우 return
        if (m_targetType == BlockType.NONE)
        {
            return false;
        }

        // 특수 블록인 경우 특수 블록을 바로 터트림
        if (m_targetType >= BlockType.CROSS && m_targetType != BlockType.NULL)
        {
            // 특수 블록 터트림
            if (_explode)
            {
                Explode(true);
            }

            return true;
        }

        // 상하 검사
        UpDownInspect();
        // 특수 블록 조건 맞으면 일단 저장
        m_saveMatchCount = m_matchCount;
        m_saveMatchTiles.Clear();
        m_saveMatchTiles = new List<GameObject>(m_matchTiles);
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = BlockType.CROSS;
                break;
            case 5:
                m_newBlock = BlockType.RANDOM;
                break;
            default:
                break;
        }

        // 좌우 검사
        LeftRightInspect();
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = BlockType.CROSS;
                break;
            case 5:
                if (m_saveMatchCount >= 3 && m_saveMatchCount < 5)
                {
                    m_newBlock = BlockType.SUN;
                }
                else
                {
                    m_newBlock = BlockType.RANDOM;
                }
                break;
            case 6:
                m_newBlock = BlockType.SUN;
                break;
            case 7:
                m_newBlock = BlockType.COSMIC;
                break;
            default:
                break;
        }

        if (_explode && m_matchCount >= 3)
        {
            Debug.Log("count : " + m_matchCount);
            Debug.Log("터지는 타입 : " + m_newBlock);
        }

        if (m_newBlock >= BlockType.CROSS)
        {
            // 특수 블록 생성 조건 만족함
            if (_explode)
            {
                Explode(false);
            }
            else
            {
                StageMgr.Instance.SetMatchOK(m_matchTiles);
            }
            return true;
        }

        // 만약 특수 블록에 해당되지 않는다면 MOON 검사 실시
        if (m_newBlock == BlockType.NONE)
        {
            if (MoonInspect())
            {
                // 터트리는 함수
                if (_explode)
                {
                    Explode(false);
                }
                else
                {
                    StageMgr.Instance.SetMatchOK(m_matchTiles);
                }
                return true;
            }
        }

        // MOON의 조건에도 되지 않는다면 기본으로 터트림
        if (m_matchCount == 3)
        {
            // 터트리는 함수
            if (_explode)
            {
                Explode(false);
            }
            else
            {
                StageMgr.Instance.SetMatchOK(m_matchTiles);
            }
            return true;
        }

        return false;
    }

    void UpDownInspect()
    {
        for (int i = 1; i <= 2; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.y = m_targetMatrix.y + i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                break;
            }

            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            if (type == m_targetType)
            {
                m_matchTiles.Add(tile);
                m_matchCount++;
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i <= 2; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.y = m_targetMatrix.y - i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                break;
            }

            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            if (type == m_targetType)
            {
                m_matchTiles.Add(tile);
                m_matchCount++;
            }
            else
            {
                break;
            }
        }

        // 매치가 안 되는 상황이라면 m_matchTiles를 이전 상태로 돌림
        if (m_matchCount < 3 && m_matchCount > 1)
        {
            m_matchTiles.Clear();
            m_matchCount = 1;
        }
    }

    void LeftRightInspect()
    {
        int count = 1;
        for (int i = 1; i <= 2; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.x = m_targetMatrix.x - i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                break;
            }

            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            if (type == m_targetType)
            {
                m_matchTiles.Add(tile);
                m_matchCount++;
                count++;
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i <= 2; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.x = m_targetMatrix.x + i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                break;
            }

            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            if (type == m_targetType)
            {
                m_matchTiles.Add(tile);
                m_matchCount++;
                count++;
            }
            else
            {
                break;
            }
        }

        // 매치가 안 되는 상황이라면 m_matchTiles를 이전 상태로 돌림
        if (count < 3 && count > 1)
        {
            if (m_matchCount > 1)
            {
                m_matchTiles = new List<GameObject>(m_saveMatchTiles);
            }
            m_matchCount = m_saveMatchCount;
        }
    }

    bool MoonInspect()
    {
        /*
            1. 대각선 왼쪽 위, 오른쪽 위, 왼쪽 아래, 오른쪽 아래에 동일한 타입이 있는지 검사
            2. 동일한 타입이 있을 시 왼쪽 위는 왼쪽과 위를 검사 *** 동일하게 나머지도 검사
            3. 만약 m_matchCount가 3이라면 그거 포함해서 터트리고(MOON인데 터트릴 것이 5개인 경우), 아니라면 여기서 추가 검사한 것만 터트림(4개)
        */

        /*
         * 대각선 왼쪽 위, 오른쪽 위, 왼쪽 아래, 오른쪽 아래에 동일한 타입이 있는지 검사
         * 동일한 타입이 있을 시 왼쪽 위는 왼쪽과 위를 검사 *** 동일하게 나머지도 검사
         * + 이제 본인을 기준으로 3개 매치가 되는게 있는지도 검사(5개 일 경우)
         */

        // 오른쪽 아래
        if (Inspect(new Vector2Int(1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(1, 1)))
            {
                return true;
            }
        }
        // 왼쪽 위
        if (Inspect(new Vector2Int(-1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, 1)))
            {
                return true;
            }
        }
        // 오른쪽 위
        if (Inspect(new Vector2Int(1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(1, -1)))
            {
                return true;
            }
        }
        // 왼쪽 위
        if (Inspect(new Vector2Int(-1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, -1)))
            {
                return true;
            }
        }

        return false;
    }

    // _AddMatrix만큼 합한 행렬의 타일의 타입 검사
    bool Inspect(in Vector2Int _AddMatrix)
    {
        Vector2Int matrix = m_targetMatrix;

        matrix += _AddMatrix;
        GameObject tile = StageMgr.Instance.GetTile(matrix);
        if (tile == null)
        {
            return false;
        }

        BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
        if (type == m_targetType)
        {
            return true;
        }
        return false;
    }

    // 대각선 타일이 동일한 타입일 때 양옆 추가 검사
    bool MoonAddInspect(in Vector2Int _AddMatrix)
    {
        if (Inspect(new Vector2Int(_AddMatrix.x, 0)) == true && Inspect(new Vector2Int(0, _AddMatrix.y)) == true)
        {
            m_newBlock = BlockType.MOON;

            Vector2Int matrix = m_targetMatrix;
            matrix += _AddMatrix;
            GameObject tile = StageMgr.Instance.GetTile(matrix);
            m_matchTiles.Add(tile);

            // 중복 검사 후 매치 타일들에 넣어주기
            matrix = m_targetMatrix;
            matrix += new Vector2Int(_AddMatrix.x, 0);
            tile = StageMgr.Instance.GetTile(matrix);
            if (!m_matchTiles.Contains(tile))
            {
                m_matchTiles.Add(tile);
            }

            matrix = m_targetMatrix;
            matrix += new Vector2Int(0, _AddMatrix.y);
            tile = StageMgr.Instance.GetTile(matrix);
            if (!m_matchTiles.Contains(tile))
            {
                m_matchTiles.Add(tile);
            }

            return true;
        }

        return false;
    }

    void Explode(in bool _isSpecial)
    {
        // 로그 추가
        LogMgr.Instance.AddMatchLog(m_targetType, m_targetTile, m_matchTiles, m_newBlock);

        // 미리 터질 타일에 신호
        m_targetTile.GetComponent<Tile>().StartExplodeEffect();
        foreach (GameObject tile in m_matchTiles)
        {
            if (tile.GetComponent<Tile>().GetMyBlockType() != BlockType.NULL)
            {
                tile.GetComponent<Tile>().StartExplodeEffect();
            }
        }

        // 캡쳐 로그 추가
        LogMgr.Instance.CaptureLog();

        // 매치되는 타일 중 전파되는 장애물이 있는지 확인
        if (!_isSpecial)
        {
            m_contagiousObstacle = ObstacleType.NONE;
            ObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();

            if (obstacleType != ObstacleType.NONE)
            {
                m_contagiousObstacle = obstacleType;
            }

            foreach (GameObject tile in m_matchTiles)
            {
                obstacleType = tile.GetComponent<Tile>().GetPropagationObstacle();
                // 있다면 전파되는 장애물로 장애물을 추가하게 함
                if (obstacleType != ObstacleType.NONE)
                {
                    m_contagiousObstacle = obstacleType;
                }
            }
        }

        // 특수 블록 생성 조건에 해당 될 경우
        if (m_newBlock >= BlockType.CROSS)
        {
            m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle, m_newBlock);
        }
        // 아닌 경우 터트림
        else
        {
            m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle);
        }

        // m_matchTiles에 등록된 타일들을 터트림
        foreach (GameObject tile in m_matchTiles)
        {
            if (tile.GetComponent<Tile>().GetMyBlockType() != BlockType.NULL)
            {
                tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
            }
        }
    }

    public bool SimulationMatch(in GameObject _originalTile, in GameObject _changeTile)
    {
        // 테스트할 타입으로 매치 실행
        m_targetType = _originalTile.GetComponent<Tile>().GetMyBlockType();

        BlockType saveType = _changeTile.GetComponent<Tile>().GetMyBlockType();
        _originalTile.GetComponent<Tile>().SetMyBlockType(saveType);

        SetTargetTile(_changeTile);
        m_targetMatrix = _changeTile.GetComponent<Tile>().GetMatrix();

        if (CheckMatch(_changeTile, false))
        {
            _originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
            return true;
        }
        _originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
        return false;
    }

    public bool SimulateBlockMove(in GameObject _tile)
    {
        // 상하좌우로 이동시켜서 매치가 되는지 테스트
        if (_tile == null)
        {
            return false;
        }

        // 움직일 수 없는 타일 일 경우 매치 시도 불가능
        if (_tile.GetComponent<Tile>().GetTileType() == TileType.IMMOVABLE)
        {
            return false;
        }

        // 빈 블록일 경우 매치 시도 불가능
        if (_tile.GetComponent<Tile>().GetMyBlockType() == BlockType.NONE)
        {
            return false;
        }

        // 특수 블록일 경우 매치 가능
        if (_tile.GetComponent<Tile>().GetMyBlockType() >= BlockType.CROSS)
        {
            return true;
        }

        Vector2Int matrix = _tile.GetComponent<Tile>().GetMatrix();
        // 블록 타입을 고정
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();

        Vector2Int upMatrix = new Vector2Int(matrix.x, matrix.y - 1);
        if (SimulationInspect(matrix, upMatrix))
        {
            return true;
        }

        Vector2Int downMatrix = new Vector2Int(matrix.x, matrix.y + 1);
        if (SimulationInspect(matrix, downMatrix))
        {
            return true;
        }

        Vector2Int leftMatrix = new Vector2Int(matrix.x - 1, matrix.y);
        if (SimulationInspect(matrix, leftMatrix))
        {
            return true;
        }

        Vector2Int rightMatrix = new Vector2Int(matrix.x + 1, matrix.y);
        if (SimulationInspect(matrix, rightMatrix))
        {
            return true;
        }

        return false;
    }

    bool SimulationInspect(in Vector2Int _originalMatrix, in Vector2Int _changeMatrix)
    {
        // 본인이 바뀌면 그 바뀐 타일의 타입도 다르게 해줘야 함!
        // 즉, 일시적으로만 서로 타입을 바꾼 채로 두고 return 후에 다시 돌려놔야 함
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);
        GameObject changeTile = StageMgr.Instance.GetTile(_changeMatrix);

        if (changeTile != null)
        {
            if (changeTile.GetComponent<Tile>().GetTileType() == TileType.MOVABLE)
            {
                return SimulationMatch(originalTile, changeTile);
            }
        }

        return false;
    }

    public void SpecialExplode(in GameObject _tile ,in BlockType _blockType)
    {
        m_targetTile = _tile;
        m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _blockType;

        m_newBlock = BlockType.NONE;
        m_matchTiles.Clear();

        // 매치되는 타일 중 전파되는 장애물이 있는지 확인
        m_contagiousObstacle = ObstacleType.NONE;

        // 앞 장애물이 없을 경우
        if (m_targetTile.GetComponent<Tile>().GetMyFrontObstacleType() == ObstacleType.NONE)
        {
            ObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();
            if (obstacleType != ObstacleType.NONE)
            {
                m_contagiousObstacle = obstacleType;
            }
        }

        switch (m_targetType)
        {
            case BlockType.CROSS:
                CrossExplode();
                break;
            case BlockType.SUN:
                SunExplode();
                break;
            case BlockType.RANDOM:
                RandomMatch(m_targetTile, BlockType.NONE, m_contagiousObstacle);
                break;
            case BlockType.COSMIC:
                CosmicExplode();
                break;
            case BlockType.MOON:
                MoonExplode();
                break;
            case BlockType.DOUBLE_CROSS:
                DoubleCrossExplode();
                break;
            case BlockType.CROSS_SUN:
                CrossSunExplode();
                break;
            case BlockType.CROSS_MOON:
                SpecialMoonExplode(BlockType.CROSS);
                break;
            case BlockType.DOUBLE_SUN:
                DoubleSunExplode();
                break;
            case BlockType.SUN_MOON:
                SpecialMoonExplode(BlockType.SUN);
                break;
            case BlockType.DOUBLE_MOON:
                SpecialMoonExplode(BlockType.MOON);
                break;
            case BlockType.DOUBLE_RANDOM:
                CosmicExplode();
                break;
            case BlockType.RANDOM_CROSS:
                RandomMatch(m_targetTile, BlockType.CROSS, m_contagiousObstacle);
                break;
            case BlockType.RANDOM_SUN:
                RandomMatch(m_targetTile, BlockType.SUN, m_contagiousObstacle);
                break;
            case BlockType.RANDOM_MOON:
                RandomMatch(m_targetTile, BlockType.MOON, m_contagiousObstacle);
                break;
            default:
                break;
        }
    }

    public void SpecialCompositionExplode(in GameObject _tile1, in GameObject _tile2, in ObstacleType _contagiousObstacle)
    {
        m_contagiousObstacle = _contagiousObstacle;

        BlockType type1 = _tile1.GetComponent<Tile>().GetMyBlockType();
        BlockType type2 = _tile2.GetComponent<Tile>().GetMyBlockType();

        SetTargetTile(_tile2);
        m_targetMatrix = _tile2.GetComponent<Tile>().GetMatrix();

        _tile1.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        switch (type1)
        {
            case BlockType.CROSS:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.DOUBLE_CROSS);
                            break;
                        case BlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.CROSS_SUN);
                            break;
                        case BlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_CROSS);
                            break;
                        case BlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                            break;
                        case BlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.CROSS_MOON);
                            break;
                        default:
                            break;
                    }
                }                
                break;
            case BlockType.SUN:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.CROSS_SUN);
                            break;
                        case BlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.DOUBLE_SUN);
                            break;
                        case BlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_SUN);
                            break;
                        case BlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                            break;
                        case BlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.SUN_MOON);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case BlockType.RANDOM:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_CROSS);
                            break;
                        case BlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_SUN);
                            break;
                        case BlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_MOON);
                            break;
                        case BlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                            break;
                        case BlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case BlockType.COSMIC:
                {
                    _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                }
                break;
            case BlockType.MOON:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.CROSS_MOON);
                            break;
                        case BlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.SUN_MOON);
                            break;
                        case BlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.RANDOM_MOON);
                            break;
                        case BlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.COSMIC);
                            break;
                        case BlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(BlockType.DOUBLE_MOON);
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }

        m_targetType = _tile2.GetComponent<Tile>().GetMyBlockType();
        _tile2.GetComponent<Tile>().Explode(_contagiousObstacle);
    }

    void DoubleCrossExplode()
    {
        // 가로 세로 3줄씩
        LengthAndWidthExplode(1, 1);
    }

    void CrossSunExplode()
    {
        // 가로 세로 5줄씩
        LengthAndWidthExplode(2, 2);
    }

    void DoubleSunExplode()
    {
        // 9x9
        SurroundingsExplode(4, 4);
    }

    void SpecialMoonExplode(in BlockType _specialType)
    {
        // 주변 8곳 터트림
        SurroundingsExplode(1, 1);

        #region 클리어 조건 중 하나 랜덤으로 가서 파괴
        // 달 추격 프리팹 소환
        if (_specialType == BlockType.MOON)
        {
            SummonChasingMoon(_specialType);
            SummonChasingMoon(_specialType);
            SummonChasingMoon(_specialType);
        }
        else
        {
            SummonChasingMoon(_specialType);
        }
        #endregion
    }

    void SummonChasingMoon(in BlockType _specialType)
    {
        GameObject chasingMoon = Instantiate(m_chasingMoonPrefab, m_targetTile.transform.position, m_targetTile.transform.rotation);
        chasingMoon.GetComponent<ChasingMoon>().SetMyTile(m_targetTile);

        chasingMoon.GetComponent<ChasingMoon>().SetBlockType(_specialType);
        chasingMoon.GetComponent<ChasingMoon>().SetContagiousObstacleType(m_contagiousObstacle);
    }

    void CrossExplode()
    {
        LengthAndWidthExplode(0, 0);
    }

    void SunExplode()
    {
        // 5x5 파괴
        SurroundingsExplode(2, 2);
    }

    public void RandomMatch(in GameObject _targetTile ,in BlockType _randomType, in ObstacleType _contagiousObstacle)
    {
        SetTargetTile(_targetTile);
        m_contagiousObstacle = _contagiousObstacle;

        StartCoroutine(RandomExplode(_randomType));
    }

    IEnumerator RandomExplode(BlockType _type)
    {
        // 교차 시킨 블록 타입 정보를 알고, 그 타입들을 서치해서 제거
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        m_randomExplodeCompleteCount = 0;
        List<GameObject> explodeTiles = new List<GameObject>();

        m_targetTile.GetComponent<Tile>().SetRandomExecute(true);

        // 랜덤 진행 중에는 빈 공간 채우기 잠시 멈추기... 다시 실행 신호 보낼 때 까지
        MoveMgr.Instance.SetCheckEmptyEnabled(false);

        #region 단독으로 실행 됐을 경우
        if (_type == BlockType.NONE)
        {
            // 일반 블록 중 하나 랜덤으로 지정해주기
            int randomType = Random.Range(0, StageMgr.Instance.GetMaxBlockType());
            _type = (BlockType)randomType;
        }
        #endregion

        #region 일반 블록일 경우
        if (_type < BlockType.CROSS)
        {
            for (int x = 0; x <= maxMatrix.x; x++)
            {
                for (int y = 0; y <= maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if (type == _type)
                    {
                        // 해당되는 블록 흔들리는 효과
                        explodeTiles.Add(tile);
                        tile.GetComponent<Tile>().RandomEffect(true);

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        #endregion

        #region 특수 블록일 경우
        if (_type >= BlockType.CROSS)
        {
            if (_type == BlockType.RANDOM || _type == BlockType.COSMIC)
            {
                CosmicExplode();
            }

            // 제일 많은 블록을 해당하는 특수 블록으로 변경 후 터트림
            BlockType mostType = StageMgr.Instance.GetMostNormalBlockType();

            for (int x = 0; x <= maxMatrix.x; x++)
            {
                for (int y = 0; y <= maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if (type == mostType)
                    {
                        // 해당하는 특수 블록으로 전환
                        // Front장애물 안에 있는 경우 일반 블록 처럼 처리
                        if (tile.GetComponent<Tile>().GetFrontObstacleEmpty())
                        {
                            tile.GetComponent<Tile>().SetMyBlockType(_type);
                        }

                        // 해당되는 블록 흔들리는 효과
                        explodeTiles.Add(tile);
                        tile.GetComponent<Tile>().RandomEffect(true);

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        #endregion

        // 다 끝나면 끝났다는 사인 보냄 해당되는 블록들에게 동시에 보냄
        foreach (GameObject tile in explodeTiles)
        {
            tile.GetComponent<Tile>().SetRandomExplode(true);
            tile.GetComponent<Tile>().RandomEffect(false);
            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
        }

        // 다 터지는 동안 대기하고 있던 랜덤 블록을 없애줌
        m_targetTile.GetComponent<Tile>().SetRandomComplete(true);

        // explodeTiles.Count해서 그 개수만큼 완료 신호 돌아오면 진행됨
        yield return new WaitUntil(() => m_randomExplodeCompleteCount == explodeTiles.Count);

        // 다시 빈공간 채우기 활성화
        MoveMgr.Instance.SetCheckEmptyEnabled(true);
        MoveMgr.Instance.StartCheckEmpty();
    }

    public void RandomExplodeComplete()
    {
        m_randomExplodeCompleteCount++;
    }

    void CosmicExplode()
    {
        // 전부 제거
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        for (int i = 0; i <= maxMatrix.x; i++)
        {
            for (int j = 0; j <= maxMatrix.y; j++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(i, j));
                if (tile != null)
                {
                    m_matchTiles.Add(tile);
                }
            }
        }

        Explode(true);
    }

    void MoonExplode()
    {
        // 십자칸 파괴 후 클리어 조건 중 하나 랜덤으로 가서 파괴
        #region 십자칸 파괴
        // 중복 방지
        m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        // 상하좌우 파괴
        GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x - 1, m_targetMatrix.y));
        if (tile != null)
        {
            m_matchTiles.Add(tile);
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + 1, m_targetMatrix.y));
        if (tile != null)
        {
            m_matchTiles.Add(tile);
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y - 1));
        if (tile != null)
        {
            m_matchTiles.Add(tile);
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y + 1));
        if (tile != null)
        {
            m_matchTiles.Add(tile);
        }
        #endregion

        Explode(true);

        #region 클리어 조건 중 하나 랜덤으로 가서 파괴
        // 달 추격 프리팹 소환
        SummonChasingMoon(BlockType.NONE);
        #endregion
    }

    // 주변 터트림 : Sun 관련 함수에서 사용, 특수 블록 합성 Moon에서도 사용
    void SurroundingsExplode(in int _x, in int _y) 
    {
        // 중복 터짐 방지
        m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        for (int x = -_x; x <= _x; x++)
        {
            for (int y = -_y; y <= _y; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, m_targetMatrix.y + y));
                if (tile != null && m_targetTile != tile)
                {
                    m_matchTiles.Add(tile);
                }
            }
        }

        Explode(true);
    }

    // 가로세로 터트림 : Cross 관련 함수에서 사용
    void LengthAndWidthExplode(in int _x, in int _y)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // 중복 터짐 방지
        m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        for (int y = -_y; y <= _y; y++)
        {
            for (int x = 0; x <= maxMatrix.x; x++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, m_targetMatrix.y + y));

                if (tile != null && m_targetTile != tile)
                {
                    // 이미 처리한 타일 건너뜀
                    if (m_matchTiles.Contains(tile))
                    {
                        continue;
                    }

                    m_matchTiles.Add(tile);
                }
            }
        }

        for (int x = -_x; x <= _x; x++)
        {
            for (int y = 0; y <= maxMatrix.y; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, y));

                if (tile != null && m_targetTile != tile)
                {
                    // 이미 처리한 타일 건너뜀
                    if (m_matchTiles.Contains(tile))
                    {
                        continue;
                    }

                    m_matchTiles.Add(tile);
                }
            }
        }

        Explode(true);
    }

    public void ChasingMoonExplode(in GameObject _tile, in ObstacleType _contagiousObstacleType = ObstacleType.NONE, in BlockType _explodeType = BlockType.NONE)
    {
        LogMgr.Instance.ChasingMoonExplodeLog(_tile, _explodeType);

        _tile.GetComponent<Tile>().StartExplodeEffect();
        LogMgr.Instance.CaptureLog();

        // 여기에 특수블록이면 바로 특수 블록을 터트림(장애물 위여도) 아니면 그냥 Explode
        if (_explodeType >= BlockType.CROSS)
        {
            SetTargetTile(_tile);
            m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
            m_contagiousObstacle = _contagiousObstacleType;
            m_targetType = _explodeType;

            _tile.GetComponent<Tile>().ChasingMoonExplode(_contagiousObstacleType, _explodeType);
        }
        else
        {
            _tile.GetComponent<Tile>().Explode(_contagiousObstacleType);
        }

        MoveMgr.Instance.StartCheckEmpty();
    }
}