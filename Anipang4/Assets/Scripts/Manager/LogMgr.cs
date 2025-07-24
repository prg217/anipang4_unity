using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LogMgr : BaseMgr<LogMgr>
{
    #region 변수

    [SerializeField]
    Camera m_captureCamera;

    // 매치 로그
    List<string> m_matchLogs = new List<string>();

    // 매치 직전 사진 로그
    List<Texture2D> m_captureLogs = new List<Texture2D>();

    #endregion

    public string GetMatchLog(in int _index)
    {
        return m_matchLogs[_index];
    }
    public Texture2D GetCaptureLog(in int _index)
    {
        return m_captureLogs[_index];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMatchLog(in BlockType _targetType, in GameObject _targetTile, in List<GameObject> _matchTiles, in BlockType _newBlockType = BlockType.NONE)
    {
        // 매치 기준 타입을 알려줌
        string log = _targetType.ToString();
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();
        log += " : ";

        // 매치 되는 타일 행렬 알려줌
        foreach (var tile in _matchTiles)
        {
            log += ", ";
            log += tile.GetComponent<Tile>().GetMatrix().ToString();
        }
        
        // 새로 생성되는 특수 블록
        if (_newBlockType != BlockType.NONE)
        {
            log += " = ";
            log += _newBlockType.ToString();
            log += "생성";
        }

        // 로그 저장
        m_matchLogs.Add(log);
    }

    public void ChasingMoonExplodeLog(in GameObject _targetTile, in BlockType _explodeType)
    {
        string log = "MOON 추격 터짐 -> ";
        log += _explodeType;
        log += " : ";

        // 매치 되는 타일 행렬 알려줌
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();

        // 로그 저장
        m_matchLogs.Add(log);
    }

    public void CaptureLog()
    {
        if (m_captureCamera != null)
        {
            if (m_captureCamera.GetComponent<CaptureCamera>() != null)
            {
                m_captureLogs = new List<Texture2D>(m_captureCamera.GetComponent<CaptureCamera>().Capture());
            }
        }
    }

    public void UpdateLog()
    {
        UIMgr.Instance.LogUpdate(m_matchLogs);
    }
}
