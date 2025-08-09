using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public abstract class BaseMgr<T> : MonoBehaviour where T : BaseMgr<T>, new()
{
    #region �̱���
    static T m_instance;
    static readonly object m_lock = new object();

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                lock (m_lock)
                {
                    if (m_instance == null)
                    {
                        m_instance = new T();
                    }
                }
            }
            return m_instance;
        }
    }
    #endregion

    void Awake()
    {
        #region �̱���
        if (m_instance == null)
        {
            m_instance = (T)this;
            DontDestroyOnLoad(gameObject);

            OnAwake();
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }

    protected virtual void OnDestroy()
    {
        if (m_instance == this)
        {
            OnDestroyed();
            m_instance = null;
        }
    }

    // �Ŵ��������� �̰ɷ� ȣ��
    protected virtual void OnAwake() { }
    protected virtual void OnDestroyed() { }
}
