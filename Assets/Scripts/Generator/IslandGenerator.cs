using UnityEngine;
using System.Collections.Generic;

public class IslandResourceSpawner : MonoBehaviour {
  [Header("Resource Prefabs")] public GameObject[] resourcePrefabs;

  [Header("Groups")] public int minGroups = 3, maxGroups = 6;
  public int minInGroup = 3, maxInGroup = 7;
  public float groupRadius = 3f;
  public float groupCenterEdgeMargin = 3f;

  [Header("Cluster")] public float minDistanceBetweenGroups = 7f;
  public ClusterIslandGenerator island;

  private List<Vector3> groupCenters = new List<Vector3>();

  public void SpawnResources() {
    // Удаляем старое
    List<GameObject> toDelete = new List<GameObject>();
    foreach (Transform child in transform)
      if (child.gameObject.name.StartsWith("Resource_"))
        toDelete.Add(child.gameObject);
#if UNITY_EDITOR
    foreach (var obj in toDelete)
      DestroyImmediate(obj);
#else
        foreach (var obj in toDelete)
            Destroy(obj);
#endif

    // Фильтрация валидных точек по удалённости от края
    List<Vector3> filteredPoints = new List<Vector3>();
    float margin = groupCenterEdgeMargin;
    foreach (var p in island.ValidLandPoints) {
      // Простейший вариант: все точки не ближе margin к границе (по расстоянию до центра острова)
      // Можно усложнить: если island реально круглый, сравнивай длину до центра, если нет — фильтруй по y, по своей маске и т.д.
      if (IsInnerPoint(p, margin))
        filteredPoints.Add(p);
    }

    int numGroups = Mathf.Min(Random.Range(minGroups, maxGroups + 1), filteredPoints.Count);
    groupCenters.Clear();

    // Выбор центров групп (не слишком близко друг к другу)
    int attempts = 0;
    while (groupCenters.Count < numGroups && attempts < 1000) {
      var center = filteredPoints[Random.Range(0, filteredPoints.Count)];
      bool tooClose = false;
      foreach (var c in groupCenters)
        if ((c - center).sqrMagnitude < minDistanceBetweenGroups * minDistanceBetweenGroups) {
          tooClose = true;
          break;
        }

      if (!tooClose)
        groupCenters.Add(center);

      attempts++;
    }

    // Теперь расставляем ресурсы внутри групп
    int resourceId = 0;
    foreach (var center in groupCenters) {
      int countInGroup = Random.Range(minInGroup, maxInGroup + 1);
      int placed = 0;
      int groupAttempts = 0;
      while (placed < countInGroup && groupAttempts < 100) {
        // Случайная точка внутри круга groupRadius
        Vector2 offset2D = Random.insideUnitCircle * groupRadius;
        Vector3 spawnPos = center + new Vector3(offset2D.x, 0, offset2D.y);

        // Находим ближайшую валидную точку (чтобы точно не на краю/воде)
        Vector3 closest = FindClosestValid(spawnPos, island.ValidLandPoints, groupRadius * 1.5f);
        if (closest != Vector3.positiveInfinity) {
          var prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
          var go = Instantiate(prefab, closest, Quaternion.identity, transform);
          go.name = $"Resource_{resourceId++}";
          placed++;
        }

        groupAttempts++;
      }
    }
  }

  // Простейшая фильтрация: центр — (0,0), ограничиваем радиус.
  bool IsInnerPoint(Vector3 p, float margin) {
    float r = Mathf.Sqrt(p.x * p.x + p.z * p.z);
    float maxR = (island.islandSize * 0.5f) - margin;
    return r < maxR && p.y > 0.05f;
  }

  // Поиск ближайшей подходящей точки среди суши (на случай если случайная точка оказалась вне острова)
  Vector3 FindClosestValid(Vector3 target, List<Vector3> candidates, float maxDist) {
    float minSqr = maxDist * maxDist;
    Vector3 best = Vector3.positiveInfinity;
    foreach (var c in candidates) {
      float d = (c - target).sqrMagnitude;
      if (d < minSqr) {
        minSqr = d;
        best = c;
      }
    }

    return best;
  }
}