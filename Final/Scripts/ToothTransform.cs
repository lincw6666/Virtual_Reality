using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Mesh[] f_mesh = new Mesh[Teeth.TOOTH_NUM];
            Vector3 f_lingual_pos;

            // Import all final teeth.
            f_obj.AddComponent<Teeth>();
            f_obj.GetComponent<Teeth>().obj = new GameObject[Teeth.TOOTH_NUM];
            f_obj.GetComponent<Teeth>().param = new ToothParam[Teeth.TOOTH_NUM];
            import.ImportFinal(f_obj);
            f_teeth = f_obj.GetComponent<Teeth>();
            for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
                if (f_teeth.obj[i].GetComponent<MeshFilter>())
                    f_mesh[i] = f_teeth.obj[i].GetComponent<MeshFilter>().mesh;
            }
            f_lingual_pos = (f_mesh[18].bounds.center + f_mesh[31].bounds.center) / 2.0f;

            for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
                if (!f_mesh[i]) continue;   // Skip missing teeth.

                Mesh i_mesh = teeth.obj[i].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices;
                Vector3 center = teeth.param[i].GetCenter();
                Vector3 v1i, v1f, v3i, v3f;     // v1, v3 for initial and final
                Quaternion rotate_init_yz;      // rotate the tooth making v1 match y, v3 match z
                Quaternion rotate_yz_final;     // rotate from yz to final v1, v3
                //Quaternion rotate_init_final;   // rotate from initial to final

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
                //rotate_init_final = rotate_yz_final * rotate_init_yz;

                // Move to the correct position.
                vertices = i_mesh.vertices;
                for (int j = 0; j < i_mesh.vertexCount; j++) {
                    vertices[j] = rotate_init_yz * vertices[j];     // Align with yz axes.

                    /*
                    // Update width, height.
                    float disty = Vector3.Distance(new Vector3(0, vertices[j].y, 0), vertices[j]) * 2.0f;
                    float distx = Vector3.Distance(new Vector3(vertices[j].x, 0, 0), vertices[j]) * 2.0f;
                    if (disty > teeth.param[i].width) teeth.param[i].width = disty;
                    if (distx > teeth.param[i].height) teeth.param[i].height = distx;
                    */

                    // Update up, down, left, right.
                    if (vertices[j].y > teeth.param[i].up) teeth.param[i].up = vertices[j].y;
                    else if (vertices[j].y < teeth.param[i].down) teeth.param[i].down = vertices[j].y;
                    if (vertices[j].z > teeth.param[i].right) teeth.param[i].right = vertices[j].z;
                    else if (vertices[j].z < teeth.param[i].left) teeth.param[i].left = vertices[j].z;

                    vertices[j] = rotate_yz_final * vertices[j];
                    vertices[j] += f_mesh[i].bounds.center;
                }
                teeth.obj[i].GetComponent<MeshFilter>().mesh.vertices = vertices;

                // Set tooth parameters.
                teeth.param[i].SetV1Vec(f_teeth.param[i].GetV1());
                teeth.param[i].SetCenter(f_mesh[i].bounds.center, true);
                // Set mesh collider.
                teeth.obj[i].GetComponent<MeshCollider>().sharedMesh
                    = teeth.obj[i].GetComponent<MeshFilter>().mesh;
            }

            // Destroy final teeth's object.
            foreach(GameObject tmp_obj in f_obj.GetComponent<Teeth>().obj) {
                GameObject.Destroy(tmp_obj);
            }
            GameObject.Destroy(f_obj);
        }

        public void TranslateVx(uint t_id, uint v_id, float speed) {
            Mesh mesh;
            Vector3[] vertices;
            Vector3 v_mov = new Vector3();

            switch (v_id) {
                case (uint)Controller.AXIS.v1:
                    v_mov = teeth.param[t_id].GetV1();
                    break;
                case (uint)Controller.AXIS.v2:
                    v_mov = teeth.param[t_id].GetV2();
                    break;
                case (uint)Controller.AXIS.v3:
                    v_mov = teeth.param[t_id].GetV3();
                    break;
                default:
                    return;
            }

            mesh = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < mesh.vertexCount; i++) {
                vertices[i] += speed * v_mov;
            }
            teeth.obj[t_id].GetComponent<MeshFilter>().mesh.vertices = vertices;

            // Update center.
            teeth.param[t_id].SetCenter(teeth.param[t_id].GetCenter() + speed * v_mov, false);
            // Update collider.
            teeth.obj[t_id].GetComponent<MeshCollider>().sharedMesh
                = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
        }

        public void RotateVx(uint t_id, uint v_id, float degree) {
            Mesh mesh;
            Vector3[] vertices;
            Vector3 axis = new Vector3();

            switch (v_id) {
                case (uint)Controller.AXIS.v1:
                    axis = teeth.param[t_id].GetV1();
                    break;
                case (uint)Controller.AXIS.v2:
                    axis = teeth.param[t_id].GetV2();
                    break;
                default:
                    return;
            }

            mesh = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < mesh.vertexCount; i++) {
                vertices[i] -= teeth.param[t_id].GetCenter();     // Move to origin.
                vertices[i] = Quaternion.AngleAxis(degree, axis) * vertices[i]; // Rotate.
                vertices[i] += teeth.param[t_id].GetCenter();     // Move back.
            }
            teeth.obj[t_id].GetComponent<MeshFilter>().mesh.vertices = vertices;

            // Update v.
            if (v_id == 1)
                teeth.param[t_id].SetV2Vec(Quaternion.AngleAxis(degree, axis) * teeth.param[t_id].GetV2());
            else
                teeth.param[t_id].SetV1Vec(Quaternion.AngleAxis(degree, axis) * teeth.param[t_id].GetV1());
            // Update collider.
            teeth.obj[t_id].GetComponent<MeshCollider>().sharedMesh
                = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
        }

        public void RotateBox(uint t_id, uint side, float degree) {
            Mesh mesh;
            Vector3[] vertices;
            Vector3 axis = new Vector3();
            Vector3 point = new Vector3();
            Vector3 center = teeth.param[t_id].GetCenter();
            Vector3 v1_point = center + teeth.param[t_id].GetV1();
            Vector3 v2_point = center + teeth.param[t_id].GetV2();

            switch (side) {
                case (uint)Controller.AXIS.up:
                    axis = teeth.param[t_id].GetV3();
                    point = center + (teeth.param[t_id].up / teeth.param[t_id].GetV1().magnitude) * teeth.param[t_id].GetV1();
                    break;
                case (uint)Controller.AXIS.down:
                    axis = teeth.param[t_id].GetV3();
                    point = center + (teeth.param[t_id].down / teeth.param[t_id].GetV1().magnitude) * teeth.param[t_id].GetV1();
                    break;
                case (uint)Controller.AXIS.left:
                    axis = teeth.param[t_id].GetV1();
                    point = center + (teeth.param[t_id].left / teeth.param[t_id].GetV3().magnitude) * teeth.param[t_id].GetV3();
                    break;
                case (uint)Controller.AXIS.right:
                    axis = teeth.param[t_id].GetV1();
                    point = center + (teeth.param[t_id].right / teeth.param[t_id].GetV3().magnitude) * teeth.param[t_id].GetV3();
                    break;
                default:
                    return;
            }

            mesh = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < mesh.vertexCount; i++) {
                vertices[i] -= point;     // Move to origin.
                vertices[i] = Quaternion.AngleAxis(degree, axis) * vertices[i]; // Rotate.
                vertices[i] += point;     // Move back.
            }
            teeth.obj[t_id].GetComponent<MeshFilter>().mesh.vertices = vertices;

            // Update center.
            center = Quaternion.AngleAxis(degree, axis) * (center - point) + point;
            teeth.param[t_id].SetCenter(
                center,
                false
            );
            // Update v.
            v1_point = Quaternion.AngleAxis(degree, axis) * (v1_point - point) + point;
            v2_point = Quaternion.AngleAxis(degree, axis) * (v2_point - point) + point;
            teeth.param[t_id].SetV1Vec(v1_point - center);
            teeth.param[t_id].SetV2Vec(v2_point - center);
            // Update collider.
            teeth.obj[t_id].GetComponent<MeshCollider>().sharedMesh
                = teeth.obj[t_id].GetComponent<MeshFilter>().mesh;
        }
    }
}
