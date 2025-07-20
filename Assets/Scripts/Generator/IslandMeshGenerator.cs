using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClusterIslandGenerator : MonoBehaviour {
  [Header("Seed")] public int seed = 0;
  [Header("Clusters")] public int minClusters = 1, maxClusters = 5;
  public float minRadius = 4f, maxRadius = 7f;
  public float minHeight = 1.5f, maxHeight = 3.5f;
  public float minDist = 6f, maxDist = 12f;
  [Header("Island Mesh")] public int resolution = 64;
  public float islandSize = 24f;

  [Header("Plateau shape")] [Range(0.5f, 0.98f)]
  public float plateauPercent = 0.8f;

  public List<Vector3> ValidLandPoints = new List<Vector3>();

  public void Generate() {
    var clusters = GenerateClusters(seed, minClusters, maxClusters, minRadius, maxRadius, minHeight, maxHeight, minDist,
      maxDist);

    int vertsX = resolution + 1;
    int vertsZ = resolution + 1;
    Vector3[] vertices = new Vector3[vertsX * vertsZ];
    int[] triangles = new int[resolution * resolution * 6];
    ValidLandPoints.Clear();

    // 1. Генерация высот
    for (int z = 0; z < vertsZ; z++) {
      for (int x = 0; x < vertsX; x++) {
        float px = (x - vertsX * 0.5f) / resolution * islandSize;
        float pz = (z - vertsZ * 0.5f) / resolution * islandSize;
        Vector2 p = new Vector2(px, pz);

        float h = 0f;
        foreach (var cluster in clusters) {
          Vector2 delta = p - cluster.center;
          float dist = delta.magnitude;

          float angle = Mathf.Atan2(delta.y, delta.x);
          float edgeNoise = Mathf.PerlinNoise(
            Mathf.Cos(angle) * cluster.noiseScale + cluster.noiseOffset,
            Mathf.Sin(angle) * cluster.noiseScale + cluster.noiseOffset
          ) * cluster.noiseStrength;
          float effectiveRadius = cluster.radius * (1f + edgeNoise);

          if (dist > effectiveRadius) continue;

          float plateauPart = effectiveRadius * plateauPercent;
          if (dist < plateauPart) {
            h = Mathf.Max(h, cluster.height);
          }
          else {
            float t = Mathf.InverseLerp(effectiveRadius, plateauPart, dist);
            h = Mathf.Max(h, Mathf.Lerp(0, cluster.height, 1 - t));
          }
        }

        Vector3 pos = new Vector3(px, h, pz);
        vertices[z * vertsX + x] = pos;
      }
    }

    // 2. Создаём карту "суша/не суша" для быстрого анализа окрестностей
    float minLandHeight = 0.01f;
    float minEdgeMargin = 1.0f;
    float maxIslandRadius = islandSize * 0.5f;
    bool[,] isLand = new bool[vertsX, vertsZ];

    for (int z = 0; z < vertsZ; z++)
      for (int x = 0; x < vertsX; x++) {
        var pos = vertices[z * vertsX + x];
        float px = pos.x;
        float pz = pos.z;
        float h = pos.y;
        float r = Mathf.Sqrt(px * px + pz * pz);
        isLand[x, z] = h > minLandHeight && r < (maxIslandRadius - minEdgeMargin);
      }

    // 3. Только те точки, вокруг которых в радиусе 2 клеток вся суша
    int checkRadius = 2;
    for (int z = checkRadius; z < vertsZ - checkRadius; z++) {
      for (int x = checkRadius; x < vertsX - checkRadius; x++) {
        if (!isLand[x, z]) continue;

        bool allDeepLand = true;
        for (int dz = -checkRadius; dz <= checkRadius && allDeepLand; dz++)
          for (int dx = -checkRadius; dx <= checkRadius; dx++) {
            if (!isLand[x + dx, z + dz]) {
              allDeepLand = false;
              break;
            }
          }

        if (allDeepLand)
          ValidLandPoints.Add(vertices[z * vertsX + x]);
      }
    }

    // 4. Меш
    int ti = 0;
    for (int z = 0; z < resolution; z++) {
      for (int x = 0; x < resolution; x++) {
        int i0 = z * vertsX + x;
        int i1 = z * vertsX + x + 1;
        int i2 = (z + 1) * vertsX + x;
        int i3 = (z + 1) * vertsX + x + 1;

        triangles[ti++] = i0;
        triangles[ti++] = i2;
        triangles[ti++] = i1;
        triangles[ti++] = i1;
        triangles[ti++] = i2;
        triangles[ti++] = i3;
      }
    }

    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    GetComponent<MeshFilter>().mesh = mesh;
  }

  List<IslandCluster> GenerateClusters(int seed, int minClusters, int maxClusters, float minRadius, float maxRadius,
    float minHeight, float maxHeight, float minDist, float maxDist) {
    var clusters = new List<IslandCluster>();
    Random.InitState(seed);

    int count = Random.Range(minClusters, maxClusters + 1);
    clusters.Add(new IslandCluster {
      center = Vector2.zero,
      radius = Random.Range(minRadius, maxRadius),
      height = Random.Range(minHeight, maxHeight),
      noiseScale = Random.Range(0.7f, 1.4f),
      noiseStrength = Random.Range(0.08f, 0.25f),
      noiseOffset = Random.Range(0, 1000f)
    });

    for (int i = 1; i < count; i++) {
      float angle = Random.value * Mathf.PI * 2f;
      float dist = Random.Range(minDist, maxDist);
      Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

      clusters.Add(new IslandCluster {
        center = pos,
        radius = Random.Range(minRadius, maxRadius),
        height = Random.Range(minHeight, maxHeight),
        noiseScale = Random.Range(0.7f, 1.4f),
        noiseStrength = Random.Range(0.08f, 0.25f),
        noiseOffset = Random.Range(0, 1000f)
      });
    }

    return clusters;
  }

  [System.Serializable]
  public struct IslandCluster {
    public Vector2 center;
    public float radius;
    public float height;
    public float noiseScale;
    public float noiseStrength;
    public float noiseOffset;
  }
}