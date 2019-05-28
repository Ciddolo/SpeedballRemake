using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const int NUMBER_OF_PLAYERS = 5;

    public bool IsInBallPossession { get; set; }
    public GameObject CurrentPlayer { get; set; }
    public GameObject ClientOwner { get; set; }
    public bool IsSpawned { get; private set; }

    [SerializeField]
    private GameObject[] players;
    private int index;
    private CameraManager cameraManager;

    void Awake()
    {
        players = new GameObject[NUMBER_OF_PLAYERS];
        cameraManager = Camera.main.GetComponent<CameraManager>();
    }

    //private void Spawn()
    //{
    //    players = new GameObject[NUMBER_OF_PLAYERS];
    //    string color = "DEFAULT_";
    //    if (TeamColor.r == 1)
    //        color = "RED_";
    //    else if (TeamColor.b == 1)
    //        color = "BLUE_";
    //    for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
    //    {
    //        GameObject player;
    //        if (i == 0)
    //        {
    //            player = Instantiate(GoalkeeperPrefab, transform);
    //            player.name = color + "GK";
    //        }
    //        else
    //        {
    //            player = Instantiate(PlayerPrefab, transform);
    //            player.name = color + "PLAYER_" + i;
    //        }
    //        player.gameObject.GetComponent<PlayerManager>().Team = this;
    //        player.gameObject.GetComponent<SpriteRenderer>().color = TeamColor;
    //        player.transform.localPosition = TeamPositions.transform.GetChild(i).transform.localPosition;
    //        player.GetComponent<PlayerManager>().Index = i;
    //        AddPlayer(player);
    //    }
    //    CurrentPlayer = players[3];
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
    //    cameraManager.CurrentPlayer = CurrentPlayer;
    //    IsSpawned = true;
    //}

    public GameObject SelectPlayer(int index)
    {
        this.index = index;
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
        CurrentPlayer = players[index];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
        return CurrentPlayer;
    }

    public GameObject SelectPlayer(uint netId)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerManager>().NetId == netId)
            {
                CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
                CurrentPlayer = players[i];
                CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
                index = CurrentPlayer.GetComponent<PlayerManager>().Index;
                cameraManager.CurrentPlayer = CurrentPlayer;
                return CurrentPlayer;
            }
        }
        return null;
    }

    public GameObject SelectPlayer(GameObject player)
    {
        index = player.GetComponent<PlayerManager>().Index;
        if (CurrentPlayer)
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
        //if (players == null)
        //    return;
        //for (int i = 0; i < players.Length; i++)
        //{
        //    transform.GetChild(i).transform.localPosition = TeamPositions.transform.GetChild(i).transform.localPosition;
        //}
    }
}
