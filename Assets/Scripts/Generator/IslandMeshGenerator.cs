using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SteppedIslandGenerator : MonoBehaviour {
  public int resolution = 40;
  public float islandSize = 50f;
  public int levels = 4;
  public float maxHeight = 10f;
  public float noiseScale = 0.12f;

  [Header("Shape randomization")] public float shapeNoiseScale = 1.2f;
  public float shapeNoiseStrength = 0.25f;
  public int shapeSeed = 0;

  [Button("Generate")]
  public void Generate() {
    Random.InitState(shapeSeed);
    int vertsX = resolution + 1;
    int vertsZ = resolution + 1;

    // Временные массивы
    Vector3[] allVertices = new Vector3[vertsX * vertsZ];
    bool[] isLand = new bool[vertsX * vertsZ];
    float offsetX = Random.value * 1000f;
    float offsetZ = Random.value * 1000f;

    float landThreshold = 0.25f; // подбирай под свой визуал

    // Генерация вершин и mask
    for (int z = 0; z < vertsZ; z++) {
      for (int x = 0; x < vertsX; x++) {
        float fx = (float)x / resolution;
        float fz = (float)z / resolution;
        float px = (fx - 0.5f) * islandSize;
        float pz = (fz - 0.5f) * islandSize;

        float distNorm = Mathf.Sqrt(px * px + pz * pz) / (islandSize * 0.5f);
        float angle = Mathf.Atan2(pz, px);
        float shapeNoise = Mathf.PerlinNoise(
          Mathf.Cos(angle) * shapeNoiseScale + offsetX,
          Mathf.Sin(angle) * shapeNoiseScale + offsetZ
        ) * shapeNoiseStrength;

        float maskRaw = 1f - distNorm + shapeNoise;
        float mask = maskRaw > 0 ? Mathf.SmoothStep(0, 1, maskRaw) : 0f;
        float edgeMargin = 0.08f; // поиграйся с этим
        isLand[z * vertsX + x] = mask > (landThreshold - edgeMargin);

        float noise = Mathf.PerlinNoise(px * noiseScale + offsetX, pz * noiseScale + offsetZ);
        float heightRaw = noise * mask * maxHeight;

        float step = maxHeight / levels;
        float quantized = Mathf.Floor(heightRaw / step) * step;
        if (quantized > (levels - 2) * step)
          quantized = maxHeight;

        allVertices[z * vertsX + x] = new Vector3(px, quantized, pz);
      }
    }

    // Чистые вершины и ремап
    List<Vector3> finalVertices = new List<Vector3>();
    Dictionary<int, int> vertRemap = new Dictionary<int, int>();

    // Треугольники только внутри острова
    List<int> finalTriangles = new List<int>();
    for (int z = 0; z < resolution; z++) {
      for (int x = 0; x < resolution; x++) {
        int i0 = z * vertsX + x;
        int i1 = z * vertsX + x + 1;
        int i2 = (z + 1) * vertsX + x;
        int i3 = (z + 1) * vertsX + x + 1;

        // Первый треугольник
        if (isLand[i0] && isLand[i2] && isLand[i1]) {
          finalTriangles.Add(GetOrAddVertex(i0, allVertices, finalVertices, vertRemap));
          finalTriangles.Add(GetOrAddVertex(i2, allVertices, finalVertices, vertRemap));
          finalTriangles.Add(GetOrAddVertex(i1, allVertices, finalVertices, vertRemap));
        }

        // Второй треугольник
        if (isLand[i1] && isLand[i2] && isLand[i3]) {
          finalTriangles.Add(GetOrAddVertex(i1, allVertices, finalVertices, vertRemap));
          finalTriangles.Add(GetOrAddVertex(i2, allVertices, finalVertices, vertRemap));
          finalTriangles.Add(GetOrAddVertex(i3, allVertices, finalVertices, vertRemap));
        }
      }
    }

    // Меш
    Mesh mesh = new Mesh();
    mesh.indexFormat = finalVertices.Count > 65000
      ? UnityEngine.Rendering.IndexFormat.UInt32
      : UnityEngine.Rendering.IndexFormat.UInt16;
    mesh.vertices = finalVertices.ToArray();
    mesh.triangles = finalTriangles.ToArray();
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    GetComponent<MeshFilter>().mesh = mesh;
  }

  // Ремап вершин
  static int GetOrAddVertex(int idx, Vector3[] allVertices, List<Vector3> finalVertices, Dictionary<int, int> remap) {
    if (remap.TryGetValue(idx, out int found)) return found;
    int newIndex = finalVertices.Count;
    finalVertices.Add(allVertices[idx]);
    remap[idx] = newIndex;
    return newIndex;
  }

#if UNITY_EDITOR
  [Button("Generate Random Seed")]
  void EditorGenRandom() {
    shapeSeed++;
    Generate();
  }
#endif
}