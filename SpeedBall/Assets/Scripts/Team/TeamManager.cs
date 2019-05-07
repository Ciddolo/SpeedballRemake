using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const int NUMBER_OF_PLAYERS = 5;

    public GameObject PlayerPrefab;

    public bool IsInBallPossession { get; set; }
    public GameObject CurrentPlayer { get; set; }

    private GameObject[] players;
    private int index;
    private CameraManager cameraManager;

    void Awake()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();
        players = new GameObject[NUMBER_OF_PLAYERS];
        GameObject RedTeamPositions = GameObject.Find("RedTeamPositions");
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            GameObject player = Instantiate(PlayerPrefab, transform);
            player.transform.localPosition = RedTeamPositions.transform.GetChild(i).transform.localPosition;
            player.GetComponent<PlayerManager>().Index = i;
            if (i == 0)
                player.name = "GK";
            else
                player.name = "PLAYER" + i;
            AddPlayer(player);
        }
        CurrentPlayer = players[3];
        CurrentPlayer.GetComponent<PlayerManager>().IsSelected = true;
        cameraManager.CurrentPlayer = CurrentPlayer;
    }

    void Update()
    {
        if (IsInBallPossession)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            SelectPreviousPlayer();
        if (Input.GetKeyDown(KeyCode.E))
            SelectNextPlayer();
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
}
