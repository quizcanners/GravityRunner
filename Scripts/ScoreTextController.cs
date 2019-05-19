using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTextController : MonoBehaviour
{
    public TextMeshProUGUI text;

    private int displayedValue;

    public int targetValue;

    private float scoreToAdd;

    public float scoreAddSpeed;

    private void UpdateText() => text.text = displayedValue.ToString();
    
    public void Clear() {
        displayedValue = 0;
        targetValue = 0;
        UpdateText();
    }

    // Update is called once per frame
    void Update() {

        if (targetValue < displayedValue)
        {
            displayedValue = targetValue;
            UpdateText();
        }
        else
        {

            scoreToAdd += scoreAddSpeed * Time.deltaTime * (targetValue - displayedValue);

            if (scoreToAdd > 1)
            {
                displayedValue += Mathf.FloorToInt(scoreToAdd);
                scoreToAdd %= 1;
                UpdateText();
            }
        }
    }
}
