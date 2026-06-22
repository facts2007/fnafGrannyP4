using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RandomSpawner.cs
///
/// Generic random-position spawner. Drop one of these per "thing that needs
/// random placement" (RightFusebox, Crowbar, the 3 non-barricaded fuses).
///
/// SETUP:
/// - prefabToSpawn: the prefab to instantiate (e.g. RightFusebox prefab, Crowbar prefab, Fuse prefab)
/// - spawnPoints[]: empty GameObjects placed around the map as possible spawn locations
/// - spawnCount: how many of the prefab to spawn (e.g. 1 for fusebox/crowbar, 3 for the random fuses)
/// - matchSpawnRotation: if true, spawned object copies the spawn point's rotation too
///
/// USAGE:
/// Call SpawnAtRandomPoints() once at scene start (e.g. from a GameManager's Start(),
/// or this script's own Start() if it should just spawn immediately on load).
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    [Header("What To Spawn")]
    public GameObject prefabToSpawn;

    [Header("Possible Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Settings")]
    [Tooltip("How many copies of the prefab to spawn, picked from DIFFERENT random spawn points (no duplicates).")]
    public int spawnCount = 1;
    public bool matchSpawnRotation = true;

    [Tooltip("If true, spawns automatically on Start(). Turn off if a GameManager should control timing instead.")]
    public bool spawnOnStart = true;

    [HideInInspector] public List<GameObject> spawnedInstances = new List<GameObject>();

    void Start()
    {
        if (spawnOnStart)
            SpawnAtRandomPoints();
    }

    /// <summary>Spawns `spawnCount` copies of prefabToSpawn at random, non-repeating spawn points.</summary>
    public void SpawnAtRandomPoints()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[RandomSpawner] {name}: No prefab assigned!");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"[RandomSpawner] {name}: No spawn points assigned!");
            return;
        }

        if (spawnCount > spawnPoints.Count)
        {
            Debug.LogWarning($"[RandomSpawner] {name}: spawnCount ({spawnCount}) is greater than available spawn points ({spawnPoints.Count}). Clamping.");
            spawnCount = spawnPoints.Count;
        }

        // Shuffle a copy of the spawn point list (Fisher-Yates), then take the first N.
        List<Transform> shuffled = new List<Transform>(spawnPoints);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        spawnedInstances.Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            Transform point = shuffled[i];
            Quaternion rot = matchSpawnRotation ? point.rotation : Quaternion.identity;

            GameObject instance = Instantiate(prefabToSpawn, point.position, rot);
            spawnedInstances.Add(instance);

            Debug.Log($"[RandomSpawner] {name}: Spawned '{prefabToSpawn.name}' at '{point.name}'");
        }
    }
}