using UnityEngine;
using UnityEngine.Rendering;

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

                    /* 
                     * ����� �ٲٴ� �Լ��� �� �ڸ�
                     * �Լ� ���� �� m_moving = false, ���� �� Ÿ�ϵ� �ʱ�ȭ ���� �� ��
                    */
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
        m_moving = true;

        #region �� �� �ϳ��� ������ �� ���� �������� Ȯ��
        // -1 : ��� ����, 0 : ������ �� ����, 1 : ������ �� ����
        int tileType1 = (int)m_pClickedTile1.GetComponent<Tile>().GetTileType();
        int tileType2 = (int)m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // �� �� �ϳ��� ������ �� ���� ���¶�� �������� ����
        if (tileType1 < 1 || tileType2 < 1)
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
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // ��� ��ȯ�� �� �� �Ϸᰡ �Ǿ��ٸ�
        if (m_completeCount >= 2)
        {
            // Ÿ�ϵ� ���� ���ΰ�ħ
            m_pClickedTile1.GetComponent<Tile>().Refresh();
            m_pClickedTile2.GetComponent<Tile>().Refresh();

            if (!m_reMoving)
            {
                // ��ġ �õ� �� ��ġ�� �� �Ǹ� ���󺹱�
                bool match1 = MatchMgr.Instance.CheckMatch(m_pClickedTile1);
                bool match2 = MatchMgr.Instance.CheckMatch(m_pClickedTile2);

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

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;
            m_reMoving = false;

            m_completeCount = 0;

            // �������� �Ŵ����� �������� �˻� �䱸
            StageMgr.Instance.CheckStage();
        }
    }
}
