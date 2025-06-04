using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    #region �̱���
    static GameMgr instance;

    MoveMgr moveMgr = new MoveMgr();
    UIMgr UIMgr = new UIMgr();

    public static GameMgr Instance
    {
        get
        {
            if (instance == null) instance = new GameMgr();
            return instance;
        }
    }
    #endregion

    #region ����

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �Ŵ������� ��� �ʱ�ȭ
        //moveMgr.Init();
        //UIMgr.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
