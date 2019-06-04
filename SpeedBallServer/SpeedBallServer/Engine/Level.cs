using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Level
    {
        public PlayersInfo PlayerInfo;
        public ComplexLevelObject Ball, NetTeamOne, NetTeamTwo, WarpRight, WarpLeft;
        public List<SimpleLevelObject> TeamOneSpawnPositions, TeamTwoSpawnPositions;
        public List<ComplexLevelObject> Walls, Bumpers;
    }

    public class SimpleLevelObject
    {
        public Vector2 Position;
        public string Name;
    }

    public class ComplexLevelObject : SimpleLevelObject
    {
        public float Width, Height;
    }

    public class PlayersInfo
    {
        public bool IsCircle;
        public float Width, Height;
        public int DefaultPlayerIndex, GoalkeeperIndex;
    }
}
