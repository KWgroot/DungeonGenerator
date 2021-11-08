using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject player, wallVertical, wallHorizontal, drawAreaHorizontal, drawAreaVertical, horizontalTorch, verticalTorch, stairs;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;
    List<Vector3Int> possibleTorchHorizontalPosition;
    List<Vector3Int> possibleTorchVerticalPosition;

    private GameObject floorParent, ceilingParent, torchParent, stairParent;
    private int torchIndex = 1;

    [HideInInspector]
    public static DungeonCreator current;
    [HideInInspector]
    public float progress;
    public Image fadeImage;

    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();
    }
    /// <summary>
    /// The manager of creating any new dungeon from the variables set in the generator.
    /// </summary>
    public void CreateDungeon(bool fadeIn = false)
    {
        DestroyAllChildren(); // Reset dungeon

        if (fadeIn)
        {
            StartCoroutine(FadeIn());
        }

        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
        List<Node> listOfRooms = generator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, roomBottomCornerModifier,
            roomTopCornerMidifier, roomOffset, corridorWidth);

        GameObject wallParent = new GameObject("WallParent"); // Create empty for wall coming walls
        wallParent.transform.parent = transform;
        wallParent.AddComponent<MeshFilter>();
        wallParent.AddComponent<MeshRenderer>();
        wallMat.mainTextureScale = new Vector2(1, ceilingHeight);

        floorParent = new GameObject("Floors", typeof(MeshFilter), typeof(MeshRenderer)); // Create empty for floors, ceilings and torches
        floorParent.layer = 3;
        floorParent.transform.parent = transform;
        ceilingParent = new GameObject("Ceilings", typeof(MeshFilter), typeof(MeshRenderer));
        ceilingParent.transform.parent = transform;
        torchParent = new GameObject("Torches");
        torchParent.transform.parent = transform;

        stairParent = new GameObject("Stairs");
        stairParent.transform.parent = transform;

        possibleDoorVerticalPosition = new List<Vector3Int>(); // Lists of positions to check for overlaps
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        possibleTorchHorizontalPosition = new List<Vector3Int>();
        possibleTorchVerticalPosition = new List<Vector3Int>();

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            if (i == 0)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, startRoomMat, true); // Start room
            else if (i == listOfRooms.Count / 2)
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, endRoomMat); // End room
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
                CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, roomMat); // Just create room (floor)

            progress = (((float)listOfRooms.Count / ((float)listOfRooms.Count - (float)i)) / 100f);
        }

        CreateWalls(wallParent);
        progress = 0.80f;

        // combine floor
        CombineWallMeshes(floorParent, true);
        progress = 0.95f;

        // combine ceiling
        CombineWallMeshes(ceilingParent, true);
        progress = 0.975f;

        // combine wall meshes
        CombineWallMeshes(wallParent);
        progress = 1f;
    }

    private IEnumerator FadeIn()
    {
        for (float i = 1; i >= 0; i -= Time.deltaTime)
        {
            fadeImage.color = new Color(1, 1, 1, i);
            yield return null;
        }
    }

    /// <summary>
    /// Combines all meshes from a parent object into a single mesh on the parent
    /// </summary>
    /// <param name="parent">Parent object to set new mesh to.</param>
    /// <param name="floor">True if creating a floor.</param>
    private void CombineWallMeshes(GameObject parent, bool floor = false)
    {
        Vector3 position = parent.transform.position; // Places all objects in empty on 0,0,0
        parent.transform.position = Vector3.zero;

        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 1; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        parent.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        parent.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        if (!floor)
            parent.GetComponent<Renderer>().material = wallMat;
        else
            parent.GetComponent<Renderer>().material = roomMat;
        parent.transform.gameObject.SetActive(true);
        parent.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        parent.transform.position = position;
        if (floor)
            parent.AddComponent<MeshCollider>();
    }
    /// <summary>
    /// Loops through all walls and torches to be placed and calls corresponding function to instantiate them.
    /// </summary>
    /// <param name="wallParent">Parent object to place all walls and torches in.</param>
    private void CreateWalls(GameObject wallParent)
    {
        foreach (Vector3Int wallPosition in possibleWallHorizontalPosition) // Create horizontal wall from points
        {
            if (possibleDoorHorizontalPosition.Contains(wallPosition))
                CreateWall(wallParent, wallPosition, wallHorizontal, true);
            else
                CreateWall(wallParent, wallPosition, wallHorizontal, false);
        }

        foreach (Vector3Int wallPosition in possibleWallVerticalPosition) // Create vertical wall from points
        {
            if (possibleDoorVerticalPosition.Contains(wallPosition))
                CreateWall(wallParent, wallPosition, wallVertical, true);
            else
                CreateWall(wallParent, wallPosition, wallVertical, false);
        }

        foreach (Vector3Int torchPosition in possibleTorchHorizontalPosition) // Create torches on horizontal walls from points
        {
            if (possibleDoorHorizontalPosition.Contains(torchPosition))
                CreateTorch(torchPosition, horizontalTorch, true);
            else
                CreateTorch(torchPosition, horizontalTorch, false);
        }

        foreach (Vector3Int torchPosition in possibleTorchVerticalPosition) // Create torches on vertical walls from points
        {
            if (possibleDoorVerticalPosition.Contains(torchPosition))
                CreateTorch(torchPosition, verticalTorch, true);
            else
                CreateTorch(torchPosition, verticalTorch, false);
        }
    }
    /// <summary>
    /// Instantiates walls based on given points.
    /// </summary>
    /// <param name="wallParent">Parent object of all objects.</param>
    /// <param name="wallPosition">Position to instantiate object.</param>
    /// <param name="wallPrefab">Prefab to instantiate.</param>
    /// <param name="flip">Should triangles be flipped?</param>
    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab, bool flip)
    {
        if (flip)
            wallPosition.y = 0;

        GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
        wall.transform.localScale = new Vector3(1, ceilingHeight, 1); // Make wall adjusted to ceiling height.

        if (flip) // flip all triangles to support one sided rendering.
            wall.transform.GetChild(0).GetComponent<MeshFilter>().mesh.triangles = wall.transform.GetChild(0).GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
    }
    /// <summary>
    /// Instantiates torches based on given points.
    /// </summary>
    /// <param name="torchPosition">Position to place torch.</param>
    /// <param name="torchPrefab">Prefab to instantiate.</param>
    /// <param name="flip">Should torch be flipped?</param>
    private void CreateTorch(Vector3 torchPosition, GameObject torchPrefab, bool flip)
    {
        torchPosition.y = ceilingHeight - 1;
        // throws LOD warnings, can't figure out how to fix.
        GameObject torch = Instantiate(torchPrefab, torchPosition, Quaternion.identity, torchParent.transform);

        torch.name = "Torch #" + torchIndex;
        torchIndex++;

        if (flip)
            torch.transform.Rotate(0, 180, 0);
    }
    /// <summary>
    /// Creates floor and ceiling meshes to support the dungeon layout.
    /// </summary>
    /// <param name="bottomLeftCorner">Bottom left vertice of mesh to be drawn.</param>
    /// <param name="topRightCorner">Top right vertice of mesh to be drawn.</param>
    /// <param name="myMat">Material to give newly created mesh.</param>
    /// <param name="spawnRoom">Is this room the spawn room?</param>
    /// <param name="removeCorridor">Should corridor floor be removed?</param>
    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, Material myMat, bool spawnRoom = false, bool removeCorridor = false)
    {
        // First setup all dimensions of the mesh.
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

        // Create ceiling object and get it ready for all ceilings.
        GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
        dungeonCeiling.transform.parent = transform;

        if (!removeCorridor) // If this floor is a corridor to be removed for a special area, dont make the floor mesh.
        {
            GameObject dungeonFloor = new GameObject("Floor" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer)); // Otherwise just make it.
            dungeonFloor.transform.position = Vector3.zero;
            dungeonFloor.transform.localScale = Vector3.one;
            dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
            dungeonFloor.GetComponent<MeshRenderer>().material = myMat;
            dungeonFloor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dungeonFloor.AddComponent<BoxCollider>();
            dungeonFloor.transform.parent = floorParent.transform;
            dungeonFloor.layer = 3;
            dungeonFloor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }

        dungeonCeiling.transform.position = Vector3.zero; // Same mesh as the floor, but flipped triangles.
        dungeonCeiling.transform.localScale = Vector3.one;
        dungeonCeiling.GetComponent<MeshFilter>().mesh = mesh;
        dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles = dungeonCeiling.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        dungeonCeiling.GetComponent<MeshRenderer>().material = roomMat;
        dungeonCeiling.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dungeonCeiling.AddComponent<BoxCollider>();
        dungeonCeiling.transform.parent = ceilingParent.transform;
        dungeonCeiling.transform.Translate(0f, ceilingHeight, 0f);
        dungeonCeiling.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        if (spawnRoom) // If this room is the spawn room, place the player here.
            Instantiate(player, new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, 0f,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2), Quaternion.identity);

        if (myMat == endRoomMat)
        {
            for (int i = 0; i <= (ceilingHeight / 3); i++)
            {
                GameObject staircase = Instantiate(stairs, new Vector3(bottomLeftCorner.x + (topRightCorner.x - bottomLeftCorner.x) / 2, i * 3,
                bottomLeftCorner.y + (topRightCorner.y - bottomLeftCorner.y) / 2), Quaternion.identity, stairParent.transform);

                if (!((i + 1) <= (ceilingHeight / 3)))
                    staircase.GetComponent<BoxCollider>().enabled = true;
            }
        }

        // Start seeing where the walls are placed based on the floors and if this wall gets a torch.
        bool hasTorch = false;
        int myTorchFrequency = torchFrequency;
        int wallLength = (int)bottomRightV.x - (int)bottomLeftV.x;
        while (wallLength > roomWidthMin) // Longer walls get more torches.
        {
            wallLength -= roomWidthMin;
            myTorchFrequency++;
        }

        int increment = (int)Math.Ceiling((bottomRightV.x - bottomLeftV.x) / myTorchFrequency);
        int index = 1;

        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            Vector3 wallPosition = new Vector3(row, 0, bottomLeftV.z); // Set wall / torch positions based on the floor mesh.

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

        // Repeat process for each side of all floors.
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
        }
    }
    /// <summary>
    /// Adds position to list of walls to see if there's overlap anywhere, includes torches.
    /// </summary>
    /// <param name="wallStart">Start point of the new wall.</param>
    /// <param name="wallList">List that holds this specific orientation of walls.</param>
    /// <param name="flipList">List of points with walls and torches that need to be flipped.</param>
    /// <param name="hasTorch">Does this torch have a wall?</param>
    /// <param name="torchList">List of torch points.</param>
    /// <param name="horizontal">Horizontal or vertical wall?</param>
    private void AddWallPositionToList(Vector3 wallStart, List<Vector3Int> wallList, List<Vector3Int> flipList, bool hasTorch, List<Vector3Int> torchList, bool horizontal)
    {
        Vector3Int point = Vector3Int.CeilToInt(new Vector3(wallStart.x, 0, wallStart.z));

        if (wallList.Contains(point)) // If point is already in list, this is a doorway.
        {
            wallList.Remove(point);
            if (torchList.Contains(point))
                torchList.Remove(point);
        }
        else // If it's a new point, place wall and torch if needed.
        {
            wallList.Add(point);
            if (wallStart.y == 1)
            {
                flipList.Add(point);
            }
            if (hasTorch)
                torchList.Add(point);
        }
    }
    /// <summary>
    /// Reset the scene for a new dungeon.
    /// </summary>
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

        torchIndex = 1;
    }
}