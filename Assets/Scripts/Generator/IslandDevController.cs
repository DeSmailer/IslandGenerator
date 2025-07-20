using NaughtyAttributes;
using UnityEngine;

public class IslandDevController : MonoBehaviour {
  public ClusterIslandGenerator island;
  public IslandResourceSpawner spawner;

  [Button("Next Seed")]
  public void NextSeed() {
    island.seed++;
    GenerateIsland();
  }

  [Button("Generate Island")]
  public void GenerateIsland() {
    island.Generate();
  }

  [Button("Spawn Resources")]
  public void SpawnResources() {
    spawner.SpawnResources();
  }

  [Button("Generate All")]
  public void GenerateAll() {
    island.seed++;
    island.Generate();
    spawner.SpawnResources();
  }
}