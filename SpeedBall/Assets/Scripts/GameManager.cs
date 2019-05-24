using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject BallPrefab;

    public static uint RedScore { get; set; }
    public static uint BlueScore { get; set; }
    public static GameObject Ball { get; private set; }

    private static CameraManager cameraManager;
    private static TeamManager redTeamManager;
    private static TeamManager blueTeamManager;

    void Start()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();
        redTeamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        blueTeamManager = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
        Ball = Instantiate(BallPrefab);
        Ball.transform.localPosition = Vector2.zero;
        cameraManager.Ball = Ball;
    }



    public static void TeamScore(int team)
    {
        if (team == 1)
            RedScore++;
        else
            BlueScore++;

        redTeamManager.ResetPositions();
        blueTeamManager.ResetPositions();
    }
}
