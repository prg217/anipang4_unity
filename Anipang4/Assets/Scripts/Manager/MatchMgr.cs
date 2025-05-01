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

        // Ÿ���� ��� �ִ� ��� return
        if (m_targetType == BlockType.NONE)
        {
            return false;
        }

        // Ư�� ����� ��� Ư�� ����� �ٷ� ��Ʈ��
        if (m_targetType >= BlockType.CROSS)
        {
            // Ư�� ��� ��Ʈ���� �Լ�
            if (_explode)
            {
                SpecialExplode();
            }

            return true;
        }

        // ���� �˻�
        UpDownInspect();
        // ���⼭ Ư�� ��� ���� ������ �ϴ� ���� �� �¿� �˻� �� �߰��� ��� �� ������...
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
            // Ư�� ��� ���� ���� ������
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
                else
                {
                    StageMgr.Instance.SetMatchOK(m_matchTiles);
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
        Debug.Log("������ Ÿ�� : " + m_targetType);
        // Ư�� ��� ���ǿ� �ش� �� ���
        if (m_newBlock >= BlockType.CROSS)
        {
            m_targetTile.GetComponent<Tile>().SetMyBlockType(m_newBlock);
        }
        // �ƴ� ��� ��� Ÿ�� ���� : NONE
        else
        {
            m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }

        // m_matchTiles�� ��ϵ� Ÿ�ϵ��� ��� Ÿ�� ���� : NONE
        foreach (GameObject tile in m_matchTiles)
        {
            tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
        }

        StageMgr.Instance.OffOutline();
    }

    public bool SimulationMatch(in GameObject _tile)
    {// ����, ����, 2ĭ �� �̷��� 3ĭ�� ���� ��ġ�� �����Ǵ� ��Ȳ->����
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

        // �� ����� ��� ��ġ �õ� �Ұ���
        if (_tile.GetComponent<Tile>().GetMyBlockType() == BlockType.NONE)
        {
            return false;
        }

        Vector2Int matrix = _tile.GetComponent<Tile>().GetMatrix();
        // ��� Ÿ���� ����
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();

        Vector2Int upMatrix = new Vector2Int(matrix.x, matrix.y - 1);
        if (SimulationInspect(matrix, upMatrix))
        {
            Debug.Log("��");
            return true;
        }

        Vector2Int downMatrix = new Vector2Int(matrix.x, matrix.y + 1);
        if (SimulationInspect(matrix, downMatrix))
        {
            Debug.Log("�Ʒ�");
            return true;
        }

        Vector2Int leftMatrix = new Vector2Int(matrix.x - 1, matrix.y);
        if (SimulationInspect(matrix, leftMatrix))
        {
            Debug.Log("��");
            return true;
        }

        Vector2Int rightMatrix = new Vector2Int(matrix.x + 1, matrix.y);
        if (SimulationInspect(matrix, rightMatrix))
        {
            Debug.Log("��");
            return true;
        }

        return false;
    }

    bool SimulationInspect(in Vector2Int _originalMatrix, in Vector2Int _changeMatrix)
    {
        // ������ �ٲ�� �� �ٲ� Ÿ���� Ÿ�Ե� �ٸ��� ����� ��!
        // ��, �Ͻ������θ� ���� Ÿ���� �ٲ� ä�� �ΰ� return �Ŀ� �ٽ� �������� ��
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
        // ���� ���� �� ����
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
        // 5x5 �ı�
        // ���� ������ 2
        // �� �Ʒ� 2
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2;  y <= 2; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, m_targetMatrix.y + y));
                if (tile != null)
                {
                    // �Ϲ� ����� ���
                    if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
                    {
                        tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
                    }
                    // Ư�� ����� ���
                    else
                    {
                        // Ư�� ����� ��ġ �������� ��ġ ����
                        CheckMatch(tile);
                    }
                }
            }
        }
    }

    void RandomExplode()
    {
        // ���� ��Ų ��� Ÿ�� ���� �˾ƾ� ��
        // �� ��� Ÿ�Ե� ��ġ�ؼ� ����

    }

    void CosmicExplode()
    {
        // ���� ����
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
        // ����ĭ �ı� �� Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        #region ����ĭ �ı�
        // ���� �ı�
        m_targetTile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);

        // �����¿� �ı�
        GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x - 1, m_targetMatrix.y));
        if (tile != null)
        {
            // �Ϲ� ����� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // Ư�� ����� ���
            else
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + 1, m_targetMatrix.y));
        if (tile != null)
        {
            // �Ϲ� ����� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // Ư�� ����� ���
            else
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y - 1));
        if (tile != null)
        {
            // �Ϲ� ����� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // Ư�� ����� ���
            else
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                CheckMatch(tile);
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y + 1));
        if (tile != null)
        {
            // �Ϲ� ����� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().SetMyBlockType(BlockType.NONE);
            }
            // Ư�� ����� ���
            else
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                CheckMatch(tile);
            }
        }
        #endregion

        #region Ŭ���� ���� �� �ϳ� �������� ���� �ı�

        #endregion
    }
}
