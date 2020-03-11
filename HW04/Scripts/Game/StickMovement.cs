using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickMovement : MonoBehaviour
{
    public StickController stick_ctrl;

    private Mesh mesh;
    private Vector3[] origin_vertices;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        mesh = this.GetComponent<MeshFilter>().mesh;
        origin_vertices = new Vector3[mesh.vertexCount];
        System.Array.Copy(mesh.vertices, origin_vertices, mesh.vertexCount);
        offset = origin_vertices[StickController.root_of_control_stick];

        /*
        // Find the root of the control stick.
        int id = base_id;
        float min = vertices[base_id].y;

        for (int i = base_id + 1; i < limit_id; i++) {
            if (vertices[i].y < min) {
                id = i;
                min = vertices[i].y;
            }
        }
        Debug.Log(id);
        */

        /*
        // Destroy the control stick.
        Vector3[] newVertices = new Vector3[mesh.vertexCount];

        for (int i = 0; i < mesh.vertexCount; i++) {
            if (base_id <= i && i < limit_id) {
                newVertices[i] = Vector3.zero;
            }
            else {
                newVertices[i] = vertices[i];
            }
        }
        mesh.vertices = newVertices;
        */
    }

    // Update is called once per frame
    void Update()
    {
        /* Move the stick. */
        Vector3[] vertices = new Vector3[mesh.vertexCount];
        System.Array.Copy(origin_vertices, vertices, mesh.vertexCount);
        for (int i = StickController.base_id; i < StickController.limit_id; i++) {
            // Move to origin.
            vertices[i] -= offset;
            // Rotate.
            vertices[i] = stick_ctrl.NowRotate() * vertices[i];
            // Move back.
            vertices[i] += offset;
        }
        mesh.vertices = vertices;
    }
}
