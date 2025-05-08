using UnityEngine;

public class Prison : Obstacle
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 레벨에 따라 장애물 스프라이트 바꾸기
    public override void SetLevel(int _level)
    {
        m_level = _level;
    }
}
