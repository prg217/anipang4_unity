using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LogMgr : BaseMgr<LogMgr>
{
    #region ����

    [SerializeField]
    Camera m_captureCamera;

    // ��ġ �α�
    List<string> m_matchLogs = new List<string>();

    // ��ġ ���� ���� �α�
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
        // ��ġ ���� Ÿ���� �˷���
        string log = _targetType.ToString();
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();
        log += " : ";

        // ��ġ �Ǵ� Ÿ�� ��� �˷���
        foreach (var tile in _matchTiles)
        {
            log += ", ";
            log += tile.GetComponent<Tile>().GetMatrix().ToString();
        }
        
        // ���� �����Ǵ� Ư�� ���
        if (_newBlockType != BlockType.NONE)
        {
            log += " = ";
            log += _newBlockType.ToString();
            log += "����";
        }

        // �α� ����
        m_matchLogs.Add(log);
    }

    public void ChasingMoonExplodeLog(in GameObject _targetTile, in BlockType _explodeType)
    {
        string log = "MOON �߰� ���� -> ";
        log += _explodeType;
        log += " : ";

        // ��ġ �Ǵ� Ÿ�� ��� �˷���
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();

        // �α� ����
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
