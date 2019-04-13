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
        teeth = GameObject.Find("/Tooth").GetComponent<Teeth>();
        controller = GameObject.Find("/Controller").GetComponent<Controller>();

        // Exit the app if source directory does not exist.
        if (!Directory.Exists(controller.src_dir_path)) {
            Debug.Log("No such directory: " + controller.src_dir_path);
            controller.QuitApp();
        }
        
        // Import all stl models which are under data/separated/initial.
        for (int i = 0; i < teeth.TOOTH_NUM; i++) {
            string src_path = controller.src_dir_path + i.ToString() + ".stl";
            Mesh mesh;
            Material material;

            if (!File.Exists(src_path)) continue;

            mesh = Parabox.STL.pb_Stl_Importer.Import(src_path)[0];
            material = new Material(Shader.Find("VR_Final_Project/MyPhongShader"));

            teeth.obj[i].GetComponent<MeshFilter>().mesh = mesh;
            teeth.obj[i].GetComponent<MeshRenderer>().material = material;
        }

        // Set teeth parameters.
        teeth.param = new Tooth.ToothParam[teeth.TOOTH_NUM];
        for (int i = 0; i < teeth.TOOTH_NUM; i++) {
            teeth.param[i] = new Tooth.ToothParam(
                teeth.obj[i].GetComponent<MeshFilter>().mesh, 
                new Vector3(),
                i
            );
        }
        // Set lingual's position and v2.
        Vector3 lingual_pos = (teeth.obj[18].transform.TransformPoint(teeth.param[18].GetCenter()) +
                                teeth.obj[31].transform.TransformPoint(teeth.param[31].GetCenter())) / 2.0f;
        for (int i = 0; i < teeth.TOOTH_NUM; i++) {
            teeth.param[i].SetLingualPos(lingual_pos);
        }
    }

    public void ImportFinal(GameObject f_obj) {
        // Exit the app if final directory does not exist.
        teeth = GameObject.Find("/Tooth").GetComponent<Teeth>();
        controller = GameObject.Find("/Controller").GetComponent<Controller>();
        if (!Directory.Exists(controller.final_dir_path)) {
            Debug.Log("No such directory: " + controller.final_dir_path);
            controller.QuitApp();
        }

        Teeth f_teeth;

        f_teeth = f_obj.GetComponent<Teeth>();
        for (int i = 0; i < teeth.TOOTH_NUM; i++) {
            string src_path = controller.final_dir_path + i.ToString() + ".stl";
            f_obj.GetComponent<Teeth>().obj[i] = new GameObject();
            if (!File.Exists(src_path)) continue;

            f_obj.GetComponent<Teeth>().obj[i].AddComponent<MeshFilter>();
            f_teeth.obj[i].GetComponent<MeshFilter>().mesh = Parabox.STL.pb_Stl_Importer.Import(src_path)[0];
        }
    }
}
