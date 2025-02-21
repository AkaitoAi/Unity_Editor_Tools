using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public GameObject clockObj;
    public Text timerText;
    [Tooltip("If yes, Input time in seconds")]
    [SerializeField] private bool useSeconds = true;
    public float time;

    private float minutes, seconds;

    private GameManager gameManager;

    private void Start()
    {
        if(!useSeconds) time *= 60f;

        gameManager = GameManager.GetInstance();
    }

    private void Update()
    {
        if (gameManager.timeRunning) Timer();
    }

    private void Timer()
    {
        
        if (time > 0f)
        {
            time -= Time.deltaTime;
            DisplayTime(time);
        }
        else
        {
            time = 0f;

            gameManager.timeRunning = false;
            //gameManager.LevelFailed();
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        minutes = Mathf.FloorToInt(timeToDisplay / 60);
        seconds = Mathf.FloorToInt(timeToDisplay % 60);


        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (time <= 10f)
        {
            timerText.color = Color.red;
            timerText.text = seconds.ToString();
            //GameManager.Instance.hurryUpObj.SetActive(true);
        }
    }

    public void TimeRunningState(bool _state)
    {
        gameManager.timeRunning = _state;
        clockObj.SetActive(_state);
        timerText.enabled = _state;
    }
}
