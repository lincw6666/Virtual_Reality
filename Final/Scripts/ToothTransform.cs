using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Tooth
{
    public class ToothTransform
    {
        private Teeth teeth;
        private Controller controller;
        private readonly ImportSTL import = new ImportSTL();

        public void Init() {
            teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
            controller = GameObject.Find("/Controller").GetComponent<Controller>();
        }

        public void SetCorrectPosition() {
            GameObject f_obj = new GameObject("Final Teeth");
            Teeth f_teeth;
            Mesh[] f_mesh = new Mesh[teeth.TOOTH_NUM];
            Vector3 f_lingual_pos;

            // Import all final teeth.
            f_obj.AddComponent<Teeth>();
            f_obj.GetComponent<Teeth>().obj = new GameObject[teeth.TOOTH_NUM];
            f_obj.GetComponent<Teeth>().param = new ToothParam[teeth.TOOTH_NUM];
            import.ImportFinal(f_obj);
            f_teeth = f_obj.GetComponent<Teeth>();
            for (int i = 0; i < teeth.TOOTH_NUM; i++) {
                if (f_teeth.obj[i].GetComponent<MeshFilter>())
                    f_mesh[i] = f_teeth.obj[i].GetComponent<MeshFilter>().mesh;
            }
            f_lingual_pos = (f_mesh[18].bounds.center + f_mesh[31].bounds.center) / 2.0f;

            for (int i = 0; i < teeth.TOOTH_NUM; i++) {
                if (!f_mesh[i]) continue;   // Skip missing teeth.

                Mesh i_mesh = teeth.obj[i].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices;
                Vector3 center = teeth.param[i].GetCenter();
                Vector3 v1i, v1f, v3i, v3f;     // v1, v3 for initial and final
                Quaternion rotate_init_yz;      // rotate the tooth making v1 match y, v3 match z
                Quaternion rotate_yz_final;     // rotate from yz to final v1, v3
                Quaternion rotate_init_final;   // rotate from initial to final

                // Get v1, v3.
                f_teeth.param[i] = new ToothParam(f_mesh[i], f_lingual_pos, i);
                v1i = teeth.param[i].GetV1();
                v3i = teeth.param[i].GetV3();
                v1f = f_teeth.param[i].GetV1();
                v3f = f_teeth.param[i].GetV3();

                // Move tooth to origin.
                vertices = i_mesh.vertices;
                for (int j = 0; j < i_mesh.vertexCount; j++) {
                    vertices[j] -= center;
                }
                teeth.obj[i].GetComponent<MeshFilter>().mesh.vertices = vertices;

                // Set initial to yz rotation quaternion.
                rotate_init_yz = Quaternion.FromToRotation(v1i, Vector3.up).normalized;
                rotate_init_yz = Quaternion.FromToRotation(rotate_init_yz * v3i, Vector3.forward).normalized * rotate_init_yz;
                // Set yz to final rotation quaternion.
                rotate_yz_final = Quaternion.FromToRotation(v1f, Vector3.up).normalized;
                rotate_yz_final = Quaternion.FromToRotation(rotate_yz_final * v3f, Vector3.forward).normalized * rotate_yz_final;
                rotate_yz_final = Quaternion.Inverse(rotate_yz_final);
                // Set initial to final rotation quaternion.
                rotate_init_final = rotate_yz_final * rotate_init_yz;

                // Move to the correct position.
                vertices = i_mesh.vertices;
                for (int j = 0; j < i_mesh.vertexCount; j++) {
                    vertices[j] = rotate_init_yz * vertices[j];     // Align with yz axes.

                    // Update width, height.
                    float disty = Vector3.Distance(new Vector3(0, vertices[j].y, 0), vertices[j]) * 2.0f;
                    float distx = Vector3.Distance(new Vector3(vertices[j].x, 0, 0), vertices[j]) * 2.0f;
                    if (disty > teeth.param[i].width) teeth.param[i].width = disty;
                    if (distx > teeth.param[i].height) teeth.param[i].height = distx;

                    vertices[j] = rotate_yz_final * vertices[j];
                    vertices[j] += f_mesh[i].bounds.center;
                }
                teeth.obj[i].GetComponent<MeshFilter>().mesh.vertices = vertices;

                // Set tooth parameters.
                teeth.param[i].SetV1Vec(f_teeth.param[i].GetV1());
                teeth.param[i].SetCenter(f_mesh[i].bounds.center, true);
            }

            // Destroy final teeth's object.
            foreach(GameObject tmp_obj in f_obj.GetComponent<Teeth>().obj) {
                GameObject.Destroy(tmp_obj);
            }
            GameObject.Destroy(f_obj);
        }
    }
}
