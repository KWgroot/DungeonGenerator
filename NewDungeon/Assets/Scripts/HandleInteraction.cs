using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandleInteraction : MonoBehaviour
{
    private TextMeshProUGUI text;
    public float raycastDistance, segmentLength;
    private DungeonCreator dungeonVariables;
    public Material bridgeMat;
    public bool vertical = false;

    private bool inRange = false, interacting;
    private new Camera camera;
    private int layerMask = 1 << 6;
    private Vector3 previousMousePos, previousPoint;

    // Start is called before the first frame update
    void Start()
    {
        camera = transform.GetChild(0).GetComponent<Camera>(); // Get my own specific camera for drawing.
        dungeonVariables = GameObject.FindGameObjectWithTag("Dungeon Creator").GetComponent<DungeonCreator>(); // Variables to size me.
        text = GameObject.FindGameObjectWithTag("UIText").GetComponent<TextMeshProUGUI>(); // Text to show you can draw here.
        transform.GetChild(2).transform.localScale = new Vector3(1, 3, dungeonVariables.corridorWidth); // Scale me based on the dungeon sizes.
        // move by 0.5 depending on orientation due to int calculation not fitting with uneven sized corridors.
        if (dungeonVariables.corridorWidth % 2 == 1)
            if (!vertical)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f);
            }
            else
            {
                transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
            }
    }

    // Update is called once per frame
    void Update()
    {
        if (inRange && !interacting && Input.GetKeyDown("e")) // Player is close and wants to interact.
            InteractWithMe(true);
        else if (interacting && (Input.GetKeyDown("e") || Input.GetKeyDown("escape"))) // Player is interacting and wants to stop.
            InteractWithMe(false);

        if (interacting && Input.GetMouseButton(0) && previousMousePos != Input.mousePosition) // Currently drawing and moving the mouse.
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, layerMask))
            {
                if (hit.transform.name != "Segment" && previousPoint != Vector3.zero) // Hit an empty space.
                {
                    Vector3 bottomLeftV;
                    Vector3 bottomRightV;
                    Vector3 topLeftV;
                    Vector3 topRightV;

                    if (!vertical)
                    {
                        if ((hit.point.x - previousPoint.x) < 0) // right to left
                        {
                            bottomLeftV = new Vector3(hit.point.x, hit.point.y, transform.position.z + (float)dungeonVariables.corridorWidth / 2f);
                            bottomRightV = new Vector3(hit.point.x, hit.point.y, transform.position.z - (float)dungeonVariables.corridorWidth / 2f);
                            topLeftV = new Vector3(previousPoint.x, previousPoint.y, transform.position.z + (float)dungeonVariables.corridorWidth / 2f);
                            topRightV = new Vector3(previousPoint.x, previousPoint.y, transform.position.z - (float)dungeonVariables.corridorWidth / 2f);
                        }
                        else // left to right
                        {
                            bottomLeftV = new Vector3(previousPoint.x, previousPoint.y, transform.position.z + (float)dungeonVariables.corridorWidth / 2f);
                            bottomRightV = new Vector3(previousPoint.x, previousPoint.y, transform.position.z - (float)dungeonVariables.corridorWidth / 2f);
                            topLeftV = new Vector3(hit.point.x, hit.point.y, transform.position.z + (float)dungeonVariables.corridorWidth / 2f);
                            topRightV = new Vector3(hit.point.x, hit.point.y, transform.position.z - (float)dungeonVariables.corridorWidth / 2f);
                        }
                    }
                    else
                    {
                        if ((hit.point.z - previousPoint.z) < 0) // right to left
                        {
                            bottomLeftV = new Vector3(transform.position.x + (float)dungeonVariables.corridorWidth / 2f, previousPoint.y, previousPoint.z);
                            bottomRightV = new Vector3(transform.position.x - (float)dungeonVariables.corridorWidth / 2f, previousPoint.y, previousPoint.z);
                            topLeftV = new Vector3(transform.position.x + (float)dungeonVariables.corridorWidth / 2f, hit.point.y, hit.point.z);
                            topRightV = new Vector3(transform.position.x - (float)dungeonVariables.corridorWidth / 2f, hit.point.y, hit.point.z);
                        }
                        else // left to right
                        {
                            bottomLeftV = new Vector3(transform.position.x + (float)dungeonVariables.corridorWidth / 2f, hit.point.y, hit.point.z);
                            bottomRightV = new Vector3(transform.position.x - (float)dungeonVariables.corridorWidth / 2f, hit.point.y, hit.point.z);
                            topLeftV = new Vector3(transform.position.x + (float)dungeonVariables.corridorWidth / 2f, previousPoint.y, previousPoint.z);
                            topRightV = new Vector3(transform.position.x - (float)dungeonVariables.corridorWidth / 2f, previousPoint.y, previousPoint.z);
                        }
                    }
                    // Start creating a mesh for the segment drawn.
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

                    // Parent of all segments.
                    GameObject bridgeSegment = new GameObject("Segment", typeof(MeshFilter), typeof(MeshRenderer));

                    bridgeSegment.transform.position = Vector3.zero;
                    bridgeSegment.transform.localScale = Vector3.one;
                    bridgeSegment.GetComponent<MeshFilter>().mesh = mesh;
                    bridgeSegment.GetComponent<MeshRenderer>().material = bridgeMat;
                    bridgeSegment.transform.parent = transform.GetChild(1).transform;
                    bridgeSegment.layer = 6;
                    bridgeSegment.AddComponent<BoxCollider>();
                    bridgeSegment.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                }

                previousPoint = hit.point;
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Stopped drawing.
            previousPoint = Vector3.zero;

        previousMousePos = Input.mousePosition;
    }
    /// <summary>
    /// Interaction with object starts.
    /// </summary>
    /// <param name="startInteracting">Start or stop interacting.</param>
    private void InteractWithMe(bool startInteracting)
    {
        if (startInteracting) // If starting to interact, lock mouse, set up colliders and camera.
        {
            interacting = true;
            camera.enabled = true;
            text.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<SphereCollider>().enabled = false;
            GameObject.FindGameObjectWithTag("Player").transform.GetChild(2).GetComponent<ThirdPersonController>().enabled = false;

            while (transform.GetChild(1).childCount != 0)
            {
                foreach (Transform item in transform.GetChild(1))
                {
                    DestroyImmediate(item.gameObject);
                }
            }

            transform.GetChild(1).GetComponent<MeshFilter>().mesh.Clear();
            DestroyImmediate(transform.GetChild(1).GetComponent<MeshCollider>());
        }
        else // Stop interacting and finalize whatever bridge created.
        {
            interacting = false;
            camera.enabled = false;
            text.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<SphereCollider>().enabled = true;
            GameObject.FindGameObjectWithTag("Player").transform.GetChild(2).GetComponent<ThirdPersonController>().enabled = true;

            CombineMeshes(transform.GetChild(1).gameObject);
            previousPoint = Vector3.zero;
        }
    }
    /// <summary>
    /// Combines meshes into any parent given.
    /// </summary>
    /// <param name="bridge">Parent object.</param>
    private void CombineMeshes(GameObject bridge)
    {
        Vector3 position = bridge.transform.position;
        bridge.transform.position = Vector3.zero;

        MeshFilter[] meshFilters = bridge.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 1; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        bridge.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        bridge.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        bridge.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        bridge.GetComponent<Renderer>().material = bridgeMat;
        bridge.transform.gameObject.SetActive(true);
        bridge.gameObject.isStatic = true;        
        bridge.transform.position = position;
        bridge.transform.localScale = Vector3.one;
        if (!vertical)
        {
            bridge.transform.localScale = new Vector3(bridge.transform.localScale.x / bridge.transform.parent.localScale.x, 1, 1);
        }
        else
        {
            bridge.transform.localScale = new Vector3(1, 1, bridge.transform.localScale.x / bridge.transform.parent.localScale.x);
        }
        bridge.AddComponent<MeshCollider>();
        bridge.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    private void OnTriggerEnter(Collider other)
    {
        text.enabled = true;
        inRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        text.enabled = false;
        inRange = false;
    }
}
