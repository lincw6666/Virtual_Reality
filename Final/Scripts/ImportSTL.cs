using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImportSTL : MonoBehaviour
{
    public string src_dir_path;
    public GameObject [] output_obj;

    private const uint TOOTH_NUM = 32;
    private Tooth.ToothParam [] teeth;
    // For debugging.
    private readonly MeshOP.BoundingBox bound_box = new MeshOP.BoundingBox();

    // Start is called before the first frame update
    void Start()
    {
        src_dir_path = Application.dataPath + src_dir_path;

        // Exit the app if source directory does not exist.
        if (!Directory.Exists(src_dir_path)) {
            Debug.Log("No such directory: " + src_dir_path);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Import all stl models which are under data/separated/initial.
        for (int i = 0; i < TOOTH_NUM; i++) {
            string src_path = src_dir_path + i.ToString() + ".stl";
            Mesh mesh;
            Material material;

            if (!File.Exists(src_path)) continue;

            mesh = Parabox.STL.pb_Stl_Importer.Import(src_path)[0];
            material = new Material(Shader.Find("VR_Final_Project/MyPhongShader"));

            output_obj[i].GetComponent<MeshFilter>().mesh = mesh;
            output_obj[i].GetComponent<MeshRenderer>().material = material;
        }

        // Set teeth parameters.
        teeth = new Tooth.ToothParam[TOOTH_NUM];
        for (int i = 0; i < TOOTH_NUM; i++) {
            teeth[i] = new Tooth.ToothParam(
                output_obj[i].GetComponent<MeshFilter>().mesh, 
                new Vector3(),
                i
            );
        }
        // Set lingual's position and v2.
        Vector3 lingual_pos = (output_obj[18].transform.TransformPoint(teeth[18].GetCenter()) +
                                output_obj[31].transform.TransformPoint(teeth[31].GetCenter())) / 2.0f;
        for (int i = 0; i < TOOTH_NUM; i++) {
            teeth[i].SetV2(lingual_pos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        /***********************************************************
         * Debug message
         **********************************************************/
        for (int i = 0; i < TOOTH_NUM; i++) {
            // Draw bounding box.
            DrawBoundingBox((uint) i);

            // Draw v1, v2 of each tooth.
            DrawV1(output_obj[i].GetComponent<Transform>(), (uint) i);
            DrawV2(output_obj[i].GetComponent<Transform>(), (uint) i);
        }
        /***********************************************************
         * End Debug message
         **********************************************************/
    }

    /***********************************************************
     * Debug functions
     **********************************************************/
    void DrawBoundingBox(uint id) {
        bound_box.DrawBoundingBox(
            output_obj[id].GetComponent<Transform>(),
            output_obj[id].GetComponent<MeshFilter>().mesh.bounds
        );
    }

    void DrawV1(Transform transform, uint id) {
        Vector3 world_center = transform.TransformPoint(teeth[id].GetCenter());
        Vector3 world_v1 = transform.TransformPoint(teeth[id].GetCenter() + teeth[id].GetV1()*20.0f);
        Debug.DrawLine(world_center, world_v1, Color.green);
    }
    
    void DrawV2(Transform transform, uint id) {
        Vector3 world_lingual = transform.TransformPoint(teeth[id].GetLingualPos());
        Vector3 world_v2 = transform.TransformPoint(teeth[id].GetLingualPos() + teeth[id].GetV2() * 30.0f);
        Debug.DrawLine(world_lingual, world_v2, Color.red);
    }
    /***********************************************************
     * End Debug functions
     **********************************************************/
}
