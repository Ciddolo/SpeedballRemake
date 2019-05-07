using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int Index { get; set; }
    public bool IsSelected { get; set; }

    private TeamManager teamManager;
    private PlayerMove move;

    void Start()
    {
        teamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        move = gameObject.GetComponent<PlayerMove>();
    }

    public void BallReceived()
    {
        teamManager.SelectPlayer(gameObject);
        teamManager.IsInBallPossession = true;
    }

    public void BallThrown()
    {
        teamManager.IsInBallPossession = false;
    }
}
