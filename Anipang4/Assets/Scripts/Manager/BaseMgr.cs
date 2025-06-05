using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public abstract class BaseMgr<T> : MonoBehaviour where T : BaseMgr<T>, new()
{
    #region �̱���
    static T _instance;
    static readonly object _lock = new object();
    static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
    #endregion

    void Awake()
    {
        #region �̱���
        if (_instance == null)
        {
            _instance = (T)this;
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
        if (_instance == this)
        {
            OnDestroyed();
            _instance = null;
        }
    }

    // �Ŵ��������� �̰ɷ� ȣ��
    protected virtual void OnAwake() { }
    protected virtual void OnDestroyed() { }
}
