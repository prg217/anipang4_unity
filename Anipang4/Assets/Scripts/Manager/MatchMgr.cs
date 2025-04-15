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

    int m_matchCount = 1; // ���� �������� ���
    int m_saveMatchCount = 0; 
    List<GameObject> m_matchTiles; // ��ġ�� �Ǵ� Ÿ�ϵ� ����(�Ŀ� ��Ʈ��)

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

    public bool CheckMatch(in GameObject _Tile)
    {
        if (_Tile == null)
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

        m_targetMatrix = _Tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _Tile.GetComponent<Tile>().GetMyBlockType();

        // ���� �˻�
        UpDownInspect();
        // ���⼭ Ư�� ��� ���� ������ �ϴ� ���� �� �¿� �˻� �� �߰��� ��� �� ������...
        m_saveMatchCount = m_matchCount;
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
            return true;
        }

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� MOON �˻� �ǽ�
        if (m_newBlock == BlockType.NONE)
        {
            MoonInspect();
        }

        // MOON�� ���ǿ��� ���� �ʴ´ٸ� �⺻���� ��Ʈ��
        if (m_matchCount == 3)
        {
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
        if (m_matchCount < 2 && m_matchCount > 0)
        {
            m_matchTiles.RemoveRange(m_matchCount - m_matchCount, m_matchCount);
            m_matchCount = 1;
        }
    }

    void LeftRightInspect()
    {
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
            }
            else
            {
                break;
            }
        }

        // ��ġ�� �� �Ǵ� ��Ȳ�̶�� m_matchTiles�� ���� ���·� ����
        int count = m_matchCount - m_saveMatchCount;
        if (count < 2 && count > 0)
        {
            m_matchTiles.RemoveRange(m_matchCount - count, count);
            m_matchCount = m_saveMatchCount;
        }
    }

    void MoonInspect()
    {
        /*
            1. �밢�� ���� ��, ������ ��, ���� �Ʒ�, ������ �Ʒ��� ������ Ÿ���� �ִ��� �˻�
            2. ������ Ÿ���� ���� �� ���� ���� ���ʰ� ���� �˻� *** �����ϰ� �������� �˻�
            3. ���� m_matchCount�� 3�̶�� �װ� �����ؼ� ��Ʈ����(MOON�ε� ��Ʈ�� ���� 5���� ���), �ƴ϶�� ���⼭ �߰� �˻��� �͸� ��Ʈ��(4��)
        */

        if (DiagonalInspect(new Vector2Int(1, 1)))
        {
            // ������ �� ĭ, ���� �� ĭ �˻�
            // �װ͵鵵 �Ȱ��� Ÿ���̸� MOON
            // m_matchTiles�� �־��ִµ�, �ߺ��Ǵ��� �˻��ϰ� �־���� ��
        }

        DiagonalInspect(new Vector2Int(-1, 1));
        DiagonalInspect(new Vector2Int(1, -1));
        DiagonalInspect(new Vector2Int(-1, -1));
    }

    bool DiagonalInspect(Vector2Int _AddMatrix)
    {
        Vector2Int matrix = m_targetMatrix;

        // ������ ��
        matrix += _AddMatrix;
        GameObject tile = StageMgr.Instance.GetTile(matrix);
        BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
        if (type == m_targetType)
        {
            return true;
        }
        return false;
    }

    public bool SimulationMatch(in GameObject _Tile)
    {
        // �����¿�� �̵����Ѽ� ��ġ�� �Ǵ��� �׽�Ʈ
        if (_Tile == null)
        {
            return false;
        }

        return true;
    }
}
