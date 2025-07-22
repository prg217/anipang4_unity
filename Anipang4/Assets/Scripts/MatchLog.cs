using UnityEngine;
using UnityEngine.UI;

public class MatchLog : MonoBehaviour
{
    int m_index = -1;

    public void SetIndex(in int _index)
    {
        m_index = _index; 
    }

    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(LogButton);
    }

    void LogButton()
    {
        UIMgr.Instance.ShowCaptureLog(m_index);
    }
}
