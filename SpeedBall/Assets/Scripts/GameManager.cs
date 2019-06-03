using UnityEngine;

public enum GameState
{
    WaitingForPlayers,
    ResettingPlayersPositions,
    Playing,
    Ended
}

public class GameManager : MonoBehaviour
{
    public GameObject ClientPrefab;
    public GameObject GoalkeeperPrefab;
    public GameObject PlayerPrefab;
    public GameObject BallPrefab;
    public GameObject WallPrefab;
    public GameObject GoalPrefab;

    public static GameState StateOfGame { get; set; }
    public static uint RedScore { get; set; }
    public static uint BlueScore { get; set; }

    private static CameraManager cameraManager;
    private static TeamManager redTeamManager;
    private static TeamManager blueTeamManager;

    private string address;
    private int port;
    private ClientManager client;

    void Start()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();
        redTeamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        blueTeamManager = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();

        address = "127.0.0.1";
        port = 5000;

        //InstantiateClient(address, port);
    }

    private void InstantiateClient(string address, int port)
    {
        client = Instantiate(ClientPrefab, transform).GetComponent<ClientManager>();
        client.GoalkeeperPrefab = GoalkeeperPrefab;
        client.PlayerPrefab = PlayerPrefab;
        client.BallPrefab = BallPrefab;
        client.WallPrefab = WallPrefab;
        client.GoalPrefab = GoalPrefab;
        client.Address = address;
        client.Port = port;
    }
}
