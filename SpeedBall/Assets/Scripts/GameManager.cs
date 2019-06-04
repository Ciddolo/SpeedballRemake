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
    public static GameState StateOfGame { get; set; }
    public static uint RedScore { get; set; }
    public static uint BlueScore { get; set; }
    public static float MatchTime { get; set; }

    public UIManager uiManager;
    private static CameraManager cameraManager;
    private static TeamManager redTeamManager;
    private static TeamManager blueTeamManager;

    void Start()
    {
        uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        cameraManager = Camera.main.GetComponent<CameraManager>();
        redTeamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        blueTeamManager = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
    }

    void Update()
    {
        if (!ClientManager.IsInitialized)
            return;

        if (StateOfGame == GameState.WaitingForPlayers)
        {
            if (uiManager.IsActiveHomeUI || uiManager.IsActiveEndMatchUI)
            {
                uiManager.SetActiveHomeUI(false);
                uiManager.SetActiveEndMatchUI(false);
            }

            if (!uiManager.IsActiveMatchUI)
                uiManager.SetActiveMatchUI(true);
        }
        else if (StateOfGame == GameState.Playing)
        {
            if (uiManager.IsActiveHomeUI || uiManager.IsActiveEndMatchUI)
            {
                uiManager.SetActiveHomeUI(false);
                uiManager.SetActiveEndMatchUI(false);
            }

            if (!uiManager.IsActiveMatchUI)
                uiManager.SetActiveMatchUI(true);
        }
        else if (StateOfGame == GameState.Ended)
        {
            if (uiManager.IsActiveHomeUI || uiManager.IsActiveMatchUI)
            {
                uiManager.SetActiveHomeUI(false);
                uiManager.SetActiveMatchUI(false);
            }

            if (!uiManager.IsActiveEndMatchUI)
                uiManager.SetActiveEndMatchUI(true);
        }
    }
}
