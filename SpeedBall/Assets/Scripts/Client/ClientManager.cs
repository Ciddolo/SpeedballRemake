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
    private delegate void SpawnPrefab(byte[] data);
    private Dictionary<int, SpawnPrefab> spawnTable;
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
        spawnTable = new Dictionary<int, SpawnPrefab>();

        spawnTable[(int)NetPrefab.Player] = SpawnPlayer;
        spawnTable[(int)NetPrefab.Goalkeeper] = SpawnGoalkeeper;
        spawnTable[(int)NetPrefab.Wall] = SpawnWall;
        spawnTable[(int)NetPrefab.Goal] = SpawnGoal;
        spawnTable[(int)NetPrefab.Ball] = SpawnBall;

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

        Receiver();

        CalculatePing();
        if (isGettingPing)
            calculatePing += Time.deltaTime;

        if (!isInitialized)
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
        if (Input.GetKeyDown(selectPreviousPlayer))
        {
            uint playerId = teamManager.SelectPreviousPlayer();
            Send(new Packet((byte)PacketsCommands.Input, (byte)InputType.SelectPlayer, playerId));
        }
        if (Input.GetKeyDown(selectNextPlayer))
        {
            uint playerId = teamManager.SelectNextPlayer();
            Send(new Packet((byte)PacketsCommands.Input, (byte)InputType.SelectPlayer, playerId));
        }
    }

    private void PlayerInput()
    {
        //MOVE
        Vector2 direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        if (teamManager.CurrentPlayer.GetComponent<PlayerMove>() != null)
        {
            teamManager.CurrentPlayer.GetComponent<PlayerMove>().Direction = direction;
            Send(new Packet((byte)PacketsCommands.Input, (byte)InputType.Movement, direction.x, direction.y));
        }
        //SHOT
        if (true)
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().AimDirection = aimDirection;
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().InputKey = Input.GetKey(shot);
            teamManager.CurrentPlayer.GetComponent<PlayerShot>().InputKeyUp = Input.GetKeyUp(shot);
        }
        if (true) //TACKLE
        {
            Vector2 aimDirection = new Vector2(Input.GetAxis(horizontalAimAxisName), Input.GetAxis(verticalAimAxisName)).normalized;
            teamManager.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().AimDirection = aimDirection;
            teamManager.CurrentPlayer.transform.GetChild(1).GetComponent<PlayerTackle>().InputKeyDown = Input.GetKeyDown(tackle);
            if (Input.GetKeyDown(tackle))
                SendTackle();
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
        Send(new Packet((byte)PacketsCommands.Input, (byte)InputType.Shot, directionX, directionY, force));
    }

    public void SendTackle()
    {
        Send(new Packet((byte)PacketsCommands.Input, (byte)InputType.Tackle));
    }

    public void CalculatePing()
    {
        calculatePing = 0.0f;
        isGettingPing = true;
        Send(new Packet((byte)PacketsCommands.Ping));
    }

    public float GetPing()
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
                        continue;

                    teamNetId = BitConverter.ToUInt32(data, 5);
                    currentPlayerId = BitConverter.ToUInt32(data, 9);
                    Init();

                    uint packetId = BitConverter.ToUInt32(data, 1);
                    Packet ack = new Packet(PacketsCommands.Ack, packetId);
                    socket.SendTo(ack.GetData(), endPoint);
                }
                else if (command == (byte)PacketsCommands.Spawn)
                {
                    uint packetId = BitConverter.ToUInt32(data, 1);
                    uint type = BitConverter.ToUInt32(data, 5);
                    uint id = BitConverter.ToUInt32(data, 9);

                    Packet ack = new Packet(PacketsCommands.Ack, packetId);
                    socket.SendTo(ack.GetData(), endPoint);

                    if (!spawnedObjects.ContainsKey(id))
                    {
                        if (type == (uint)NetPrefab.Goalkeeper)
                            spawnTable[(int)NetPrefab.Goalkeeper](data);
                        else if (type == (uint)NetPrefab.Player)
                            spawnTable[(int)NetPrefab.Player](data);
                        else if (type == (uint)NetPrefab.Wall)
                            spawnTable[(int)NetPrefab.Wall](data);
                        else if (type == (uint)NetPrefab.Goal)
                            spawnTable[(int)NetPrefab.Goal](data);
                        else if (type == (uint)NetPrefab.Ball)
                            spawnTable[(int)NetPrefab.Ball](data);
                    }
                }
                else if (command == (byte)PacketsCommands.Update)
                {
                    uint id = BitConverter.ToUInt32(data, 5);
                    float x = BitConverter.ToSingle(data, 9);
                    float y = BitConverter.ToSingle(data, 13);

                    Vector2 newPosition = new Vector2(x, y);
                    spawnedObjects[id].transform.position = newPosition;
                    spawnedObjects[id].GetComponent<SmoothingManager>().GetNextUpdate(newPosition);

                    if (spawnedObjects[id].GetComponent<BallBehaviour>() != null)
                    {
                        float scale = BitConverter.ToSingle(data, 17);
                        float newScale = 1.0f + scale;
                        spawnedObjects[id].transform.localScale = new Vector3(newScale, newScale, newScale);
                    }
                }
                else if (command == (byte)PacketsCommands.GameInfo)
                {
                    uint teamRedScore = BitConverter.ToUInt32(data, 5);
                    uint teamBlueScore = BitConverter.ToUInt32(data, 9);
                    uint currentRedPlayer = BitConverter.ToUInt32(data, 13);
                    uint currentBluePlayer = BitConverter.ToUInt32(data, 17);
                    uint currentGameState = BitConverter.ToUInt32(data, 21);

                    GameManager.StateOfGame = (GameState)currentGameState;
                    teamManager.EnlightenPlayer(currentRedPlayer, 0);
                    teamManager.EnlightenPlayer(currentBluePlayer, 1);
                    GameManager.RedScore = teamRedScore;
                    GameManager.BlueScore = teamBlueScore;
                }
                else if (command == (byte)PacketsCommands.Pong)
                {
                    uint packetId = BitConverter.ToUInt32(data, 2);
                    isGettingPing = false;
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

    private void SpawnPlayer(byte[] data)
    {
        TeamManager team;
        Color color;
        uint id = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        float height = BitConverter.ToSingle(data, 21);
        float width = BitConverter.ToSingle(data, 25);
        uint teamId = BitConverter.ToUInt32(data, 29);
        GameObject objectToSpawn = null;
        string name;

        if (teamId == 0)
        {
            team = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
            color = Color.red;
            name = "RED_PLAYER_" + ++redPlayers;
        }
        else
        {
            team = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
            color = Color.blue;
            name = "BLUE_PLAYER_" + ++bluePlayers;
        }

        objectToSpawn = Instantiate(PlayerPrefab, team.transform);
        objectToSpawn.name = name;
        objectToSpawn.transform.position = new Vector2(x, y);
        objectToSpawn.transform.localScale = new Vector2(width, height);
        spawnedObjects.Add(id, objectToSpawn);

        objectToSpawn.GetComponent<PlayerManager>().NetId = id;
        objectToSpawn.GetComponent<PlayerManager>().Team = team;
        objectToSpawn.GetComponent<SpriteRenderer>().color = color;
        objectToSpawn.GetComponent<SmoothingManager>().ClientMng = this;

        if (teamId == teamNetId)
        {
            teamManager.AddMyPlayer(objectToSpawn);
            if (currentPlayerId == id)
            {
                teamManager.SelectPlayer(id);
                teamManager.Index = 3;
            }
        }
        else
            teamManager.AddEnemyPlayer(objectToSpawn);
    }

    private void SpawnGoalkeeper(byte[] data)
    {
        TeamManager team;
        Color color;
        uint id = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        float height = BitConverter.ToSingle(data, 21);
        float width = BitConverter.ToSingle(data, 25);
        uint teamId = BitConverter.ToUInt32(data, 29);
        GameObject objectToSpawn = null;
        string name;

        if (teamId == 0)
        {
            team = GameObject.Find("RedTeamPlayers").GetComponent<TeamManager>();
            color = Color.red;
            name = "RED_GOALKEEPER";
        }
        else
        {
            team = GameObject.Find("BlueTeamPlayers").GetComponent<TeamManager>();
            color = Color.blue;
            name = "BLUE_GOALKEEPER";
        }

        objectToSpawn = Instantiate(GoalkeeperPrefab, team.transform);
        objectToSpawn.name = name;
        objectToSpawn.transform.position = new Vector2(x, y);
        objectToSpawn.transform.localScale = new Vector2(width, height);
        spawnedObjects.Add(id, objectToSpawn);

        objectToSpawn.GetComponent<PlayerManager>().NetId = id;
        objectToSpawn.GetComponent<PlayerManager>().Team = team;
        objectToSpawn.GetComponent<SpriteRenderer>().color = color;
        objectToSpawn.GetComponent<SmoothingManager>().ClientMng = this;

        if (teamId == teamNetId)
            teamManager.AddMyPlayer(objectToSpawn);
        else
            teamManager.AddEnemyPlayer(objectToSpawn);
    }

    private void SpawnWall(byte[] data)
    {
        uint id = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        float height = BitConverter.ToSingle(data, 21);
        float width = BitConverter.ToSingle(data, 25);

        GameObject objectToSpawn = Instantiate(WallPrefab, GameObject.Find("Walls").transform);
        objectToSpawn.name = "WALL_" + ++walls;
        objectToSpawn.transform.position = new Vector2(x, y);
        objectToSpawn.transform.localScale = new Vector2(width, height);
        spawnedObjects.Add(id, objectToSpawn);
    }

    private void SpawnGoal(byte[] data)
    {
        uint id = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        float height = BitConverter.ToSingle(data, 21);
        float width = BitConverter.ToSingle(data, 25);
        uint teamId = BitConverter.ToUInt32(data, 29);

        GameObject objectToSpawn = Instantiate(GoalPrefab, GameObject.Find("Goals").transform);

        if (teamId == 0)
        {
            objectToSpawn.GetComponent<SpriteRenderer>().color = Color.red;
            objectToSpawn.name = "RED_GOAL";
        }
        else
        {
            objectToSpawn.GetComponent<SpriteRenderer>().color = Color.blue;
            objectToSpawn.name = "BLUE_GOAL";
        }

        objectToSpawn.transform.position = new Vector2(x, y);
        objectToSpawn.transform.localScale = new Vector2(width, height);
        spawnedObjects.Add(id, objectToSpawn);
    }

    private void SpawnBall(byte[] data)
    {
        uint id = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        float height = BitConverter.ToSingle(data, 21);
        float width = BitConverter.ToSingle(data, 25);

        GameObject objectToSpawn = Instantiate(BallPrefab);
        objectToSpawn.GetComponent<SmoothingManager>().ClientMng = this;
        objectToSpawn.name = "BALL";
        objectToSpawn.transform.position = new Vector2(x, y);
        objectToSpawn.transform.localScale = new Vector2(width, height);
        spawnedObjects.Add(id, objectToSpawn);
    }
}
