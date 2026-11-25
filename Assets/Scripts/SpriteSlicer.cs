
using UnityEngine;
using System.Collections.Generic;

public static class SpriteSlicer
{
    public static bool Slice(
        GameObject target,
        Vector2 worldStart,
        Vector2 worldEnd)
    {
        SpriteRenderer sr =
            target.GetComponent<SpriteRenderer>();

        BoxCollider2D box =
            target.GetComponent<BoxCollider2D>();

        if (sr == null || sr.sprite == null || box == null)
            return false;

        if (!target.CompareTag("breakable"))
            return false;

        if ((worldEnd - worldStart).sqrMagnitude < 0.000001f)
            return false;

        Sprite sprite = sr.sprite;
        Bounds bounds = sprite.bounds;

        float z = target.transform.position.z;

        Vector2 start = target.transform.InverseTransformPoint(
            new Vector3(worldStart.x, worldStart.y, z));

        Vector2 end = target.transform.InverseTransformPoint(
            new Vector3(worldEnd.x, worldEnd.y, z));

        // Visual boundaries
        List<Vector2> visualPolygon = CreateRectangle(
            bounds.min,
            bounds.max
        );

        // Physical boundaries from the fitted collider
        Vector2 halfSize = box.size / 2f;

        List<Vector2> collisionPolygon = CreateRectangle(
            box.offset - halfSize,
            box.offset + halfSize
        );

        // Slice visual geometry
        List<Vector2> visualA =
            ClipPolygon(visualPolygon, start, end, true);

        List<Vector2> visualB =
            ClipPolygon(visualPolygon, start, end, false);

        // Slice collision geometry separately
        List<Vector2> collisionA =
            ClipPolygon(collisionPolygon, start, end, true);

        List<Vector2> collisionB =
            ClipPolygon(collisionPolygon, start, end, false);

        if (!ValidPolygon(visualA) ||
            !ValidPolygon(visualB) ||
            !ValidPolygon(collisionA) ||
            !ValidPolygon(collisionB))
        {
            return false;
        }

        CreatePiece(
            target, sr, visualA, collisionA, "PieceA");

        CreatePiece(
            target, sr, visualB, collisionB, "PieceB");

        target.SetActive(false);
        Object.Destroy(target);

        return true;
    }

    static List<Vector2> CreateRectangle(
        Vector2 min, Vector2 max)
    {
        return new List<Vector2>
        {
            new Vector2(min.x, min.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y),
            new Vector2(min.x, max.y)
        };
    }

    static bool ValidPolygon(List<Vector2> polygon)
    {
        return polygon.Count >= 3 &&
               Area(polygon) > 0.00001f;
    }

    static float Side(
        Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        Vector2 relative = point - start;

        return direction.x * relative.y
             - direction.y * relative.x;
    }

    static List<Vector2> ClipPolygon(
        List<Vector2> polygon,
        Vector2 start,
        Vector2 end,
        bool positive)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];

            float a = Side(current, start, end);
            float b = Side(next, start, end);

            bool currentInside = positive ? a >= 0 : a <= 0;
            bool nextInside = positive ? b >= 0 : b <= 0;

            if (currentInside)
                AddUnique(result, current);

            if (currentInside != nextInside)
            {
                float t = a / (a - b);

                Vector2 intersection =
                    Vector2.Lerp(current, next, t);

                AddUnique(result, intersection);
            }
        }

        if (result.Count > 1 &&
            (result[0] - result[result.Count - 1])
            .sqrMagnitude < 0.00000001f)
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    static void AddUnique(
        List<Vector2> points, Vector2 point)
    {
        if (points.Count == 0 ||
            (points[points.Count - 1] - point)
            .sqrMagnitude > 0.00000001f)
        {
            points.Add(point);
        }
    }

    static float Area(List<Vector2> polygon)
    {
        float area = 0;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            area += a.x * b.y - b.x * a.y;
        }

        return Mathf.Abs(area) * 0.5f;
    }

    // Find the center of each individual fragment
    static Vector2 GetCenter(List<Vector2> polygon)
    {
        Vector2 center = Vector2.zero;
        float total = 0f;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            float cross = a.x * b.y - b.x * a.y;

            center += (a + b) * cross;
            total += cross;
        }

        if (Mathf.Abs(total) < 0.000001f)
            return polygon[0];

        return center / (3f * total);
    }

    // Preserve the original texture mapping
    static Vector2 GetSpriteUV(
        Sprite sprite, Vector2 point)
    {
        Vector2[] vertices = sprite.vertices;
        Vector2[] uv = sprite.uv;
        ushort[] triangles = sprite.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int ia = triangles[i];
            int ib = triangles[i + 1];
            int ic = triangles[i + 2];

            Vector2 a = vertices[ia];
            Vector2 b = vertices[ib];
            Vector2 c = vertices[ic];

            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 ap = point - a;

            float denominator =
                ab.x * ac.y - ab.y * ac.x;

            if (Mathf.Abs(denominator) < 0.000001f)
                continue;

            float v =
                (ap.x * ac.y - ap.y * ac.x)
                / denominator;

            float w =
                (ab.x * ap.y - ab.y * ap.x)
                / denominator;

            float u = 1f - v - w;

            return uv[ia] * u
                 + uv[ib] * v
                 + uv[ic] * w;
        }

        return Vector2.zero;
    }

    static void CreatePiece(
        GameObject original,
        SpriteRenderer sr,
        List<Vector2> visualPolygon,
        List<Vector2> collisionPolygon,
        string pieceName)
    {
        Vector2 center = GetCenter(collisionPolygon);

        GameObject piece = new GameObject(
            original.name + "_" + pieceName);

        piece.transform.SetParent(
            original.transform.parent, false);

        piece.transform.localRotation =
            original.transform.localRotation;

        piece.transform.localScale =
            original.transform.localScale;

        // Give the fragment its own centered pivot
        piece.transform.position =
            original.transform.TransformPoint(center);

        piece.layer = original.layer;

        MeshFilter filter =
            piece.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            piece.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        Vector3[] vertices =
            new Vector3[visualPolygon.Count];

        Vector2[] uv =
            new Vector2[visualPolygon.Count];

        Color[] colors =
            new Color[visualPolygon.Count];

        Sprite sprite = sr.sprite;

        for (int i = 0; i < visualPolygon.Count; i++)
        {
            // Recenter geometry without resizing it
            vertices[i] = visualPolygon[i] - center;

            uv[i] = GetSpriteUV(
                sprite, visualPolygon[i]);

            colors[i] = sr.color;
        }

        List<int> triangles = new List<int>();

        for (int i = 1; i < visualPolygon.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i);
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;

        renderer.sharedMaterial = sr.sharedMaterial;

        MaterialPropertyBlock properties =
            new MaterialPropertyBlock();

        properties.SetTexture("_MainTex", sprite.texture);
        properties.SetTexture("_BaseMap", sprite.texture);

        renderer.SetPropertyBlock(properties);

        renderer.sortingLayerID = sr.sortingLayerID;
        renderer.sortingOrder = sr.sortingOrder;

        // Recenter the collision geometry too
        List<Vector2> centeredCollider =
            new List<Vector2>();

        foreach (Vector2 point in collisionPolygon)
        {
            centeredCollider.Add(point - center);
        }

        // Generate collider from fitted physical boundaries
        MeshCollider collider =
            piece.AddComponent<MeshCollider>();

        collider.sharedMesh =
            CreateColliderMesh(centeredCollider, 0.1f);

        collider.convex = true;

        Rigidbody rb = piece.AddComponent<Rigidbody>();

        rb.useGravity = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;
    }

    static Mesh CreateColliderMesh(
        List<Vector2> polygon,
        float thickness)
    {
        Mesh mesh = new Mesh();

        int count = polygon.Count;
        float halfDepth = thickness / 2f;

        Vector3[] vertices = new Vector3[count * 2];
        List<int> triangles = new List<int>();

        for (int i = 0; i < count; i++)
        {
            vertices[i] = new Vector3(
                polygon[i].x,
                polygon[i].y,
                -halfDepth);

            vertices[i + count] = new Vector3(
                polygon[i].x,
                polygon[i].y,
                halfDepth);
        }

        // Front and back faces
        for (int i = 1; i < count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i);

            triangles.Add(count);
            triangles.Add(count + i);
            triangles.Add(count + i + 1);
        }

        // Side faces
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            triangles.Add(i);
            triangles.Add(next);
            triangles.Add(next + count);

            triangles.Add(i);
            triangles.Add(next + count);
            triangles.Add(i + count);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
