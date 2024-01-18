using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2; // if detail = 0 set MSI to 1, otherwise multiply by two
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2*meshSimplificationIncrement;
        int meshSizeUnSimple = borderedSize - 2;
        float topLeftX = (meshSizeUnSimple - 1) / -2f;
        float topLeftZ = (meshSizeUnSimple - 1) / 2f;

       int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);
        //int vertexIndex = 0;
        int[,] vertexIndexMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderedVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize -1 || x == 0 || x == borderedSize -1;

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

        // loop through heightmap
        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for(int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndexMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPos = new Vector3(topLeftX + percent.x* meshSizeUnSimple, height, topLeftZ - percent.y* meshSizeUnSimple);
                
                meshData.AddVertex(vertexPos, percent, vertexIndex);

                if (x < borderedSize-1 && y < borderedSize -1) // ignore the right, and bottom edge, vertices of the map.
                {
                    int a = vertexIndexMap[x, y];
                    int b = vertexIndexMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndexMap[x, y+meshSimplificationIncrement];
                    int d = vertexIndexMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b); 
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

    // constructor
    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1)*6];

        borderVertex = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertex[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            vertices[vertexIndex] = vertexPos;
            uvs[vertexIndex] = uv;
        }
    }

    // add triangles
    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
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
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        // regular triangles
        int triangleCount = triangles.Length/3;

        for(int i = 0; i < triangleCount; i++) 
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndex(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;

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
            if(vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndex(int indexA, int indexB, int indexC) 
    {

        Vector3 pointA = (indexA < 0) ? borderVertex[-indexA-1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertex[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertex[-indexC - 1] : vertices[indexC];

        // cross product to calculate the surface normals
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    // getting the mesh from the meshdata
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        mesh.normals = CalculateNormals();

        return mesh;
    }
}
