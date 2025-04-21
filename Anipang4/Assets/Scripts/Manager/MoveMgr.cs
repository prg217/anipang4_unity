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
        //Debug.Log(m_pClickedTile1);
        //Debug.Log(m_pClickedTile2);
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
            if (!m_reMoving)
            {
                // ��ġ �õ� �� ��ġ�� �� �Ǹ� ���󺹱�
                bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

                if (!m_emptyMoving)
                {
                    // �� �� ��ġ�� ���� �ʾҴٸ�
                    if (match1 == false && match2 == false)
                    {
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
                }
            }

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;

            m_completeCount = 0;

            // �� ���� ä���
            if (!m_emptyMoving)
            {
                //StartCoroutine(CheckEmpty());
            }
            StartCoroutine(CheckEmpty());
            // �������� �Ŵ����� �������� �˻� �䱸
            StageMgr.Instance.CheckStage();
        }
    }

    IEnumerator CheckEmpty()
    {
        m_emptyMoving = true;

        Vector2Int maxMatrix = StageMgr.Instance.GetMaxMatrix();
        // �Ʒ��ʺ��� ��� Ÿ�ϵ鿡�� �� ���� üũ�ϰ� �� �� �� Ÿ�Ͽ��� ��� �޾ƿ�
        for (int i = maxMatrix.y; i >= 0; i--)
        {
            bool isEmpty = false;
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);
                // Ÿ���� ����� ��� ��ϵ� �� Ÿ�Ͽ��� ����� ��������
                if (tile.GetComponent<Tile>().IsBlockEmpty())
                {
                    tile.GetComponent<Tile>().EmptyMoving();
                    isEmpty = true;
                }
            }
            // �ڷ�ƾ���� �ð� ���� �� ����ǰ� ��
            if (isEmpty)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        // �׸��� ��ġ üũ
        for (int i = maxMatrix.y; i >= 0; i--)
        {
            for (int j = 0; j <= maxMatrix.x; j++)
            {
                Vector2Int matrix = new Vector2Int(j, i);
                GameObject tile = StageMgr.Instance.GetTile(matrix);
                bool match = MatchMgr.Instance.CheckMatch(tile);
                if (match)
                {
                    //m_emptyMoving = false;
                    //yield break;
                }
            }
        }

        // ������ ��ġ�� �������� üũ

        m_emptyMoving = false;
    }
}
