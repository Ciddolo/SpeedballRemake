using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public enum PacketsCommands
{
    Join = 0,
    Welcome = 1,
    Spawn = 2,
    Update = 3,
    Input = 4,
    GameInfo = 5,
    Pong = 253,
    Ping = 254,
    Ack = 255
}

public class ClientManager : MonoBehaviour
{
    private const int MAX_PACKETS = 100;

    [SerializeField]
    private string address;
    [SerializeField]
    private int port;
    private Socket socket;
    private IPEndPoint endPoint;

    private uint teamNetId;
    private uint currentPlayerId;

    [SerializeField]
    private TeamManager teamManager;
    [SerializeField]
    private bool isInitialized;
    private string horizontalAxisName;
    private string verticalAxisName;
    private string horizontalAimAxisName;
    private string verticalAimAxisName;
    private KeyCode changeCameraMode;
    private KeyCode selectPreviousPlayer;
    private KeyCode selectNextPlayer;
    private KeyCode shot;
    private KeyCode tackle;

    void Awake()
    {
        isInitialized = false;

        //Test
        teamNetId = 0;
        Init();
        //End test

        if (!(address == null) || !(port == 0))
            return;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        endPoint = new IPEndPoint(IPAddress.Parse(address), port);

        Packet join = new Packet((byte)PacketsCommands.Join);
        socket.SendTo(join.GetData(), endPoint);
    }

    void Start()
    {
        horizontalAxisName = "RedHorizontal";
        verticalAxisName = "RedVertical";
        horizontalAimAxisName = "RedHorizontalAim";
        verticalAimAxisName = "RedVerticalAim";
        selectPreviousPlayer = KeyCode.Q;
        selectNextPlayer = KeyCode.E;
        shot = KeyCode.Space;
        tackle = KeyCode.RightControl;
        changeCameraMode = KeyCode.RightShift;
    }

    void Update()
    {
        //Receiver();

        if (!isInitialized || !teamManager.IsSpawned)
            return;

        TeamInput();
        PlayerInput();
        CameraInput();
    }

    private void Init()
    {
        if (isInitialized)
            return;

        if (teamNetId == 0)
            teamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        else if (teamNetId == 1)
            teamManager = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
        //else exceptions
        teamManager.ClientOwner = gameObject;
        isInitialized = true;
    }

    private void TeamInput()
    {
        if (!teamManager.IsInBallPossession)
        {
            if (Input.GetKeyDown(selectPreviousPlayer))
                teamManager.SelectPreviousPlayer();
            if (Input.GetKeyDown(selectNextPlayer))
                teamManager.SelectNextPlayer();
        }
    }

    private void PlayerInput()
    {
        //MOVE
        Vector2 direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        if (teamManager.CurrentPlayer.GetComponent<PlayerMove>() != null)
            teamManager.CurrentPlayer.GetComponent<PlayerMove>().Direction = direction;
        //SHOT
        if (teamManager.CurrentPlayer.GetComponent<PlayerManager>().Ball != null)
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().AimDirection = aimDirection;
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().InputKey = Input.GetKey(shot);
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().InputKeyUp = Input.GetKeyUp(shot);
        }
        else //TACKLE
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            teamManager.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().AimDirection = aimDirection;
            teamManager.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().InputKeyDown = Input.GetKeyDown(tackle);
        }
    }

    private void CameraInput()
    {
        if (Input.GetKeyDown(changeCameraMode))
            Camera.main.GetComponent<CameraManager>().IncreaseIndex();
    }

    private void Receiver()
    {
        byte[] data = new byte[256];
        for (int i = 0; i < MAX_PACKETS; i++)
        {
            int rlen = -1;
            try
            {
                rlen = socket.Receive(data);
            }
            catch
            {
                break;
            }

            if (rlen > 0)
            {
                byte command = data[0];
                if (command == (byte)PacketsCommands.Welcome)
                {
                    if (isInitialized)
                        return;
                    teamNetId = BitConverter.ToUInt32(data, 5);
                    currentPlayerId = BitConverter.ToUInt32(data, 9);
                    Init();
                }
            }
        }
    }
}
