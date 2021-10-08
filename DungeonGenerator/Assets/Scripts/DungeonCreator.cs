using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonWidth, dungeonLength, roomWidthMin, roomLengthMin, maxIterations, corridorWidth;
    public Material roomMat, startRoomMat, endRoomMat, wallMat;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;
    public GameObject player, wallVertical, wallHorizontal;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;

    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
        List<Node> listOfRooms = generator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, roomBottomCornerModifier,
            roomTopCornerMidifier, roomOffset, corridorWidth);

        GameObject wallParent = new GameObject("WallParent");
        wallParent.transform.parent = transform;
        wallParent.AddComponent<MeshFilter>();
        wallParent.AddComponent<MeshRenderer>();
        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            if (i == 0)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, startRoomMat, true);
            else if (i == listOfRooms.Count / 2)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, endRoomMat);
            else
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, roomMat);
        }

        CreateWalls(wallParent);

        // combine wall meshes
        CombineWallMeshes(wallParent);
    }

    private void CombineWallMeshes(GameObject wallCollection)
    {
        Vector3 position = wallCollection.transform.position;
        wallCollection.transform.position = Vector3.zero;

        MeshFilter[] meshFilters = wallCollection.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 1; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        wallCollection.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        wallCollection.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        wallCollection.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        wallCollection.GetComponent<Renderer>().material = wallMat;
        wallCollection.transform.gameObject.SetActive(true);
        wallCollection.gameObject.isStatic = true;
        wallCollection.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        wallCollection.transform.position = position;
        //wallCollection.AddComponent<MeshCollider>();
    }

    private void CreateWalls(GameObject wallParent)
    {
        foreach (Vector3Int wallPosition in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
        foreach (Vector3Int wallPosition in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPosition, wallVertical);
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
    }

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, Material myMat, bool spawnRoom = false)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("Floor" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
        GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = myMat;
        dungeonFloor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dungeonFloor.transform.parent = transform;
        dungeonFloor.AddComponent<BoxCollider>();
        dungeonFloor.layer = 3;
        dungeonFloor.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        dungeonCeiling.transform.position = Vector3.zero;
        dungeonCeiling.transform.localScale = Vector3.one;
        dungeonCeiling.GetComponent<MeshFilter>().mesh = mesh;
        dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles = dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        dungeonCeiling.GetComponent<MeshRenderer>().material = roomMat;
        dungeonCeiling.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dungeonCeiling.transform.parent = transform;
        dungeonCeiling.transform.Translate(0f, 4f, 0f);
        dungeonCeiling.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        if (spawnRoom)
            Instantiate(player, new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, 0f,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2), Quaternion.identity);
        /*else if (spawnRoom && GameObject.FindGameObjectWithTag("Player") != null)
            GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, 0f,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2);*/

        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            Vector3 wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
        {
            Vector3 wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            Vector3 wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            Vector3 wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
    }

    private void AddWallPositionToList(Vector3 wallStart, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallStart);
        if (wallList.Contains(point))
        {
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }

    private void DestroyAllChildren()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
            DestroyImmediate(GameObject.FindGameObjectWithTag("Player").transform.gameObject);

        while (transform.childCount != 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}