using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Tooth
{
    public class ToothParam
    {
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
            center = new Vector3();
        }

        public ToothParam(Mesh obj_mesh, Vector3 lingual_pos, int id) {
            SetToothParam(obj_mesh, lingual_pos, id);
        }

        /*************************************************
         * End Constructors
         ************************************************/

        /*************************************************
         * Access private datas
         ************************************************/

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

        public void SetToothParam(Mesh obj_mesh, Vector3 i_lingual_pos, int id) {
            SetV1(obj_mesh.normals, id);
            SetCenter(obj_mesh.bounds.center);
            SetV2(i_lingual_pos);
        }
        
        private void SetV1(Vector3 [] normal, int id) {
            uint sample_num = (uint) normal.Length / 500;

            if (normal.Length == 0) return ;

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
                    if (Vector3.Dot(normal[now_id].normalized, y) > 0.96f) {
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
                if (Vector3.Dot(normal[i].normalized, rand_normal[max_id]) > 0.8f)
                    v1 += normal[i];
            }
            v1 = v1.normalized;
        }

        public void SetV2(Vector3 i_lingual_pos) {
            lingual_pos = i_lingual_pos;
            v2 = Vector3.Normalize(center - lingual_pos);
            SetV3();
        }

        private void SetV3() {
            v3 = Vector3.Cross(v1, v2).normalized;
        }

        private void SetCenter(Vector3 i_center) {
            center = i_center;
        }

        /*************************************************
         * End Build tooth vectors
         ************************************************/
    }
}
