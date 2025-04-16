using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using static Unity.VisualScripting.Metadata;

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
        for (int i = 0; i < m_Board.transform.childCount; i++)
        {
            Transform line = m_Board.transform.GetChild(i);

            // ������ �ڽ��� Ÿ�ϵ� ���
            for (int j = 0; j < line.transform.childCount; j++)
            {
                Transform tile = line.transform.GetChild(j);

                if (tile.CompareTag("Tile"))
                {
                    Vector2Int matrix = tile.GetComponent<Tile>().GetMatrix();
                    m_Tiles.Add(matrix, tile.gameObject);
                }
            }
        }
        #endregion
    }

    #region ����

    [SerializeField]
    GameObject m_Board;
    // �ʿ� �ִ� Ÿ�ϵ�
    Dictionary<Vector2Int, GameObject> m_Tiles = new Dictionary<Vector2Int, GameObject>();

    #endregion ���� ��

    #region Get�Լ�
    public GameObject GetTile(in Vector2Int _Matrix) { return m_Tiles[_Matrix]; }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckStage()
    {

    }
}
