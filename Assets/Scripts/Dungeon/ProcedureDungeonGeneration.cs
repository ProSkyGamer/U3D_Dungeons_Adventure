using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static DungeonRoom;
using Random = UnityEngine.Random;


public class ProcedureDungeonGeneration : MonoBehaviour
{
    [SerializeField] private Vector2Int maxDungeonSize = new(5, 8);

    [Serializable]
    private class DungeonRoomType
    {
        public Transform dungeonRoom;
        public int maxRoomCount = 1;
        [HideInInspector] public int createdRoomCount;
    }

    [SerializeField] private List<DungeonRoomType> dungeonRoomVariationsPrefabsList = new();
    [SerializeField] private DungeonRoomType startingRoom;

    private int dungeonRooms;

    private readonly List<DungeonRoom> allRoomsList = new();

    private List<List<bool>> dungeonMapUsedTilesList = new()
    {
        new List<bool>(1)
    };

    private void Awake()
    {
        dungeonRooms = 0;
        foreach (var dungeonRoomVariation in dungeonRoomVariationsPrefabsList)
            dungeonRooms += dungeonRoomVariation.maxRoomCount;

        GenerateDungeon();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var room in allRoomsList) Destroy(room.gameObject);
            allRoomsList.Clear();
            dungeonMapUsedTilesList.Clear();
            dungeonMapUsedTilesList = new List<List<bool>>
            {
                new(1)
            };
            foreach (var roomTransform in GetComponentsInChildren<Transform>())
                if (roomTransform != transform && transform != roomTransform)
                    Destroy(roomTransform.gameObject);

            foreach (var dungeonRoomVariation in dungeonRoomVariationsPrefabsList)
                dungeonRoomVariation.createdRoomCount = 0;
            GenerateDungeon();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            var tileMapString = "";
            foreach (var tileList in dungeonMapUsedTilesList)
            {
                foreach (var tile in tileList) tileMapString += tile ? "x " : "o ";

                tileMapString += "\n";
            }

            Debug.Log(tileMapString);
        }
    }

    private void GenerateDungeon()
    {
        if (dungeonRooms > maxDungeonSize.x * maxDungeonSize.y)
        {
            Debug.LogError($"Dungeon rooms can't be {dungeonRooms} on field " +
                           $"{maxDungeonSize.x}x{maxDungeonSize.y}! Rooms value changed to " +
                           $"{maxDungeonSize.x * maxDungeonSize.y}");
            dungeonRooms = maxDungeonSize.x * maxDungeonSize.y;
        }

        GenerateDungeonRoom(startingRoom.dungeonRoom, transform.position,
            new Vector2Int(maxDungeonSize.x / 2, maxDungeonSize.y / 2),
            new Vector2Int(0, 0));


        while (allRoomsList.Count < dungeonRooms)
        {
            var roomToCreatePrefab = GetRandomAvailableDungeonRoomType(out var isHaveAvailableRoom);
            if (isHaveAvailableRoom)
            {
                var isRoomAdded = false;

                for (var i = 0; i < allRoomsList.Count && !isRoomAdded; i++)
                    while (allRoomsList[i].GetUnusedExitsCount() > 0)
                    {
                        UseRandomDungeonRoomExit(allRoomsList[i], roomToCreatePrefab, out isRoomAdded);
                        if (isRoomAdded)
                            break;
                    }

                if (!isRoomAdded)
                {
                    for (var i = 0; i < allRoomsList.Count && !isRoomAdded; i++)
                        while (allRoomsList[i].GetAvailableExitsCount() > 0)
                        {
                            allRoomsList[i].TryAddRandomExit(out var isAdded);
                            if (isAdded)
                            {
                                UseRandomDungeonRoomExit(allRoomsList[i], roomToCreatePrefab, out isRoomAdded);
                                if (isRoomAdded)
                                    break;
                            }
                        }

                    if (!isRoomAdded)
                    {
                        Debug.LogError("Комната не добавлена!");
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        for (var i = 1; i < allRoomsList.Count; i++) allRoomsList[i].UnlockAllExits();
    }

    private DungeonRoom GenerateDungeonRoom(Transform roomToCreatePrefab, Vector3 roomTransformPosition,
        Vector2Int roomGridPosition, Vector2Int roomPosition)
    {
        var currentRoom = Instantiate(roomToCreatePrefab, roomTransformPosition,
            Quaternion.identity, transform).GetComponent<DungeonRoom>();

        currentRoom.TryGenerateExits();
        currentRoom.SetRoomGridPosition(roomGridPosition);
        var createdRoomSize = roomToCreatePrefab.GetComponent<DungeonRoom>().GetRoomSize();
        var sideXCheckLength = (createdRoomSize.x - 1) / 2;
        var sideYCheckLength = (createdRoomSize.y - 1) / 2;

        var minYPosition = roomPosition.y - sideYCheckLength;
        while (minYPosition < 0)
        {
            dungeonMapUsedTilesList.Insert(0, new List<bool>(dungeonMapUsedTilesList[0].Count));

            minYPosition++;
            roomPosition.y++;
            foreach (var dungeonRoom in allRoomsList)
                dungeonRoom.SetRoomPosition(dungeonRoom.GetRoomPosition() + new Vector2Int(0, 1));
        }

        var maxYPosition = roomPosition.y + sideYCheckLength;
        while (maxYPosition > dungeonMapUsedTilesList.Count - 1)
            dungeonMapUsedTilesList.Add(new List<bool>(dungeonMapUsedTilesList[0].Count));

        var minXPosition = roomPosition.x - sideXCheckLength;
        while (minXPosition < 0)
        {
            foreach (var dungeonMapTileList in dungeonMapUsedTilesList) dungeonMapTileList.Insert(0, false);
            foreach (var dungeonRoom in allRoomsList)
                dungeonRoom.SetRoomPosition(dungeonRoom.GetRoomPosition() + new Vector2Int(1, 0));

            minXPosition++;
            roomPosition.x++;
        }

        var maxXPosition = roomPosition.x + sideXCheckLength;
        while (maxXPosition > dungeonMapUsedTilesList[roomPosition.y].Count - 1)
            foreach (var dungeonMapTileList in dungeonMapUsedTilesList)
                dungeonMapTileList.Add(false);

        var maxValue = 0;
        foreach (var dungeonMapTileList in dungeonMapUsedTilesList)
            if (maxValue < dungeonMapTileList.Count)
                maxValue = dungeonMapTileList.Count;

        foreach (var dungeonMapTileList in dungeonMapUsedTilesList)
            if (dungeonMapTileList.Count < maxValue)
                for (var j = dungeonMapTileList.Count; j < maxValue; j++)
                    dungeonMapTileList.Add(false);

        if (roomPosition.x - sideXCheckLength < 0) roomPosition.x = sideXCheckLength;

        if (roomPosition.y - sideYCheckLength < 0)
            roomPosition.y = sideYCheckLength;

        currentRoom.SetRoomPosition(roomPosition);

        for (var i = roomPosition.y - sideYCheckLength; i <= roomPosition.y + sideYCheckLength; i++)
        for (var j = roomPosition.x - sideXCheckLength; j <= roomPosition.x + sideXCheckLength; j++)
            dungeonMapUsedTilesList[i][j] = true;

        allRoomsList.Add(currentRoom);
        return currentRoom;
    }

    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    private void UseRandomDungeonRoomExit(DungeonRoom dungeonRoom, Transform dungeonRoomPrefab, out bool isRoomAdded)
    {
        isRoomAdded = false;
        dungeonRoom.UseClosestExit(out var exit);

        var roomPosition = dungeonRoom.GetRoomPosition();
        var hallPosition = dungeonRoom.GetRoomPosition();

        var hallGridSize = dungeonRoom.GetHallGridSize();

        var roomLocation = dungeonRoom.transform.position;
        var roomGridPosition = dungeonRoom.GetRoomGridPosition();
        var hallLocation = roomLocation;

        var creatingRoomSize = dungeonRoomPrefab.GetComponent<DungeonRoom>().GetRoomSize();
        var createdRoomSize = dungeonRoom.GetRoomSize();
        var hallSize = dungeonRoom.GetHallSize();

        switch (exit)
        {
            case Exits.Right:
                roomGridPosition.x++;
                roomLocation += new Vector3(createdRoomSize.x / 2 + creatingRoomSize.x / 2 + hallSize.x + 1f, 0f, 0f);
                hallLocation += new Vector3(createdRoomSize.x / 2 + hallSize.x / 2 + 0.5f, 0f, 0f);

                roomPosition.x += (createdRoomSize.x - 1) / 2 + hallGridSize.x + (creatingRoomSize.x - 1) / 2 + 1;
                hallPosition.x += (createdRoomSize.x - 1) / 2 + (hallGridSize.x - 1) / 2 + 1;
                break;
            case Exits.Left:
                roomGridPosition.x--;
                roomLocation += new Vector3(-createdRoomSize.x / 2 + -creatingRoomSize.x / 2 - hallSize.x + -1f, 0f,
                    0f);
                hallLocation += new Vector3(-createdRoomSize.x / 2 - hallSize.x / 2 + -0.5f, 0f, 0f);

                roomPosition.x -= (createdRoomSize.x - 1) / 2 + hallGridSize.x + (creatingRoomSize.x - 1) / 2 + 1;
                hallPosition.x -= (createdRoomSize.x - 1) / 2 + (hallGridSize.x - 1) / 2 + 1;
                break;
            case Exits.Top:
                roomGridPosition.y++;

                hallGridSize = new Vector2Int(hallGridSize.y, hallGridSize.x);

                roomLocation += new Vector3(0f, 0f, createdRoomSize.y / 2 + creatingRoomSize.y / 2 + hallSize.x + 1f);
                hallLocation += new Vector3(0f, 0f, createdRoomSize.y / 2 + hallSize.x / 2 + 0.5f);

                roomPosition.y += (createdRoomSize.y - 1) / 2 + hallGridSize.y + (creatingRoomSize.y - 1) / 2 + 1;
                hallPosition.y += (createdRoomSize.y - 1) / 2 + (hallGridSize.y - 1) / 2 + 1;
                break;
            case Exits.Bottom:
                roomGridPosition.y--;

                hallGridSize = new Vector2Int(hallGridSize.y, hallGridSize.x);

                roomLocation += new Vector3(0f, 0f,
                    -createdRoomSize.y / 2 + -creatingRoomSize.y / 2 - hallSize.x + -1f);
                hallLocation += new Vector3(0f, 0f, -createdRoomSize.y / 2 - hallSize.x / 2 + -0.5f);

                roomPosition.y -= (createdRoomSize.y - 1) / 2 + hallGridSize.y + (creatingRoomSize.y - 1) / 2 + 1;
                hallPosition.y -= (createdRoomSize.y - 1) / 2 + (hallGridSize.y - 1) / 2 + 1;
                break;
        }

        if (CheckPositionAvailability(roomPosition, creatingRoomSize, roomGridPosition) &&
            CheckPositionAvailability(hallPosition, hallGridSize))
        {
            var nextRoom = GenerateDungeonRoom(dungeonRoomPrefab, roomLocation, roomGridPosition, roomPosition);
            isRoomAdded = true;

            var hallGridSideXSize = (hallGridSize.x - 1) / 2;
            var hallGridSideYSize = (hallGridSize.y - 1) / 2;

            hallPosition.y = hallPosition.y - hallGridSideYSize > 0 ? hallPosition.y : hallGridSideYSize;
            hallPosition.x = hallPosition.x - hallGridSideXSize > 0 ? hallPosition.x : hallGridSideXSize;

            for (var i = hallPosition.y - hallGridSideYSize; i <= hallPosition.y + hallGridSideYSize; i++)
            for (var j = hallGridSize.x - hallGridSideXSize; j <= hallGridSize.x + hallGridSideXSize; j++)
                dungeonMapUsedTilesList[i][j] = true;

            dungeonRoom.UseExit(exit);
            switch (exit)
            {
                case Exits.Right:
                    nextRoom.UseExit(Exits.Left);
                    Instantiate(dungeonRoom.GetRoomHall(), hallLocation, Quaternion.identity, transform);
                    break;
                case Exits.Left:
                    nextRoom.UseExit(Exits.Right);
                    Instantiate(dungeonRoom.GetRoomHall(), hallLocation, Quaternion.identity, transform);
                    break;
                case Exits.Top:
                    nextRoom.UseExit(Exits.Bottom);
                    Instantiate(dungeonRoom.GetRoomHall(), hallLocation, new Quaternion(0f, 0.707f, 0f, 0.707f),
                        transform);
                    break;
                case Exits.Bottom:
                    nextRoom.UseExit(Exits.Top);
                    Instantiate(dungeonRoom.GetRoomHall(), hallLocation, new Quaternion(0f, 0.707f, 0f, 0.707f),
                        transform);
                    break;
            }
        }
        else
        {
            dungeonRoom.RemoveExit(exit);
        }
    }

    private bool CheckPositionAvailability(Vector2Int centerPosition, Vector2Int checkingSize,
        Vector2Int gridPosition = default)
    {
        if (gridPosition != default)
            if (gridPosition.x > maxDungeonSize.x || gridPosition.y > maxDungeonSize.y ||
                gridPosition.x < 0 || gridPosition.y < 0)
                return false;

        var sideXCheckLength = (checkingSize.x - 1) / 2;
        var sideYCheckLength = (checkingSize.y - 1) / 2;
        if (centerPosition.y + sideYCheckLength < 0 ||
            centerPosition.y - sideYCheckLength >= dungeonMapUsedTilesList.Count)
            return true;

        if (centerPosition.x + sideXCheckLength < 0 ||
            centerPosition.x - sideXCheckLength >= dungeonMapUsedTilesList[0].Count)
            return true;

        for (var i = centerPosition.y - sideYCheckLength; i <= centerPosition.y + sideYCheckLength; i++)
        {
            if (i >= dungeonMapUsedTilesList.Count)
                continue;
            if (i > 0)
                for (var j = centerPosition.x - sideXCheckLength;
                     j <= centerPosition.x + sideXCheckLength;
                     j++)
                {
                    if (j >= dungeonMapUsedTilesList[i].Count)
                        continue;
                    if (j > 0)
                        if (dungeonMapUsedTilesList[i][j])
                            return false;
                }
        }

        return true;
    }

    private Transform GetRandomAvailableDungeonRoomType(out bool isHaveAvailableRoom)
    {
        isHaveAvailableRoom = false;
        List<DungeonRoomType> currentAvailableDungeonRoomType = new();
        foreach (var dungeonRoom in dungeonRoomVariationsPrefabsList)
            if (dungeonRoom.createdRoomCount < dungeonRoom.maxRoomCount)
                currentAvailableDungeonRoomType.Add(dungeonRoom);

        if (currentAvailableDungeonRoomType.Count > 0)
        {
            isHaveAvailableRoom = true;

            var roomTypeIndex = Random.Range(0, currentAvailableDungeonRoomType.Count);
            currentAvailableDungeonRoomType[roomTypeIndex].createdRoomCount++;
            return currentAvailableDungeonRoomType[roomTypeIndex].dungeonRoom;
        }

        return default;
    }
}
