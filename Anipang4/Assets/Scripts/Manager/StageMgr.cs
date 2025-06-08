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
    #region ����

    bool m_gameEnd =false;

    [Header("���� ���")]
    [SerializeField]
    GameObject m_board;
    // Ÿ�� �ִ� ���
    Vector2Int m_maxMatrix = new Vector2Int(0, 0);
    // �ʿ� �ִ� Ÿ�ϵ�
    Dictionary<Vector2Int, GameObject> m_tiles = new Dictionary<Vector2Int, GameObject>();

    #region Stage ���� ����
    [Header("Stage ���� ����")]
    [SerializeField]
    int m_maxBlockType = 5;
    [SerializeField]
    int m_maxMoveCount = 20;

    [SerializeField]
    StageClearConditions m_stageClearConditions;
    #endregion

    #region Hint ���� ����
    // ��ġ�� ������ ��ϵ� ����
    List<GameObject> m_matchOK = new List<GameObject>();
    List<List<GameObject>> m_matchOKs = new List<List<GameObject>>();
    bool m_hint = false;
    bool m_hintStart = false;

    Coroutine m_hintCoroutine; // ��Ʈ �ڷ�ƾ

    bool m_waitingMoveComplete = false; // �������� Ŭ����, ���� ���� �� ������ �Ϸ�
    #endregion

    #endregion ���� ��

    #region Get�Լ�
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
            // ��ȿ Ÿ���� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() != BlockType.NULL)
            {
                return tile;
            }
        }
    }
    #endregion

    #region Set�Լ�
    public void SetMatchOK(in List<GameObject> _matchOK)
    {
        m_matchOK = new List<GameObject>(_matchOK);
    }
    public void SetHint(in bool _hint)
    {
        m_hint = _hint;
        if (m_hint == false)
        {
            // ������ ����Ǿ� �ִ� m_matchOKs Ÿ�ϵ鿡�� �ܰ����� ����� �� �� �ʱ�ȭ
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

    #region �̺�Ʈ
    // Ÿ���� Explode�� ����� ������ � Ÿ���� �������� ����
    void HandleTileExplode(BlockType _type)
    {
        StageInfo.AddBlock(_type, 1);
    }
    #endregion

    protected override void OnAwake()
    {
        #region Ÿ�� ���� ���
        // ������ �ڽ��� ���ε� �ҷ�����
        for (int i = 0; i < m_board.transform.childCount; i++)
        {
            Transform line = m_board.transform.GetChild(i);

            // ������ �ڽ��� Ÿ�ϵ� ���
            for (int j = 0; j < line.transform.childCount; j++)
            {
                Transform tile = line.transform.GetChild(j);

                if (tile.CompareTag("Tile"))
                {
                    Vector2Int matrix = tile.GetComponent<Tile>().GetMatrix();
                    m_tiles.Add(matrix, tile.gameObject);

                    // �ִ� ��� ����
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

        // m_stageInfo ���� �ʱ�ȭ
        StageInfo.Initialize(m_maxMoveCount);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ��� ���� ���
        foreach (BlockType type in Enum.GetValues(typeof(BlockType)))
        {
            //m_stageInfo.blockCounts.Add((BlockType)type, 0);
        }

        // ��� Ÿ���� �̺�Ʈ ����
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            Tile tileScript = tile.Value.GetComponent<Tile>();
            tileScript.OnTileExplode += HandleTileExplode;
        }

        // ��ֹ� ���� ���
        foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
        {
            //m_stageInfo.obstacleCounts.Add((ObstacleType)type, 0);
        }

        // ���� �� �� üũ(�������� ��� ������ �������� ���� ��� ���)
        CheckPossibleMatch();

        // UI�� Ŭ���� ���� �Ѱ���
        UIMgr.Instance.UpdateStageClearConditions(m_stageClearConditions);

        // �������� Ŭ���� üũ
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
            // ��Ʈ�� ������ �ڷ�ƾ �ߴ�
            StopCoroutine(m_hintCoroutine);
            m_hintCoroutine = null;
            m_hintStart = false;
        }
    }

    IEnumerator Hint()
    {
        m_hintStart = true;
        yield return new WaitForSeconds(8f);

        // ��ġ�� �ȴٸ� ��� ��ġ �Ǵ��� �����ߴٰ� �� �� ���� �˷��ֱ�
        if (m_matchOKs.Count > 0)
        {
            // �ܰ��� ����
            int random = Random.Range(0, m_matchOKs.Count);

            foreach (GameObject tile in m_matchOKs[random])
            {
                tile.GetComponent<Tile>().SetMyBlockActiveOutline();
            }
        }
        yield return new WaitForSeconds(5f);
        m_hintStart = false;
    }

    // �������� ��ġ�� �� �� �ֳ� Ȯ��
    public void CheckPossibleMatch()
    {
        m_matchOKs.Clear();

        // ������ �� �ִ� Ÿ���� ����� �����¿�� ������(�ӽ÷�) ���� ��ġ�� �Ǵ��� üũ
        // ��ġ�� �ϳ��� �ȴٸ� �ٷ� return
        // ��� ��ġ�� �� �ȴٸ� ������ �� ���� Ÿ���� �����ϰ� ��� Ÿ�� ������ ������ ��, �������� ����ϰ� ����� �ٲ�
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

            if (m_matchOK.Count >= 3/* �Ǵ� Ư�� ��� */)
            {
                m_matchOKs.Add(new List<GameObject>(m_matchOK));
            }
        }

        // ��ġ �Ұ���
        if (!m_hint)
        {
            Debug.Log("��ġ �Ұ���");
            RandomPlacement();
        }
    }

    // ������ ��ġ
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

                // Ÿ�� Ÿ���� ������ �� �ִ� ��쿡�� ����
                if (tileType == TileType.MOVABLE)
                {
                    // ������ �� �ִ� Ÿ�� ����
                    tiles.Add(tile);

                    // ��� Ÿ�� ����
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();
                    blockTypes.Add(blockType);
                }
            }
        }
        saveBlockTypes = new List<BlockType>(blockTypes);

        bool loof = false;
        do
        {
            // ������ ��ġ ����
            foreach (GameObject tile in tiles)
            {
                int random = Random.Range(0, blockTypes.Count);
                tile.GetComponent<Tile>().SetMyBlockType(blockTypes[random]);
                blockTypes.RemoveAt(random);

                // ���� �ٷ� ��ġ�� �ȴٸ� �����
                if (MatchMgr.Instance.CheckMatch(tile, false))
                {
                    blockTypes = new List<BlockType>(saveBlockTypes);
                    loof = true;
                    break;
                }
            }
            loof = false;
        } while (loof);

        // �ٽ� �������� ��ġ�� �� �� �ִ��� Ȯ��
        CheckPossibleMatch();
    }

    // Ŭ���� ���� Ȯ��
    public bool CheckStageClear()
    {
        // �ϳ��� false�� �����Ǹ� Ŭ���� ����
        bool clear = true;

        // ���� �������� ������ ������ ����
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
                    // ���� Ŭ���� ���� �޼��ߴ��� Ȯ��
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
                    // ���� Ŭ���� ���� �޼��ߴ��� Ȯ��
                    var temp = m_stageClearConditions.obstacleTypes[i];
                    temp.clear = true;
                    m_stageClearConditions.obstacleTypes[i] = temp;
                }
            }
        }

        // UI���� ����
        UIMgr.Instance.UpdateStageUI();

        // Ŭ�����ϸ�
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

        // MoveMgr���� Ŭ�� ���� �ߴ� ��û
        MoveMgr.Instance.StopClickMoving();

        // �������� �ִٸ� �������� �� �� ������ ��ٸ�
        m_waitingMoveComplete = true;
        // �̺�Ʈ ����
        MoveMgr.Instance.OnMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr���� �� ���� ä��� �Լ� �Ϸ���� ���
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // �������� �ִٸ� �������� �� �� ������ ��ٸ�

        // UI�ʿ� Ŭ���� UI ���

        // �ٽ� ȭ������ ���ƿ���

        // ���� moveCount Ƚ���� ���� ������ ���(Ư�� ��� ����)�� Ư��������� ���� ����

        // Ư����ϵ��� ��Ʈ��
        Debug.Log("�������� Ŭ����");
    }

    IEnumerator GameOver()
    {
        if (m_gameEnd)
        { yield break; }

        m_gameEnd = true;

        // MoveMgr���� Ŭ�� ���� �ߴ� ��û
        MoveMgr.Instance.StopClickMoving();

        // �������� �ִٸ� �������� �� �� ������ ��ٸ�
        m_waitingMoveComplete = true;
        // �̺�Ʈ ����
        MoveMgr.Instance.OnMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr���� �� ���� ä��� �Լ� �Ϸ���� ���
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI�ʿ� ���ӿ��� UI ���
        Debug.Log("���ӿ���");
    }

    // MoveMgr���� �� ���� ä��� �Լ� �Ϸ�
    void OnMoveCompleted()
    {
        m_waitingMoveComplete = false;
        // ���� ����
        MoveMgr.Instance.OnMoveCompleteFunction -= OnMoveCompleted;
    }

    // �������� ���� ����
    void StageInfoUpdate()
    {
        // ��ֹ� ���� �ʱ�ȭ
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

                // ��ֹ� ����
                ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();
                StageInfo.AddObstacle(frontObstacleType, 1);
                ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                StageInfo.AddObstacle(backObstacleType, 1);
            }
        }
    }

    // ������ �Է��ϰ� �ش��ϴ� Ÿ�ϵ��� ��ȯ
    public List<GameObject> SearchTiles(BlockType _blockType = BlockType.NONE, ObstacleType _obstacleType = ObstacleType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                // ��� Ÿ���� ���ǿ� �´���
                if (_blockType != BlockType.NONE)
                {
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // ��ֹ� Ÿ���� ���ǿ� �´���
                if (_obstacleType != ObstacleType.NONE)
                {
                    ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                    if (backObstacleType != _obstacleType && frontObstacleType != _obstacleType)
                    {
                        continue;
                    }
                }

                // �� ���ǿ� �´´ٸ� ����
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

                // ��� Ÿ���� ���ǿ� �´���
                if (_blockType != BlockType.NONE)
                {
                    BlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // �� ���ǿ� �´´ٸ� ����
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

                // ��ֹ� Ÿ���� ���ǿ� �´���
                if (_obstacleType != ObstacleType.NONE)
                {
                    ObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    ObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                    if (backObstacleType != _obstacleType && frontObstacleType != _obstacleType)
                    {
                        continue;
                    }
                }

                // �� ���ǿ� �´´ٸ� ����
                tiles.Add(tile);
            }
        }

        return tiles;
    }

    public void CheckGameOver()
    {
        // ���� moveCount�� 0�� �Ǿ��µ�, Ŭ���� ������ �������� ���ϸ� ���� ����
        if (StageInfo.MoveCount <= 0)
        {
            StageInfo.MoveCount = 0;

            // Ŭ���� üũ
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
