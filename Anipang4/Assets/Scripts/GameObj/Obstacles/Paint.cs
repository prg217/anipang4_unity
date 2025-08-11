using System;
using UnityEngine;

public class Paint : Obstacle
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeSprite();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void ChangeSprite()
    {
        string spritePath = "Obstacle/Paint/OB_board_paint_base";

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
