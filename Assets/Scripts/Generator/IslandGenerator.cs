using UnityEngine;
using System.Collections.Generic;

public class IslandResourceSpawner : MonoBehaviour {
  public GameObject[] resourcePrefabs;
  public int minResources = 10, maxResources = 20;
  public float minDistanceBetweenResources = 1.5f;
  public ClusterIslandGenerator island;

  public void SpawnResources() {
    if (resourcePrefabs == null || resourcePrefabs.Length == 0) return;
    if (island == null || island.ValidLandPoints == null || island.ValidLandPoints.Count == 0) {
      Debug.LogWarning("No valid points to spawn resources. Generate island first.");
      return;
    }

    // Удаляем старые ресурсы
#if UNITY_EDITOR
    foreach (Transform child in transform)
      if (child.gameObject.name.StartsWith("Resource_"))
        DestroyImmediate(child.gameObject);
#else
        foreach (Transform child in transform)
            if (child.gameObject.name.StartsWith("Resource_"))
                Destroy(child.gameObject);
#endif

    int numResources = Random.Range(minResources, maxResources + 1);
    var used = new List<Vector3>();
    int attempts = 0;
    int maxAttempts = island.ValidLandPoints.Count * 5;
    while (used.Count < numResources && attempts < maxAttempts) {
      var pos = island.ValidLandPoints[Random.Range(0, island.ValidLandPoints.Count)];
      bool tooClose = false;
      foreach (var u in used)
        if ((u - pos).sqrMagnitude < minDistanceBetweenResources * minDistanceBetweenResources) {
          tooClose = true;
          break;
        }

      if (!tooClose) {
        var prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
        var go = Instantiate(prefab, transform);
        go.name = "Resource_" + go.name;
        go.transform.position = transform.TransformPoint(pos);
        used.Add(pos);
      }

      attempts++;
    }
  }
}