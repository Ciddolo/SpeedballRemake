using UnityEngine;
using System;
using System.Collections.Generic;
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

    public GameObject PlayerPrefab;
    public GameObject GoalkeeperPrefab;
    public GameObject WallPrefab;
    public GameObject GoalPrefab;
    public GameObject BallPrefab;

    [SerializeField]
    private string address;
    [SerializeField]
    private int port;
    private Socket socket;
    private IPEndPoint endPoint;

    private uint teamNetId;
    private uint currentPlayerId;

    //private Timer pingTimer;
    //private Packet pingPacketSended;
    //private bool waitingForPong;
    //private uint pingPacketId;
    //private float sendedPingTimestamp;
    //public float PingValue;

    [SerializeField]
    private TeamManager teamManager;
    [SerializeField]
    private bool isInitialized;
    private Dictionary<uint, GameObject> netPrefabs;
    private Dictionary<uint, GameObject> spawnedObjects;
    private string horizontalAxisName;
    private string verticalAxisName;
    private string horizontalAimAxisName;
    private string verticalAimAxisName;
    private KeyCode changeCameraMode;
    private KeyCode selectPreviousPlayer;
    private KeyCode selectNextPlayer;
    private KeyCode shot;
    private KeyCode tackle;
    private uint name;

    void Awake()
    {
        netPrefabs = new Dictionary<uint, GameObject>();
        spawnedObjects = new Dictionary<uint, GameObject>();

        isInitialized = false;

        ////Test
        //teamNetId = 0;
        //Init();
        ////End

        //if (!(address == null) || !(port == 0))
        //    return;

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
        Receiver();

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
            {
                uint playerId = teamManager.SelectPreviousPlayer().GetComponent<PlayerManager>().NetId;
                Send(new Packet(PacketsCommands.Input, playerId));
            }
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

    private void Send(Packet packet)
    {
        socket.SendTo(packet.GetData(), endPoint);
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

                    uint packetId = BitConverter.ToUInt32(data, 1);
                    Packet ack = new Packet(PacketsCommands.Ack, packetId);
                    socket.SendTo(ack.GetData(), endPoint);
                }
                else if (command == (byte)PacketsCommands.Spawn)
                {
                    uint type = BitConverter.ToUInt32(data, 5);
                    uint id = BitConverter.ToUInt32(data, 9);
                    float x = BitConverter.ToSingle(data, 13);
                    float y = BitConverter.ToSingle(data, 17);
                    float height = BitConverter.ToSingle(data, 21);
                    float width = BitConverter.ToSingle(data, 25);

                    uint packetId = BitConverter.ToUInt32(data, 1);
                    Packet ack = new Packet(PacketsCommands.Ack, packetId);
                    socket.SendTo(ack.GetData(), endPoint);

                    if (!spawnedObjects.ContainsKey(id))
                    {
                        GameObject go;
                        if (type == 1)
                        {
                            go = Instantiate(PlayerPrefab);
                            go.GetComponent<PlayerManager>().NetId = id;
                            Color color;
                            uint teamId = BitConverter.ToUInt32(data, 29);
                            if (teamId == 0)
                                color = Color.red;
                            else
                                color = Color.blue;
                            go.GetComponent<SpriteRenderer>().color = color;
                            if (teamId == teamNetId)
                                teamManager.AddPlayer(go);
                        }
                        else if (type == 2)
                            go = Instantiate(WallPrefab);
                        else if (type == 3)
                        {
                            go = Instantiate(GoalPrefab);
                            Color color;
                            if (BitConverter.ToUInt32(data, 29) == 0)
                                color = Color.red;
                            else
                                color = Color.blue;
                            go.GetComponent<SpriteRenderer>().color = color;
                        }
                        else if (type == 4)
                            go = Instantiate(BallPrefab);
                        else
                            go = null;
                        spawnedObjects.Add(id, go);
                        go.name = (++name).ToString();
                        go.transform.position = new Vector2(x, y);
                        go.transform.localScale = new Vector2(width, height);
                    }
                }
                else if (command == (byte)PacketsCommands.Update)
                {
                    uint id = BitConverter.ToUInt32(data, 5);
                    float x = BitConverter.ToSingle(data, 9);
                    float y = BitConverter.ToSingle(data, 13);
                }
                else if (command == (byte)PacketsCommands.Pong)
                {
                    uint packetId = BitConverter.ToUInt32(data, 2);
                    //Smoothing stuff
                    //if (packetId != this.sendedPingId)
                    //    continue;
                    //PingValue = TimeSinceSendingPingPacket;
                    //waitingForPong = false;
                }
                else if (command == (byte)PacketsCommands.Ping)
                {
                    uint packetId = BitConverter.ToUInt32(data, 1);
                    Packet pong = new Packet(PacketsCommands.Pong, packetId);
                    socket.SendTo(pong.GetData(), endPoint);
                }
            }
        }
    }
}
