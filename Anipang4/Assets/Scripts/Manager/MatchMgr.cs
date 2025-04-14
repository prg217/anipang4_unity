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
    int m_matchCount = 0;
    int m_saveMatchCount = 0;
    List<GameObject> m_matchTiles; // ��ġ�� �Ǵ� Ÿ�ϵ� ����(�Ŀ� ��Ʈ��)

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

        // �¿� �˻�
        LeftRightInspect();

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� �밢�� �˻� �ǽ�

        // MOON�� ���ǿ��� ���� �ʴ´ٸ� �⺻���� ��Ʈ��

        return true;
    }

    void UpDownInspect()
    {
        for (int i = 0; i <= 2; i++)
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

        for (int i = 0; i <= 2; i++)
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

        for (int i = 0; i <= 2; i++)
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
