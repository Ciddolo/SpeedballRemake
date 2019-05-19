using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const uint CLIENTS_NUMBER = 2;
    public GameObject Client;
    public GameObject BallPrefab;

    public static uint RedScore { get; set; }
    public static uint BlueScore { get; set; }

    private static CameraManager cameraManager;
    private static TeamManager redTeamManager;
    private static TeamManager blueTeamManager;
    private GameObject[] clients;

    void Start()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();
        redTeamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        blueTeamManager = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
        GameObject ball = Instantiate(BallPrefab);
        ball.transform.localPosition = Vector2.zero;
        cameraManager.Ball = ball;
        clients = new GameObject[CLIENTS_NUMBER];
        for (int i = 0; i < CLIENTS_NUMBER; i++)
        {
            clients[i] = Instantiate(Client, Vector3.zero, Quaternion.identity, transform);
            if (i == 1)
            {
                clients[i].name = ("ClientRed");
                clients[i].GetComponent<ClientManager>().Team = redTeamManager;
            }
            else
            {
                clients[i].name = ("ClientBlue");
                clients[i].GetComponent<ClientManager>().Team = blueTeamManager;
            }
        }
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
