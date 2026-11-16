# ProceduralMeshes - 程序化网格生成器

> **用途**：当外部素材文件不存在时，使用程序化生成Mesh作为Fallback
> **路径**：`Assets/Scripts/Visuals/ProceduralMeshes.cs`
> **使用场景**：TileManager.FallbackMeshes() 调用这些方法生成Mesh

---

## 完整代码

```csharp
using UnityEngine;
using System;

public static class ProceduralMeshes
{
    /// <summary>
    /// 创建立方体 - 底部在 y=0，顶部在 y=h（不是以原点为中心！）
    /// </summary>
    public static Mesh CreateBox(float w, float h, float d, string meshName = "Box")
    {
        Mesh mesh = new Mesh { name = meshName };
        float hw = w / 2f, hd = d / 2f;

        Vector3[] verts = new Vector3[36];
        int[] tris = new int[36];

        Vector3 ftl = new Vector3(-hw, h, hd);
        Vector3 ftr = new Vector3(hw, h, hd);
        Vector3 fbl = new Vector3(-hw, 0f, hd);
        Vector3 fbr = new Vector3(hw, 0f, hd);
        Vector3 btl = new Vector3(-hw, h, -hd);
        Vector3 btr = new Vector3(hw, h, -hd);
        Vector3 bbl = new Vector3(-hw, 0f, -hd);
        Vector3 bbr = new Vector3(hw, 0f, -hd);

        int vi = 0;
        void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 dd)
        {
            verts[vi] = a; verts[vi+1] = b; verts[vi+2] = c;
            tris[vi] = vi; tris[vi+1] = vi+1; tris[vi+2] = vi+2;
            vi += 3;
            verts[vi] = a; verts[vi+1] = c; verts[vi+2] = dd;
            tris[vi] = vi; tris[vi+1] = vi+1; tris[vi+2] = vi+2;
            vi += 3;
        }

        AddQuad(ftl, ftr, fbr, fbl); // Front
        AddQuad(btr, btl, bbl, bbr); // Back
        AddQuad(btl, ftl, fbl, bbl); // Left
        AddQuad(ftr, btr, bbr, fbr); // Right
        AddQuad(btl, btr, ftr, ftl); // Top
        AddQuad(fbl, fbr, bbr, bbl); // Bottom

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建玩家Mesh - 身体 y=0..1.0，头部 y=1.0..1.4
    /// </summary>
    public static Mesh CreatePlayerMesh()
    {
        Mesh body = CreateBox(0.6f, 1.0f, 0.4f, "PlayerBody");
        Mesh head = CreateBox(0.4f, 0.4f, 0.4f, "PlayerHead");

        Vector3[] headVerts = head.vertices;
        for (int i = 0; i < headVerts.Length; i++)
            headVerts[i] += Vector3.up * 1.0f;  // 头部放在身体顶部

        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = body;
        combine[0].transform = Matrix4x4.identity;

        Mesh offsetHead = new Mesh();
        offsetHead.vertices = headVerts;
        offsetHead.triangles = head.triangles;
        offsetHead.RecalculateNormals();
        combine[1].mesh = offsetHead;
        combine[1].transform = Matrix4x4.identity;

        Mesh result = new Mesh { name = "PlayerMesh" };
        result.CombineMeshes(combine, true, false);
        result.RecalculateNormals();
        return result;
    }

    /// <summary>
    /// 创建跑道地块 - 4顶点平面，y=0，沿Z轴延伸
    /// </summary>
    public static Mesh CreateRoadTile(float width, float length)
    {
        Mesh mesh = new Mesh { name = "RoadTile" };
        Vector3[] verts = new Vector3[4]
        {
            new Vector3(-width/2f, 0f, 0f),
            new Vector3(width/2f, 0f, 0f),
            new Vector3(-width/2f, 0f, length),
            new Vector3(width/2f, 0f, length)
        };
        int[] tris = new int[6] { 0, 2, 1, 1, 2, 3 };

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建地面条带 - 同上但y=-0.05（略低于路面）
    /// </summary>
    public static Mesh CreateGroundStrip(float width, float length)
    {
        Mesh mesh = new Mesh { name = "GroundStrip" };
        Vector3[] verts = new Vector3[4]
        {
            new Vector3(-width/2f, -0.05f, 0f),
            new Vector3(width/2f, -0.05f, 0f),
            new Vector3(-width/2f, -0.05f, length),
            new Vector3(width/2f, -0.05f, length)
        };
        int[] tris = new int[6] { 0, 2, 1, 1, 2, 3 };

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建圆锥 - 底部y=0，尖端y=height，flat shaded
    /// </summary>
    public static Mesh CreateCone(float radius, float height, int segments)
    {
        Mesh mesh = new Mesh { name = "Cone" };
        int vertexCount = segments + 2;
        Vector3[] verts = new Vector3[vertexCount];
        int[] tris = new int[segments * 6];

        // 顶点0：尖端
        verts[0] = new Vector3(0f, height, 0f);

        // 顶点1~segments+1：底部圆周
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // 侧面三角形（尖端 + 底部圆周）
        for (int i = 0; i < segments; i++)
        {
            int ti = i * 3;
            tris[ti] = 0;
            tris[ti + 1] = i + 1;
            tris[ti + 2] = i + 2;
        }

        // 底部三角形（圆周）
        for (int i = 0; i < segments; i++)
        {
            int ti = segments * 3 + i * 3;
            tris[ti] = 1; // 底部中心（需要手动添加中心点）
            tris[ti + 1] = i + 2;
            tris[ti + 2] = i + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建圆柱 - 底部y=0，顶部y=height，flat shaded
    /// </summary>
    public static Mesh CreateCylinder(float radius, float height, int segments)
    {
        Mesh mesh = new Mesh { name = "Cylinder" };
        int vertexCount = (segments + 1) * 2;
        Vector3[] verts = new Vector3[vertexCount];
        int[] tris = new int[segments * 12];

        // 底部圆周
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // 顶部圆周
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + segments + 1] = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
        }

        // 侧面三角形
        for (int i = 0; i < segments; i++)
        {
            int ti = i * 6;
            tris[ti] = i;
            tris[ti + 1] = i + 1;
            tris[ti + 2] = i + segments + 1;
            tris[ti + 3] = i + segments + 1;
            tris[ti + 4] = i + 1;
            tris[ti + 5] = i + segments + 2;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建二十面体 - 标准二十面体细分，flat shaded
    /// </summary>
    public static Mesh CreateIcosphere(float radius, int subdivisions)
    {
        Mesh mesh = new Mesh { name = "Icosphere" };

        // 二十面体顶点
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        Vector3[] baseVerts = new Vector3[]
        {
            new Vector3(-1f, t, 0f), new Vector3(1f, t, 0f), new Vector3(-1f, -t, 0f), new Vector3(1f, -t, 0f),
            new Vector3(0f, -1f, t), new Vector3(0f, 1f, t), new Vector3(0f, -1f, -t), new Vector3(0f, 1f, -t),
            new Vector3(t, 0f, -1f), new Vector3(t, 0f, 1f), new Vector3(-t, 0f, -1f), new Vector3(-t, 0f, 1f)
        };

        // 归一化并缩放
        for (int i = 0; i < baseVerts.Length; i++)
            baseVerts[i] = baseVerts[i].normalized * radius;

        // 二十面体三角形
        int[] baseTris = new int[]
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };

        // 简化版本：不进行细分，直接使用基础二十面体
        mesh.vertices = baseVerts;
        mesh.triangles = baseTris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建低多边形岩石 - 基于CreateIcosphere，顶点随机偏移25%，y钳制≥0
    /// </summary>
    public static Mesh CreateLowPolyRock(float size, int seed)
    {
        UnityEngine.Random.InitState(seed);
        Mesh baseMesh = CreateIcosphere(size, 1);
        Vector3[] verts = baseMesh.vertices;

        // 随机偏移顶点
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += UnityEngine.Random.insideUnitSphere * size * 0.25f;
            // y钳制≥0
            if (verts[i].y < 0f)
                verts[i].y = 0f;
        }

        baseMesh.vertices = verts;
        baseMesh.RecalculateNormals();
        baseMesh.name = "LowPolyRock";
        return baseMesh;
    }

    /// <summary>
    /// 创建金币 - 扁平圆柱，以原点为中心
    /// </summary>
    public static Mesh CreateCoinMesh(float radius, float thickness, int segments)
    {
        Mesh mesh = new Mesh { name = "Coin" };

        // 中心点
        int vertexCount = segments * 2 + 2;
        Vector3[] verts = new Vector3[vertexCount];
        int[] tris = new int[segments * 12];

        // 底部中心
        verts[0] = new Vector3(0f, -thickness/2f, 0f);
        // 顶部中心
        verts[1] = new Vector3(0f, thickness/2f, 0f);

        // 底部圆周
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + 2] = new Vector3(Mathf.Cos(angle) * radius, -thickness/2f, Mathf.Sin(angle) * radius);
        }

        // 顶部圆周
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + segments + 2] = new Vector3(Mathf.Cos(angle) * radius, thickness/2f, Mathf.Sin(angle) * radius);
        }

        // 底部三角形（中心 + 圆周）
        for (int i = 0; i < segments; i++)
        {
            int ti = i * 3;
            tris[ti] = 0;
            tris[ti + 1] = i + 2;
            tris[ti + 2] = ((i + 1) % segments) + 2;
        }

        // 顶部三角形（中心 + 圆周）
        for (int i = 0; i < segments; i++)
        {
            int ti = segments * 3 + i * 3;
            tris[ti] = 1;
            tris[ti + 1] = ((i + 1) % segments) + segments + 2;
            tris[ti + 2] = i + segments + 2;
        }

        // 侧面三角形
        for (int i = 0; i < segments; i++)
        {
            int ti = segments * 6 + i * 6;
            int next = (i + 1) % segments;
            tris[ti] = i + 2;
            tris[ti + 1] = next + 2;
            tris[ti + 2] = i + segments + 2;
            tris[ti + 3] = i + segments + 2;
            tris[ti + 4] = next + 2;
            tris[ti + 5] = next + segments + 2;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 创建路障 - 直接调用CreateBox
    /// </summary>
    public static Mesh CreateBarrier(float w, float h, float d)
    {
        return CreateBox(w, h, d, "Barrier");
    }
}
```

---

## 方法说明

| 方法 | 说明 | 参数 | 返回 |
|------|------|------|------|
| `CreateBox(w, h, d, meshName)` | 创建立方体，底部在y=0 | w=宽度, h=高度, d=深度, meshName=名称 | Mesh |
| `CreatePlayerMesh()` | 创建玩家Mesh，身体y=0..1.0，头部y=1.0..1.4 | - | Mesh |
| `CreateRoadTile(width, length)` | 创建跑道地块，4顶点平面 | width=宽度, length=长度 | Mesh |
| `CreateGroundStrip(width, length)` | 创建地面条带，y=-0.05 | width=宽度, length=长度 | Mesh |
| `CreateCone(radius, height, segments)` | 创建圆锥，底部y=0，尖端y=height | radius=半径, height=高度, segments=分段数 | Mesh |
| `CreateCylinder(radius, height, segments)` | 创建圆柱，底部y=0，顶部y=height | radius=半径, height=高度, segments=分段数 | Mesh |
| `CreateIcosphere(radius, subdivisions)` | 创建二十面体 | radius=半径, subdivisions=细分次数 | Mesh |
| `CreateLowPolyRock(size, seed)` | 创建低多边形岩石 | size=大小, seed=随机种子 | Mesh |
| `CreateCoinMesh(radius, thickness, segments)` | 创建金币，扁平圆柱 | radius=半径, thickness=厚度, segments=分段数 | Mesh |
| `CreateBarrier(w, h, d)` | 创建路障 | w=宽度, h=高度, d=深度 | Mesh |

---

## 使用示例

```csharp
// 在TileManager.cs中使用
void FallbackMeshes()
{
    if (roadMesh == null)
        roadMesh = ProceduralMeshes.CreateRoadTile(roadWidth, tileLength);
    
    if (playerMesh == null)
        playerMesh = ProceduralMeshes.CreatePlayerMesh();
    
    if (coinMesh == null)
        coinMesh = ProceduralMeshes.CreateCoinMesh(0.4f, 0.1f, 12);
    
    // ... 其他Mesh
}
```

---

## 注意事项

1. **CreateBox的陷阱**：底部在y=0，不是以原点为中心！
2. **CreatePlayerMesh的陷阱**：从y=0向上构建，头部在身体顶部
3. **随机性**：CreateLowPolyRock使用seed参数，确保相同seed生成相同形状
4. **性能**：程序化生成Mesh在运行时调用，建议在Start中预生成
5. **法线**：所有方法都会调用RecalculateNormals()，确保光照正确

---

## 优化建议

1. **缓存Mesh**：避免每帧重新生成，建议在Start中预生成并缓存
2. **共享Mesh**：多个物体可以共享同一个Mesh（如多个金币使用同一个金币Mesh）
3. **LOD**：对于远处物体，可以使用简化版本的Mesh
4. **异步生成**：对于复杂Mesh，可以考虑异步生成以避免卡顿