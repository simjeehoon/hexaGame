using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public int score;
    public int highScore;
    public int defaultBonus = 100;
    public int removeLength = 3;
    public String title = "Score: ";
    private TextMeshProUGUI textMeshPro;

    void loadHighScore()
    {
        
    }

    public void SaveHighScore()
    {

    }

    void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        
        score = 0;
        UpdateText();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void UpdateText()
    {
        textMeshPro.text = title + "\n" + score.ToString();
    }

    public void NormalIncrease(int count, int step=1)
    {
        int lengthBonus = count - removeLength + 1;
        int stepBonus = 1;
        for(int i = 2 ; i <= step ; ++i)
        {
            stepBonus *= 3;
        }
        score += defaultBonus * lengthBonus * stepBonus;
        UpdateText();
    }

    public void ItemIncrease(int count)
    {
        score += defaultBonus * count;
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
