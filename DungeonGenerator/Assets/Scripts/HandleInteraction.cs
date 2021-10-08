using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandleInteraction : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float raycastDistance, segmentLength;
    public DungeonCreator dungeonVariables;
    public Material bridgeMat;

    private bool inRange = false, interacting;
    private new Camera camera;
    private int layerMask = 1 << 6;
    private Vector3 previousMousePos;

    // Start is called before the first frame update
    void Start()
    {
        camera = transform.GetChild(2).GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inRange && !interacting && Input.GetKeyDown("e"))
            InteractWithMe(true);
        else if (interacting && (Input.GetKeyDown("e") || Input.GetKeyDown("escape")))
            InteractWithMe(false);

        if (interacting && Input.GetMouseButton(0) && previousMousePos != Input.mousePosition)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, layerMask))
            {
                if (hit.transform.name != "Segment")
                {
                    Vector3 bottomLeftV = new Vector3(hit.point.x - segmentLength, hit.point.y, hit.point.z + dungeonVariables.corridorWidth / 2);
                    Vector3 bottomRightV = new Vector3(hit.point.x - segmentLength, hit.point.y, hit.point.z - dungeonVariables.corridorWidth / 2);
                    Vector3 topLeftV = new Vector3(hit.point.x + segmentLength, hit.point.y, hit.point.z + dungeonVariables.corridorWidth / 2);
                    Vector3 topRightV = new Vector3(hit.point.x + segmentLength, hit.point.y, hit.point.z - dungeonVariables.corridorWidth / 2);

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

                    GameObject bridgeSegment = new GameObject("Segment", typeof(MeshFilter), typeof(MeshRenderer));

                    bridgeSegment.transform.position = Vector3.zero;
                    bridgeSegment.transform.localScale = Vector3.one;
                    bridgeSegment.GetComponent<MeshFilter>().mesh = mesh;
                    bridgeSegment.GetComponent<MeshRenderer>().material = bridgeMat;
                    bridgeSegment.transform.parent = transform.GetChild(3).transform;
                    bridgeSegment.layer = 6;
                    bridgeSegment.AddComponent<BoxCollider>();
                    bridgeSegment.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                }

                // get the point position, create a mesh the width of a corridor
                // what should length of each mesh be? Too long and it's not painting, too short and we make too many
                // place mesh as child of bridge object
                // calculate normals
                // give either box or mesh collider depending on what mesh shape chosen
                // make sure gameobject name is easy to read to prevent overwriting
                // IN THEORY this should do it... I think... idk that's for tomorrow Koen to figure out I guess.
            }
        }

        previousMousePos = Input.mousePosition;
    }

    private void InteractWithMe(bool startInteracting)
    {
        if (startInteracting)
        {
            interacting = true;
            camera.enabled = true;
            text.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<SphereCollider>().enabled = false;

            while (transform.GetChild(3).childCount != 0)
            {
                foreach (Transform item in transform.GetChild(3))
                {
                    DestroyImmediate(item.gameObject);
                }
            }

            transform.GetChild(3).GetComponent<MeshFilter>().mesh.Clear();
            DestroyImmediate(transform.GetChild(3).GetComponent<MeshCollider>());
        }
        else
        {
            interacting = false;
            camera.enabled = false;
            text.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<SphereCollider>().enabled = true;

            CombineMeshes(transform.GetChild(3).gameObject);
        }
    }

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
        bridge.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        bridge.transform.position = position;
        bridge.AddComponent<MeshCollider>();
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
