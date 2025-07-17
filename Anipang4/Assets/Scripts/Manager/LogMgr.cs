using System.Collections.Generic;
using UnityEngine;

public class LogMgr : BaseMgr<LogMgr>
{
    #region ����

    // ��ġ �α�
    List<string> matchLogs = new List<string>();

    // ��ġ ���� ���� �α�


    #endregion

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
        //List<GameObject> m_matchTiles = new List<GameObject>();
        // ���ڷ� ��ȯ->UI�Ŵ����� ��ũ�Ѻ信 �߰��ش޶�� ��û
        // ���� �α� ����... ������ ����� �ڷ� �����ϴ� �ڷᱸ�� ������ ������(���Լ���)->But ������ pop�� ���� ������ �ؾ����� �� ������ �� �� ����, �����ϰ� Ȯ�θ� �Ϸ��°Ŷ�� ��� ����������...
        // �ֳĸ� ��ũ�Ѻ��� ���� ���� ���;� �ϴ� ���� �� �������̴ϱ�

        // ���� ��Ʈ�� �͸� ������ �ؾ��ϳ�?
        // 00(��� Ÿ�԰� ��� ��ȣ?) ��ġ : ���� ����
        // Ŭ���ϸ� �����ؼ� ������...������...?
        // �׷����� ��� ���������� ���������� üũ�ϴ� ������ �־�� �ҵ�(��, ���� ��ġ�� �����̰� �� ���� ä��Ⱑ ������ ������ �� �Ϸ�)

        // �ϴ� ���� ��ġ(+Ư�� ��� ����)->����� üũ->����� üũ ��ġ(�̰͵� ���� �Լ����� ���)
        // but Ư�� ������� ��Ʈ���� ��� ���� ���� �Լ��� ��Ʈ���� ����,

        // ��ġ ���� Ÿ���� �˷���
        string log = _targetType.ToString();
        log += " : ";

        // ��ġ �Ǵ� Ÿ�� ��� �˷���
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();
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
        matchLogs.Add(log);

        // UIMgr�� ����
    }

    public void ChasingMoonExplodeLog(in GameObject _targetTile)
    {
        string log = "MOON �߰� ���� -> ";
        log += _targetTile.GetComponent<Tile>().GetMyBlockType().ToString();
        log += " : ";

        // ��ġ �Ǵ� Ÿ�� ��� �˷���
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();

        // �α� ����
        matchLogs.Add(log);

        // UIMgr�� ����
    }

    // ���� �α׷� �Ѿ? <-���� ��� ��
    public void NextLog()
    {

    }
}
