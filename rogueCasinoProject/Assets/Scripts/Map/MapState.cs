using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe statique pour sauvegarder/restaurer l'état de la map entre scènes
/// </summary>
public static class MapState
{
    public enum EncounterType
    {
        None,
        Combat,
        Boss
    }

    [System.Serializable]
    public struct SavedHexCase
    {
        public int q;
        public int r;
        public CaseType type;
        public int depth;
        public bool isVisited;
    }

    private static Dictionary<(int, int), SavedHexCase> savedMap;
    private static int playerQ = -1;
    private static int playerR = -1;
    private static bool hasSavedState = false;
    private static EncounterType pendingEncounter = EncounterType.None;

    public static void SetPendingEncounter(EncounterType encounterType)
    {
        pendingEncounter = encounterType;
    }

    public static EncounterType GetPendingEncounter()
    {
        return pendingEncounter;
    }

    public static void ClearPendingEncounter()
    {
        pendingEncounter = EncounterType.None;
    }

    public static void SaveState(Dictionary<(int, int), HexCase> map, HexCase playerPosition)
    {
        savedMap = new Dictionary<(int, int), SavedHexCase>();

        // Sauvegarder toutes les cases
        foreach (var kvp in map)
        {
            var hexCase = kvp.Value;
            SavedHexCase saved = new SavedHexCase
            {
                q = hexCase.q,
                r = hexCase.r,
                type = hexCase.type,
                depth = hexCase.depth,
                isVisited = hexCase.isVisited
            };
            savedMap[kvp.Key] = saved;
        }

        // Sauvegarder position du joueur
        playerQ = playerPosition.q;
        playerR = playerPosition.r;

        hasSavedState = true;
        Debug.Log($"Map sauvegardée: {map.Count} cases, joueur en ({playerQ}, {playerR})");
    }

    public static bool HasSavedState()
    {
        return hasSavedState;
    }

    public static void LoadIntoMap(Dictionary<(int, int), HexCase> map, out int outPlayerQ, out int outPlayerR)
    {
        map.Clear();

        // Restaurer toutes les cases
        foreach (var kvp in savedMap)
        {
            var saved = kvp.Value;
            HexCase hexCase = new HexCase(saved.q, saved.r, saved.type, saved.depth)
            {
                isVisited = saved.isVisited
            };
            map[kvp.Key] = hexCase;
        }

        outPlayerQ = playerQ;
        outPlayerR = playerR;

        Debug.Log($"Map restaurée: {map.Count} cases, joueur restauré en ({playerQ}, {playerR})");
    }

    public static void ClearSavedState()
    {
        savedMap = null;
        playerQ = -1;
        playerR = -1;
        hasSavedState = false;
        pendingEncounter = EncounterType.None;
        Debug.Log("État de la map effacé");
    }
}
