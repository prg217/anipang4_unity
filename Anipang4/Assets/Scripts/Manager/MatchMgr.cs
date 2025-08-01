using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class MatchMgr : BaseMgr<MatchMgr>
{
    #region ����

    [Header("������ ���")]
    [SerializeField]
    GameObject m_chasingMoonPrefab; // �߰��ϴ� �� ������ ���

    Vector2Int m_targetMatrix;
    EBlockType m_targetType;
    GameObject m_targetTile;

    int m_matchCount = 1; // ���� �������� ���
    int m_saveMatchCount = 1;
    List<GameObject> m_matchTiles = new List<GameObject>(); // ��ġ�� �Ǵ� Ÿ�ϵ� ����(�Ŀ� ��Ʈ��)
    List<GameObject> m_saveMatchTiles = new List<GameObject>();

    EBlockType m_newBlock = EBlockType.NONE; // Ư�� ����� ���ǿ� ���� ��� ������ ��
    EObstacleType m_contagiousObstacle = EObstacleType.NONE; // Ÿ�Ͽ� �߰� �� ��ֹ�

    int m_randomExplodeCompleteCount = 0;

    #endregion ���� ��

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
        m_newBlock = EBlockType.NONE;
        #endregion

        #region Ÿ�� Ÿ�� ���� ����
        SetTargetTile(_tile);
        m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();
        #endregion

        // Ÿ���� ��� �ִ� ��� return
        if (m_targetType == EBlockType.NONE)
        {
            return false;
        }

        // Ư�� ����� ��� Ư�� ����� �ٷ� ��Ʈ��
        if (m_targetType >= EBlockType.CROSS && m_targetType != EBlockType.NULL)
        {
            // Ư�� ��� ��Ʈ��
            if (_explode)
            {
                Explode(true);
            }

            return true;
        }

        // ���� �˻�
        UpDownInspect();
        // Ư�� ��� ���� ������ �ϴ� ����
        m_saveMatchCount = m_matchCount;
        m_saveMatchTiles.Clear();
        m_saveMatchTiles = new List<GameObject>(m_matchTiles);
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                m_newBlock = EBlockType.RANDOM;
                break;
            default:
                break;
        }

        // �¿� �˻�
        LeftRightInspect();
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                if (m_saveMatchCount >= 3 && m_saveMatchCount < 5)
                {
                    m_newBlock = EBlockType.SUN;
                }
                else
                {
                    m_newBlock = EBlockType.RANDOM;
                }
                break;
            case 6:
                m_newBlock = EBlockType.SUN;
                break;
            case 7:
                m_newBlock = EBlockType.COSMIC;
                break;
            default:
                break;
        }

        if (m_newBlock >= EBlockType.CROSS)
        {
            // Ư�� ��� ���� ���� ������
            if (_explode)
            {
                Explode(false);
            }
            return true;
        }

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� MOON �˻� �ǽ�
        if (m_newBlock == EBlockType.NONE)
        {
            if (MoonInspect())
            {
                // ��Ʈ���� �Լ�
                if (_explode)
                {
                    Explode(false);
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
                SoundMgr.Instance.PlaySFX(ESFX.NOMAL_MATCH);

                Explode(false);
            }
            return true;
        }

        return false;
    }

    public bool CheckMatchSimulation(in GameObject _tile)
    {
        if (_tile == null)
        {
            return false;
        }

        #region �ʱ�ȭ
        m_matchTiles.Clear();
        m_saveMatchTiles.Clear();
        m_matchCount = 1;
        m_saveMatchCount = 1;
        m_newBlock = EBlockType.NONE;
        #endregion

        // Ÿ���� ��� �ִ� ��� return
        if (m_targetType == EBlockType.NONE)
        {
            return false;
        }

        // Ư�� ����� ��� Ư�� ����� �ٷ� ��Ʈ��
        if (m_targetType >= EBlockType.CROSS && m_targetType != EBlockType.NULL)
        {
            return true;
        }

        // ���� �˻�
        UpDownInspect();
        // Ư�� ��� ���� ������ �ϴ� ����
        m_saveMatchCount = m_matchCount;
        m_saveMatchTiles.Clear();
        m_saveMatchTiles = new List<GameObject>(m_matchTiles);
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                m_newBlock = EBlockType.RANDOM;
                break;
            default:
                break;
        }

        // �¿� �˻�
        LeftRightInspect();
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                if (m_saveMatchCount >= 3 && m_saveMatchCount < 5)
                {
                    m_newBlock = EBlockType.SUN;
                }
                else
                {
                    m_newBlock = EBlockType.RANDOM;
                }
                break;
            case 6:
                m_newBlock = EBlockType.SUN;
                break;
            case 7:
                m_newBlock = EBlockType.COSMIC;
                break;
            default:
                break;
        }

        if (m_newBlock >= EBlockType.CROSS)
        {
            return true;
        }

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� MOON �˻� �ǽ�
        if (m_newBlock == EBlockType.NONE)
        {
            if (MoonInspect())
            {
                return true;
            }
        }

        // MOON�� ���ǿ��� ���� �ʴ´ٸ� �⺻���� ��Ʈ��
        if (m_matchCount == 3)
        {
            return true;
        }

        return false;
    }

    // ���°� ��ȯ ��� �߰�
    public (bool result, int matchCount, List<GameObject> matchTile) CheckMatchWithStatus(in GameObject _tile, in bool _explode = true)
    {
        if (_tile == null)
        {
            return (false, 0, null);
        }

        #region �ʱ�ȭ
        m_matchTiles.Clear();
        m_saveMatchTiles.Clear();
        m_matchCount = 1;
        m_saveMatchCount = 1;
        m_newBlock = EBlockType.NONE;
        #endregion

        #region Ÿ�� Ÿ�� ���� ����
        SetTargetTile(_tile);
        m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();
        #endregion

        // Ÿ���� ��� �ִ� ��� return
        if (m_targetType == EBlockType.NONE)
        {
            return (false, 0, null);
        }

        // Ư�� ����� ��� Ư�� ����� �ٷ� ��Ʈ��
        if (m_targetType >= EBlockType.CROSS && m_targetType != EBlockType.NULL)
        {
            // Ư�� ��� ��Ʈ��
            if (_explode)
            {
                Explode(true);
            }
            return (true, m_matchCount, m_matchTiles.ToList());
        }

        // ���� �˻�
        UpDownInspect();
        // Ư�� ��� ���� ������ �ϴ� ����
        m_saveMatchCount = m_matchCount;
        m_saveMatchTiles.Clear();
        m_saveMatchTiles = new List<GameObject>(m_matchTiles);
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                m_newBlock = EBlockType.RANDOM;
                break;
            default:
                break;
        }

        // �¿� �˻�
        LeftRightInspect();
        switch (m_matchCount)
        {
            case 4:
                m_newBlock = EBlockType.CROSS;
                break;
            case 5:
                if (m_saveMatchCount >= 3 && m_saveMatchCount < 5)
                {
                    m_newBlock = EBlockType.SUN;
                }
                else
                {
                    m_newBlock = EBlockType.RANDOM;
                }
                break;
            case 6:
                m_newBlock = EBlockType.SUN;
                break;
            case 7:
                m_newBlock = EBlockType.COSMIC;
                break;
            default:
                break;
        }

        if (m_newBlock >= EBlockType.CROSS)
        {
            // Ư�� ��� ���� ���� ������
            if (_explode)
            {
                Explode(false);
            }
            return (true, m_matchCount, m_matchTiles.ToList());
        }

        // ���� Ư�� ��Ͽ� �ش���� �ʴ´ٸ� MOON �˻� �ǽ�
        if (m_newBlock == EBlockType.NONE)
        {
            if (MoonInspect())
            {
                // ��Ʈ���� �Լ�
                if (_explode)
                {
                    Explode(false);
                }
                return (true, m_matchCount, m_matchTiles.ToList());
            }
        }

        // MOON�� ���ǿ��� ���� �ʴ´ٸ� �⺻���� ��Ʈ��
        if (m_matchCount == 3)
        {
            // ��Ʈ���� �Լ�
            if (_explode)
            {
                Explode(false);
            }
            return (true, m_matchCount, m_matchTiles.ToList());
        }

        return (false, 0, null);
    }

    void UpDownInspect()
    {
        for (int i = 1; i <= 4; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.y = m_targetMatrix.y + i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
            if (tile == null)
            {
                break;
            }

            EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
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

        for (int i = 1; i <= 4; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.y = m_targetMatrix.y - i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
            if (tile == null)
            {
                break;
            }

            EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
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
        for (int i = 1; i <= 4; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.x = m_targetMatrix.x - i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
            if (tile == null)
            {
                break;
            }

            EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
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

        for (int i = 1; i <= 4; i++)
        {
            Vector2Int matrix = m_targetMatrix;
            matrix.x = m_targetMatrix.x + i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
            if (tile == null)
            {
                break;
            }

            EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
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

        /*
         * �밢�� ���� ��, ������ ��, ���� �Ʒ�, ������ �Ʒ��� ������ Ÿ���� �ִ��� �˻�
         * ������ Ÿ���� ���� �� ���� ���� ���ʰ� ���� �˻� *** �����ϰ� �������� �˻�
         * + ���� ������ �������� 3�� ��ġ�� �Ǵ°� �ִ����� �˻�(5�� �� ���)
         */

        // ������ �Ʒ�
        if (Inspect(new Vector2Int(1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(1, 1)))
            {
                return true;
            }
        }
        // ���� ��
        if (Inspect(new Vector2Int(-1, 1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, 1)))
            {
                return true;
            }
        }
        // ������ ��
        if (Inspect(new Vector2Int(1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(1, -1)))
            {
                return true;
            }
        }
        // ���� ��
        if (Inspect(new Vector2Int(-1, -1)))
        {
            if (MoonAddInspect(new Vector2Int(-1, -1)))
            {
                return true;
            }
        }

        return false;
    }

    // _AddMatrix��ŭ ���� ����� Ÿ���� Ÿ�� �˻�
    bool Inspect(in Vector2Int _AddMatrix)
    {
        Vector2Int matrix = m_targetMatrix;

        matrix += _AddMatrix;
        GameObject tile = StageMgr.Instance.GetTile(matrix);
        if (tile == null)
        {
            return false;
        }

        EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
        if (type == m_targetType)
        {
            return true;
        }
        return false;
    }

    // �밢�� Ÿ���� ������ Ÿ���� �� �翷 �߰� �˻�
    bool MoonAddInspect(in Vector2Int _AddMatrix)
    {
        if (Inspect(new Vector2Int(_AddMatrix.x, 0)) == true && Inspect(new Vector2Int(0, _AddMatrix.y)) == true)
        {
            m_newBlock = EBlockType.MOON;

            Vector2Int matrix = m_targetMatrix;
            matrix += _AddMatrix;
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

    void Explode(in bool _isSpecial)
    {
        // �α� �߰�
        LogMgr.Instance.AddMatchLog(m_targetType, m_targetTile, m_matchTiles, m_newBlock);

        // �̸� ���� Ÿ�Ͽ� ��ȣ
        m_targetTile.GetComponent<Tile>().StartExplodeEffect();
        foreach (GameObject tile in m_matchTiles)
        {
            if (tile.GetComponent<Tile>().GetMyBlockType() != EBlockType.NULL)
            {
                tile.GetComponent<Tile>().StartExplodeEffect();
            }
        }

        // ĸ�� �α� �߰�
        LogMgr.Instance.CaptureLog();

        // ��ġ�Ǵ� Ÿ�� �� ���ĵǴ� ��ֹ��� �ִ��� Ȯ��
        if (!_isSpecial)
        {
            m_contagiousObstacle = EObstacleType.NONE;
            EObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();

            if (obstacleType != EObstacleType.NONE)
            {
                m_contagiousObstacle = obstacleType;
            }

            foreach (GameObject tile in m_matchTiles)
            {
                obstacleType = tile.GetComponent<Tile>().GetPropagationObstacle();
                // �ִٸ� ���ĵǴ� ��ֹ��� ��ֹ��� �߰��ϰ� ��
                if (obstacleType != EObstacleType.NONE)
                {
                    m_contagiousObstacle = obstacleType;
                }
            }
        }

        // Ư�� ��� ���� ���ǿ� �ش� �� ���
        if (m_newBlock >= EBlockType.CROSS)
        {
            m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle, m_newBlock);
        }
        // �ƴ� ��� ��Ʈ��
        else
        {
            m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle);
        }

        // m_matchTiles�� ��ϵ� Ÿ�ϵ��� ��Ʈ��
        foreach (GameObject tile in m_matchTiles)
        {
            if (tile != null)
            {
                if (tile.GetComponent<Tile>().GetMyBlockType() != EBlockType.NULL)
                {
                    tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                }
            }
        }
    }

    public bool SimulationMatch(in GameObject _originalTile, in GameObject _changeTile)
    {
        // �׽�Ʈ�� Ÿ������ ��ġ ����
        m_targetType = _originalTile.GetComponent<Tile>().GetMyBlockType();

        EBlockType saveType = _changeTile.GetComponent<Tile>().GetMyBlockType();
        _originalTile.GetComponent<Tile>().SetMyBlockType(saveType);

        SetTargetTile(_changeTile);
        m_targetMatrix = _changeTile.GetComponent<Tile>().GetMatrix();

        if (CheckMatchSimulation(_changeTile))
        {
            _originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
            return true;
        }
        _originalTile.GetComponent<Tile>().SetMyBlockType(m_targetType);
        return false;
    }

    public (bool result, List<GameObject> simulateTiles) SimulateBlockMove(in GameObject _tile)
    {
        // �����¿�� �̵����Ѽ� ��ġ�� �Ǵ��� �׽�Ʈ

        if (_tile == null)
        {
            return (false, null);
        }

        // ������ �� ���� Ÿ�� �� ��� ��ġ �õ� �Ұ���
        if (_tile.GetComponent<Tile>().GetTileType() == ETileType.IMMOVABLE)
        {
            return (false, null);
        }

        // �� ����� ��� ��ġ �õ� �Ұ���
        if (_tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NONE || _tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
        {
            return (false, null);
        }

        // Ư�� ����� ��� ��ġ ����
        if (_tile.GetComponent<Tile>().GetMyBlockType() >= EBlockType.CROSS)
        {
            // �ϴ� �ӽ÷� null
            return (true, null);
        }

        Vector2Int matrix = _tile.GetComponent<Tile>().GetMatrix();
        // ��� Ÿ���� ����
        m_targetType = _tile.GetComponent<Tile>().GetMyBlockType();

        Vector2Int upMatrix = new Vector2Int(matrix.x, matrix.y - 1);
        if (SimulationInspect(matrix, upMatrix))
        {
            return (true, m_matchTiles.ToList());
        }

        Vector2Int downMatrix = new Vector2Int(matrix.x, matrix.y + 1);
        if (SimulationInspect(matrix, downMatrix))
        {
            return (true, m_matchTiles.ToList());
        }

        Vector2Int leftMatrix = new Vector2Int(matrix.x - 1, matrix.y);
        if (SimulationInspect(matrix, leftMatrix))
        {
            return (true, m_matchTiles.ToList());
        }

        Vector2Int rightMatrix = new Vector2Int(matrix.x + 1, matrix.y);
        if (SimulationInspect(matrix, rightMatrix))
        {
            return (true, m_matchTiles.ToList());
        }

        return (false, null);
    }

    bool SimulationInspect(in Vector2Int _originalMatrix, in Vector2Int _changeMatrix)
    {
        // ������ �ٲ�� �� �ٲ� Ÿ���� Ÿ�Ե� �ٸ��� ����� ��!
        // ��, �Ͻ������θ� ���� Ÿ���� �ٲ� ä�� �ΰ� return �Ŀ� �ٽ� �������� ��
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);
        GameObject changeTile = StageMgr.Instance.GetTile(_changeMatrix);

        if (changeTile != null)
        {
            if (changeTile.GetComponent<Tile>().GetTileType() == ETileType.MOVABLE)
            {
                return SimulationMatch(originalTile, changeTile);
            }
        }

        return false;
    }

    public void SpecialExplode(in GameObject _tile ,in EBlockType _blockType)
    {
        m_targetTile = _tile;
        m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
        m_targetType = _blockType;

        m_newBlock = EBlockType.NONE;
        m_matchTiles.Clear();

        // ��ġ�Ǵ� Ÿ�� �� ���ĵǴ� ��ֹ��� �ִ��� Ȯ��
        m_contagiousObstacle = EObstacleType.NONE;

        // �� ��ֹ��� ���� ���
        if (m_targetTile.GetComponent<Tile>().GetMyFrontObstacleType() == EObstacleType.NONE)
        {
            EObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();
            if (obstacleType != EObstacleType.NONE)
            {
                m_contagiousObstacle = obstacleType;
            }
        }

        switch (m_targetType)
        {
            case EBlockType.CROSS:
                CrossExplode();
                break;
            case EBlockType.SUN:
                SunExplode();
                break;
            case EBlockType.RANDOM:
                RandomMatch(m_targetTile, EBlockType.NONE, m_contagiousObstacle);
                break;
            case EBlockType.COSMIC:
                CosmicExplode();
                break;
            case EBlockType.MOON:
                MoonExplode();
                break;
            case EBlockType.DOUBLE_CROSS:
                DoubleCrossExplode();
                break;
            case EBlockType.CROSS_SUN:
                CrossSunExplode();
                break;
            case EBlockType.CROSS_MOON:
                SpecialMoonExplode(EBlockType.CROSS);
                break;
            case EBlockType.DOUBLE_SUN:
                DoubleSunExplode();
                break;
            case EBlockType.SUN_MOON:
                SpecialMoonExplode(EBlockType.SUN);
                break;
            case EBlockType.DOUBLE_MOON:
                SpecialMoonExplode(EBlockType.MOON);
                break;
            case EBlockType.DOUBLE_RANDOM:
                CosmicExplode();
                break;
            case EBlockType.RANDOM_CROSS:
                RandomMatch(m_targetTile, EBlockType.CROSS, m_contagiousObstacle);
                break;
            case EBlockType.RANDOM_SUN:
                RandomMatch(m_targetTile, EBlockType.SUN, m_contagiousObstacle);
                break;
            case EBlockType.RANDOM_MOON:
                RandomMatch(m_targetTile, EBlockType.MOON, m_contagiousObstacle);
                break;
            default:
                //MoveMgr.Instance.SetCheckEmptyEnabled(true);
                break;
        }
    }

    public void SpecialCompositionExplode(in GameObject _tile1, in GameObject _tile2, in EObstacleType _contagiousObstacle)
    {
        m_contagiousObstacle = _contagiousObstacle;

        EBlockType type1 = _tile1.GetComponent<Tile>().GetMyBlockType();
        EBlockType type2 = _tile2.GetComponent<Tile>().GetMyBlockType();

        SetTargetTile(_tile2);
        m_targetMatrix = _tile2.GetComponent<Tile>().GetMatrix();

        _tile1.GetComponent<Tile>().SetMyBlockType(EBlockType.NONE);

        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_COMPOSITION);

        switch (type1)
        {
            case EBlockType.CROSS:
                {
                    switch (type2)
                    {
                        case EBlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.DOUBLE_CROSS);
                            break;
                        case EBlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.CROSS_SUN);
                            break;
                        case EBlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_CROSS);
                            break;
                        case EBlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                            break;
                        case EBlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.CROSS_MOON);
                            break;
                        default:
                            break;
                    }
                }                
                break;
            case EBlockType.SUN:
                {
                    switch (type2)
                    {
                        case EBlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.CROSS_SUN);
                            break;
                        case EBlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.DOUBLE_SUN);
                            break;
                        case EBlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_SUN);
                            break;
                        case EBlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                            break;
                        case EBlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.SUN_MOON);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case EBlockType.RANDOM:
                {
                    switch (type2)
                    {
                        case EBlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_CROSS);
                            break;
                        case EBlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_SUN);
                            break;
                        case EBlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_MOON);
                            break;
                        case EBlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                            break;
                        case EBlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case EBlockType.COSMIC:
                {
                    _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                }
                break;
            case EBlockType.MOON:
                {
                    switch (type2)
                    {
                        case EBlockType.CROSS:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.CROSS_MOON);
                            break;
                        case EBlockType.SUN:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.SUN_MOON);
                            break;
                        case EBlockType.RANDOM:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.RANDOM_MOON);
                            break;
                        case EBlockType.COSMIC:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.COSMIC);
                            break;
                        case EBlockType.MOON:
                            _tile2.GetComponent<Tile>().SetMyBlockType(EBlockType.DOUBLE_MOON);
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                //MoveMgr.Instance.SetCheckEmptyEnabled(true);
                break;
        }

        m_targetType = _tile2.GetComponent<Tile>().GetMyBlockType();
        _tile2.GetComponent<Tile>().Explode(_contagiousObstacle);
    }

    void DoubleCrossExplode()
    {
        // ���� ���� 3�پ�
        LengthAndWidthExplode(1, 1);
    }

    void CrossSunExplode()
    {
        // ���� ���� 5�پ�
        LengthAndWidthExplode(2, 2);
    }

    void DoubleSunExplode()
    {
        // 9x9
        SurroundingsExplode(4, 4);
    }

    void SpecialMoonExplode(in EBlockType _specialType)
    {
        // �ֺ� 8�� ��Ʈ��
        SurroundingsExplode(1, 1);

        #region Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        // �� �߰� ������ ��ȯ
        if (_specialType == EBlockType.MOON)
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

    void SummonChasingMoon(in EBlockType _specialType)
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
        // 5x5 �ı�
        SurroundingsExplode(2, 2);
    }

    public void RandomMatch(in GameObject _targetTile ,in EBlockType _randomType, in EObstacleType _contagiousObstacle)
    {
        SetTargetTile(_targetTile);
        m_contagiousObstacle = _contagiousObstacle;

        StartCoroutine(RandomExplode(_randomType));
    }

    IEnumerator RandomExplode(EBlockType _type)
    {
        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_MATCH_3);

        // ���� ��Ų ��� Ÿ�� ������ �˰�, �� Ÿ�Ե��� ��ġ�ؼ� ����
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        m_randomExplodeCompleteCount = 0;
        List<GameObject> explodeTiles = new List<GameObject>();

        m_targetTile.GetComponent<Tile>().SetRandomExecute(true);

        MoveMgr.Instance.SetCheckEmptyEnabled(false);

        #region �ܵ����� ���� ���� ���
        if (_type == EBlockType.NONE)
        {
            // �Ϲ� ��� �� �ϳ� �������� �������ֱ�
            int randomType = Random.Range(0, StageMgr.Instance.GetMaxBlockType());
            _type = (EBlockType)randomType;
        }
        #endregion

        #region �Ϲ� ����� ���
        if (_type < EBlockType.CROSS)
        {
            for (int x = 0; x <= maxMatrix.x; x++)
            {
                for (int y = 0; y <= maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if (type == _type)
                    {
                        // �ش�Ǵ� ��� ��鸮�� ȿ��
                        explodeTiles.Add(tile);
                        tile.GetComponent<Tile>().RandomEffect(true);

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        #endregion

        #region Ư�� ����� ���
        if (_type >= EBlockType.CROSS)
        {
            if (_type == EBlockType.RANDOM || _type == EBlockType.COSMIC)
            {
                CosmicExplode();
            }

            // ���� ���� ����� �ش��ϴ� Ư�� ������� ���� �� ��Ʈ��
            EBlockType mostType = StageMgr.Instance.GetMostNormalBlockType();

            for (int x = 0; x <= maxMatrix.x; x++)
            {
                for (int y = 0; y <= maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if (type == mostType)
                    {
                        // �ش��ϴ� Ư�� ������� ��ȯ
                        // Front��ֹ� �ȿ� �ִ� ��� �Ϲ� ��� ó�� ó��
                        if (tile.GetComponent<Tile>().GetFrontObstacleEmpty())
                        {
                            tile.GetComponent<Tile>().SetMyBlockType(_type);
                        }

                        // �ش�Ǵ� ��� ��鸮�� ȿ��
                        explodeTiles.Add(tile);
                        tile.GetComponent<Tile>().RandomEffect(true);

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        #endregion

        // �� ������ �����ٴ� ���� ���� �ش�Ǵ� ��ϵ鿡�� ���ÿ� ����
        foreach (GameObject tile in explodeTiles)
        {
            tile.GetComponent<Tile>().SetRandomExplode(true);
            tile.GetComponent<Tile>().RandomEffect(false);
            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
        }

        // �� ������ ���� ����ϰ� �ִ� ���� ����� ������
        m_targetTile.GetComponent<Tile>().SetRandomComplete(true);

        // explodeTiles.Count�ؼ� �� ������ŭ �Ϸ� ��ȣ ���ƿ��� �����
        yield return new WaitUntil(() => m_randomExplodeCompleteCount == explodeTiles.Count);

        // �ٽ� ����� ä��� Ȱ��ȭ
        MoveMgr.Instance.ActiveCheckEmpty();
    }

    public void RandomExplodeComplete()
    {
        m_randomExplodeCompleteCount++;
    }

    void CosmicExplode()
    {
        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_MATCH_5);

        // ���� ����
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        for (int i = 0; i <= maxMatrix.x; i++)
        {
            for (int j = 0; j <= maxMatrix.y; j++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(i, j));
                if (tile != null)
                {
                    if (tile.GetComponent<Tile>().GetMyBlockType() != EBlockType.NULL)
                    {
                        tile.GetComponent<Tile>().SetMyBlockType(EBlockType.NONE);
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                }
            }
        }
    }

    void MoonExplode()
    {
        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_MATCH_2);

        // ����ĭ �ı� �� Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        #region ����ĭ �ı�
        // �ߺ� ����
        m_targetTile.GetComponent<Tile>().SetMyBlockType(EBlockType.NONE);

        // �����¿� �ı�
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

        #region Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        // �� �߰� ������ ��ȯ
        SummonChasingMoon(EBlockType.NONE);
        #endregion
    }

    // �ֺ� ��Ʈ�� : Sun ���� �Լ����� ���, Ư�� ��� �ռ� Moon������ ���
    void SurroundingsExplode(in int _x, in int _y) 
    {
        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_MATCH_4);

        // �ߺ� ���� ����
        m_targetTile.GetComponent<Tile>().SetMyBlockType(EBlockType.NONE);

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

    // ���μ��� ��Ʈ�� : Cross ���� �Լ����� ���
    void LengthAndWidthExplode(in int _x, in int _y)
    {
        SoundMgr.Instance.PlaySFX(ESFX.SPECIAL_MATCH_1);

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // �ߺ� ���� ����
        m_targetTile.GetComponent<Tile>().SetMyBlockType(EBlockType.NONE);

        for (int y = -_y; y <= _y; y++)
        {
            for (int x = 0; x <= maxMatrix.x; x++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, m_targetMatrix.y + y));

                if (tile != null && m_targetTile != tile)
                {
                    // �̹� ó���� Ÿ�� �ǳʶ�
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
                    // �̹� ó���� Ÿ�� �ǳʶ�
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

    public void ChasingMoonExplode(in GameObject _tile, in EObstacleType _contagiousObstacleType = EObstacleType.NONE, in EBlockType _explodeType = EBlockType.NONE)
    {
        LogMgr.Instance.ChasingMoonExplodeLog(_tile, _explodeType);

        _tile.GetComponent<Tile>().StartExplodeEffect();
        LogMgr.Instance.CaptureLog();

        // ���⿡ Ư������̸� �ٷ� Ư�� ����� ��Ʈ��(��ֹ� ������) �ƴϸ� �׳� Explode
        if (_explodeType >= EBlockType.CROSS)
        {
            SetTargetTile(_tile);
            m_targetMatrix = _tile.GetComponent<Tile>().GetMatrix();
            m_contagiousObstacle = _contagiousObstacleType;
            m_targetType = _explodeType;

            _tile.GetComponent<Tile>().ChasingMoonExplode(_contagiousObstacleType, _explodeType);
        }
        else
        {
            SoundMgr.Instance.PlaySFX(ESFX.NOMAL_MATCH);

            _tile.GetComponent<Tile>().Explode(_contagiousObstacleType);
        }
    }
}