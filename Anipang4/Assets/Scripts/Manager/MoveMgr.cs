using UnityEngine;
using UnityEngine.Rendering;

public class MoveMgr : MonoBehaviour
{
    #region 싱글톤
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
        #region 싱글톤
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

    #region 변수

    // ====== 클릭 된 타일 1, 2 ======
    GameObject m_pClickedTile1;
    GameObject m_pClickedTile2;
    // ===============================
    // 움직이는 중
    bool m_moving = false;
    int m_completeCount = 0;

    #endregion 변수 끝

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
        // 마우스 왼쪽 클릭
        if (Input.GetMouseButton(0) && !m_moving)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;
                if (m_pClickedTile1 == null)
                {
                    Debug.Log("m_pClickedTile1: " + clickedTransform.name);
                    m_pClickedTile1 = clickedTransform.gameObject;
                }
                else
                {
                    // 이미 저장된 타일을 누르고 있다면 return
                    if (m_pClickedTile1 == clickedTransform.gameObject || m_pClickedTile2 == clickedTransform.gameObject)
                    {
                        return;
                    }

                    Debug.Log("m_pClickedTile2: " + clickedTransform.name);
                    m_pClickedTile2 = clickedTransform.gameObject;
                    m_moving = true;

                    /* 
                     * 블록을 바꾸는 함수가 들어갈 자리
                     * 함수 끝날 때 m_moving = false, 저장 된 타일들 초기화 잊지 말 것
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
                Debug.Log("리셋");
            }
        }
    }

    void Moving()
    {
        #region 둘 중 하나라도 움직일 수 없는 상태인지 확인
        // -1 : 블록 없음, 0 : 움직일 수 없음, 1 : 움직일 수 있음
        int tileType1 = (int)m_pClickedTile1.GetComponent<Tile>().GetTileType();
        int tileType2 = (int)m_pClickedTile2.GetComponent<Tile>().GetTileType();

        // 둘 중 하나라도 움직일 수 없는 상태라면 움직이지 않음
        if (tileType1 < 1 || tileType2 < 1)
        {
            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            return;
        }
        #endregion

        #region 블록 움직이기
        // 타일이 가지고 있는 블록에게 상대 타일쪽으로 움직이라고 함
        GameObject block1 = m_pClickedTile1.GetComponent<Tile>().GetMyBlock();
        GameObject block2 = m_pClickedTile2.GetComponent<Tile>().GetMyBlock();

        block1.GetComponent<Block>().SetMove(m_pClickedTile2);
        block2.GetComponent<Block>().SetMove(m_pClickedTile1);
        #endregion
    }

    public void MoveComplete()
    {
        m_completeCount++;
        
        // 블록 교환이 둘 다 완료가 되었다면
        if (m_completeCount >= 2)
        {
            // 타일들 정보 새로고침
            m_pClickedTile1.GetComponent<Tile>().Refresh();
            m_pClickedTile2.GetComponent<Tile>().Refresh();

            // 매치 시도 후 매치가 안 되면 원상복구

            m_moving = false;
            m_pClickedTile1 = null;
            m_pClickedTile2 = null;

            m_completeCount = 0;
        }
    }
}
