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

    // ������ ���� ��ֹ� ��������Ʈ �ٲٱ�
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
            // ��������Ʈ �� ���̰� ��
            GetComponent<Renderer>().enabled = false;

            // Ÿ���� ������ �� �ִ� ���·� �����
            /* �߰� ���� */

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
