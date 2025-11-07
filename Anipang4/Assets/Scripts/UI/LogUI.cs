using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    [Header("MatchLogView 필요 변수")]
    [SerializeField]
    GameObject m_logContent;
    [SerializeField]
    GameObject m_logPrefab;
    [Header("MatchCapture 필요 변수")]
    [SerializeField]
    Image m_matchCaptureImg;
    [SerializeField]
    TextMeshProUGUI m_matchCaptureLogText;
    [Header("MatchLogButton 필요 변수")]
    [SerializeField]
    GameObject m_matchLogViewUI;
    [Header("MatchLogView & MatchLogButton 필요 변수")]
    [SerializeField]
    GameObject m_matchCaptureUI;

    int m_maxLogCount = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LogUpdate(in List<string> _matchLogs)
    {
        // 최신 로그 추가
        CreateLog(_matchLogs[_matchLogs.Count - 1], _matchLogs.Count - 1);
    }

    void CreateLog(in string _matchLog, in int _index)
    {
        if (m_logPrefab == null || m_logContent == null)
        {
            return;
        }

        // 프리팹으로부터 버튼 생성
        GameObject newButton = Instantiate(m_logPrefab, m_logContent.transform);
        newButton.GetComponent<MatchLog>().SetMatchCaptureUI(m_matchCaptureUI);

        // 버튼을 맨 앞(상단)으로 이동
        newButton.transform.SetAsFirstSibling();

        // 버튼 텍스트 설정
        TextMeshProUGUI buttonTextComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = _matchLog;
        }

        // 인덱스 부여
        newButton.GetComponent<MatchLog>().SetIndex(_index);

        // 최대 개수 초과 시 오래된 항목 제거
        RemoveOldLog();
    }

    void ClearAllLogs()
    {
        for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(m_logContent.transform.GetChild(i).gameObject);
        }
    }

    void RemoveOldLog()
    {
        // 현재 자식 개수가 최대 개수를 초과하는지 확인
        int currentCount = gameObject.transform.childCount;

        if (currentCount > m_maxLogCount)
        {
            int itemsToRemove = currentCount - m_maxLogCount;

            // 가장 오래된 항목부터 제거
            for (int i = 0; i < itemsToRemove; i++)
            {
                int lastIndex = gameObject.transform.childCount - 1;

                if (lastIndex >= 0)
                {
                    Transform oldestItem = gameObject.transform.GetChild(lastIndex);
                    Destroy(oldestItem.gameObject);
                }
            }
        }
    }

    public void LogCaptureChangesButton()
    {
        m_matchLogViewUI.SetActive(!m_matchLogViewUI.activeSelf);
        m_matchCaptureUI.SetActive(!m_matchCaptureUI.activeSelf);
    }

    public void ShowCaptureLog(in int _index)
    {
        if (m_matchCaptureImg == null)
        {
            return;
        }

        if (m_matchCaptureLogText != null)
        {
            m_matchCaptureLogText.text = LogMgr.Instance.GetMatchLog(_index);
        }

        Texture2D tex = LogMgr.Instance.GetCaptureLog(_index);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        m_matchCaptureImg.sprite = sprite;
    }
}
