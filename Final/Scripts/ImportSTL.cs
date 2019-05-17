using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImportSTL
{
    private Teeth teeth;
    private Controller controller;

    public void ImportInit()
    {
        teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        controller = GameObject.Find("/Controller").GetComponent<Controller>();

        // Exit the app if source directory does not exist.
        if (!Directory.Exists(controller.src_dir_path)) {
            Debug.Log("No such directory: " + controller.src_dir_path);
            Controller.QuitApp();
        }

        // Import all stl models which are under data/separated/initial.
        int[] down_root_id = new int[2];    // These two teeth define the lingual position.
        down_root_id[0] = -1;
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            string src_path = controller.src_dir_path + i.ToString() + ".stl";
            Mesh mesh;
            Material material;

            if (!File.Exists(src_path)) continue;
            
            // Update down_root_id.
            if (i >= 16) {
                if (down_root_id[0] == -1) down_root_id[0] = i;
                down_root_id[1] = i;
            }

            mesh = Parabox.STL.pb_Stl_Importer.Import(src_path)[0];
            material = new Material(Shader.Find("VR_Final_Project/MyPhongShader"));

            teeth.obj[i].GetComponent<MeshFilter>().mesh = mesh;
            teeth.obj[i].GetComponent<MeshRenderer>().material = material;
        }

        // Set teeth parameters.
        teeth.param = new Tooth.ToothParam[Teeth.TOOTH_NUM];
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            teeth.param[i] = new Tooth.ToothParam(
                teeth.obj[i].GetComponent<MeshFilter>().mesh, 
                new Vector3(),
                i
            );
        }
        // Set lingual's position and v2.
        Vector3 lingual_pos = (teeth.obj[down_root_id[0]].transform.TransformPoint(teeth.param[down_root_id[0]].GetCenter()) +
                                teeth.obj[down_root_id[1]].transform.TransformPoint(teeth.param[down_root_id[1]].GetCenter())) / 2.0f;
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            teeth.param[i].SetLingualPos(lingual_pos);
            teeth.param[i].SetPre();
            // Set mesh collider.
            teeth.obj[i].GetComponent<MeshCollider>().sharedMesh
                = teeth.obj[i].GetComponent<MeshFilter>().mesh;
        }

        // Set up, down, left and right.
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            Mesh mesh = teeth.obj[i].GetComponent<MeshFilter>().mesh;
            if (mesh.vertexCount == 0) continue;
            Vector3[] vertices;
            Vector3 center = teeth.param[i].GetCenter();
            Quaternion rotate_match_yz = Quaternion.FromToRotation(teeth.param[i].GetV1(), Vector3.up).normalized;
            rotate_match_yz = Quaternion.FromToRotation(rotate_match_yz * teeth.param[i].GetV3(), Vector3.forward).normalized * rotate_match_yz;

            vertices = mesh.vertices;
            for (int j = 0; j < mesh.vertexCount; j++) {
                // Move to origin and rotate to match yz axes.
                vertices[j] -= center;
                vertices[j] = rotate_match_yz * vertices[j];

                // Update up, down, left, right.
                if (vertices[j].y > teeth.param[i].up) teeth.param[i].up = vertices[j].y;
                else if (vertices[j].y < teeth.param[i].down) teeth.param[i].down = vertices[j].y;
                if (vertices[j].z > teeth.param[i].right) teeth.param[i].right = vertices[j].z;
                else if (vertices[j].z < teeth.param[i].left) teeth.param[i].left = vertices[j].z;

                // Move back to initial position and rotate back to initial rotation.
                vertices[j] = Quaternion.Inverse(rotate_match_yz) * vertices[j];
                vertices[j] += center;
            }
            teeth.obj[i].GetComponent<MeshFilter>().mesh.vertices = vertices;
        }
    }

    public void ImportFinal(GameObject f_obj) {
        // Exit the app if final directory does not exist.
        teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        controller = GameObject.Find("/Controller").GetComponent<Controller>();
        if (!Directory.Exists(controller.final_dir_path)) {
            Debug.Log("No such directory: " + controller.final_dir_path);
            Controller.QuitApp();
        }

        Teeth f_teeth;

        f_teeth = f_obj.GetComponent<Teeth>();
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            string src_path = controller.final_dir_path + i.ToString() + ".stl";
            f_obj.GetComponent<Teeth>().obj[i] = new GameObject();
            if (!File.Exists(src_path)) continue;

            f_obj.GetComponent<Teeth>().obj[i].AddComponent<MeshFilter>();
            f_teeth.obj[i].GetComponent<MeshFilter>().mesh = Parabox.STL.pb_Stl_Importer.Import(src_path)[0];
        }
    }
}
