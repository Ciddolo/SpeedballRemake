using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public TeamManager Team;

    private string horizontalAxisName;
    private string verticalAxisName;
    private string horizontalAimAxisName;
    private string verticalAimAxisName;
    private KeyCode changeCameraMode;
    private KeyCode selectPreviousPlayer;
    private KeyCode selectNextPlayer;
    private KeyCode shot;
    private KeyCode tackle;

    void Start()
    {
        if (Team.gameObject.name == "RedTeamPlayers")
        {
            horizontalAxisName = "RedHorizontal";
            verticalAxisName = "RedVertical";
            horizontalAimAxisName = "RedHorizontalAim";
            verticalAimAxisName = "RedVerticalAim";
            selectPreviousPlayer = KeyCode.Q;
            selectNextPlayer = KeyCode.E;
            shot = KeyCode.C;
            tackle = KeyCode.X;
        }
        else if (Team.gameObject.name == "BlueTeamPlayers")
        {
            horizontalAxisName = "BlueHorizontal";
            verticalAxisName = "BlueVertical";
            horizontalAimAxisName = "BlueHorizontalAim";
            verticalAimAxisName = "BlueVerticalAim";
            selectPreviousPlayer = KeyCode.U;
            selectNextPlayer = KeyCode.O;
            shot = KeyCode.N;
            tackle = KeyCode.M;
        }
        changeCameraMode = KeyCode.Space;
    }

    void Update()
    {
        TeamInput();
        PlayerInput();
        CameraInput();

        if (Input.GetKeyDown(KeyCode.F))
            Debug.Log(GameManager.RedScore + " - " + GameManager.BlueScore);
    }

    private void TeamInput()
    {
        if (!Team.IsInBallPossession)
        {
            if (Input.GetKeyDown(selectPreviousPlayer))
                Team.SelectPreviousPlayer();
            if (Input.GetKeyDown(selectNextPlayer))
                Team.SelectNextPlayer();
        }
    }

    private void PlayerInput()
    {
        //MOVE
        Vector2 direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        Team.CurrentPlayer.GetComponent<PlayerMove>().Direction = direction;
        //SHOT
        if (Team.CurrentPlayer.GetComponent<PlayerManager>().Ball != null)
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            Team.CurrentPlayer.GetComponent<PlayerShot>().AimDirection = aimDirection;
            Team.CurrentPlayer.GetComponent<PlayerShot>().InputKey = Input.GetKey(shot);
            Team.CurrentPlayer.GetComponent<PlayerShot>().InputKeyUp = Input.GetKeyUp(shot);
        }
        else //TACKLE
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            Team.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().AimDirection = aimDirection;
            Team.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().InputKeyDown = Input.GetKeyDown(tackle);
        }
    }

    private void CameraInput()
    {
        if (Input.GetKeyDown(changeCameraMode))
            Camera.main.GetComponent<CameraManager>().IncreaseIndex();
    }
}
