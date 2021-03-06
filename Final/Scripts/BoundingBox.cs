﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* All classes about mesh operations. */
namespace ToothDebug
{
    public class BoundingBox
    {
        private Color color = Color.green;
        private Vector3 v3FrontTopLeft;
        private Vector3 v3FrontTopRight;
        private Vector3 v3FrontBottomLeft;
        private Vector3 v3FrontBottomRight;
        private Vector3 v3BackTopLeft;
        private Vector3 v3BackTopRight;
        private Vector3 v3BackBottomLeft;
        private Vector3 v3BackBottomRight;

        private Teeth teeth;

        public void Init() {
            teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        }

        public void SetBoxColor(Color i_color) {
            color = i_color;
        }

        public void DrawBoundingBox(uint id) {
            CalcPositons(id);
            DrawBox();
        }

        private void CalcPositons(uint id) {
            Transform transform = teeth.obj[id].GetComponent<Transform>();
            Bounds bounds = teeth.obj[id].GetComponent<MeshFilter>().mesh.bounds;
            Vector3 v3Center = bounds.center;
            Vector3 v3Extents = bounds.extents;

            v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
            v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
            v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
            v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
            v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
            v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
            v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
            v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

            v3FrontTopLeft = transform.TransformPoint(v3FrontTopLeft);
            v3FrontTopRight = transform.TransformPoint(v3FrontTopRight);
            v3FrontBottomLeft = transform.TransformPoint(v3FrontBottomLeft);
            v3FrontBottomRight = transform.TransformPoint(v3FrontBottomRight);
            v3BackTopLeft = transform.TransformPoint(v3BackTopLeft);
            v3BackTopRight = transform.TransformPoint(v3BackTopRight);
            v3BackBottomLeft = transform.TransformPoint(v3BackBottomLeft);
            v3BackBottomRight = transform.TransformPoint(v3BackBottomRight);
        }

        private void DrawBox() {
            //if (Input.GetKey (KeyCode.S)) {
            Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, color);
            Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, color);
            Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, color);
            Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, color);

            Debug.DrawLine(v3BackTopLeft, v3BackTopRight, color);
            Debug.DrawLine(v3BackTopRight, v3BackBottomRight, color);
            Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, color);
            Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, color);

            Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, color);
            Debug.DrawLine(v3FrontTopRight, v3BackTopRight, color);
            Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, color);
            Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, color);
            //}
        }

        public void DrawBox(uint id) {
            Transform transform = teeth.obj[id].GetComponent<Transform>();
            Vector3 center = teeth.param[id].GetCenter();
            Vector3 v1 = teeth.param[id].GetV1();
            Vector3 v3 = teeth.param[id].GetV3();
            /*
            float w = teeth.param[id].width / 2.0f / v3.magnitude;
            float h = teeth.param[id].height / 2.0f / v1.magnitude;
            Vector3 world_left_up = transform.TransformPoint(center - w * v3 + h * v1);
            Vector3 world_right_up = transform.TransformPoint(center + w * v3 + h * v1);
            Vector3 world_left_down = transform.TransformPoint(center - w * v3 - h * v1);
            Vector3 world_right_down = transform.TransformPoint(center + w * v3 - h * v1);
            */
            float u = teeth.param[id].up / v1.magnitude;
            float d = teeth.param[id].down / v1.magnitude;
            float l = teeth.param[id].left / v3.magnitude;
            float r = teeth.param[id].right / v3.magnitude;
            Vector3 world_left_up = transform.TransformPoint(center + l * v3 + u * v1);
            Vector3 world_right_up = transform.TransformPoint(center + r * v3 + u * v1);
            Vector3 world_left_down = transform.TransformPoint(center + l * v3 + d * v1);
            Vector3 world_right_down = transform.TransformPoint(center + r * v3 + d * v1);

            Debug.DrawLine(world_left_up, world_right_up, Color.blue);
            Debug.DrawLine(world_left_up, world_left_down, Color.blue);
            Debug.DrawLine(world_right_up, world_right_down, Color.blue);
            Debug.DrawLine(world_left_down, world_right_down, Color.blue);
        }
    }
}
