using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const uint CLIENTS_NUMBER = 2;
    public GameObject Client;
    public GameObject BallPrefab;

    private CameraManager cameraManager;
    private TeamManager redTeamManager;
    private TeamManager blueTeamManager;
    private GameObject[] clients;
    private static uint clientID;

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
            clients[i].GetComponent<ClientManager>().ID = ++clientID;
            clients[i].name = ("Client" + clientID);
            if (i == 1)
                clients[i].GetComponent<ClientManager>().Team = redTeamManager;
            else
                clients[i].GetComponent<ClientManager>().Team = blueTeamManager;
        }
    }
}
