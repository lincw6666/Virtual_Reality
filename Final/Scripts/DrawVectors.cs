using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToothDebug
{
    public class DrawVectors
    {
        private Teeth teeth;

        public void Init() {
            teeth = GameObject.Find("/Tooth").GetComponent<Teeth>();
        }

        public void DrawV1(uint id) {
            Transform transform = teeth.obj[id].GetComponent<Transform>();
            Vector3 world_center = transform.TransformPoint(teeth.param[id].GetCenter());
            Vector3 world_v1 = transform.TransformPoint(teeth.param[id].GetCenter() + teeth.param[id].GetV1() * 20.0f);
            Debug.DrawLine(world_center, world_v1, Color.green);
        }

        public void DrawV2(uint id) {
            Transform transform = teeth.obj[id].GetComponent<Transform>();
            Vector3 world_lingual = transform.TransformPoint(teeth.param[id].GetLingualPos());
            Vector3 world_v2 = transform.TransformPoint(teeth.param[id].GetLingualPos() + teeth.param[id].GetV2() * 30.0f);
            Debug.DrawLine(world_lingual, world_v2, Color.red);
        }
    }
}
