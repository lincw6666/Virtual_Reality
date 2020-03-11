using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneFire
{
    private static float l_start_t = 0;
    private static float r_start_t = 0;
    private const float fire_interval = 0.1f;
    private const float laser_len = 20f;
    private const float laser_thickness = 0.25f;

    public static void Fire(float time, bool is_left, Vector3 position, Quaternion rotation) {
        if (time - GetStartTime(is_left) > fire_interval) {
            CreateMissle(position, rotation);
            SetStartTime(time, is_left);
        }
    }

    private static void SetStartTime(float time, bool is_left) {
        if (is_left) l_start_t = time;
        else r_start_t = time;
    }

    private static float GetStartTime(bool is_left) {
        return is_left ? l_start_t : r_start_t;
    }

    private static void CreateMissle(Vector3 position, Quaternion rotation) {
        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tmp.name = "fire";
        tmp.tag = "laser";
        tmp.GetComponent<BoxCollider>().isTrigger = true;
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", Color.red);
        tmp.GetComponent<MeshRenderer>().material = newMaterial;
        tmp.transform.localScale = new Vector3(laser_thickness, laser_thickness, laser_len);
        tmp.transform.position = position; //transform.TransformPoint(pos);
        tmp.transform.rotation = rotation; //transform.rotation;
        Rigidbody tmp_rigid = tmp.AddComponent<Rigidbody>();
        tmp_rigid.velocity = tmp.transform.forward * 1000f;
        tmp_rigid.useGravity = false;
    }
}