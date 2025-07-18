using UnityEngine;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour {
  public float islandRadius = 20f;
  public int resourceCount = 50;
  public GameObject[] resourcePrefabs;
  public GameObject[] enemyPrefabs;
  public int enemiesPerResource = 1;

  public Transform parentResources;
  public Transform parentEnemies;

  [ContextMenu("Generate Island")]
  public void GenerateIsland() {
    // Очистка предыдущего
    foreach (Transform child in parentResources) DestroyImmediate(child.gameObject);
    foreach (Transform child in parentEnemies) DestroyImmediate(child.gameObject);

    // Спавн ресурсов
    List<Vector3> resourcePositions = new List<Vector3>();
    for (int i = 0; i < resourceCount; i++) {
      Vector2 pos2D = Random.insideUnitCircle * islandRadius;
      Vector3 pos = new Vector3(pos2D.x, 0, pos2D.y);
      var prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
      var go = Instantiate(prefab, pos, Quaternion.identity, parentResources);
      resourcePositions.Add(pos);
    }

    // Спавн врагов рядом с ресурсами
    foreach (var resourcePos in resourcePositions) {
      for (int i = 0; i < enemiesPerResource; i++) {
        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(2f, 4f); // 2-4 метра от ресурса
        Vector3 enemyPos = resourcePos + new Vector3(offset.x, 0, offset.y);
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, enemyPos, Quaternion.identity, parentEnemies);
      }
    }
  }
}