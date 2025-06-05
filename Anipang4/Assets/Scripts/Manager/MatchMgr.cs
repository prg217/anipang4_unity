using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class MatchMgr : BaseMgr<MatchMgr>
{
    #region ����

    [Header("������ ���")]
    [SerializeField]
    GameObject m_chasingMoonPrefab; // �߰��ϴ� �� ������ ���

    Vector2Int m_targetMatrix;
    BlockType m_targetType;
    GameObject m_targetTile;

    int m_matchCount = 1; // ���� �������� ���
    int m_saveMatchCount = 1;
    List<GameObject> m_matchTiles = new List<GameObject>(); // ��ġ�� �Ǵ� Ÿ�ϵ� ����(�Ŀ� ��Ʈ��)
    List<GameObject> m_saveMatchTiles = new List<GameObject>();

    BlockType m_newBlock = BlockType.NONE; // Ư�� ����� ���ǿ� ���� ��� ������ ��
    ObstacleType m_contagiousObstacle = ObstacleType.NONE; // Ÿ�Ͽ� �߰� �� ��ֹ�

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
        if (m_targetType >= BlockType.CROSS && m_targetType != BlockType.NULL)
        {
            // Ư�� ��� ��Ʈ��
            if (_explode)
            {
                m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle);
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
            Debug.Log("������ Ÿ�� : " + m_newBlock);
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
            matrix.y = m_targetMatrix.y + i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
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
            // Ÿ���� ���� ��� �н�
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
            matrix.x = m_targetMatrix.x - i;

            GameObject tile = StageMgr.Instance.GetTile(matrix);
            // Ÿ���� ���� ��� �н�
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
            // Ÿ���� ���� ��� �н�
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

        BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
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
        // ��ġ�Ǵ� Ÿ�� �� ���ĵǴ� ��ֹ��� �ִ��� Ȯ��
        m_contagiousObstacle = ObstacleType.NONE;
        ObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();
        if (obstacleType != ObstacleType.NONE)
        {
            m_contagiousObstacle = obstacleType;
        }

        foreach (GameObject tile in m_matchTiles)
        {
            obstacleType = tile.GetComponent<Tile>().GetPropagationObstacle();
            // �ִٸ� ���ĵǴ� ��ֹ��� ��ֹ��� �߰��ϰ� ��
            if (obstacleType != ObstacleType.NONE)
            {
                m_contagiousObstacle = obstacleType;
            }
        }

        // Ư�� ��� ���ǿ� �ش� �� ���
        if (m_newBlock >= BlockType.CROSS)
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
            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
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

    public void SpecialExplode()
    {
        // ��ġ�Ǵ� Ÿ�� �� ���ĵǴ� ��ֹ��� �ִ��� Ȯ��
        m_contagiousObstacle = ObstacleType.NONE;
        ObstacleType obstacleType = m_targetTile.GetComponent<Tile>().GetPropagationObstacle();
        if (obstacleType != ObstacleType.NONE)
        {
            m_contagiousObstacle = obstacleType;
        }

        switch (m_targetType)
        {
            case BlockType.CROSS:
                CrossExplode();
                break;
            case BlockType.SUN:
                SunExplode();
                break;
            //case BlockType.RANDOM:
            //    RandomExplode();
            //    break;
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

    public void SpecialCompositionExplode(in GameObject _tile1, in GameObject _tile2, in ObstacleType _contagiousObstacle)
    {
        m_contagiousObstacle = _contagiousObstacle;

        BlockType type1 = _tile1.GetComponent<Tile>().GetMyBlockType();
        BlockType type2 = _tile2.GetComponent<Tile>().GetMyBlockType();

        m_targetTile = _tile2;
        m_targetMatrix = _tile2.GetComponent<Tile>().GetMatrix();

        _tile1.GetComponent<Tile>().Explode(_contagiousObstacle);
        _tile2.GetComponent<Tile>().Explode(_contagiousObstacle);

        switch (type1)
        {
            case BlockType.CROSS:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            DoubleCrossExplode();
                            break;
                        case BlockType.SUN:
                            CrossSunExplode();
                            break;
                        case BlockType.COSMIC:
                            CosmicExplode();
                            break;
                        case BlockType.MOON:
                            SpecialMoonExplode(BlockType.CROSS);
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
                            CrossSunExplode();
                            break;
                        case BlockType.SUN:
                            DoubleSunExplode();
                            break;
                        case BlockType.COSMIC:
                            CosmicExplode();
                            break;
                        case BlockType.MOON:
                            SpecialMoonExplode(BlockType.SUN);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case BlockType.COSMIC:
                {
                    CosmicExplode();
                }
                break;
            case BlockType.MOON:
                {
                    switch (type2)
                    {
                        case BlockType.CROSS:
                            SpecialMoonExplode(BlockType.CROSS);
                            break;
                        case BlockType.SUN:
                            SpecialMoonExplode(BlockType.SUN);
                            break;
                        case BlockType.COSMIC:
                            CosmicExplode();
                            break;
                        case BlockType.MOON:
                            SpecialMoonExplode(BlockType.MOON);
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
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

    void SpecialMoonExplode(in BlockType _specialType)
    {
        // �ֺ� 8�� ��Ʈ��
        SurroundingsExplode(1, 1);

        #region Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        // �� �߰� ������ ��ȯ
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
        // 5x5 �ı�
        SurroundingsExplode(2, 2);
    }

    public void RandomExplode(in BlockType _type, in ObstacleType _contagiousObstacle)
    {
        m_contagiousObstacle = _contagiousObstacle;
        // ���� ��Ų ��� Ÿ�� ���� �˾ƾ� ��
        // �� ��� Ÿ�Ե� ��ġ�ؼ� ����
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        #region �Ϲ� ����� ���
        if (_type < BlockType.CROSS)
        {
            for (int x = 0; x < maxMatrix.x; x++)
            {
                for (int y = 0; y < maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if (type == _type)
                    {
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                }
            }
        }
        #endregion

        #region Ư�� ����� ���
        if (_type >= BlockType.CROSS)
        {
            if (_type == BlockType.RANDOM || _type == BlockType.COSMIC)
            {
                CosmicExplode();
            }

            int randomType = Random.Range(0, StageMgr.Instance.GetMaxBlockType());

            for (int x = 0; x < maxMatrix.x; x++)
            {
                for (int y = 0; y < maxMatrix.y; y++)
                {
                    GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, y));
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    if ((int)type == randomType)
                    {
                        // �ش��ϴ� Ư�� ������� ��ȯ
                        tile.GetComponent<Tile>().SetMyBlockType(_type);
                        // ������ �� ��Ʈ����
                        /* �߰� ���� */
                        
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                }
            }
        }
        #endregion
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
                    tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                }
            }
        }
    }

    void MoonExplode()
    {
        // ����ĭ �ı� �� Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        #region ����ĭ �ı�
        // ���� �ı�
        m_targetTile.GetComponent<Tile>().Explode(m_contagiousObstacle);

        // �����¿� �ı�
        GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x - 1, m_targetMatrix.y));
        if (tile != null)
        {
            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            // �Ϲ� ����� ���
            if (type < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
            }
            // Ư�� ����� ���
            else if (type != BlockType.NULL)
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                if (m_targetTile != tile)
                {
                    CheckMatch(tile);
                }
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + 1, m_targetMatrix.y));
        if (tile != null)
        {
            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            // �Ϲ� ����� ���
            if (type < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
            }
            // Ư�� ����� ���
            else if (type != BlockType.NULL)
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                if (m_targetTile != tile)
                {
                    CheckMatch(tile);
                }
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y - 1));
        if (tile != null)
        {
            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            // �Ϲ� ����� ���
            if (type < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
            }
            // Ư�� ����� ���
            else if (type != BlockType.NULL)
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                if (m_targetTile != tile)
                {
                    CheckMatch(tile);
                }
            }
        }
        tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x, m_targetMatrix.y + 1));
        if (tile != null)
        {
            BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
            // �Ϲ� ����� ���
            if (type < BlockType.CROSS)
            {
                tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
            }
            // Ư�� ����� ���
            else if (type != BlockType.NULL)
            {
                // Ư�� ����� ��ġ �������� ��ġ ����
                if (m_targetTile != tile)
                {
                    CheckMatch(tile);
                }
            }
        }
        #endregion

        #region Ŭ���� ���� �� �ϳ� �������� ���� �ı�
        // �� �߰� ������ ��ȯ
        SummonChasingMoon(BlockType.NONE);
        #endregion
    }

    // �ֺ� ��Ʈ�� : Sun ���� �Լ����� ���, Ư�� ��� �ռ� Moon������ ���
    void SurroundingsExplode(in int _x, in int _y) 
    {
        for (int x = -_x; x <= _x; x++)
        {
            for (int y = -_y; y <= _y; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, m_targetMatrix.y + y));
                if (tile != null)
                {
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // �Ϲ� ����� ���
                    if (type < BlockType.CROSS)
                    {
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                    // Ư�� ����� ���
                    else if (type != BlockType.NULL)
                    {
                        // Ư�� ����� ��ġ �������� ��ġ ����
                        if (m_targetTile == tile)
                        {
                            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                        }
                        else
                        {
                            CheckMatch(tile);
                        }
                    }
                }
            }
        }
    }

    // ���μ��� ��Ʈ�� : Cross ���� �Լ����� ���
    void LengthAndWidthExplode(in int _x, in int _y)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // �̹� ó���� Ÿ�� ����
        HashSet<GameObject> processedTile = new HashSet<GameObject>();

        for (int y = -_y; y <= _y; y++)
        {
            for (int x = 0; x <= maxMatrix.x; x++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(x, m_targetMatrix.y + y));

                if (tile != null)
                {
                    // �̹� ó���� Ÿ�� �ǳʶ�
                    if (processedTile.Contains(tile))
                        continue;

                    processedTile.Add(tile);

                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // �Ϲ� ����� ���
                    if (type < BlockType.CROSS)
                    {
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                    // Ư�� ����� ���
                    else if (type != BlockType.NULL)
                    {
                        // Ư�� ����� ��ġ �������� ��ġ ����
                        if (m_targetTile == tile)
                        {
                            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                        }
                        else
                        {
                            CheckMatch(tile);
                        }
                    }
                }
            }
        }

        for (int x = -_x; x <= _x; x++)
        {
            for (int y = 0; y <= maxMatrix.y; y++)
            {
                GameObject tile = StageMgr.Instance.GetTile(new Vector2Int(m_targetMatrix.x + x, y));

                if (tile != null)
                {
                    // �̹� ó���� Ÿ�� �ǳʶ�
                    if (processedTile.Contains(tile))
                        continue;

                    processedTile.Add(tile);

                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // �Ϲ� ����� ���
                    if (type < BlockType.CROSS)
                    {
                        tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                    }
                    // Ư�� ����� ���
                    else if (type != BlockType.NULL)
                    {
                        // Ư�� ����� ��ġ �������� ��ġ ����
                        if (m_targetTile == tile)
                        {
                            tile.GetComponent<Tile>().Explode(m_contagiousObstacle);
                        }
                        else
                        {
                            CheckMatch(tile);
                        }
                    }
                }
            }
        }
    }

    public void SoloExplode(in GameObject _tile, in ObstacleType _contagiousObstacleType = ObstacleType.NONE)
    {
        _tile.GetComponent<Tile>().Explode(_contagiousObstacleType);
        // �� ���� üũ
        StartCoroutine(MoveMgr.Instance.CheckEmpty());
    }
}