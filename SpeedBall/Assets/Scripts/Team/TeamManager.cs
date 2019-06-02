using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const int NUMBER_OF_PLAYERS = 5;

    //public bool IsInBallPossession { get; set; }
    public GameObject CurrentPlayer { get; set; }
    public GameObject CurrentEnemyPlayer { get; set; }
    public GameObject ClientOwner { get; set; }
    public bool IsSpawned { get; private set; }
    public int Index { get; set; }

    [SerializeField]
    private GameObject[] myPlayers;
    [SerializeField]
    private GameObject[] enemyPlayers;
    private CameraManager cameraManager;

    void Awake()
    {
        myPlayers = new GameObject[NUMBER_OF_PLAYERS];
        enemyPlayers = new GameObject[NUMBER_OF_PLAYERS];
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

    //public GameObject SelectPlayer(int index)
    //{
    //    this.index = index;
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
    //    CurrentPlayer = myPlayers[index];
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
    //    cameraManager.CurrentPlayer = CurrentPlayer;
    //    return CurrentPlayer;
    //}

    public GameObject SelectPlayer(uint netId)
    {
        for (int i = 0; i < myPlayers.Length; i++)
        {
            if (myPlayers[i].GetComponent<PlayerManager>().NetId == netId)
            {
                //if (CurrentPlayer != null)
                //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
                CurrentPlayer = myPlayers[i];
                //CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
                cameraManager.CurrentPlayer = CurrentPlayer;
                return CurrentPlayer;
            }
        }
        return null;
    }

    public void EnlightenPlayer(uint netId, uint team)
    {
        if (team == 0)
        {
            for (int i = 0; i < myPlayers.Length; i++)
            {
                if (myPlayers[i].GetComponent<PlayerManager>().NetId == netId)
                {
                    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
                    CurrentPlayer = myPlayers[i];
                    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
                    //index = CurrentPlayer.GetComponent<PlayerManager>().Index;
                    cameraManager.CurrentPlayer = CurrentPlayer;
                }
            }

        }
        else
        {
            for (int i = 0; i < enemyPlayers.Length; i++)
            {
                if (enemyPlayers[i].GetComponent<PlayerManager>().NetId == netId)
                {
                    if (CurrentEnemyPlayer != null)
                        CurrentEnemyPlayer.GetComponent<PlayerManager>().IsSelected = false;
                    CurrentEnemyPlayer = enemyPlayers[i];
                    CurrentEnemyPlayer.GetComponent<PlayerManager>().IsSelected = true;
                }
            }
        }
    }

    //public GameObject SelectPlayer(GameObject player)
    //{
    //    index = player.GetComponent<PlayerManager>().Index;
    //    if (CurrentPlayer)
    //        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
    //    CurrentPlayer = player;
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
    //    cameraManager.CurrentPlayer = CurrentPlayer;
    //    return CurrentPlayer;
    //}

    //public GameObject SelectNextPlayer()
    //{
    //    if (++index > myPlayers.Length - 1)
    //        index = 1;
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
    //    CurrentPlayer = myPlayers[index];
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
    //    cameraManager.CurrentPlayer = CurrentPlayer;
    //    Debug.Log("[N] MY PLAYERS LEN:[" + myPlayers.Length + "] INDEX:[" + index + "] PLAYER NAME:[" + CurrentPlayer.name + "]");
    //    return CurrentPlayer;
    //}

    //public GameObject SelectPreviousPlayer()
    //{
    //    if (--index < 1)
    //        index = 4;
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = false;
    //    CurrentPlayer = myPlayers[index];
    //    CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
    //    cameraManager.CurrentPlayer = CurrentPlayer;
    //    Debug.Log("[P] MY PLAYERS LEN:[" + myPlayers.Length + "] INDEX:[" + index + "] PLAYER NAME:[" + CurrentPlayer.name + "]");
    //    return CurrentPlayer;
    //}

    public uint SelectNextPlayer()
    {
        if (++Index > myPlayers.Length - 1)
            Index = 1;
        Debug.Log("[N] MY PLAYERS LEN:[" + myPlayers.Length + "] INDEX:[" + Index + "] PLAYER NAME:[" + CurrentPlayer.name + "]");
        return myPlayers[Index].GetComponent<PlayerManager>().NetId;
    }

    public uint SelectPreviousPlayer()
    {
        if (--Index < 1)
            Index = 4;
        Debug.Log("[P] MY PLAYERS LEN:[" + myPlayers.Length + "] INDEX:[" + Index + "] PLAYER NAME:[" + CurrentPlayer.name + "]");
        return myPlayers[Index].GetComponent<PlayerManager>().NetId;
    }

    public int AddMyPlayer(GameObject player)
    {
        for (int i = 0; i < myPlayers.Length; i++)
        {
            if (myPlayers[i] == null)
            {
                myPlayers[i] = player;
                return 1;
            }
        }
        return -1;
    }

    public int AddEnemyPlayer(GameObject player)
    {
        for (int i = 0; i < enemyPlayers.Length; i++)
        {
            if (enemyPlayers[i] == null)
            {
                enemyPlayers[i] = player;
                return 1;
            }
        }
        return -1;
    }

    //public void ResetPositions()
    //{
    //    if (players == null)
    //        return;
    //    for (int i = 0; i < players.Length; i++)
    //    {
    //        transform.GetChild(i).transform.localPosition = TeamPositions.transform.GetChild(i).transform.localPosition;
    //    }
    //}
}
