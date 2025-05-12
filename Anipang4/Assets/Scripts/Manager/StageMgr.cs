using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;
using System.Collections;
using Unity.Burst.CompilerServices;

public class StageMgr : MonoBehaviour
{
    #region �̱���
    static StageMgr instance;

    public static StageMgr Instance
    {
        get
        {
            if (instance == null) instance = new StageMgr();
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
    }

    #region ����

    [SerializeField]
    GameObject m_board;
    // Ÿ�� �ִ� ���
    Vector2Int m_maxMatrix = new Vector2Int(0, 0);
    // �ʿ� �ִ� Ÿ�ϵ�
    Dictionary<Vector2Int, GameObject> m_tiles = new Dictionary<Vector2Int, GameObject>();

    #region Stage ���� ����
    [SerializeField]
    int m_maxBlockType = 5;
    [SerializeField]
    //int m_maxMove = 20;
    #endregion

    #region Hint ���� ����
    // ��ġ�� ������ ��ϵ� ����
    List<GameObject> m_matchOK = new List<GameObject>();
    List<List<GameObject>> m_matchOKs = new List<List<GameObject>>();
    bool m_hint = false;
    bool m_hintStart = false;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ���� �� �� üũ(�������� ��� ������ �������� ���� ��� ���)
        CheckPossibleMatch();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_hint && !m_hintStart)
        {
            StartCoroutine(Hint());
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

                if (MatchMgr.Instance.SimulationMatch(tile))
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
    public void CheckStage()
    {

    }
}
