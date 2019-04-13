using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Tooth
{
    public class ToothParam
    {
        public float width, height;
        /*
         * The following 3 vectores are for rotating and translating the tooth.
         * v1: point from root to top of tooth.
         * v2: point from lingual side to lip side.
         * v3: point at the direction of cross product of v1 and v2.
         */
        private Vector3 v1, v2, v3;

        private Vector3 center; // Tooth's center in local space.
        private Vector3 lingual_pos;

        /*************************************************
         * Constructors
         ************************************************/

        public ToothParam() {
            v1 = new Vector3();
            v2 = new Vector3();
            v3 = new Vector3();
            width = 0.0f; height = 0.0f;
            center = new Vector3();
        }

        public ToothParam(Mesh obj_mesh, Vector3 lingual_pos, int id) {
            width = 0.0f; height = 0.0f;
            SetToothParam(obj_mesh.normals, obj_mesh.bounds.center, lingual_pos, id);
        }

        /*************************************************
         * End Constructors
         ************************************************/

        /*************************************************
         * Access private datas
         ************************************************/

        public void SetV1Vec(Vector3 v) { v1 = v; SetV3(); }
        public void SetV2Vec(Vector3 v) { v2 = v; SetV3(); }
        public void SetCenter(Vector3 pos, bool is_set_v2) {
            center = pos;
            if (is_set_v2) SetV2();
        }
        public void SetLingualPos(Vector3 pos) { lingual_pos = pos; SetV2(); }

        public Vector3 GetV1() { return v1; }
        public Vector3 GetV2() { return v2; }
        public Vector3 GetV3() { return v3; }
        public Vector3 GetVx(uint id) {
            switch (id) {
                case 1:
                    return v1;
                case 2:
                    return v2;
                case 3:
                    return v3;
                default:
                    return new Vector3();
            }
        }
        public Vector3 GetCenter() { return center; }
        public Vector3 GetLingualPos() { return lingual_pos; }

        /*************************************************
         * End Access private datas
         ************************************************/

        /*************************************************
         * Build tooth vectors
         ************************************************/

        public void SetToothParam(Vector3[] obj_normals, Vector3 i_center, Vector3 i_lingual_pos, int id) {
            SetV1(obj_normals, id);
            SetCenter(i_center, true);
            SetLingualPos(i_lingual_pos);
        }
        
        private void SetV1(Vector3 [] normal, int id) {
            if (normal.Length == 0) return;

            uint sample_num = (uint) normal.Length / 500;
            Vector3 [] rand_normal = new Vector3[sample_num];
            uint [] cnt = new uint[sample_num];
            Vector3 y;
            uint now_id = 0, max = 0, max_id = 0;

            if (id < 16) y = new Vector3(0, 1, 0);
            else y = new Vector3(0, -1, 0);

            // Choose "sample_num" number of normal randomly.
            for (int i = 0; i < sample_num; i++) {
                cnt[i] = 0;
                while (true) {
                    if (now_id >= normal.Length) {
                        sample_num = (uint) i;
                        break;
                    }
                    // Choose normals which are orthogonal to the xz plane.
                    if (Vector3.Dot(normal[now_id].normalized, y) > 0.94f) {
                        rand_normal[i] = normal[now_id++].normalized;
                        break;
                    }
                    ++now_id;
                }
            }

            // Count the number of normals that are familier with rand_normal.
            for (int i = 0; i < normal.Length; i++) {
                Vector3 norm_normal = normal[i].normalized;

                for (int j = 0; j < sample_num; j++) {
                    if (Vector3.Dot(norm_normal, rand_normal[j]) > 0.8f)
                        ++cnt[j];
                }
            }

            // The rand_normal with maximum cnt will be v1.
            for (int i = 0; i < sample_num; i++) {
                if (cnt[i] > max) {
                    max = cnt[i];
                    max_id = (uint) i;
                }
            }

            // Choose the average of the normals which are similar to rand_normal[max_id].
            v1 = new Vector3();
            for (int i = 0; i < normal.Length; i++) {
                if (Vector3.Dot(normal[i].normalized, rand_normal[max_id]) > 0.7f)
                    v1 += normal[i];
            }
            v1 = v1.normalized;
        }

        private void SetV2() {
            v2 = Vector3.Normalize(center - lingual_pos);
            SetV3();
        }

        private void SetV3() {
            v3 = Vector3.Cross(v1, v2).normalized;
        }

        /*************************************************
         * End Build tooth vectors
         ************************************************/
    }
}
