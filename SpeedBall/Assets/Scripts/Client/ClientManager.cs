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

    public TeamManager Team;

    [SerializeField]
    private string address;
    [SerializeField]
    private int port;
    private Socket socket;
    private IPEndPoint endPoint;

    private uint netId;

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
        TeamInput();
        PlayerInput();
        CameraInput();
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
        if (Team.CurrentPlayer.GetComponent<PlayerMove>() != null)
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
                    uint prefabType = BitConverter.ToUInt32(data, 1);
                    netId = BitConverter.ToUInt32(data, 5);
                }
            }
        }
    }
}
