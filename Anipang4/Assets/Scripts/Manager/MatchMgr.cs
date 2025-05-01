using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MatchMgr : MonoBehaviour
{
    #region 싱글톤
    static MatchMgr instance;

    public static MatchMgr Instance
    {
        get
        {
            if (instance == null) instance = new MatchMgr();
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

    Vector2Int m_targetMatrix;
    BlockType m_targetType;
    GameObject m_targetTile;

    int m_matchCount = 1; // 본인 포함으로 계산
    int m_saveMatchCount = 1;
    List<GameObject> m_matchTiles = new List<GameObject>(); // 매치가 되는 타일들 저장(후에 터트림)
    List<GameObject> m_saveMatchTiles = new List<GameObject>();

    BlockType m_newBlock = BlockType.NONE; // 특수 블록의 조건에 맞을 경우 생성될 블럭

    #endregion 변수 끝

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
            m_targetTile = _tile;
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
        if (m_targetType >= BlockType.CROSS)
        {
            // 특수 블록 터트리는 함수
            if (_explode)
            {
                SpecialExplode();
            }

            return true;
        }

        // 상하 검사
        UpDownInspect();
        // 여기서 특수 블록 조건 맞으면 일단 저장 후 좌우 검사 때 추가로 블록 더 없으면...
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
                m_newBlock = BlockType.SUN;
                break;
            case 7:
                m_newBlock = BlockType.COSMIC;
                break;
            default:
                break;
        }

        if (m_newBlock >= BlockType.CROSS)
        {
            // 특수 블록 생성 조건 만족함
            if (_explode)
            {
                Explode();
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
                    Explode();
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
                Explode();
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
            matrix.y += i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                continue;
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
            matrix.y -= i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                continue;
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
            matrix.x -= i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                continue;
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
            matrix.x += i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // 타일이 없을 경우 패스
            if (tile == null)
            {
                continue;
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

        if (Inspect(new Vector2Int(1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(1, 1)))
            {
                return true;
            }
        }
        else if (Inspect(new Vector2Int(-1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, 1)))
            {
                return true;
            }
        }
        else if (Inspect(new Vector2Int(1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(1, -1)))
            {
                return true;
            }
        }
        else if (Inspect(new Vector2Int(-1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, -1)))
            {
                return true;
            }
        }

        return false;
    }

    // _AddMatrix만큼 합한 행렬의 타일의 타입 검사
    bool Inspect(Vector2Int _AddMatrix)
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
    bool MoonAddInspect(Vector2Int _AddMatrix)
    {
        if (Inspect(new Vector2Int(_AddMatrix.x, 0)) == true && Inspect(new Vector2Int(0, _AddMatrix.y)) == true)
        {
            m_newBlock = BlockType.MOON;

            Vector2Int matrix = m_targetMatrix;
            matrix += new Vector2Int(1, 1);
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

    void Explode()
    {
        Debug.Log("터지는 타입 : " + m_targetType);
        // 특수 블록 조건에 해당 될 경우
        if (m_newBlock >= BlockType.CROSS)
        {
            m_targetTile.GetComponent<Tile>().SetMyBlockType(m_newBlock);
        }
        // 아닌 경우 블록 타입 변경 : NONE
        else
        {
            m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }

        // m_matchTiles에 등록된 타일들의 블록 타입 변경 : NONE
        foreach (GameObject tile in m_matchTiles)
        {
            tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }

        StageMgr.Instance.OffOutline();
    }

    public bool SimulationMatch(in GameObject _tile)
    {// 본인, 본인, 2칸 밑 이렇게 3칸이 정상 매치로 판정되는 상황->오류
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

        Vector2Int matrix = _tile.GetComponent<Tile>().GetMatrix();
        // 블록 타입을 고정
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();

        Vector2Int upMatrix = new Vector2Int(matrix.x, matrix.y - 1);
        if (SimulationInspect(matrix, upMatrix))
        {
            Debug.Log("위");
            return true;
        }

        Vector2Int downMatrix = new Vector2Int(matrix.x, matrix.y + 1);
        if (SimulationInspect(matrix, downMatrix))
        {
            Debug.Log("아래");
            return true;
        }

        Vector2Int leftMatrix = new Vector2Int(matrix.x - 1, matrix.y);
        if (SimulationInspect(matrix, leftMatrix))
        {
            Debug.Log("왼");
            return true;
        }

        Vector2Int rightMatrix = new Vector2Int(matrix.x + 1, matrix.y);
        if (SimulationInspect(matrix, rightMatrix))
        {
            Debug.Log("오");
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
                BlockType saveType = changeTile.GetComponent<Tile>().GetMyBlockType();
                originalTile.GetComponent<Tile>().SetMyBlockType(saveType);

                m_targetTile = changeTile;
                m_targetMatrix = _changeMatrix;

                if (CheckMatch(changeTile, false))
                {
                    originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
                    return true;
                }
            }
        }

        originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
        return false;
    }

    void SpecialExplode()
    {
        StageMgr.Instance.OffOutline();

        switch (m_targetType)
        {
            case BlockType.CROSS:
                CrossExplode();
                break;
            case BlockType.SUN:
                SunExplode();
                break;
            case BlockType.RANDOM:
                RandomExplode();
                break;
            case BlockType.COSMIC:
                CosmicExplode();
                break;
            case BlockType.MOON:
                MoonExplode();
                break;
            default:
                break;
        }
    }

    void CrossExplode()
    {
        // 가로 세로 다 없앰
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        for (int i = 0; i <= maxMatrix.x; i++)
        {
            GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(i, m_targetMatrix.y));
            if (tile != null)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
        }

        for (int i = 0; i <= maxMatrix.y; i++)
        {
            GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, i));
            if (tile != null)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
        }
    }

    void SunExplode()
    {
        // 5x5 파괴
        // 왼쪽 오른쪽 2
        // 위 아래 2
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2;  y <= 2; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, m_targetMatrix.y + y));
                if (tile != null)
                {
                    // 일반 블록인 경우
                    if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
                    {
                        tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
                    }
                    // 특수 블록인 경우
                    else
                    {
                        // 특수 블록의 위치 기준으로 매치 실행
                        CheckMatch(tile);
                    }
                }
            }
        }
    }

    void RandomExplode()
    {
        // 교차 시킨 블록 타입 정보 알아야 함
        // 그 블록 타입들 서치해서 제거

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
                    tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
                }
            }
        }
    }

    void MoonExplode()
    {
        // 십자칸 파괴 후 클리어 조건 중 하나 랜덤으로 가서 파괴
        #region 십자칸 파괴
        // 본인 파괴
        m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        // 상하좌우 파괴
        GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x - 1, m_targetMatrix.y));
        if (tile != null)
        {
            // 일반 블록인 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // 특수 블록인 경우
            else
            {
                // 특수 블록의 위치 기준으로 매치 실행
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + 1, m_targetMatrix.y));
        if (tile != null)
        {
            // 일반 블록인 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // 특수 블록인 경우
            else
            {
                // 특수 블록의 위치 기준으로 매치 실행
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y - 1));
        if (tile != null)
        {
            // 일반 블록인 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // 특수 블록인 경우
            else
            {
                // 특수 블록의 위치 기준으로 매치 실행
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y + 1));
        if (tile != null)
        {
            // 일반 블록인 경우
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // 특수 블록인 경우
            else
            {
                // 특수 블록의 위치 기준으로 매치 실행
                CheckMatch(tile);
            }
        }
        #endregion

        #region 클리어 조건 중 하나 랜덤으로 가서 파괴

        #endregion
    }
}
