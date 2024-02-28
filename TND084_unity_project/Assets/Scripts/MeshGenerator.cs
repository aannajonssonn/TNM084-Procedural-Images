using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2; // if detail = 0 set MSI to 1, otherwise multiply by two
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnSimple = borderedSize - 2;
        float topLeftX = (meshSizeUnSimple - 1) / -2f;
        float topLeftZ = (meshSizeUnSimple - 1) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);
        int[,] vertexIndexMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderedVertexIndex = -1;

        int centerVertex = 0;
        int rightVertex = 0;
        int leftVertex = 0;
        int topVertex = 0;
        int bottomVertex = 0;

        //int centerVertex, rightVertex, leftVertex, topVertex, bottomVertex;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndexMap[x, y] = borderedVertexIndex;
                    borderedVertexIndex--;
                }
                else
                {
                    vertexIndexMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }

            }
        }

        // loop through heightmap aaa
        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndexMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPos = new Vector3(topLeftX - percent.x * meshSizeUnSimple, height, topLeftZ - percent.y * meshSizeUnSimple); // VÄNDE PÅ ALLT HEHE

                meshData.AddVertex(vertexPos, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1) // ignore the right, and bottom edge, vertices of the map.
                {
                    // to create triangles
                    int a = vertexIndexMap[x, y];
                    int b = vertexIndexMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndexMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndexMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    meshData.AddTriangle(a, c, d); //ÖHHH
                    meshData.AddTriangle(d, b, a); //OMGEALUL

                    if (vertexIndex >= 0) // do not include border verticies as they're not part of the mesh
                    {
                        centerVertex = vertexIndex;

                        rightVertex = vertexIndexMap[x + meshSimplificationIncrement, y];
                        bottomVertex = vertexIndexMap[x, y - meshSimplificationIncrement];

                        if ((x-1) == 0)
                        {
                            leftVertex = centerVertex;
                        }
                        else
                        {
                            leftVertex = vertexIndexMap[x - meshSimplificationIncrement, y];
                        }

                        if ((y-1) == 0)
                        {
                            topVertex = centerVertex;
                        }
                        else
                        {
                            topVertex = vertexIndexMap[x, y + meshSimplificationIncrement];
                        }

                        meshData.VertexNormalCrossMethod(centerVertex, leftVertex, rightVertex, topVertex, bottomVertex);
                    }
                }

                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] borderVertex;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    Vector3[] vertexNormals;

    Vector3 left;
    Vector3 right;
    Vector3 top;
    Vector3 bottom;

    // constructor
    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertex = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];

        vertexNormals = new Vector3[vertices.Length];
    }

    public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex)
    {
        // if border vertex
        if (vertexIndex < 0)
        {
            borderVertex[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            vertices[vertexIndex] = vertexPos;
            uvs[vertexIndex] = uv;
        }

    }

    // Cross Method to calculate the Normals
    /*
                    o (top)
                    ^
                    |
                    |
     (left) o < - - o - - > o (right)
                    |
                    |
                    v
                    o (bottom)

     */

    public void VertexNormalCrossMethod(int centerVertex, int leftVertex, int rightVertex, int topVertex, int bottomVertex)
    {
        // Get the neighbouring vertices
        if (leftVertex < 0)
        {
            left = borderVertex[-leftVertex - 1];
        }
        else
        {
            left = vertices[leftVertex];
        }

        if (rightVertex < 0)
        {
            right = borderVertex[-rightVertex - 1];
        }
        else
        {
            right = vertices[rightVertex];
        }

        if (topVertex < 0)
        {
            top = borderVertex[-topVertex - 1];
        }
        else
        {
            top = vertices[topVertex];
        }

        if (bottomVertex < 0)
        {
            bottom = borderVertex[-bottomVertex - 1];
        }
        else
        {
            bottom = vertices[bottomVertex];
        }

        // Calculate the vector between the verticies
        Vector3 sideAB = right - left; // X-axis
        Vector3 sideCD = bottom - top; // Y-axis (or Z if you look in Unity)

        Vector3 n = Vector3.Cross(sideAB, sideCD);
        n.Normalize();
        vertexNormals[centerVertex] = n *-1;

    }

    // add triangles
    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;

            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }

    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexTriangleNormals = new Vector3[vertices.Length];
        // regular triangles
        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndex(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexTriangleNormals[vertexIndexA] += triangleNormal;
            vertexTriangleNormals[vertexIndexB] += triangleNormal;
            vertexTriangleNormals[vertexIndexC] += triangleNormal;

        }

        // border triangles
        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndex(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexTriangleNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexTriangleNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexTriangleNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexTriangleNormals.Length; i++)
        {
            vertexTriangleNormals[i].Normalize();
        }

        return vertexTriangleNormals;
    }


    Vector3 SurfaceNormalFromIndex(int indexA, int indexB, int indexC)
    {

        Vector3 pointA = (indexA < 0) ? borderVertex[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertex[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertex[-indexC - 1] : vertices[indexC];

        // cross product to calculate the surface normals
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointA - pointC;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    // print normals
    public void printNormals(Vector3[] normals)
    {
        /*for (int i = 0; i < vertices.Length; i += 500)
        {*/
        Debug.Log("Vertex: " + vertexNormals[50] + "     uni: " + normals[50]);
        Debug.Log("Vertex: " + vertexNormals[3000] + "     uni: " + normals[3000]);
        //}
    }


    // getting the mesh from the meshdata
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        //mesh.normals = CalculateNormals(); // triangles
        //mesh.normals = normalizedVertexNormals(); // cross method
        mesh.normals = vertexNormals;

        //Debug.Log("Vertex: " + vertices.Length + "   normals:" + vertexNormals.Length );
        printNormals(mesh.normals);
        return mesh;
    }
}
