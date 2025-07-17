using System.Collections.Generic;
using UnityEngine;

public class LogMgr : BaseMgr<LogMgr>
{
    #region 변수

    // 매치 로그
    List<string> matchLogs = new List<string>();

    // 매치 직전 사진 로그


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
        // 글자로 변환->UI매니저에 스크롤뷰에 추가해달라고 요청
        // 현재 로그 저장... 오래된 기록이 뒤로 가야하니 자료구조 스택이 낫겠음(후입선출)->But 스택은 pop을 통해 방출을 해야지만 그 다음을 알 수 있음, 저장하고 확인만 하려는거라면 계속 마지막꺼만...
        // 왜냐면 스크롤뷰의 제일 위에 나와야 하는 것이 맨 마지막이니까

        // 직접 터트린 것만 나오게 해야하나?
        // 00(블록 타입과 행렬 번호?) 매치 : ㅁㅁ 생성
        // 클릭하면 연속해서 터진거...나오게...?
        // 그러려면 어떤게 연속적으로 터진거인지 체크하는 기준이 있어야 할듯(즉, 직접 매치가 시작이고 빈 공간 채우기가 완전히 끝났을 때 완료)

        // 일단 직접 매치(+특수 블록 생성)->빈공간 체크->빈공간 체크 매치(이것도 같은 함수에서 담당)
        // but 특수 블록으로 터트리는 경우 위와 같은 함수로 터트리지 않음,

        // 매치 기준 타입을 알려줌
        string log = _targetType.ToString();
        log += " : ";

        // 매치 되는 타일 행렬 알려줌
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();
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
        matchLogs.Add(log);

        // UIMgr에 연동
    }

    public void ChasingMoonExplodeLog(in GameObject _targetTile)
    {
        string log = "MOON 추격 터짐 -> ";
        log += _targetTile.GetComponent<Tile>().GetMyBlockType().ToString();
        log += " : ";

        // 매치 되는 타일 행렬 알려줌
        log += _targetTile.GetComponent<Tile>().GetMatrix().ToString();

        // 로그 저장
        matchLogs.Add(log);

        // UIMgr에 연동
    }

    // 다음 로그로 넘어감? <-아직 고민 중
    public void NextLog()
    {

    }
}
