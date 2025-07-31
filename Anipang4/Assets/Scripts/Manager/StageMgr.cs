using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using Random = UnityEngine.Random;

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
    SStageClearConditions m_stageClearConditions;
    #endregion

    #region Hint ���� ����
    // ��ġ�� ������ ��ϵ� ����
    List<List<GameObject>> m_matchOKs = new List<List<GameObject>>();
    bool m_hint = false;
    bool m_hintStart = false;

    Coroutine m_hintCoroutine; // ��Ʈ �ڷ�ƾ

    bool m_waitingMoveComplete = false; // �������� Ŭ����, ���� ���� �� ������ �Ϸ�
    #endregion

    #endregion ���� ��

    #region Get�Լ�
    public GameObject GetTile(in Vector2Int _matrix)
    { 
        if (m_tiles.ContainsKey(_matrix))
        {
            return m_tiles[_matrix];
        }

        return null;
    }
    public int GetMaxBlockType() { return m_maxBlockType; }
    public Vector2Int GetMaxMatrix() { return m_maxMatrix; }
    public Dictionary<EObstacleType, bool> GetClearObstacleTypes()
    {
        if (m_stageClearConditions.obstacleTypes != null)
        {
            Dictionary<EObstacleType, bool> types = new Dictionary<EObstacleType, bool>();

            for (int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
            {
                EObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
                bool clear = m_stageClearConditions.obstacleTypes[i].clear;
                types.Add(obstacleType, clear);
            }

            return types;
        }
        return null;
    }
    public Dictionary<EBlockType, bool> GetClearBlockTypes()
    {
        if (m_stageClearConditions.blockTypes != null)
        {
            Dictionary<EBlockType, bool> types = new Dictionary<EBlockType, bool>();

            for (int i = 0; i < m_stageClearConditions.blockTypes.Count; i++)
            {
                EBlockType blockType = m_stageClearConditions.blockTypes[i].type;
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
            Vector2Int randomMatrix = new Vector2Int(Random.Range(0, m_maxMatrix.x + 1), Random.Range(0, m_maxMatrix.y + 1));
            GameObject tile = GetTile(randomMatrix);
            // ��ȿ Ÿ���� ���
            if (tile.GetComponent<Tile>().GetMyBlockType() != EBlockType.NULL)
            {
                return tile;
            }
        }
    }

    public EBlockType GetMostNormalBlockType()
    {
        EBlockType mostType = EBlockType.NULL;
        int mostCount = 0;
        // �Ϲ� ��� Ÿ�� ���� ī��Ʈ
        for (int i = 0; i < (int)EBlockType.CROSS; i++)
        {
            int count = SearchTiles((EBlockType)i).Count;
            if (mostCount < count)
            {
                mostCount = count;
                mostType = (EBlockType)i;
            }
        }
        
        return mostType;
    }
    #endregion

    #region Set�Լ�
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
    void HandleTileExplode(EBlockType _type)
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
        // ��� Ÿ���� �̺�Ʈ ����
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            Tile tileScript = tile.Value.GetComponent<Tile>();
            tileScript.OnTileExplode += HandleTileExplode;
        }

        // ���� �� �� üũ(�������� ��� ������ �������� ���� ��� ���)
        StartCheckPossibleMatch();

        // UI�� Ŭ���� ���� �Ѱ���
        UIMgr.Instance.UpdateStageClearConditions(m_stageClearConditions);

        // �������� Ŭ���� üũ
        CheckStageClear();

        // BGM ���
        // ���� �������� �߰� �� ���������� ���� �� �� �ְ� ������ ��
        SoundMgr.Instance.PlayBGM(EBGM.STAGE);
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

    public void StartCheckPossibleMatch()
    {
        StartCoroutine(CheckPossibleMatch());
    }

    // �������� ��ġ�� �� �� �ֳ� Ȯ��
    IEnumerator CheckPossibleMatch()
    {
        m_matchOKs.Clear();

        // ������ �� �ִ� Ÿ���� ����� �����¿�� ������(�ӽ÷�) ���� ��ġ�� �Ǵ��� üũ
        // ��� ��ġ�� �� �ȴٸ� ������ �� ���� Ÿ���� �����ϰ� ��� Ÿ�� ������ ������ ��, �������� ����ϰ� ����� �ٲ�
        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                var(result, matchOK) = MatchMgr.Instance.SimulateBlockMove(tile);

                if (result)
                {
                    if (matchOK != null)
                    {
                        matchOK.Add(tile);
                        m_hint = true;

                        // ��Ʈ�� ���� ��ġ �Ǵ� ����� ������ ������
                        if (matchOK.Count >= 3)
                        {
                            m_matchOKs.Add(new List<GameObject>(matchOK));
                        }
                    }
                }
            }
        }

        // ��ġ �Ұ���
        if (!m_hint)
        {
            UIMgr.Instance.RandomPlacementUI(true);
            yield return new WaitForSeconds(0.5f);
            RandomPlacement();
            yield return new WaitForSeconds(0.5f);
            UIMgr.Instance.RandomPlacementUI(false);
        }
    }

    // ������ ��ġ
    void RandomPlacement()
    {
        List<GameObject> tiles = new List<GameObject>();
        List<EBlockType> blockTypes = new List<EBlockType>();
        List<EBlockType> saveBlockTypes = new List<EBlockType>();

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

                ETileType tileType = tile.GetComponent<Tile>().GetTileType(); 

                // Ÿ�� Ÿ���� ������ �� �ִ� ��쿡�� ����
                if (tileType == ETileType.MOVABLE)
                {
                    // ������ �� �ִ� Ÿ�� ����
                    tiles.Add(tile);

                    // ��� Ÿ�� ����
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();
                    blockTypes.Add(blockType);
                }
            }
        }
        saveBlockTypes = new List<EBlockType>(blockTypes);

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
                    blockTypes = new List<EBlockType>(saveBlockTypes);
                    loof = true;
                    break;
                }
            }
            loof = false;
        } while (loof);

        // �ٽ� �������� ��ġ�� �� �� �ִ��� Ȯ��
        StartCheckPossibleMatch();
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
                EBlockType blockType = m_stageClearConditions.blockTypes[i].type;
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
                EObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
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
        MoveMgr.Instance.OnEmptyMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr���� �� ���� ä��� �Լ� �Ϸ���� ���
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI�ʿ� Ŭ���� UI ���
        UIMgr.Instance.StageClear(true);
        yield return new WaitForSeconds(1f);
        // �ٽ� ȭ������ ���ƿ���
        UIMgr.Instance.StageClear(false);

        // ���� moveCount Ƚ���� ���� ������ ���(Ư�� ��� ����)�� Ư��������� ���� ����
        // ����, ����� ���� Ư�� ���
        EBlockType[] selectableBlockTypes = { EBlockType.CROSS, EBlockType.SUN, EBlockType.MOON };
        // �ߺ� ����
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
        while (StageInfo.MoveCount > 0)
        {
            StageInfo.MoveCount--;

            while (true)
            {
                // ���� ��� Ÿ���� Ư�� ���or�ٲ� �� ���� ����� ���
                if (usedPositions.Count >= (m_maxMatrix.x * m_maxMatrix.y))
                {
                    // �ٷ� MoveCount 0���� ����� ��������
                    StageInfo.MoveCount = 0;
                    break;
                }

                // ������ Ÿ�� �ϳ��� ����
                int x = Random.Range(0, m_maxMatrix.x + 1);
                int y = Random.Range(0, m_maxMatrix.y + 1);
                Vector2Int randomPos = new Vector2Int(x, y);

                // ������ �ߺ��Ǹ� �н�
                if (usedPositions.Contains(randomPos))
                {
                    continue;
                }
                usedPositions.Add(randomPos);

                GameObject tile = m_tiles[randomPos];
                EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();

                // Ÿ���� �Ϲ� ������� Ȯ��
                if (type > EBlockType.NONE && type < EBlockType.CROSS)
                {
                    // ������ Ư�� ������� ��ȯ(����, ����� ����)
                    EBlockType randomBlockType = selectableBlockTypes[Random.Range(0, selectableBlockTypes.Length)];
                    tile.GetComponent<Tile>().SetMyBlockType(randomBlockType);
                    break;
                }
            }

            yield return new WaitForSeconds(0.15f);
        }

        // Ư����ϵ��� ��Ʈ��
        foreach (KeyValuePair<Vector2Int, GameObject> tile in m_tiles)
        {
            EBlockType type = tile.Value.GetComponent<Tile>().GetMyBlockType();

            if (type != EBlockType.NULL && type >= EBlockType.CROSS)
            {
                tile.Value.GetComponent<Tile>().Explode(EObstacleType.NONE);
                MoveMgr.Instance.ActiveCheckEmpty();
                yield return new WaitForSeconds(0.5f);
            }
        }

        // �� ��Ʈ���� �������� ���
        UIMgr.Instance.ClearResult();
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
        MoveMgr.Instance.OnEmptyMoveCompleteFunction += OnMoveCompleted;

        // MoveMgr���� �� ���� ä��� �Լ� �Ϸ���� ���
        yield return new WaitUntil(() => !m_waitingMoveComplete);

        // UI�ʿ� ���ӿ��� UI ���
        UIMgr.Instance.GameOver();
    }

    // MoveMgr���� �� ���� ä��� �Լ� �Ϸ�
    void OnMoveCompleted()
    {
        m_waitingMoveComplete = false;
        // ���� ����
        MoveMgr.Instance.OnEmptyMoveCompleteFunction -= OnMoveCompleted;
    }

    // �������� ���� ����
    void StageInfoUpdate()
    {
        // ��ֹ� ���� �ʱ�ȭ
        foreach (EObstacleType type in Enum.GetValues(typeof(EObstacleType)))
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
                EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();
                StageInfo.AddObstacle(frontObstacleType, 1);
                EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                StageInfo.AddObstacle(backObstacleType, 1);
            }
        }
    }

    // ������ �Է��ϰ� �ش��ϴ� Ÿ�ϵ��� ��ȯ
    public List<GameObject> SearchTiles(EBlockType _blockType = EBlockType.NONE, EObstacleType _obstacleType = EObstacleType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // ��� Ÿ���� ���ǿ� �´���
                if (_blockType != EBlockType.NONE)
                {
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

                    if (blockType != _blockType)
                    {
                        continue;
                    }
                }

                // ��ֹ� Ÿ���� ���ǿ� �´���
                if (_obstacleType != EObstacleType.NONE)
                {
                    EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

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
    public List<GameObject> SearchTiles(EBlockType _blockType = EBlockType.NONE)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // ��� Ÿ���� ���ǿ� �´���
                if (_blockType != EBlockType.NONE)
                {
                    EBlockType blockType = tile.GetComponent<Tile>().GetMyBlockType();

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
    public List<GameObject> SearchTiles(EObstacleType _obstacleType)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // ��ֹ� Ÿ���� ���ǿ� �´���
                if (_obstacleType != EObstacleType.NONE)
                {
                    EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();
                    EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

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
    // �����ϰ� �˻�
    public List<GameObject> SearchTilesExcept(EObstacleType _obstacleType)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i <= m_maxMatrix.y; i++)
        {
            for (int j = 0; j <= m_maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = GetTile(matrix);

                if (tile.GetComponent<Tile>().GetMyBlockType() == EBlockType.NULL)
                {
                    continue;
                }

                // ��ֹ� Ÿ���� ���ǿ� �´���
                if (_obstacleType != EObstacleType.NONE)
                {
                    if (_obstacleType < EObstacleType.FRONT_END)
                    {
                        EObstacleType frontObstacleType = tile.GetComponent<Tile>().GetMyFrontObstacleType();

                        if (frontObstacleType != _obstacleType)
                        {
                            tiles.Add(tile);
                        }
                    }
                    else
                    {
                        EObstacleType backObstacleType = tile.GetComponent<Tile>().GetMyBackObstacleType();

                        if (backObstacleType != _obstacleType)
                        {
                            tiles.Add(tile);
                        }
                    }
                }
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
