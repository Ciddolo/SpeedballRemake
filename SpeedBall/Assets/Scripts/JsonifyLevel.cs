using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class SimpleLevelObject
{
    public Vector2 Position;
    public string Name;

    public SimpleLevelObject(Transform r)
    {
        Position = r.position;
        Name = r.gameObject.name;
    }
}

[Serializable]
public class ComplexLevelObject : SimpleLevelObject
{
    public float Width, Height;

    public ComplexLevelObject(Transform go) : base(go)
    {
        Width = go.localScale.x;
        Height = go.localScale.y;
    }
}

[Serializable]
public class Level
{
    public PlayersInfo PlayerInfo;
    public ComplexLevelObject Ball, NetTeamOne, NetTeamTwo, WarpRight, WarpLeft;
    public List<SimpleLevelObject> TeamOneSpawnPositions, TeamTwoSpawnPositions;
    public List<ComplexLevelObject> Walls;
}

[Serializable]
public class PlayersInfo
{
    public bool IsCircle;
    public float Width, Height;
    public int DefaultPlayerIndex, GoalkeeperIndex;

    public PlayersInfo(Transform transform)
    {
        ComplexLevelObject data = new ComplexLevelObject(transform);

        IsCircle = false;
        Width = data.Width;
        Height = data.Height;
        DefaultPlayerIndex = 3;
        GoalkeeperIndex = 0;
    }
}

[ExecuteInEditMode]
public class JsonifyLevel : MonoBehaviour
{
    public bool Execute = false;

    public GameObject PlayerPrefab, BallPrefab, NetTeamOneGameObject, NetTeamTwoGameObject, WarpRight, WarpLeft;
    public GameObject WallsParentGameObject, PlayerOneTeamParent, PlayerTwoTeamParent;

    void Update()
    {
        if (Execute)
        {
            Execute = false;

            Level levelToSerialize = new Level();

            levelToSerialize.Walls = GetComplexLevelObjects(WallsParentGameObject); ;

            levelToSerialize.TeamTwoSpawnPositions = GetSimpleLevelObjects(PlayerOneTeamParent);
            levelToSerialize.TeamOneSpawnPositions = GetSimpleLevelObjects(PlayerTwoTeamParent);

            levelToSerialize.PlayerInfo = new PlayersInfo(PlayerPrefab.transform);

            levelToSerialize.NetTeamOne = new ComplexLevelObject(NetTeamOneGameObject.transform);
            levelToSerialize.NetTeamTwo = new ComplexLevelObject(NetTeamTwoGameObject.transform);
            levelToSerialize.Ball = new ComplexLevelObject(BallPrefab.transform);

            levelToSerialize.WarpRight = new ComplexLevelObject(WarpRight.transform);
            levelToSerialize.WarpLeft = new ComplexLevelObject(WarpLeft.transform);

            string serializedLevel = JsonUtility.ToJson(levelToSerialize, true);

            string path = Application.dataPath + "/Level.json";
            File.WriteAllText(path, serializedLevel);
        }
    }

    public List<SimpleLevelObject> GetSimpleLevelObjects(GameObject parent)
    {
        Transform[] childs = parent.GetComponentsInChildren<Transform>();

        List<SimpleLevelObject> objects = new List<SimpleLevelObject>();

        foreach (var child in childs)
        {
            if (parent.transform == child)
                continue;

            SimpleLevelObject ob;
            ob = new SimpleLevelObject(child);

            objects.Add(ob);
        }

        return objects;
    }

    public List<ComplexLevelObject> GetComplexLevelObjects(GameObject parent)
    {
        Transform[] childs = parent.GetComponentsInChildren<Transform>();

        List<ComplexLevelObject> objects = new List<ComplexLevelObject>();

        foreach (var child in childs)
        {
            BoxCollider2D rigid = child.GetComponent<BoxCollider2D>();
            if (rigid != null)
            {
                ComplexLevelObject ob;
                ob = new ComplexLevelObject(child);

                objects.Add(ob);
            }
        }

        return objects;
    }
}
