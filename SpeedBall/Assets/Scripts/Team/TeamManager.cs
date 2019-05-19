using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const int NUMBER_OF_PLAYERS = 5;

    public GameObject PlayerPrefab;
    public GameObject TeamPositions;
    public Color TeamColor;

    public bool IsInBallPossession { get; set; }
    public GameObject CurrentPlayer { get; set; }

    private GameObject[] players;
    private int index;
    private CameraManager cameraManager;

    void Awake()
    {
        string color = "DEFAULT_";
        if (TeamColor.r == 1)
            color = "RED_";
        else if (TeamColor.b == 1)
            color = "BLUE_";

        cameraManager = Camera.main.GetComponent<CameraManager>();
        players = new GameObject[NUMBER_OF_PLAYERS];
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            GameObject player = Instantiate(PlayerPrefab, transform);
            player.gameObject.GetComponent<PlayerManager>().Team = this;
            player.gameObject.GetComponent<SpriteRenderer>().color = TeamColor;
            player.transform.localPosition = TeamPositions.transform.GetChild(i).transform.localPosition;
            player.GetComponent<PlayerManager>().Index = i;
            if (i == 0)
                player.name = color + "GK";
            else
                player.name = color + "PLAYER_" + i;
            AddPlayer(player);
        }
        CurrentPlayer = players[3];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
    }

    public GameObject SelectPlayer(int index)
    {
        this.index = index;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
        CurrentPlayer = players[index];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
        return CurrentPlayer;
    }

    public GameObject SelectPlayer(GameObject player)
    {
        index = player.GetComponent<PlayerManager>().Index;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
        CurrentPlayer = player;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
        return CurrentPlayer;
    }

    public GameObject SelectNextPlayer()
    {
        if (++index > players.Length - 1)
            index = 1;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
        CurrentPlayer = players[index];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
        return CurrentPlayer;
    }

    public GameObject SelectPreviousPlayer()
    {
        if (--index < 1)
            index = 4;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
        CurrentPlayer = players[index];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
        return CurrentPlayer;
    }

    public int AddPlayer(GameObject player)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = player;
                return 1;
            }
        }
        return -1;
    }

    public void ResetPositions()
    {
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            transform.GetChild(i).transform.localPosition = TeamPositions.transform.GetChild(i).transform.localPosition;
        }
    }
}
