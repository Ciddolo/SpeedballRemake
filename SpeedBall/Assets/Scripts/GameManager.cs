using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject BallPrefab;

    private CameraManager cameraManager;
    private TeamManager redTeamManager;

    void Start()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();
        redTeamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        GameObject ball = Instantiate(BallPrefab);
        ball.transform.localPosition = Vector2.zero;
        cameraManager.Ball = ball;
    }
}
