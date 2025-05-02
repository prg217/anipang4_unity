using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class MoveMgr : MonoBehaviour
{
    #region �̱���
    static MoveMgr instance;

    public static MoveMgr Instance
    {
        get
        {
            if (instance == null) instance = new MoveMgr();
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

    // ====== Ŭ�� �� Ÿ�� 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ===============================
    // �����̴� ��
    bool m_moving = false;
    int m_completeCount = 0;
    bool m_isClickMoving = false;

    // �ǵ�����
    bool m_reMoving = false;
    // �� �� ä��� ��
    bool m_emptyMoving = false;

    #endregion ���� ��

    #region Set�Լ�
    public void SetClickedTileAndMoving(in GameObject _tile1, in GameObject _tile2)
    {
        m_pClickedTile1 = _tile1;
        m_pClickedTile2 = _tile2;

        Moving();
    }
    #endregion 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckClick();
    }

    void CheckClick()
    {
        // ���콺 ���� Ŭ��
        if (Input.GetMouseButton(0) && !m_moving)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;

                // Ư�� ����� ��� ���콺 �� �� Ŭ������ ��ġ(Random ����)
                /* �ڵ� �߰� ���� */

                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;
                }
                else
                {
                    // �̹� ����� Ÿ���� ������ �ִٸ� return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
                    }

                    m_pClickedTile2 = clickedTransform.gameObject;

                    m_isClickMoving = true;
                    Moving();
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && !m_moving)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;
                m_pClickedTile1 = null;
                m_pClickedTile2 = null;
            }
        }
    }

    void Moving()
    {
        if (m_pClickedTile1 == null || m_pClickedTile2 == null)
        {
            return;
        }

        m_moving = true;

        #region �� �� �ϳ��� ������ �� ���� �������� Ȯ��
        TileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        TileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // �� �� �ϳ��� ������ �� ���� ���¶�� �������� ����
        if (tileType1 == TileType.IMMOVABLE || tileType2 == TileType.IMMOVABLE)
        {
            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            return;
        }
        #endregion

        #region ��� �����̱�
        // Ÿ���� ������ �ִ� ��Ͽ��� ��� Ÿ�������� �����̶�� ��
        GameObject block1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlock();
        GameObject block2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlock();

        block1.GetComponent<Block>().SetMove(m_pClickedTile2);
        block2.GetComponent<Block>().SetMove(m_pClickedTile1);
        #endregion

        // Ÿ�ϵ� ���� ���ΰ�ħ
        m_pClickedTile1.GetComponent<Tile>().Refresh();
        m_pClickedTile2.GetComponent<Tile>().Refresh();
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // ��� ��ȯ�� �� �� �Ϸᰡ �Ǿ��ٸ�
        if (m_completeCount >= 2)
        {
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                // �� �� Ư�� ����� ���->Ư�� ��� �ռ�
                /* �ڵ� �߰� ���� */

                // ��ġ �õ� �� ��ġ�� �� �Ǹ� ���󺹱�
                bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                // �� �� ��ġ�� ���� �ʾҴٸ�
                if (match1 == false && match2 == false)
                {
                    Debug.LogWarning("���󺹱�");
                    // ���󺹱�
                    m_reMoving = true;
                    GameObject tempTile = m_pClickedTile1;
                    m_pClickedTile1 = m_pClickedTile2;
                    m_pClickedTile2 = tempTile;
                    Moving();

                    m_moving = false;
                    m_completeCount = 0;
                    return;
                }

                // ��ġ �� �� ���� ä��� ����
                StartCoroutine(CheckEmpty());
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;
            m_isClickMoving = false;

            m_completeCount = 0;
        }
    }

    public IEnumerator CheckEmpty()
    {
        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // �������� �ؿ� Ÿ���� �� ������� Ȯ�� �� �������� ����
        /*
         * 1. ������ �Ʒ� ����� �ִ�->�ű�� ����
         * 2. �Ʒ� ����� �ִµ� �밢�� �Ʒ��� �����->�� �Ʒ��� ���� ������ �� ���� Ÿ���̶�� �������� ����
        */
        for (int i = 0; i <= maxMatrix.y; i++)
        {
            bool isEmpty = false;
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                // ����
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                // �� Ÿ��
                Vector2Int downMatrix = new Vector2Int(j, i + 1);
                GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                if (downTile != null)
                {
                    if (downTile.GetComponent<Tile>().IsBlockEmpty())
                    {
                        // �ٷ� �� ����� ����ִ� ��� �ڽ��� ����� ������ ����
                        tile.GetComponent<Tile>().EmptyMoving(downTile);
                        isEmpty = true;
                    }
                    else
                    {
                        #region ����, ������ Ÿ���� ������ �� ���� ��� �밢�� �˻���� ��
                        Vector2Int leftMatrix = new Vector2Int(j - 1, i);
                        GameObject leftTile = StageMgr.Instance.GetTile(leftMatrix);
                        Vector2Int rightMatrix = new Vector2Int(j + 1, i);
                        GameObject rightTile = StageMgr.Instance.GetTile(rightMatrix);

                        if (DiagonalTest(matrix, leftMatrix))
                        {
                            isEmpty = true;
                        }
                        else
                        {
                            if (DiagonalTest(matrix, rightMatrix))
                            {
                                Debug.Log("������ �Ʒ� ����");
                                isEmpty = true;
                            }
                        }

                        #endregion
                    }
                }
            }
            // �ڷ�ƾ���� �ð� ���� �� ����ǰ� ��
            if (isEmpty)
            {
                yield return new WaitForSeconds(0.25f);
            }
        }

        // �׸��� ��ġ üũ
        for (int length = 0; length <= maxMatrix.y; length++)
        {
            for (int i = maxMatrix.y; i >= 0; i--)
            {
                for (int j = 0; j <= maxMatrix.x; j++)
                {
                    Vector2Int matrix = new Vector2Int(j, i);
                    GameObject tile = StageMgr.Instance.GetTile(matrix);
                    BlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // Ư�� ����� �ƴ� ��쿡�� ��ġ ������ ��
                    if (type < BlockType.CROSS)
                    {
                        MatchMgr.Instance.CheckMatch(tile);
                    }
                }
            }
        }

        // �� ���� ���� üũ(����Լ��� �� ���� ���� �� ���� �ϱ�)
        // ���� �� ������ ������, �ű�� ����� ���� �� ���� ���(���� ��, ��, ������ ���� ��� ������ �� ���� Ÿ��) �н�
        for (int i = 0; i <= maxMatrix.y; i++)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    TileType tileType = tile.GetComponent<Tile>().GetTileType();
                    if (tileType == TileType.MOVABLE)
                    {
                        #region ���� ��, ��, ������ ���� ��� ������ �� ���� üũ
                        Vector2Int leftUpMatrix = new Vector2Int(j - 1, i - 1);
                        Vector2Int upMatrix = new Vector2Int(j, i - 1);
                        Vector2Int rightUpMatrix = new Vector2Int(j + 1, i - 1);
                        if (EmptySpaceTest(rightUpMatrix) && EmptySpaceTest(upMatrix) && EmptySpaceTest(leftUpMatrix))
                        {
                            break;
                        }
                        #endregion

                        StartCoroutine(CheckEmpty());
                        yield break;
                    }
                }
            }
        }

        // ������ ��ġ�� �������� üũ
        StageMgr.Instance.CheckPossibleMatch();

        m_emptyMoving = false;
    }

    // CheckEmpty���� �밢�� �˻��� �� ���
    bool DiagonalTest(in Vector2Int _originalMatrix, in Vector2Int _checkMatrix)
    {
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);
        GameObject checkTile = StageMgr.Instance.GetTile(_checkMatrix);

        if (originalTile != null && checkTile != null)
        {
            TileType type = checkTile.GetComponent<Tile>().GetTileType();
            if (type == TileType.IMMOVABLE)
            {
                // ������ �� ���� ��� �� �˻�
                Vector2Int downMatrix = new Vector2Int(_checkMatrix.x, _checkMatrix.y + 1);
                GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                if (downTile != null)
                {
                    if (downTile.GetComponent<Tile>().IsBlockEmpty())
                    {
                        originalTile.GetComponent<Tile>().EmptyMoving(downTile);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // CheckEmpty���� �� ���� �˻� : Ÿ���� ������ �� ���� �˻�
    bool EmptySpaceTest(in Vector2Int _matrix)
    {
        GameObject tile = StageMgr.Instance.GetTile(_matrix);
        if (tile != null)
        {
            TileType type = tile.GetComponent<Tile>().GetTileType();
            if (type == TileType.IMMOVABLE)
            {
                return true;
            }
        }

        return false;
    }
}
