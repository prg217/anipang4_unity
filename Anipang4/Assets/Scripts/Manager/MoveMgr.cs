using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;

public class MoveMgr : BaseMgr<MoveMgr>
{
    #region ����

    // ====== �巡�� Ŭ�� �� Ÿ�� 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ======================================
    bool m_specialClicked = false;
    // �����̴� ��
    bool m_moving = false;
    int m_completeCount = 0;
    bool m_isClickMoving = false;
    // ������ �Ϸ�
    bool m_moveComplete = false;

    // �ǵ�����
    bool m_reMoving = false;
    // �� �� ä��� ��
    bool m_emptyMoving = false;

    bool m_clickOK = true;

    #endregion ���� ��

    #region Set�Լ�
    public void SetClickedTileAndMoving(in GameObject _tile1, in GameObject _tile2)
    {
        m_pClickedTile1 = _tile1;
        m_pClickedTile2 = _tile2;

        Moving();
    }
    #endregion

    public event System.Action OnEmptyMoveCompleteFunction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_clickOK)
        {
            CheckClick();
        }
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
                Debug.Log("��������� ���µ�");
                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;

                    // Ư�� ����� ��� ���콺 �� �� Ŭ������ ��ġ(Random ����)
                    BlockType type = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                    if (type >= BlockType.CROSS && type != BlockType.RANDOM)
                    {
                        m_specialClicked = true;
                    }
                }
                else
                {
                    Debug.Log("���� �ȿ���");
                    // �̹� ����� Ÿ���� ������ �ִٸ� return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
                    }

                    // ���� ���ڸ� ä��� ���̶�� ���ڸ� ä��� �����̴��� ���߰� ���� ���� ó��
                    if (m_emptyMoving)
                    {
                        Debug.Log("����!");
                        StopCoroutine(CheckEmpty());
                        m_emptyMoving = false;
                    }

                    m_pClickedTile2 = clickedTransform.gameObject;

                    m_specialClicked = false;
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
                // Ư�� ����� �� �� Ŭ������ ���
                if (m_specialClicked)
                {
                    // ���� �������� ���� moveCount����
                    ConsumeMove();

                    m_specialClicked = false;
                    m_isClickMoving = true;

                    ObstacleType obType = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                    MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    // ��ġ �� �� ���� ä��� ����
                    StartCoroutine(CheckEmpty());
                }

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

        // �� �� �ϳ��� ������ �� ���� �������� Ȯ��
        if (IsMovementImpossible())
        {
            return;
        }
        
        // ���ڸ� ä��Ⱑ �ƴ� ������ ����
        if (!m_emptyMoving)
        {
            // �����¿쿡 �ִ� Ÿ������ Ȯ��
            if (!CheckAdjacentTile())
            {
                return;
            }
        }

        #region �̸� ��ġ �õ� �� reMoving ������� �Ǵ�
        if (!m_reMoving && m_isClickMoving)
        {
            bool match1 = MatchMgr.Instance.SimulationMatch(m_pClickedTile1, m_pClickedTile2);
            bool match2 = MatchMgr.Instance.SimulationMatch(m_pClickedTile2, m_pClickedTile1);

            if (match1 == false && match2 == false)
            {
                // ���󺹱�
                m_reMoving = true;
            }
        }
        #endregion

        // ���� �������� ���� moveCount����
        ConsumeMove();

        #region ��� �����̱�
        // Ÿ���� ������ �ִ� ��Ͽ��� ��� Ÿ�������� �����̶�� ��
        GameObject block1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlock();
        GameObject block2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlock();

        block1.GetComponent<Block>().SetMove(m_pClickedTile2, m_emptyMoving);
        block2.GetComponent<Block>().SetMove(m_pClickedTile1, m_emptyMoving);
        #endregion
        m_moveComplete = false;

        // Ÿ�ϵ� ���� ���ΰ�ħ
        m_pClickedTile1.GetComponent<Tile>().Refresh();
        m_pClickedTile2.GetComponent<Tile>().Refresh();
    }

    bool CheckAdjacentTile()
    {
        // �����¿� üũ
        Vector2Int matrix1 = m_pClickedTile1.GetComponent<Tile>().GetMatrix();
        Vector2Int matrix2 = m_pClickedTile2.GetComponent<Tile>().GetMatrix();

        // ����ư �Ÿ�(�Ÿ� �ּ� ��)
        int distance = Mathf.Abs(matrix1.x - matrix2.x) + Mathf.Abs(matrix1.y - matrix2.y);

        if (distance == 1)
        {
            return true;
        }

        return false;
    }

    bool IsMovementImpossible()
    {
        TileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        TileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // �� �� �ϳ��� ������ �� ���� ���¶�� �������� ����
        if (tileType1 == TileType.IMMOVABLE || tileType2 == TileType.IMMOVABLE)
        {
            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            return true;
        }

        return false;
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // ��� ��ȯ�� �� �� �Ϸᰡ �Ǿ��ٸ�
        if (m_completeCount >= 2)
        {
            m_moveComplete = true;
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                BlockType type1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                BlockType type2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlockType();

                #region ��ֹ� ����
                ObstacleType obType1 = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                ObstacleType obType2 = m_pClickedTile2.GetComponent<Tile>().GetPropagationObstacle();
                ObstacleType obType = obType1;

                if (obType1 != ObstacleType.NONE)
                {
                    obType = obType1;
                }
                else if (obType2 != ObstacleType.NONE)
                {
                    obType = obType2;
                }
                #endregion

                // �� �� Ư�� ����� ���->Ư�� ��� �ռ�
                if (type1 >= BlockType.CROSS && type2 >= BlockType.CROSS)
                {
                    MatchMgr.Instance.SpecialCompositionExplode(m_pClickedTile1, m_pClickedTile2, obType);
                }
                // �ƴѰ��
                else
                {
                    #region ����+�Ϲ�

                    // ������ �Ϲ� ����� ���->���� Explode ����
                    if (type1 == BlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomExplode(type2, obType);
                    }
                    else if (type2 == BlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomExplode(type1, obType);
                    }

                    #endregion

                    // ��ġ �õ� �� ��ġ�� �� �Ǹ� ���󺹱�
                    bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);
                }

                // ��ġ �� �� ���� ä��� ����
                StartCoroutine(CheckEmpty());
            }
            else if (m_reMoving)
            {
                // �� �� ��ġ�� ���� �ʾҴٸ�
                // ���󺹱�
                GameObject tempTile = m_pClickedTile1;
                m_pClickedTile1 = m_pClickedTile2;
                m_pClickedTile2 = tempTile;
                Moving();
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;
            m_isClickMoving = false;

            m_completeCount = 0;
        }
    }

    void ConsumeMove()
    {
        if (m_isClickMoving && !m_reMoving)
        {
            StageInfo.MoveCount--;
            StageMgr.Instance.CheckGameOver();
        }
    }

    public IEnumerator CheckEmpty()
    {
        // �� ���� üũ�ϴ� �߿��� �������� �Ŵ����� ��Ʈ�� �� �ϰ� ����
        StageMgr.Instance.SetHint(false);

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
                                isEmpty = true;
                            }
                        }

                        #endregion
                    }
                }
            }
            // ������ ������ ����ǰ� ��
            if (isEmpty)
            {
                yield return new WaitUntil(() => m_moveComplete);
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

                        // �� ������ ���� �� �ٽ� ó������
                        StartCoroutine(CheckEmpty());
                        yield break;
                    }
                }
            }
        }

        // Ŭ���� Ȯ��
        StageMgr.Instance.CheckStageClear();

        // �������� Ŭ����, ���� ���� �� �Ϸ� ��ȣ ����
        OnEmptyMoveCompleteFunction?.Invoke();

        // ������ ��ġ�� �������� üũ
        StageMgr.Instance.CheckPossibleMatch();

        // �ٽ� ��Ʈ�� �� �� �ְ� ����
        StageMgr.Instance.SetHint(true);
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

    // CheckEmpty���� �� ���� �˻� : Ÿ���� ������ �� ���� �˻�(������ �� ���� �� true)
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
            else
            {
                return false;
            }
        }

        return true;
    }

    public void StopClickMoving()
    {
        // Ŭ���ؼ� �����̴� �ൿ�� ���ϰ� ��
        m_clickOK = false;
    }
}
