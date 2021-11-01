using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonWidth, dungeonLength, roomWidthMin, roomLengthMin, maxIterations, corridorWidth, ceilingHeight;
    public Material roomMat, startRoomMat, endRoomMat, wallMat;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;
    [Range(1, 20)]
    public int amountOfDrawRooms;
    [Range(2, 20)]
    public int torchFrequency;
    public GameObject player, wallVertical, wallHorizontal, drawAreaHorizontal, drawAreaVertical, testCube;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;
    List<Vector3> possibleTorchHorizontalPosition;
    List<Vector3> possibleTorchVerticalPosition;

    private GameObject floorParent, ceilingParent, torchParent;

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
        //wallParent.layer = 3;
        wallParent.AddComponent<MeshFilter>();
        wallParent.AddComponent<MeshRenderer>();

        floorParent = new GameObject("Floors", typeof(MeshFilter), typeof(MeshRenderer));
        floorParent.layer = 3;
        floorParent.transform.parent = transform;
        ceilingParent = new GameObject("Ceilings", typeof(MeshFilter), typeof(MeshRenderer));
        //ceilingParent.layer = 3;
        ceilingParent.transform.parent = transform;
        torchParent = new GameObject("Torches");
        torchParent.transform.parent = transform;

        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        possibleTorchHorizontalPosition = new List<Vector3>();
        possibleTorchVerticalPosition = new List<Vector3>();

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            if (i == 0)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, startRoomMat, true);
            else if (i == listOfRooms.Count / 2)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, endRoomMat);
            else if (i > listOfRooms.Count / 2 && (i % amountOfDrawRooms) == 0) // Working on corridor
            {
                if (listOfRooms[i].Direction == Direction.Horizontal) // Horizontal corridor
                {
                    CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, roomMat, false, true);
                    GameObject area = GameObject.Instantiate(drawAreaHorizontal, new Vector3(listOfRooms[i].BottomLeftAreaCorner.x + (float)(listOfRooms[i].TopRightAreaCorner.x - listOfRooms[i].BottomLeftAreaCorner.x) / 2f,
                        0, listOfRooms[i].BottomLeftAreaCorner.y + (listOfRooms[i].TopRightAreaCorner.y - listOfRooms[i].BottomLeftAreaCorner.y) / 2), Quaternion.identity);
                    area.transform.localScale = new Vector3(listOfRooms[i].TopRightAreaCorner.x - listOfRooms[i].BottomLeftAreaCorner.x, 1, 1);
                    area.transform.GetChild(0).GetComponent<Camera>().orthographicSize = 1.2f + (listOfRooms[i].TopRightAreaCorner.x - listOfRooms[i].BottomLeftAreaCorner.x) / 10f;
                    area.transform.parent = transform;
                }
                else if (listOfRooms[i].Direction == Direction.Vertical) // Vertical corridor
                {
                    CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, roomMat, false, true);
                    GameObject area = GameObject.Instantiate(drawAreaVertical, new Vector3(listOfRooms[i].BottomLeftAreaCorner.x + (listOfRooms[i].TopRightAreaCorner.x - listOfRooms[i].BottomLeftAreaCorner.x) / 2,
                        0, listOfRooms[i].BottomLeftAreaCorner.y + (float)(listOfRooms[i].TopRightAreaCorner.y - listOfRooms[i].BottomLeftAreaCorner.y) / 2f), Quaternion.identity);
                    area.transform.Rotate(0, 90, 0);
                    area.transform.localScale = new Vector3(listOfRooms[i].TopRightAreaCorner.y - listOfRooms[i].BottomLeftAreaCorner.y, 1, 1);
                    area.transform.GetChild(0).GetComponent<Camera>().orthographicSize = 1.2f + (listOfRooms[i].TopRightAreaCorner.y - listOfRooms[i].BottomLeftAreaCorner.y) / 10f;
                    area.transform.parent = transform;
                    //bottomleft is now topleft
                    //topright is now bottomright
                }
            }
            else
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, roomMat);
        }

        CreateWalls(wallParent);

        // combine floor
        CombineWallMeshes(floorParent, true);

        // combine ceiling
        CombineWallMeshes(ceilingParent, true);

        // combine wall meshes
        CombineWallMeshes(wallParent);
    }

    private void CombineWallMeshes(GameObject wallCollection, bool floor = false)
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
        if (!floor)
            wallCollection.GetComponent<Renderer>().material = wallMat;
        else
            wallCollection.GetComponent<Renderer>().material = roomMat;
        wallCollection.transform.gameObject.SetActive(true);
        //wallCollection.gameObject.isStatic = true;
        wallCollection.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        wallCollection.transform.position = position;
        if (floor)
            wallCollection.AddComponent<BoxCollider>();
        //wallCollection.AddComponent<MeshCollider>();
    }

    private void CreateWalls(GameObject wallParent)
    {
        /*possibleWallHorizontalPosition = possibleWallHorizontalPosition.Distinct().ToList();
        List<Vector3Int> horizontalDoors = possibleWallHorizontalPosition.Intersect(possibleDoorHorizontalPosition).ToList();
        possibleWallHorizontalPosition.RemoveAll(x => horizontalDoors.Contains(x));*/
        foreach (Vector3Int wallPosition in possibleWallHorizontalPosition)
        {
            if (possibleDoorHorizontalPosition.Contains(wallPosition))
                CreateWall(wallParent, wallPosition, wallHorizontal, true);
            else
                CreateWall(wallParent, wallPosition, wallHorizontal, false);
        }

        /*possibleWallVerticalPosition = possibleWallVerticalPosition.Distinct().ToList();
        List<Vector3Int> verticalDoors = possibleWallVerticalPosition.Intersect(possibleDoorVerticalPosition).ToList();
        possibleWallVerticalPosition.RemoveAll(x => verticalDoors.Contains(x));*/
        foreach (Vector3Int wallPosition in possibleWallVerticalPosition)
        {
            if (possibleDoorVerticalPosition.Contains(wallPosition))
                CreateWall(wallParent, wallPosition, wallVertical, true);
            else
                CreateWall(wallParent, wallPosition, wallVertical, false);
        }

        foreach (Vector3 torchPosition in possibleTorchHorizontalPosition)
        {
            CreateTorch(torchPosition, testCube);
        }
        foreach (Vector3 torchPosition in possibleTorchVerticalPosition)
        {
            CreateTorch(torchPosition, testCube);
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab, bool flip)
    {
        if (flip)
            wallPosition.y = 0;

        GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);

        if (flip)
            wall.transform.GetChild(0).GetComponent<MeshFilter>().mesh.triangles = wall.transform.GetChild(0).GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
    }

    private void CreateTorch(Vector3 torchPosition, GameObject torchPrefab)
    {
        Instantiate(torchPrefab, torchPosition, Quaternion.identity, torchParent.transform);
    }

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, Material myMat, bool spawnRoom = false, bool removeCorridor = false)
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

        GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
        dungeonCeiling.transform.parent = transform;

        if (!removeCorridor)
        {
            GameObject dungeonFloor = new GameObject("Floor" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
            dungeonFloor.transform.position = Vector3.zero;
            dungeonFloor.transform.localScale = Vector3.one;
            dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
            dungeonFloor.GetComponent<MeshRenderer>().material = myMat;
            dungeonFloor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dungeonFloor.transform.parent = floorParent.transform;
            //dungeonFloor.AddComponent<BoxCollider>();
            dungeonFloor.layer = 3;
            dungeonFloor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }

        dungeonCeiling.transform.position = Vector3.zero;
        dungeonCeiling.transform.localScale = Vector3.one;
        dungeonCeiling.GetComponent<MeshFilter>().mesh = mesh;
        dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles = dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        dungeonCeiling.GetComponent<MeshRenderer>().material = roomMat;
        dungeonCeiling.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dungeonCeiling.transform.parent = ceilingParent.transform;
        dungeonCeiling.transform.Translate(0f, ceilingHeight, 0f);
        dungeonCeiling.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        if (spawnRoom)
            Instantiate(player, new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, 0f,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2), Quaternion.identity);
        /*else if (spawnRoom && GameObject.FindGameObjectWithTag("Player") != null)
            GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, 0f,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2);*/

        //float increment = (float)Math.Round((bottomRightV.x - bottomLeftV.x / torchFrequency) * 2f, MidpointRounding.AwayFromZero) / 2f;
        bool hasTorch = false;
        int myTorchFrequency = torchFrequency;
        int wallLength = (int)bottomRightV.x - (int)bottomLeftV.x;
        while (wallLength > roomWidthMin)
        {
            wallLength -= roomWidthMin;
            myTorchFrequency++;
        }

        int increment = (int)Math.Ceiling((bottomRightV.x - bottomLeftV.x) / myTorchFrequency);
        int index = 1;

        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            Vector3 wallPosition = new Vector3(row, 0, bottomLeftV.z);

            if (row == (int)bottomLeftV.x + (increment * index))
            {
                if (increment <= 1)
                    index += 2;
                else
                    index++;
                //this wall should have torch
                hasTorch = true;
            }

            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, hasTorch, possibleTorchHorizontalPosition, true);

            hasTorch = false;
        }

        myTorchFrequency = torchFrequency;
        wallLength = (int)topRightV.x - (int)topLeftV.x;
        while (wallLength > roomWidthMin)
        {
            wallLength -= roomWidthMin;
            myTorchFrequency++;
        }

        increment = (int)Math.Ceiling((topRightV.x - topLeftV.x) / myTorchFrequency);
        index = 1;

        for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
        {
            Vector3 wallPosition = new Vector3(row, 0, topRightV.z);

            if (row == (int)topLeftV.x + (increment * index))
            {
                if (increment <= 1)
                    index += 2;
                else
                    index++;
                hasTorch = true;
            }
            wallPosition.y = 1;
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, hasTorch, possibleTorchHorizontalPosition, true);

            hasTorch = false;
        }

        myTorchFrequency = torchFrequency;
        wallLength = (int)topLeftV.z - (int)bottomLeftV.z;
        while (wallLength > roomLengthMin)
        {
            wallLength -= roomLengthMin;
            myTorchFrequency++;
        }

        increment = (int)Math.Ceiling((topLeftV.z - bottomLeftV.z) / myTorchFrequency);
        index = 1;

        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            Vector3 wallPosition = new Vector3(bottomLeftV.x, 0, col);

            if (col == (int)bottomLeftV.z + (increment * index))
            {
                if (increment <= 1)
                    index += 2;
                else
                    index++;
                hasTorch = true;
            }
            wallPosition.y = 1;
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, hasTorch, possibleTorchVerticalPosition, false);

            hasTorch = false;
        }

        myTorchFrequency = torchFrequency;
        wallLength = (int)topRightV.z - (int)bottomRightV.z;
        while (wallLength > roomLengthMin)
        {
            wallLength -= roomLengthMin;
            myTorchFrequency++;
        }

        increment = (int)Math.Ceiling((topRightV.z - bottomRightV.z) / myTorchFrequency);
        index = 1;

        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            Vector3 wallPosition = new Vector3(bottomRightV.x, 0, col);

            if (col == (int)bottomLeftV.z + (increment * index))
            {
                if (increment <= 1)
                    index += 2;
                else
                    index++;
                hasTorch = true;
            }

            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, hasTorch, possibleTorchVerticalPosition, false);

            hasTorch = false;

            //Vector3 torchPosition = new Vector3(bottomRightV.x, ceilingHeight - 1, col + 0.5f);
            //Instantiate(testCube, torchPosition, Quaternion.identity);
        }

        /*possibleWallHorizontalPosition = possibleWallHorizontalPosition.Distinct().ToList();
        List<Vector3Int> horizontalDoors = possibleWallHorizontalPosition.Intersect(possibleDoorHorizontalPosition).ToList();
        possibleWallHorizontalPosition.RemoveAll(x => horizontalDoors.Contains(x));

        possibleWallVerticalPosition = possibleWallVerticalPosition.Distinct().ToList();
        List<Vector3Int> verticalDoors = possibleWallVerticalPosition.Intersect(possibleDoorVerticalPosition).ToList();
        possibleWallVerticalPosition.RemoveAll(x => verticalDoors.Contains(x));*/
    }

    private void AddWallPositionToList(Vector3 wallStart, List<Vector3Int> wallList, List<Vector3Int> flipList, bool hasTorch, List<Vector3> torchList, bool horizontal)
    {
        Vector3Int point = Vector3Int.CeilToInt(new Vector3(wallStart.x, 0, wallStart.z));

        Vector3 torchPoint = new Vector3(point.x, ceilingHeight - 1, point.z);

        if (wallList.Contains(point))
        {
            wallList.Remove(point);
            //doorList.Add(new Vector3Int(point.x, 1, point.z));
            //wallList.Remove(new Vector3Int(point.x, 1, point.z));
            if (torchList.Contains(torchPoint))
                torchList.Remove(torchPoint);
        }
        /*else if (wallList.Contains(new Vector3Int(point.x, 1, point.z)))
        {
            //doorList.Add(point);
            //wallList.Remove(point);
            doorList.Add(new Vector3Int(point.x, 1, point.z));
            wallList.Remove(new Vector3Int(point.x, 1, point.z));
            if (torchList.Contains(torchPoint))
                torchList.Remove(torchPoint);
        }*/
        else
        {
            wallList.Add(point);
            if (wallStart.y == 1)
            {
                flipList.Add(point);
            }
            if (hasTorch)
                torchList.Add(torchPoint);
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