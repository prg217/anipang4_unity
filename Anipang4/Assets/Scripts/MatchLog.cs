using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MatchLog : MonoBehaviour
{
    GameObject m_matchCaptureUI;

    int m_index = -1;

    public void SetMatchCaptureUI(GameObject _obj)
    {
        m_matchCaptureUI = _obj;
    }

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
        if (m_matchCaptureUI != null)
        {
            m_matchCaptureUI.GetComponent<LogUI>().ShowCaptureLog(m_index);
        }
    }
}
