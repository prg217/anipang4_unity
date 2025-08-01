using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveMgr : BaseMgr<MoveMgr>
{
    #region ����

    [SerializeField]
    GameObject m_moveEffect;

    // ====== �巡�� Ŭ�� �� Ÿ�� 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ======================================

    int m_completeCount = 0;

    // ====== ���� ======
    bool m_isClickMoving = false;
    // �����̴� ��
    bool m_moving = false;
    bool m_specialClicked = false;
    // �ǵ�����
    bool m_reMoving = false;
    // Ŭ�� ������ ����
    bool m_clickOK = true;
    // ==================

    // ====== ����� ä��� ======
    // ����� ä��� ��
    bool m_emptyMoving = false;
    // ����� ä��� �߿� Ŭ��
    bool m_clickDuringEmptyMoving = false;
    // ����� ä��� Ȱ�� ����
    bool m_checkEmptyEnabled = true;

    List<Coroutine> checkEmptyCoroutines = new List<Coroutine>();
    // ==========================

    #endregion ���� ��

    #region Set�Լ�
    public void SetCheckEmptyEnabled(in bool _setting)
    {
        m_checkEmptyEnabled = _setting;

        if (!m_checkEmptyEnabled)
        {
            StopCheckEmpty();
        }
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
        Vector2 inputPosition = Vector2.zero;
        bool hasInput = false;
        bool isReleased = false;

        if (Input.GetMouseButton(0) && !m_moving)
        {
            inputPosition = Input.mousePosition;
            hasInput = true;
        }
        else if ((Input.touchCount > 0) && !m_moving)
        {
            inputPosition = Input.GetTouch(0).position;
            hasInput = true;
        }

        if (hasInput)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;

                // ���� ���ڸ� ä��� ���̶�� ���ڸ� ä��� �����̴��� ���߰� ���� ���� ó��
                if (m_emptyMoving)
                {
                    StopCheckEmpty();

                    m_emptyMoving = false;
                    m_pClickedTile1 = null;
                    m_pClickedTile2 = null;

                    m_clickDuringEmptyMoving = true;
                }

                if (m_pClickedTile1 == null)
                {
                    m_pClickedTile1 = clickedTransform.gameObject;

                    // Ư�� ����� ��� ���콺 �� �� Ŭ������ ��ġ(Random ����)
                    EBlockType type = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                    if (type >= EBlockType.CROSS && type != EBlockType.RANDOM)
                    {
                        m_specialClicked = true;
                    }
                }
                else
                {
                    // �̹� ����� Ÿ���� ������ �ִٸ� return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
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
            inputPosition = Input.mousePosition;
            isReleased = true;      
        }
        else if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) && !m_moving)
        {
            inputPosition = Input.GetTouch(0).position;
            isReleased = true;
        }

        if (isReleased)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                // Ư�� ����� �� �� Ŭ������ ���
                if (m_specialClicked)
                {
                    // �� ��ֹ��� �ִ� ��� ��Ʈ���� ����
                    if (m_pClickedTile1.GetComponent<Tile>().GetFrontObstacleEmpty())
                    {
                        // ���� �������� ���� moveCount����
                        ConsumeMove();

                        m_specialClicked = false;
                        m_isClickMoving = true;

                        MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                        // ��ġ �� �� ���� ä��� ����
                        if (!m_emptyMoving)
                        {
                            StartCheckEmpty();
                        }
                    }
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

        // ���� Ŭ���ؼ� �� ���� üũ
        if (!m_emptyMoving)
        {
            m_moving = true;
        }

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

        SoundMgr.Instance.PlaySFX(ESFX.BLOCK_SWAP);

        #region ��� �����̱�
        // Ÿ���� ������ �ִ� ��Ͽ��� ��� Ÿ�������� �����̶�� ��
        m_pClickedTile1.GetComponent<Tile>().SetBlockMove(m_pClickedTile2, m_emptyMoving);
        m_pClickedTile2.GetComponent<Tile>().SetBlockMove(m_pClickedTile1, m_emptyMoving);

        // ��� �����̴� ����Ʈ
        // ���� ������ ���� ����
        if (!m_emptyMoving)
        {
            StartCoroutine(ActiveMoveEffect());
        }
        #endregion

        // Ÿ�ϵ� ���� ���ΰ�ħ
        m_pClickedTile1.GetComponent<Tile>().Refresh();
        m_pClickedTile2.GetComponent<Tile>().Refresh();
    }

    IEnumerator ActiveMoveEffect()
    {
        Vector3 midPos = (m_pClickedTile1.transform.position + m_pClickedTile2.transform.position) / 2f;
        m_moveEffect.transform.position = midPos;

        // �����¿쿡 ���� ���� ������
        Vector2Int matrix1 = m_pClickedTile1.GetComponent<Tile>().GetMatrix();
        Vector2Int matrix2 = m_pClickedTile2.GetComponent<Tile>().GetMatrix();
        // ����
        if (matrix1.x - matrix2.x != 0)
        {
            m_moveEffect.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // ����
        else
        {
            m_moveEffect.transform.rotation = Quaternion.Euler(0, 0, 90f);
        }

        m_moveEffect.SetActive(true);

        // �����̴� �ð��� 0.5��
        yield return new WaitForSeconds(0.5f);

        m_moveEffect.SetActive(false);
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
        ETileType tileType1 = m_pClickedTile1.GetComponent<Tile>().GetTileType();
        ETileType tileType2 = m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // �� �� �ϳ��� ������ �� ���� ���¶�� �������� ����
        if (tileType1 == ETileType.IMMOVABLE || tileType2 == ETileType.IMMOVABLE)
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
            if (!m_reMoving && m_isClickMoving && !m_emptyMoving)
            {
                if (m_pClickedTile1 == null)
                {
                    return;
                }

                EBlockType type1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlockType();
                EBlockType type2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlockType();

                #region ��ֹ� ����
                EObstacleType obType1 = m_pClickedTile1.GetComponent<Tile>().GetPropagationObstacle();
                EObstacleType obType2 = m_pClickedTile2.GetComponent<Tile>().GetPropagationObstacle();
                EObstacleType obType = obType1;

                if (obType1 != EObstacleType.NONE)
                {
                    obType = obType1;
                }
                else if (obType2 != EObstacleType.NONE)
                {
                    obType = obType2;
                }
                #endregion

                // �� �� Ư�� ����� ���->Ư�� ��� �ռ�
                if (type1 >= EBlockType.CROSS && type2 >= EBlockType.CROSS)
                {
                    MatchMgr.Instance.SpecialCompositionExplode(m_pClickedTile1, m_pClickedTile2, obType);

                    if (type1 == EBlockType.RANDOM || type2 == EBlockType.RANDOM)
                    {
                        return;
                    }
                }
                // �ƴѰ��
                else
                {
                    #region ����+�Ϲ�

                    // ������ �Ϲ� ����� ���->���� Explode ����
                    if (type1 == EBlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile1, type2, obType);
                    }
                    else if (type2 == EBlockType.RANDOM)
                    {
                        MatchMgr.Instance.RandomMatch(m_pClickedTile2, type1, obType);
                    }

                    #endregion

                    // ��ġ �õ� �� ��ġ�� �� �Ǹ� ���󺹱�
                    bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                    bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                    if (type1 == EBlockType.RANDOM || type2 == EBlockType.RANDOM)
                    {
                        return;
                    }
                }
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

            // ����� ä��� ����
            if (!m_emptyMoving)
            {
                StartCheckEmpty();
            }

            if (m_clickDuringEmptyMoving)
            {
                m_emptyMoving = true;
            }
        }
    }

    public void StartCheckEmpty()
    {
        if (!m_checkEmptyEnabled)
        {
            return;
        }

        // ������ �̹� �۵��ϰ� �ִ� CheckEmpty�� �ִٸ� ���߰� �ٽ� �ϰ� ��
        foreach (Coroutine checkEmptyCoroutine in checkEmptyCoroutines)
        {
            if (checkEmptyCoroutine != null)
            {
                StopCoroutine(checkEmptyCoroutine);
            }
        }
        checkEmptyCoroutines.Clear();

        Coroutine coroutine = StartCoroutine(CheckEmpty());
        checkEmptyCoroutines.Add(coroutine);
    }

    void StopCheckEmpty()
    {
        // ������ �̹� �۵��ϰ� �ִ� CheckEmpty�� �ִٸ� ����
        foreach (Coroutine checkEmptyCoroutine in checkEmptyCoroutines)
        {
            if (checkEmptyCoroutine != null)
            {
                StopCoroutine(checkEmptyCoroutine);
            }
        }
        checkEmptyCoroutines.Clear();

        // �ٽ� ��Ʈ�� �� �� �ְ� ����
        StageMgr.Instance.SetHint(true);
        m_emptyMoving = false;
    }

    public void ActiveCheckEmpty()
    {
        SetCheckEmptyEnabled(true);
        StartCheckEmpty();
    }

    void ConsumeMove()
    {
        if (m_isClickMoving && !m_reMoving)
        {
            StageInfo.MoveCount--;
            StageMgr.Instance.CheckGameOver();
        }
    }

    // ���� �Ʒ��� �ִ� �� ���� ã��
    Vector2Int SearchLastDownEmptyTile(in Vector2Int _matrix)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        Vector2Int lastEmptyMatrix = new Vector2Int(-1, -1);

        for (int i = _matrix.y + 1; i <= maxMatrix.y; i++)
        {
            // �ִ� y�� ���ٸ�, ����� �־ �� ������ ���� ����
            if (_matrix.y >= maxMatrix.y)
            {
                return(new Vector2Int(-1, -1));
            }
            Vector2Int downMatrix = new Vector2Int(_matrix.x, i);
            GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

            if (downTile.GetComponent<Tile>().IsBlockEmpty())
            {
                lastEmptyMatrix = downMatrix;
            }
            // �� ������ �ƴ϶�� �ٷ� ����������
            else
            {
                break;
            }
        }

        return lastEmptyMatrix;
    }

    // ���� �밢�� �Ʒ��� �ִ� �� ���� ã��
    Vector2Int SearchLastDiagonalEmptyTile(in Vector2Int _matrix)
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        Vector2Int lastEmptyMatrix = new Vector2Int(-1, -1);
        Vector2Int testMatrix = _matrix;

        while (true)
        {
            Vector2Int leftMatrix = new Vector2Int(testMatrix.x - 1, testMatrix.y);
            Vector2Int rightMatrix = new Vector2Int(testMatrix.x + 1, testMatrix.y);

            if (DiagonalTest(testMatrix, leftMatrix))
            {
                lastEmptyMatrix = new Vector2Int(leftMatrix.x, leftMatrix.y + 1);
            }
            else if (DiagonalTest(testMatrix, rightMatrix))
            {
                lastEmptyMatrix = new Vector2Int(rightMatrix.x, rightMatrix.y + 1);
            }
            // �밢�� �Ʒ��� �� ������ ������ ��������
            else
            {
                break;
            }

            testMatrix = lastEmptyMatrix;
        }

        return lastEmptyMatrix;
    }

    public void EmptyMoving(in GameObject _startTile, in Vector2Int _point)
    {
        // _point�� ���� �� �̵�(�� ������ �ٷ� �����̵� ��Ű��)
        GameObject pointTile = StageMgr.Instance.GetTile(_point);

        _startTile.GetComponent<Tile>().SetBlockMove(pointTile, true);
        pointTile.GetComponent<Tile>().BlockTeleport(_startTile);

        _startTile.GetComponent<Tile>().Refresh();
        pointTile.GetComponent<Tile>().Refresh();

        if (_startTile.GetComponent<Tile>().IsEmptyCreateTile())
        {
            _startTile.GetComponent<Tile>().CreateBlock();
        }
    }

    public IEnumerator CheckEmpty()
    {
        // �� ���� üũ�ϴ� �߿��� �������� �Ŵ����� ��Ʈ�� �� �ϰ� ����
        StageMgr.Instance.SetHint(false);

        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        // SearchEmptyTile�� �̿��� ���� �� �� ���� Ÿ�� ��ġ(��������)
        // �밢���� �ִٸ� ���� ��->�밢��->���� ��(�� �ִٸ�)or�밢�� ``` �ݺ� ������ �̵�
        bool isEmpty = true;

        for (int i = maxMatrix.y; i >= 0; i--)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                // ����
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                // �̵� �Ұ� Ÿ���̶�� �н�
                if (tile.GetComponent<Tile>().GetTileType() == ETileType.IMMOVABLE)
                {
                    continue;
                }

                // �� �����ε�, ���� ����̶��
                if (tile.GetComponent<Tile>().IsEmptyCreateTile())
                {
                    tile.GetComponent<Tile>().CreateBlock();
                    continue;
                }

                Vector2Int lastEmpty = new Vector2Int(-1, -1);

                while (true)
                {
                    lastEmpty = SearchLastDownEmptyTile(matrix);
                    if (lastEmpty == new Vector2Int(-1, -1))
                    {
                        lastEmpty = SearchLastDiagonalEmptyTile(matrix);

                        // �� ������ ���� �� �������� �Ѿ
                        if (lastEmpty == new Vector2Int(-1, -1))
                        {
                            break;
                        }
                    }
                    break;
                }

                // �� ������ ���� �� �������� �Ѿ
                if (lastEmpty == new Vector2Int(-1, -1))
                {
                    continue;
                }

                // �� ������ ���� �̵�
                tile.GetComponent<Tile>().EmptyMoving(lastEmpty);
            }

            yield return new WaitForSeconds(0.05f);
        }

        // �� ���� ���� üũ
        // ���� �� ������ ������, �ű�� ����� ���� �� ���� ���(���� ��, ��, ������ ���� ��� ������ �� ���� Ÿ��) �н�
        isEmpty = DoubleCheckEmpty();

        if (isEmpty)
        {
            StartCheckEmpty();
            yield break;
        }


        // ��ġ üũ
        for (int length = 0; length <= maxMatrix.y; length++)
        {
            for (int i = maxMatrix.y; i >= 0; i--)
            {
                for (int j = 0; j <= maxMatrix.x; j++)
                {
                    Vector2Int matrix = new Vector2Int(j, i);
                    GameObject tile = StageMgr.Instance.GetTile(matrix);
                    EBlockType type = tile.GetComponent<Tile>().GetMyBlockType();
                    // Ư�� ����� �ƴ� ��쿡�� ��ġ ������ ��
                    if (type < EBlockType.CROSS)
                    {
                        var(result, matchCount, matchTiles) = MatchMgr.Instance.CheckMatchWithStatus(tile, false);

                        if (result)
                        {
                            // ��ġ �����ϸ� �� ��ġ ������ Ÿ�ϵ� ���ڸ� �� �˻�
                            // ->���� ���� ��ġ ī��Ʈ�� ���� Ÿ�Ϸ� ��Ʈ��
                            GameObject mostMatchCountTile = tile;

                            foreach (GameObject matchTile in matchTiles)
                            {
                                var (result2, matchCount2, _) = MatchMgr.Instance.CheckMatchWithStatus(tile, false);

                                if (result2)
                                {
                                    if (matchCount < matchCount2)
                                    {
                                        matchCount = matchCount2;
                                        mostMatchCountTile = matchTile;
                                    }
                                }
                            }

                            MatchMgr.Instance.CheckMatch(mostMatchCountTile);
                            // ��ġ�� �Ͼ�ٴ� ���� �� ������ ����ٴ� ��
                            isEmpty = true;
                        }
                    }
                }
            }
        }

        // ��ġ üũ �� �ٽ� �� ���� �˻�
        if (isEmpty)
        {
            StartCheckEmpty();
            yield break;
        }

        // �α� ����
        LogMgr.Instance.UpdateLog();

        // Ŭ���� Ȯ��
        StageMgr.Instance.CheckStageClear();

        // �������� Ŭ����, ���� ���� �� �Ϸ� ��ȣ ����
        OnEmptyMoveCompleteFunction?.Invoke();

        // ������ ��ġ�� �������� üũ
        StageMgr.Instance.StartCheckPossibleMatch();

        // �ٽ� ��Ʈ�� �� �� �ְ� ����
        StageMgr.Instance.SetHint(true);
        m_emptyMoving = false;
    }

    bool DoubleCheckEmpty()
    {
        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();

        for (int i = 0; i <= maxMatrix.y; i++)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);

                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    ETileType tileType = tile.GetComponent<Tile>().GetTileType();
                    if (tileType == ETileType.MOVABLE)
                    {
                        // �� ���� Ÿ���� ���
                        if (tile.GetComponent<Tile>().IsEmptyCreateTile())
                        {
                            return true;
                        }

                        #region ���� ��, ��, ������ ���� ��� ������ �� ���� üũ
                        Vector2Int leftUpMatrix = new Vector2Int(j - 1, i - 1);
                        Vector2Int upMatrix = new Vector2Int(j, i - 1);
                        Vector2Int rightUpMatrix = new Vector2Int(j + 1, i - 1);
                        if (EmptySpaceTest(rightUpMatrix) && EmptySpaceTest(upMatrix) && EmptySpaceTest(leftUpMatrix))
                        {
                            break;
                        }
                        #endregion

                        return true;
                    }
                }
            }
        }
        return false;
    }

    // CheckEmpty���� �밢�� �˻��� �� ���
    bool DiagonalTest(in Vector2Int _originalMatrix, in Vector2Int _checkMatrix)
    {
        GameObject originalTile = StageMgr.Instance.GetTile(_originalMatrix);

        if (originalTile == null)
        {
            return false;
        }

        // y�� ���� �� �˻��ؼ� ���ʿ� �׳� ����� ����(�� ������̰�)�� ���� �̵� �Ұ���� �밢�� �̵�
        for (int y = _checkMatrix.y; y >= 0; y--)
        {
            GameObject checkUpTile = StageMgr.Instance.GetTile(new Vector2Int(_checkMatrix.x, y));

            if (checkUpTile != null)
            {
                ETileType type = checkUpTile.GetComponent<Tile>().GetTileType();

                // ���� �����ִ� �� ���� �̵� �Ұ���� �밢�� �̵� �׽�Ʈ �õ�
                if (type == ETileType.IMMOVABLE)
                {
                    // ������ �� ���� ��� �� �˻�
                    Vector2Int downMatrix = new Vector2Int(_checkMatrix.x, _checkMatrix.y + 1);
                    GameObject downTile = StageMgr.Instance.GetTile(downMatrix);

                    if (downTile != null)
                    {
                        if (downTile.GetComponent<Tile>().IsBlockEmpty())
                        {
                            return true;
                        }
                    }
                    break;
                }

                // ������ �� �ִ� ����� �ִٸ� ��������
                if (!checkUpTile.GetComponent<Tile>().IsBlockEmpty())
                {
                    break;
                }

                // ���� ����̶�� ��������
                if (checkUpTile.GetComponent<Tile>().IsEmptyCreateTile())
                {
                    break;
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
            ETileType type = tile.GetComponent<Tile>().GetTileType();
            if (type == ETileType.IMMOVABLE)
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
        Debug.Log("�� ������");
        // Ŭ���ؼ� �����̴� �ൿ�� ���ϰ� ��
        m_clickOK = false;
    }
}
