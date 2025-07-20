using UnityEngine;
using System.Collections.Generic;

public class IslandResourceSpawner : MonoBehaviour {
  public GameObject[] resourcePrefabs;
  public int minResources = 10, maxResources = 20;
  public float minDistanceBetweenResources = 1.5f;
  public ClusterIslandGenerator island;

  public void SpawnResources() {
    // Собираем список ресурсов на удаление
    List<GameObject> toDelete = new List<GameObject>();
    foreach (Transform child in transform)
      if (child.gameObject.name.StartsWith("Resource_"))
        toDelete.Add(child.gameObject);

    // Удаляем всё что нужно
#if UNITY_EDITOR
    foreach (var obj in toDelete)
      DestroyImmediate(obj);
#else
    foreach (var obj in toDelete)
        Destroy(obj);
#endif

    Debug.Log(
      $"Спавним {minResources} ресурсов из {resourcePrefabs.Length} префабов по {island.ValidLandPoints.Count} точкам");

    for (int i = 0; i < minResources && i < island.ValidLandPoints.Count; i++) {
      var pos = island.ValidLandPoints[Random.Range(0, island.ValidLandPoints.Count)];
      var prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
      var go = Instantiate(prefab, pos, Quaternion.identity, transform);
      go.name = "Resource_" + go.name;
      Debug.Log($"Resource spawned at {pos}");
    }
  }
}