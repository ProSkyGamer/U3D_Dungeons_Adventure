using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(DungeonRoomSettings))]
public class DungeonRoom : NetworkBehaviour
{
    [SerializeField] private Vector2Int roomSize = new(3, 3);
    [SerializeField] private Transform roomHall;
    [SerializeField] private Vector2 hallSize = new(3f, 1.8f);
    [SerializeField] private Vector2Int hallGridSize = new(3, 2);
    private Vector2Int roomPosition;

    private readonly List<Exits> usedExits = new();
    private readonly List<Exits> unusedExits = new();
    private readonly List<Exits> availableExits = new() { Exits.Right, Exits.Left, Exits.Top, Exits.Bottom };

    private Vector2Int roomGridPosition;
    [SerializeField] private Transform rightExit;
    [SerializeField] private Transform leftExit;
    [SerializeField] private Transform topExit;
    [SerializeField] private Transform bottomExit;

    public enum Exits
    {
        Right,
        Left,
        Top,
        Bottom
    }

    public void SetRoomGridPosition(Vector2Int position)
    {
        roomGridPosition = position;
    }

    public void SetRoomPosition(Vector2Int position)
    {
        roomPosition = position;
        gameObject.name = $"Room {position.x} {position.y}";
    }

    public void TryGenerateExits()
    {
        for (var i = 0; i < availableExits.Count; i++)
            if (Random.Range(0, 2) == 1)
            {
                unusedExits.Add(availableExits[i]);
                availableExits.RemoveAt(i);
                i--;
            }
    }

    public void TryAddRandomExit(out bool isAdded)
    {
        isAdded = false;
        var toAdd = Random.Range(0, availableExits.Count);

        if (availableExits.Count != 0)
        {
            unusedExits.Add(availableExits[toAdd]);
            availableExits.RemoveAt(toAdd);
            isAdded = true;
        }
    }

    public void UseClosestExit(out Exits exitDirection)
    {
        exitDirection = unusedExits[0];

        usedExits.Add(exitDirection);
        unusedExits.RemoveAt(0);
    }

    public void UseExit(Exits exitToUse)
    {
        usedExits.Add(exitToUse);
        unusedExits.Remove(exitToUse);
        availableExits.Remove(exitToUse);
    }

    public void RemoveExit(Exits exit)
    {
        LockExit(exit);
        usedExits.Remove(exit);
        unusedExits.Remove(exit);
        availableExits.Remove(exit);
    }

    public void UnlockAllExits()
    {
        if (!IsServer) return;

        var exits = new int[usedExits.Count];
        for (var i = 0; i < usedExits.Count; i++) exits[i] = (int)usedExits[i];
        UnlockAllExitsServerRpc(exits);
    }

    [ServerRpc]
    private void UnlockAllExitsServerRpc(int[] exits)
    {
        UnlockAllExitsClientRpc(exits);
    }

    [ClientRpc]
    private void UnlockAllExitsClientRpc(int[] exits)
    {
        usedExits.Clear();
        foreach (var exit in exits) usedExits.Add((Exits)exit);

        foreach (var exit in usedExits)
            switch (exit)
            {
                case Exits.Right:
                    rightExit.gameObject.SetActive(false);
                    break;
                case Exits.Left:
                    leftExit.gameObject.SetActive(false);
                    break;
                case Exits.Top:
                    topExit.gameObject.SetActive(false);
                    break;
                case Exits.Bottom:
                    bottomExit.gameObject.SetActive(false);
                    break;
            }
    }

    public void UnlockAllStartExits()
    {
        if (!IsServer) return;

        var exits = new int[usedExits.Count];
        for (var i = 0; i < usedExits.Count; i++) exits[i] = (int)usedExits[i];
        UnlockAllStartExitsServerRpc(exits);
    }

    [ServerRpc]
    private void UnlockAllStartExitsServerRpc(int[] exits)
    {
        UnlockAllStartExitsClientRpc(exits);
    }

    [ClientRpc]
    private void UnlockAllStartExitsClientRpc(int[] exits)
    {
        usedExits.Clear();
        foreach (var exit in exits) usedExits.Add((Exits)exit);

        foreach (var exit in usedExits)
            switch (exit)
            {
                case Exits.Right:
                    rightExit.gameObject.SetActive(false);
                    break;
                case Exits.Left:
                    leftExit.gameObject.SetActive(false);
                    break;
                case Exits.Top:
                    topExit.gameObject.SetActive(false);
                    break;
                case Exits.Bottom:
                    bottomExit.gameObject.SetActive(false);
                    break;
            }
    }

    private void LockExit(Exits exit)
    {
        if (!IsServer) return;

        LockExitServerRpc((int)exit);
    }

    [ServerRpc]
    private void LockExitServerRpc(int exitToLock)
    {
        LockExitClientRpc(exitToLock);
    }

    [ClientRpc]
    private void LockExitClientRpc(int exitToLock)
    {
        switch ((Exits)exitToLock)
        {
            case Exits.Right:
                rightExit.gameObject.SetActive(true);
                break;
            case Exits.Left:
                leftExit.gameObject.SetActive(true);
                break;
            case Exits.Top:
                topExit.gameObject.SetActive(true);
                break;
            case Exits.Bottom:
                bottomExit.gameObject.SetActive(true);
                break;
        }
    }

    public int GetUnusedExitsCount()
    {
        return unusedExits.Count;
    }

    public int GetAvailableExitsCount()
    {
        return availableExits.Count;
    }

    public Vector2Int GetRoomSize()
    {
        return roomSize;
    }

    public Transform GetRoomHall()
    {
        return roomHall;
    }

    public Vector2 GetHallSize()
    {
        return hallSize;
    }

    public Vector2Int GetRoomGridPosition()
    {
        return roomGridPosition;
    }

    public Vector2Int GetRoomPosition()
    {
        return roomPosition;
    }

    public Vector2Int GetHallGridSize()
    {
        return hallGridSize;
    }
}
