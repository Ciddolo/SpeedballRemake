using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    private TextMeshProUGUI redScoreGameText;
    private TextMeshProUGUI blueScoreGameText;
    private TextMeshProUGUI timerText;
    private float minutes;
    private float seconds;

    void Start()
    {
        redScoreGameText = GameObject.Find("RedScore").GetComponent<TextMeshProUGUI>();
        blueScoreGameText = GameObject.Find("BlueScore").GetComponent<TextMeshProUGUI>();
        timerText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        Score();
        if (GameManager.StateOfGame == GameState.Playing)
            Timer();
    }

    private void Score()
    {
        redScoreGameText.text = GameManager.RedScore.ToString();
        blueScoreGameText.text = GameManager.BlueScore.ToString();
    }

    public void ResetTimer()
    {
        seconds = 0.0f;
        minutes = 0.0f;
    }

    private void Timer()
    {
        seconds += Time.deltaTime;
        if (seconds >= 60.0f)
        {
            seconds = 0.0f;
            minutes++;
        }
        timerText.text = minutes.ToString("00") + ":" + Mathf.FloorToInt(seconds).ToString("00");
    }
}
