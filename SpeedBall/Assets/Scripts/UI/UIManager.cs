using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private const int DEFAULT_NUMBER_FONT_SIZE = 60;
    private const int DEFAULT_WORD_FONT_SIZE = 45;

    public GameObject HomeUI;
    public GameObject MatchUI;
    public GameObject EndMatchUI;
    public Text RedEndScoreText;
    public Text BlueEndScoreText;
    public TextMeshProUGUI RedScoreGameText;
    public TextMeshProUGUI BlueScoreGameText;
    public TextMeshProUGUI TimerText;

    public bool IsActiveHomeUI { get { return HomeUI.activeSelf; } }
    public bool IsActiveMatchUI { get { return MatchUI.activeSelf; } }
    public bool IsActiveEndMatchUI { get { return EndMatchUI.activeSelf; } }

    private float minutes;
    private float seconds;
    private bool isInitialized;

    void Update()
    {
        Score();
        Timer();
    }

    public void SetActiveHomeUI(bool value)
    {
        HomeUI.SetActive(value);
    }

    public void SetActiveMatchUI(bool value)
    {
        MatchUI.SetActive(value);
    }

    public void SetActiveEndMatchUI(bool value)
    {
        EndMatchUI.SetActive(value);
    }

    private void Score()
    {
        RedEndScoreText.text = GameManager.RedScore.ToString();
        BlueEndScoreText.text = GameManager.BlueScore.ToString();
        RedScoreGameText.text = GameManager.RedScore.ToString();
        BlueScoreGameText.text = GameManager.BlueScore.ToString();
    }

    //public void ResetTimer()
    //{
    //    seconds = 0.0f;
    //    minutes = 0.0f;
    //}

    private void Timer()
    {
        //seconds += Time.deltaTime;
        //if (seconds >= 60.0f)
        //{
        //    seconds = 0.0f;
        //    minutes++;
        //}

        float timeWithoutExtraTime = GameManager.MatchTime - 20.0f;
        minutes = (int)(timeWithoutExtraTime / 60);
        seconds = timeWithoutExtraTime - (minutes * 60);

        //Debug.Log(minutes + ":" + seconds);

        if (minutes == 0 && seconds < 1.0f)
        {
            TimerText.fontSize = DEFAULT_WORD_FONT_SIZE;
            TimerText.color = Color.red;
            TimerText.text = "EXTRA TIME";
        }
        else
        {
            TimerText.fontSize = DEFAULT_NUMBER_FONT_SIZE;
            TimerText.color = Color.black;
            TimerText.text = minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
        }
    }
}
