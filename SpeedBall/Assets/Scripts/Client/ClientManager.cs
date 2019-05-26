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

public enum NetPrefab
{
    MIN,
    Player,
    Wall,
    Goal,
    Ball,
    Goalkeeper
}

public enum InputType
{
    SelectPlayer,
    Shot,
    Tackle,
    Movement
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

    [SerializeField]
    private TeamManager teamManager;
    [SerializeField]
    private bool isInitialized;
    private bool isGettingPing;
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
    private uint redPlayers;
    private uint bluePlayers;
    private uint walls;
    private float calculatePing;
    private float currentPing;

    void Awake()
    {
        netPrefabs = new Dictionary<uint, GameObject>();
        spawnedObjects = new Dictionary<uint, GameObject>();

        isInitialized = false;

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
        if (Input.GetKeyDown(KeyCode.L))
            CalculatePing();

        Receiver();

        if (isGettingPing)
        {
            calculatePing += Time.deltaTime;
        }

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
                Send(new Packet(PacketsCommands.Input, InputType.SelectPlayer, playerId));
            }
            if (Input.GetKeyDown(selectNextPlayer))
            {
                uint playerId = teamManager.SelectNextPlayer().GetComponent<PlayerManager>().NetId;
                Send(new Packet(PacketsCommands.Input, InputType.SelectPlayer, playerId));
            }
        }
    }

    private void PlayerInput()
    {
        //MOVE
        Vector2 direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        if (teamManager.CurrentPlayer.GetComponent<PlayerMove>() != null)
        {
            teamManager.CurrentPlayer.GetComponent<PlayerMove>().Direction = direction;
            Send(new Packet(PacketsCommands.Input, InputType.Movement, direction.x, direction.y));
        }
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
            Send(new Packet(PacketsCommands.Input, InputType.Tackle));
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

    public void SendShot(float directionX, float directionY, float force)
    {
        Send(new Packet(PacketsCommands.Input, InputType.Shot, directionX, directionY, force));
    }

    private void CalculatePing()
    {
        calculatePing = 0.0f;
        isGettingPing = true;
        Send(new Packet(PacketsCommands.Ping));
    }

    private float GetPing()
    {
        currentPing = calculatePing;
        return currentPing;
    }

    private void Receiver()
    {
        byte[] data = new byte[256];
        EndPoint sender = new IPEndPoint(0, 0);

        for (int i = 0; i < MAX_PACKETS; i++)
        {
            int rlen = -1;
            try
            {
                rlen = socket.ReceiveFrom(data, ref sender);
            }
            catch
            {
                break;
            }

            if (rlen > 0)
            {
                //if (sender != endPoint)
                //    continue;

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
                        GameObject objectToSpawn = null;
                        string objectToSpawnName = "";

                        if (type == (uint)NetPrefab.Goalkeeper || type == (uint)NetPrefab.Player)
                        {
                            TeamManager team;
                            Color color;
                            uint teamId = BitConverter.ToUInt32(data, 29);

                            if (teamId == 0)
                            {
                                team = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
                                color = Color.red;
                                objectToSpawnName = "RED";
                            }
                            else
                            {
                                team = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
                                color = Color.blue;
                                objectToSpawnName = "BLUE";
                            }

                            if (type == (uint)NetPrefab.Goalkeeper)
                            {
                                objectToSpawn = Instantiate(GoalkeeperPrefab, team.transform);
                                objectToSpawnName += "_GOALKEEPER";
                            }
                            else
                            {
                                objectToSpawn = Instantiate(PlayerPrefab, team.transform);
                                if (teamId == 0)
                                    objectToSpawnName = "PLAYER_" + ++redPlayers;
                                else
                                    objectToSpawnName = "PLAYER_" + ++bluePlayers;
                            }

                            objectToSpawn.GetComponent<PlayerManager>().NetId = id;
                            objectToSpawn.GetComponent<SpriteRenderer>().color = color;

                            if (teamId == teamNetId)
                                teamManager.AddPlayer(objectToSpawn);
                        }
                        else if (type == (uint)NetPrefab.Wall)
                        {
                            objectToSpawn = Instantiate(WallPrefab);
                            objectToSpawnName = "WALL_" + ++walls;
                        }
                        else if (type == (uint)NetPrefab.Goal)
                        {
                            objectToSpawn = Instantiate(GoalPrefab);
                            Color color;
                            uint teamId = BitConverter.ToUInt32(data, 29);
                            if (teamId == 0)
                            {
                                color = Color.red;
                                objectToSpawnName = "RED_GOAL";
                            }
                            else
                            {
                                color = Color.blue;
                                objectToSpawnName = "BLUE_GOAL";
                            }
                            objectToSpawn.GetComponent<SpriteRenderer>().color = color;
                        }
                        else if (type == (uint)NetPrefab.Ball)
                        {
                            objectToSpawn = Instantiate(BallPrefab);
                            objectToSpawnName = "BALL";
                        }

                        objectToSpawn.name = objectToSpawnName;
                        objectToSpawn.transform.position = new Vector2(x, y);
                        objectToSpawn.transform.localScale = new Vector2(width, height);

                        spawnedObjects.Add(id, objectToSpawn);
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
                    isGettingPing = false;
                    Debug.Log(GetPing());
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
