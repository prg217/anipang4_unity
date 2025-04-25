using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MatchMgr : MonoBehaviour
{
    #region �̱���
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
        #region �̱���
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

    #region ����

    Vector2Int m_targetMatrix;
    BlockType m_targetType;
    GameObject m_targetTile;

    int m_matchCount = 1; // ���� �������� ���
    int m_saveMatchCount = 1; 
    List<GameObject> m_matchTiles = new List<GameObject>(); // ��ġ�� �Ǵ� Ÿ�ϵ� ����(�Ŀ� ��Ʈ��)
    List<GameObject> m_saveMatchTiles = new List<GameObject>();

    BlockType m_newBlock = BlockType.NONE; // Ư�� ����� ���ǿ� ���� ��� ������ ��

    #endregion ���� ��

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
        ! �ٸ� Ÿ���� ����� ���� ��� �ٷ� �˻縦 �������� ��    

            �켱���� ���� ó��
        ~�����¿� 2��Ͼ� �˻�~(���� ����)
        1. �� 7�� �̻�->COSMIC
        2. �� 5�� �̻�(����, �¿� �� �� �ϳ�)->RANDOM
        3. �� 5�� �̻�(���� ������ �� �Ǵ� ���)->SUN
        4. �� 4�� �̻�(����, �¿� �� �� �ϳ�)->CROSS
        5. �� 4�� �̻�(�밢�� �˻�)->MOON
        6. �� 3�� �̻�->�⺻
        */

        #region �ʱ�ȭ
        m_matchTiles.Clear();
        m_saveMatchTiles.Clear();
        m_matchCount = 1;
        m_saveMatchCount = 1;
        m_newBlock = BlockType.NONE;
        #endregion

        #region Ÿ�� Ÿ�� ���� ����
        if (_explode)
        {
            m_targetTile = _tile;
            m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
            m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();
        }
        #endregion

        // Ư�� ����� ��� Ư�� ����� �ٷ� ��Ʈ��
        if (m_targetType >= BlockType.CROSS )
        {
            // Ư�� ��� ��Ʈ���� �Լ�

            return true;
        }

        // ���� �˻�
        UpDownInspect();
        // ���⼭ Ư�� ��� ���� ������ �ϴ� ���� �� �¿� �˻� �� �߰��� ��� �� ������...
        m_saveMatchCount = m_matchCount;
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

        // �¿� �˻�
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
            // Ư�� ��� ���� ������
            if (_explode)
            {
                Explode();
            }

            return true;
        }

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� MOON �˻� �ǽ�
        if (m_newBlock == BlockType.NONE)
        {
            if (MoonInspect())
            {
                // ��Ʈ���� �Լ�
                if (_explode)
                {
                    Explode();
                }
                return true;
            }
        }

        // MOON�� ���ǿ��� ���� �ʴ´ٸ� �⺻���� ��Ʈ��
        if (m_matchCount == 3)
        {
            // ��Ʈ���� �Լ�
            if (_explode)
            {
                Explode();
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
            // Ÿ���� ���� ��� �н�
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
            // Ÿ���� ���� ��� �н�
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

        // ��ġ�� �� �Ǵ� ��Ȳ�̶�� m_matchTiles�� ���� ���·� ����
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
            // Ÿ���� ���� ��� �н�
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
            // Ÿ���� ���� ��� �н�
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

        // ��ġ�� �� �Ǵ� ��Ȳ�̶�� m_matchTiles�� ���� ���·� ����
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
            1. �밢�� ���� ��, ������ ��, ���� �Ʒ�, ������ �Ʒ��� ������ Ÿ���� �ִ��� �˻�
            2. ������ Ÿ���� ���� �� ���� ���� ���ʰ� ���� �˻� *** �����ϰ� �������� �˻�
            3. ���� m_matchCount�� 3�̶�� �װ� �����ؼ� ��Ʈ����(MOON�ε� ��Ʈ�� ���� 5���� ���), �ƴ϶�� ���⼭ �߰� �˻��� �͸� ��Ʈ��(4��)
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
        else if(Inspect(new Vector2Int(1, -1)))
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

    // _AddMatrix��ŭ ���� ����� Ÿ���� Ÿ�� �˻�
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

    // �밢�� Ÿ���� ������ Ÿ���� �� �翷 �߰� �˻�
    bool MoonAddInspect(Vector2Int _AddMatrix)
    {
        if (Inspect(new Vector2Int(_AddMatrix.x, 0)) == true && Inspect(new Vector2Int(0, _AddMatrix.y)) == true)
        {
            m_newBlock = BlockType.MOON;

            Vector2Int matrix = m_targetMatrix;
            matrix += new Vector2Int(1, 1);
            GameObject tile = StageMgr.Instance.GetTile(matrix);
            m_matchTiles.Add(tile);

            // �ߺ� �˻� �� ��ġ Ÿ�ϵ鿡 �־��ֱ�
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
        // Ư�� ��� ���ǿ� �ش� �� ���
        if (m_newBlock >= BlockType.CROSS)
        {
            m_targetTile.GetComponent<Tile>().SetMyBlockType(m_newBlock);
        }
        // �ƴ� ��� ��� : NONE
        else
        {
            Debug.Log("Ÿ�� ���� " + m_targetTile.name);
            m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }

        // m_matchTiles�� ��ϵ� Ÿ�ϵ��� ��� : NONE
        foreach (GameObject tile in m_matchTiles)
        {
            Debug.Log("���� " + tile.name);
            tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }
    }

    public bool SimulationMatch(in GameObject _tile)
    {
        // �����¿�� �̵����Ѽ� ��ġ�� �Ǵ��� �׽�Ʈ
        if (_tile == null)
        {
            return false;
        }

        // ������ �� ���� Ÿ�� �� ��� ��ġ �õ� �Ұ���
        if (_tile.GetComponent<Tile>().GetTileType() == TileType.IMMOVABLE)
        {
            return false;
        }

        Vector2Int matrix = _tile.GetComponent<Tile>().GetMatrix();
        // ��� Ÿ���� ����
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();

        #region �� �˻�
        Vector2Int upMatrix = new Vector2Int(matrix.x, matrix.y - 1);
        GameObject upTile = StageMgr.Instance.GetTile(upMatrix);
        if (upTile.GetComponent<Tile>().GetTileType() == TileType.MOVABLE)
        {
            m_targetTile = upTile;
            m_targetMatrix = upMatrix;

            if (CheckMatch(m_targetTile, false))
            {
                return true;
            }
        }
        #endregion

        #region �Ʒ� �˻�
        Vector2Int downMatrix = new Vector2Int(matrix.x, matrix.y + 1);
        GameObject downTile = StageMgr.Instance.GetTile(downMatrix);
        if (downTile.GetComponent<Tile>().GetTileType() == TileType.MOVABLE)
        {
            m_targetTile = downTile;
            m_targetMatrix = downMatrix;

            if (CheckMatch(m_targetTile, false))
            {
                return true;
            }
        }
        #endregion

        #region ���� �˻�
        Vector2Int leftMatrix = new Vector2Int(matrix.x - 1, matrix.y);
        GameObject leftTile = StageMgr.Instance.GetTile(leftMatrix);
        if (leftTile.GetComponent<Tile>().GetTileType() == TileType.MOVABLE)
        {
            m_targetTile = leftTile;
            m_targetMatrix = leftMatrix;

            if (CheckMatch(m_targetTile, false))
            {
                return true;
            }
        }
        #endregion

        #region ������ �˻�
        Vector2Int rightMatrix = new Vector2Int(matrix.x + 1, matrix.y);
        GameObject rightTile = StageMgr.Instance.GetTile(rightMatrix);
        if (rightTile.GetComponent<Tile>().GetTileType() == TileType.MOVABLE)
        {
            m_targetTile = rightTile;
            m_targetMatrix = rightMatrix;

            if (CheckMatch(m_targetTile, false))
            {
                return true;
            }
        }
        #endregion

        return false;
    }
}
