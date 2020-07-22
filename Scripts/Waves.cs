using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public int dimensions = 10;
    public float UVScale;
    public Octave[] Octaves;


    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }

    public void Start()
    {
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTris();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
    }

    void Update()
    {
        var verts = Mesh.vertices;
        for (int i = 0; i <= dimensions; ++i)
        {
            for (int j = 0; j <= dimensions; ++j)
            {
                float y = 0f;
                for (int x = 0; x < Octaves.Length; ++x)
                {
                    if (Octaves[x].alternate)
                    {
                        Double perl = Mathf.PerlinNoise((i * Octaves[x].scale.x) / dimensions, (j * Octaves[x].scale.y) / dimensions) * Math.PI * 2f;
                        y += Mathf.Cos((float)perl + Octaves[x].speed.magnitude * Time.time) * Octaves[x].height;
                    }
                    else
                    {
                        Single perl = Mathf.PerlinNoise((x * Octaves[x].scale.x + Time.time * Octaves[x].speed.x) / dimensions, (j * Octaves[x].scale.y + Time.time * Octaves[x].speed.y) / dimensions) - 0.5f;
                        y += perl * Octaves[x].height;
                    } 
                }
                verts[Index(i, j)] = new Vector3(i, y, j);
            }
        }
        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }

    private Vector3[] GenerateVerts()
    {
        Vector3[] verts = new Vector3[(dimensions + 1) * (dimensions + 1)];
        for (int i = 0; i <= dimensions; ++i)
        {
            for (int j = 0; j <= dimensions; ++j)
            {
                verts[Index(i, j)] = new Vector3(i, 0, j);
            }
        }

        return verts;
    }

    private int[] GenerateTris()
    {
        int[] tris = new int[Mesh.vertices.Length * 6];

        for (int i = 0; i < dimensions; ++i)
        {
            for (int j = 0; j < dimensions; ++j)
            {
                tris[Index(i, j) * 6 + 0] = Index(i, j);
                tris[Index(i, j) * 6 + 1] = Index(i + 1, j + 1);
                tris[Index(i, j) * 6 + 2] = Index(i + 1, j);
                tris[Index(i, j) * 6 + 3] = Index(i, j);
                tris[Index(i, j) * 6 + 4] = Index(i, j + 1);
                tris[Index(i, j) * 6 + 5] = Index(i + 1, j + 1);
            }
        }
        return tris;
    }

    private Vector2[] GenerateUVs()
    {
        Vector2[] uvs = new Vector2[Mesh.vertices.Length];
        for(int i = 0; i <= dimensions; ++i)
        {
            for(int j = 0; j <= dimensions; ++j)
            {
                Vector2 vec = new Vector2((i / UVScale) % 2, (j / UVScale % 2));
                uvs[Index(i, j)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }
        return uvs;
    }

    private int Index(int i, int j)
    {
        return i * (dimensions + 1) + j;
    }


    public float GetHeight(Vector3 position)
    {
        //scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if the position is outside the plane
        p1.x = Mathf.Clamp(p1.x, 0, dimensions);
        p1.z = Mathf.Clamp(p1.z, 0, dimensions);
        p2.x = Mathf.Clamp(p2.x, 0, dimensions);
        p2.z = Mathf.Clamp(p2.z, 0, dimensions);
        p3.x = Mathf.Clamp(p3.x, 0, dimensions);
        p3.z = Mathf.Clamp(p3.z, 0, dimensions);
        p4.x = Mathf.Clamp(p4.x, 0, dimensions);
        p4.z = Mathf.Clamp(p4.z, 0, dimensions);

        //get the max distance to one of the edges and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        //weighted sum
        var height = Mesh.vertices[Index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[Index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[Index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[Index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * transform.lossyScale.y / dist;

    }

}
