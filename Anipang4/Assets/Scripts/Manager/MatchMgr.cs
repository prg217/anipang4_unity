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
    int m_matchCount = 0;
    int m_saveMatchCount = 0;
    List<GameObject> m_matchTiles; // 매치가 되는 타일들 저장(후에 터트림)

    #endregion 변수 끝

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CheckMatch(in GameObject _Tile)
    {
        if (_Tile == null)
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

        m_targetMatrix = _Tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _Tile.GetComponent<Tile>().GetMyBlockType();

        // 상하 검사
        UpDownInspect();
        // 여기서 특수 블록 조건 맞으면 일단 저장 후 좌우 검사 때 추가로 블록 더 없으면...
        m_saveMatchCount = m_matchCount;

        // 좌우 검사
        LeftRightInspect();

        // 만약 특수 블록에 해당되지 않는다면 대각선 검사 실시

        // MOON의 조건에도 되지 않는다면 기본으로 터트림

        return true;
    }

    void UpDownInspect()
    {
        for (int i = 0; i <= 2; i++)
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

        for (int i = 0; i <= 2; i++)
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
        if (m_matchCount < 2 && m_matchCount > 0)
        {
            m_matchTiles.RemoveRange(m_matchCount - m_matchCount, m_matchCount);
            m_matchCount = 0;
        }
    }

    void LeftRightInspect()
    {
        for (int i = 0; i <= 2; i++)
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
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i <= 2; i++)
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
            }
            else
            {
                break;
            }
        }

        // 매치가 안 되는 상황이라면 m_matchTiles를 이전 상태로 돌림
        int count = m_matchCount - m_saveMatchCount;
        if (count < 2 && count > 0)
        {
            m_matchTiles.RemoveRange(m_matchCount - count, count);
            m_matchCount = m_saveMatchCount;
        }
    }

    public bool SimulationMatch(in GameObject _Tile)
    {
        // 상하좌우로 이동시켜서 매치가 되는지 테스트
        if (_Tile == null)
        {
            return false;
        }

        return true;
    }
}
