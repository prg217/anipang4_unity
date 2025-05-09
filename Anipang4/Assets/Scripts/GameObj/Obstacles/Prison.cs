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

        ChangeSprite();
    }

    public override void AddLevel(int _addLevel)
    {
        m_level += _addLevel;
        if (m_level < 0)
        {
            m_level = 0;
        }

        ChangeSprite();
    }

    void ChangeSprite()
    {
        if (m_level == 0)
        {
            // 스프라이트 안 보이게 함
            GetComponent<Renderer>().enabled = false;

            // 타일을 움직일 수 있는 상태로 만들기
            /* 추가 예정 */

            return;
        }

        GetComponent<Renderer>().enabled = true;

        string spritePath = "Obstacle/Prison/prison_";
        spritePath += m_level.ToString();
        spritePath += ".png";

        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            //targetImage.sprite = newSprite;
        }
    }
}
