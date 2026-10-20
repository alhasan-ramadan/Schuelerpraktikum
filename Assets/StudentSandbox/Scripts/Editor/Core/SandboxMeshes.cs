using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudentSandbox.EditorTools
{
    /// <summary>
    /// Erzeugt einfache Low-Poly-Meshes (Kegel, Dächer, Rampen, Kristalle …) als Mesh-Assets.
    /// Alle Flächen sind "flach schattiert" (jedes Dreieck hat eigene Eckpunkte),
    /// dadurch entsteht der typische facettierte Low-Poly-Look.
    /// </summary>
    public static class SandboxMeshes
    {
        static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        public static void CreateAll()
        {
            Cache.Clear();
            Save(MK.Sphere, Icosphere(1, 0f, 1));
            Save(MK.BlobA, Icosphere(1, 0.09f, 11));
            Save(MK.BlobB, Icosphere(1, 0.11f, 23));
            Save(MK.BlobC, Icosphere(1, 0.1f, 37));
            Save(MK.Rock, Rock());
            Save(MK.Crystal, Crystal());
            Save(MK.Star, Star());
            Save(MK.Torus, Torus(14, 6, 0.4f, 0.1f));
            Save(MK.ArrowBoard, ArrowBoard());
            Save(MK.Cone6, Cone(6));
            Save(MK.Cone8, Cone(8));
            Save(MK.Pyramid, Pyramid());
            Save(MK.Prism, Prism());
            Save(MK.Wedge, Wedge());
            Save(MK.Cylinder6, Cylinder(6));
            Save(MK.Cylinder8, Cylinder(8));
            Save(MK.Cylinder12, Cylinder(12));
            AssetDatabase.SaveAssets();
        }

        public static Mesh Get(string key)
        {
            if (Cache.TryGetValue(key, out Mesh cached) && cached != null) return cached;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathFor(key));
            if (mesh == null) throw new ArgumentException("[Student Sandbox] Unbekanntes Mesh: " + key + " (Konstante in MK verwenden und CreateAll() vorher aufrufen)");
            Cache[key] = mesh;
            return mesh;
        }

        static string PathFor(string key) => SandboxPaths.Meshes + "/" + key + ".asset";

        static void Save(string key, Mesh mesh)
        {
            mesh.name = key;
            string path = PathFor(key);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                Cache[key] = mesh;
            }
            else
            {
                // Inhalt ersetzen, GUID behalten.
                EditorUtility.CopySerialized(mesh, existing);
                existing.name = key;
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                Cache[key] = existing;
            }
        }

        // ---------- Hilfsfunktionen zum Bauen flach schattierter Meshes ----------

        sealed class FlatBuilder
        {
            readonly List<Vector3> vertices = new List<Vector3>();
            readonly List<Vector3> normals = new List<Vector3>();
            readonly List<Vector2> uvs = new List<Vector2>();
            readonly List<int> triangles = new List<int>();

            /// <summary>Dreieck im Uhrzeigersinn (von außen gesehen) – so erwartet es Unity.</summary>
            public void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                if (normal.sqrMagnitude < 1e-12f) return;
                normal.Normalize();
                int index = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0.5f, 1f));
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
            }

            /// <summary>Viereck a-b-c-d im Uhrzeigersinn.</summary>
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                Tri(a, b, c);
                Tri(a, c, d);
            }

            /// <summary>Konvexes Vieleck (Fächer), Punkte im Uhrzeigersinn von außen gesehen.</summary>
            public void Fan(IList<Vector3> points)
            {
                for (int i = 1; i < points.Count - 1; i++) Tri(points[0], points[i], points[i + 1]);
            }

            public Mesh Build()
            {
                var mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }
        }

        /// <summary>
        /// Stellt sicher, dass ein Dreieck nach außen zeigt (weg vom Mittelpunkt des Körpers).
        /// Praktisch für Körper, die um einen bekannten Mittelpunkt liegen.
        /// </summary>
        static void OutwardTri(FlatBuilder builder, Vector3 a, Vector3 b, Vector3 c, Vector3 center)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 mid = (a + b + c) / 3f;
            if (Vector3.Dot(normal, mid - center) < 0f) builder.Tri(a, c, b);
            else builder.Tri(a, b, c);
        }

        // ---------- Formen ----------

        static Mesh Icosphere(int subdivisions, float jitter, int seed)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            var points = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            for (int i = 0; i < points.Count; i++) points[i] = points[i].normalized;

            var faces = new List<int[]>
            {
                new[] {0, 11, 5}, new[] {0, 5, 1}, new[] {0, 1, 7}, new[] {0, 7, 10}, new[] {0, 10, 11},
                new[] {1, 5, 9}, new[] {5, 11, 4}, new[] {11, 10, 2}, new[] {10, 7, 6}, new[] {7, 1, 8},
                new[] {3, 9, 4}, new[] {3, 4, 2}, new[] {3, 2, 6}, new[] {3, 6, 8}, new[] {3, 8, 9},
                new[] {4, 9, 5}, new[] {2, 4, 11}, new[] {6, 2, 10}, new[] {8, 6, 7}, new[] {9, 8, 1},
            };

            var midpointCache = new Dictionary<long, int>();
            for (int s = 0; s < subdivisions; s++)
            {
                var next = new List<int[]>();
                foreach (int[] f in faces)
                {
                    int a = Midpoint(points, midpointCache, f[0], f[1]);
                    int b = Midpoint(points, midpointCache, f[1], f[2]);
                    int c = Midpoint(points, midpointCache, f[2], f[0]);
                    next.Add(new[] { f[0], a, c });
                    next.Add(new[] { f[1], b, a });
                    next.Add(new[] { f[2], c, b });
                    next.Add(new[] { a, b, c });
                }
                faces = next;
                midpointCache.Clear();
            }

            var random = new System.Random(seed);
            for (int i = 0; i < points.Count; i++)
            {
                float scale = 1f + (jitter > 0f ? ((float)random.NextDouble() * 2f - 1f) * jitter : 0f);
                points[i] = points[i] * 0.5f * scale;
            }

            var builder = new FlatBuilder();
            foreach (int[] f in faces) OutwardTri(builder, points[f[0]], points[f[1]], points[f[2]], Vector3.zero);
            return builder.Build();
        }

        static int Midpoint(List<Vector3> points, Dictionary<long, int> cache, int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (cache.TryGetValue(key, out int index)) return index;
            points.Add(((points[a] + points[b]) * 0.5f).normalized);
            index = points.Count - 1;
            cache[key] = index;
            return index;
        }

        static Mesh Rock()
        {
            // Ikosaeder ohne Unterteilung, unregelmäßig verzerrt und flach gedrückt.
            Mesh source = Icosphere(0, 0.28f, 7);
            Vector3[] vertices = source.vertices;
            var builder = new FlatBuilder();
            int[] triangles = source.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = Squash(vertices[triangles[i]]);
                Vector3 b = Squash(vertices[triangles[i + 1]]);
                Vector3 c = Squash(vertices[triangles[i + 2]]);
                OutwardTri(builder, a, b, c, Vector3.zero);
            }
            UnityEngine.Object.DestroyImmediate(source);
            return builder.Build();
        }

        static Vector3 Squash(Vector3 v) => new Vector3(v.x * 1.1f, v.y * 0.7f, v.z);

        static Mesh Crystal()
        {
            const int sides = 6;
            var builder = new FlatBuilder();
            Vector3 top = new Vector3(0f, 0.8f, 0f);
            Vector3 bottom = new Vector3(0f, -0.8f, 0f);
            for (int i = 0; i < sides; i++)
            {
                Vector3 a = Ring(i, sides, 0.5f, 0f);
                Vector3 b = Ring(i + 1, sides, 0.5f, 0f);
                OutwardTri(builder, top, a, b, Vector3.zero);
                OutwardTri(builder, bottom, b, a, Vector3.zero);
            }
            return builder.Build();
        }

        static Mesh Star()
        {
            const int points = 5;
            var outline = new List<Vector3>();
            for (int i = 0; i < points * 2; i++)
            {
                float radius = i % 2 == 0 ? 0.5f : 0.22f;
                float angle = Mathf.PI / 2f + i * Mathf.PI / points;
                outline.Add(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
            var builder = new FlatBuilder();
            Vector3 front = new Vector3(0f, 0f, -0.5f);
            Vector3 back = new Vector3(0f, 0f, 0.5f);
            for (int i = 0; i < outline.Count; i++)
            {
                Vector3 a = outline[i];
                Vector3 b = outline[(i + 1) % outline.Count];
                // Vorder- und Rückseite als Dreiecke zum Mittelpunkt.
                builder.Tri(front, a + front, b + front);
                builder.Tri(back, b + back, a + back);
                // Seitenwand.
                builder.Quad(a + front, a + back, b + back, b + front);
            }
            return FixWinding(builder.Build());
        }

        static Mesh Torus(int segments, int tubeSegments, float radius, float tubeRadius)
        {
            var builder = new FlatBuilder();
            for (int i = 0; i < segments; i++)
            {
                for (int j = 0; j < tubeSegments; j++)
                {
                    Vector3 a = TorusPoint(i, j, segments, tubeSegments, radius, tubeRadius);
                    Vector3 b = TorusPoint(i + 1, j, segments, tubeSegments, radius, tubeRadius);
                    Vector3 c = TorusPoint(i + 1, j + 1, segments, tubeSegments, radius, tubeRadius);
                    Vector3 d = TorusPoint(i, j + 1, segments, tubeSegments, radius, tubeRadius);
                    Vector3 ringCenter = TorusPoint(i, 0, segments, 1, radius, 0f);
                    Vector3 ringCenter2 = TorusPoint(i + 1, 0, segments, 1, radius, 0f);
                    Vector3 center = (ringCenter + ringCenter2) * 0.5f;
                    OutwardTri(builder, a, b, c, center);
                    OutwardTri(builder, a, c, d, center);
                }
            }
            return builder.Build();
        }

        static Vector3 TorusPoint(int i, int j, int segments, int tubeSegments, float radius, float tubeRadius)
        {
            float u = (float)i / segments * Mathf.PI * 2f;
            float v = (float)j / tubeSegments * Mathf.PI * 2f;
            float r = radius + tubeRadius * Mathf.Cos(v);
            return new Vector3(Mathf.Cos(u) * r, tubeRadius * Mathf.Sin(v), Mathf.Sin(u) * r);
        }

        static Mesh ArrowBoard()
        {
            // Pfeil zeigt nach +x. Umriss in der XY-Ebene, Dicke entlang z.
            var outline = new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.22f, -0.5f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(0.22f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };
            return Extrude(outline, 1f);
        }

        /// <summary>Extrudiert einen konvexen Umriss (XY-Ebene) entlang z (von -depth/2 bis +depth/2).</summary>
        static Mesh Extrude(List<Vector3> outline, float depth)
        {
            var builder = new FlatBuilder();
            Vector3 front = new Vector3(0f, 0f, -depth * 0.5f);
            Vector3 back = new Vector3(0f, 0f, depth * 0.5f);
            var frontFace = new List<Vector3>();
            var backFace = new List<Vector3>();
            foreach (Vector3 p in outline)
            {
                frontFace.Add(p + front);
                backFace.Add(p + back);
            }
            builder.Fan(frontFace);
            backFace.Reverse();
            builder.Fan(backFace);
            for (int i = 0; i < outline.Count; i++)
            {
                Vector3 a = outline[i];
                Vector3 b = outline[(i + 1) % outline.Count];
                builder.Quad(a + front, a + back, b + back, b + front);
            }
            return FixWinding(builder.Build());
        }

        static Mesh Cone(int sides)
        {
            var builder = new FlatBuilder();
            Vector3 apex = new Vector3(0f, 1f, 0f);
            Vector3 baseCenter = Vector3.zero;
            Vector3 center = new Vector3(0f, 0.3f, 0f);
            for (int i = 0; i < sides; i++)
            {
                Vector3 a = Ring(i, sides, 0.5f, 0f);
                Vector3 b = Ring(i + 1, sides, 0.5f, 0f);
                OutwardTri(builder, apex, a, b, center);
                OutwardTri(builder, baseCenter, a, b, center);
            }
            return builder.Build();
        }

        static Mesh Pyramid()
        {
            var builder = new FlatBuilder();
            Vector3 apex = new Vector3(0f, 1f, 0f);
            Vector3 center = new Vector3(0f, 0.3f, 0f);
            Vector3[] corners =
            {
                new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f), new Vector3(-0.5f, 0f, 0.5f),
            };
            for (int i = 0; i < 4; i++) OutwardTri(builder, apex, corners[i], corners[(i + 1) % 4], center);
            OutwardTri(builder, corners[0], corners[1], corners[2], center);
            OutwardTri(builder, corners[0], corners[2], corners[3], center);
            return builder.Build();
        }

        static Mesh Prism()
        {
            // Satteldach: Giebel-Dreieck in der XY-Ebene, First bei y = 1, Länge entlang z.
            var builder = new FlatBuilder();
            Vector3 center = new Vector3(0f, 0.33f, 0f);
            Vector3 l0 = new Vector3(-0.5f, 0f, -0.5f), r0 = new Vector3(0.5f, 0f, -0.5f), t0 = new Vector3(0f, 1f, -0.5f);
            Vector3 l1 = new Vector3(-0.5f, 0f, 0.5f), r1 = new Vector3(0.5f, 0f, 0.5f), t1 = new Vector3(0f, 1f, 0.5f);
            OutwardTri(builder, l0, r0, t0, center);
            OutwardTri(builder, l1, r1, t1, center);
            OutwardTri(builder, l0, t0, t1, center);
            OutwardTri(builder, l0, t1, l1, center);
            OutwardTri(builder, r0, r1, t1, center);
            OutwardTri(builder, r0, t1, t0, center);
            OutwardTri(builder, l0, l1, r1, center);
            OutwardTri(builder, l0, r1, r0, center);
            return builder.Build();
        }

        static Mesh Wedge()
        {
            // Rampe: Boden 1 x 1 bei y = 0, hohe Kante (y = 1) bei x = -0.5, Schräge fällt nach +x ab.
            var builder = new FlatBuilder();
            Vector3 center = new Vector3(-0.17f, 0.33f, 0f);
            Vector3 bl0 = new Vector3(-0.5f, 0f, -0.5f), br0 = new Vector3(0.5f, 0f, -0.5f), tl0 = new Vector3(-0.5f, 1f, -0.5f);
            Vector3 bl1 = new Vector3(-0.5f, 0f, 0.5f), br1 = new Vector3(0.5f, 0f, 0.5f), tl1 = new Vector3(-0.5f, 1f, 0.5f);
            OutwardTri(builder, bl0, br0, tl0, center);          // Seite vorne
            OutwardTri(builder, bl1, br1, tl1, center);          // Seite hinten
            OutwardTri(builder, bl0, tl0, tl1, center);          // Rückwand
            OutwardTri(builder, bl0, tl1, bl1, center);
            OutwardTri(builder, tl0, br0, br1, center);          // Schräge
            OutwardTri(builder, tl0, br1, tl1, center);
            OutwardTri(builder, bl0, bl1, br1, center);          // Boden
            OutwardTri(builder, bl0, br1, br0, center);
            return builder.Build();
        }

        static Mesh Cylinder(int sides)
        {
            var builder = new FlatBuilder();
            Vector3 center = new Vector3(0f, 0.5f, 0f);
            Vector3 topCenter = new Vector3(0f, 1f, 0f);
            for (int i = 0; i < sides; i++)
            {
                Vector3 a0 = Ring(i, sides, 0.5f, 0f);
                Vector3 b0 = Ring(i + 1, sides, 0.5f, 0f);
                Vector3 a1 = Ring(i, sides, 0.5f, 1f);
                Vector3 b1 = Ring(i + 1, sides, 0.5f, 1f);
                OutwardTri(builder, a0, b0, b1, center);
                OutwardTri(builder, a0, b1, a1, center);
                OutwardTri(builder, topCenter, a1, b1, center);
                OutwardTri(builder, Vector3.zero, a0, b0, center);
            }
            return builder.Build();
        }

        static Vector3 Ring(int index, int sides, float radius, float y)
        {
            // Erste Ecke so gedreht, dass eine flache Seite nach vorne (-z) zeigt.
            float angle = (index + 0.5f) / sides * Mathf.PI * 2f;
            return new Vector3(Mathf.Sin(angle) * radius, y, -Mathf.Cos(angle) * radius);
        }

        /// <summary>Dreht Dreiecke um, deren Normale zum Mittelpunkt des Meshes zeigt (für konvexe Extrusionen).</summary>
        static Mesh FixWinding(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3 center = mesh.bounds.center;
            var builder = new FlatBuilder();
            for (int i = 0; i < triangles.Length; i += 3)
                OutwardTri(builder, vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]], center);
            UnityEngine.Object.DestroyImmediate(mesh);
            return builder.Build();
        }
    }
}
