﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Controller : MonoBehaviour
{
    // Teeth data path.
    public string src_dir_path;
    public string final_dir_path;

    /* VR input.*/
    // Inputs
    public SteamVR_Input_Sources[] VR_hand;
    public SteamVR_Behaviour_Pose[] VR_pose;
    // Actions
    public SteamVR_Action_Boolean trigger_action;
    public SteamVR_Action_Boolean trackpad_action;
    public SteamVR_Action_Boolean grip_action;
    public bool[] is_trigger_click;
    public bool[] is_trigger_press;
    public bool[] is_trackpad_click;
    public bool[] is_trackpad_press;
    public bool[] is_grip_click;
    // Movements
    public Vector3 left_position = Vector3.zero;
    public Vector3 right_position = Vector3.zero;
    // Encode LEFT, RIGHT
    public static readonly int LEFT = 0, RIGHT = 1;

    // Laser object.
    public GameObject laser;

    private Teeth teeth;
    private readonly ImportSTL import = new ImportSTL();
    // Tooth transform: translation, rotation and transform to the final position and rotation.
    private readonly Tooth.ToothTransform tooth_transform = new Tooth.ToothTransform();
    // For debugging.
    private readonly ToothDebug.BoundingBox bound_box = new ToothDebug.BoundingBox();
    private readonly ToothDebug.DrawVectors debug_vec = new ToothDebug.DrawVectors();
    // For 2D fallback user events.
    private float mouse_start_t = 0;
    private const float mouse_click_threshold = 0.3f;   // Threshold for checking are we
                                                        // clicking mouse or hold it.

    // We need pre_pos to trace the movement of the mouse for 2D fallback and the track of the
    // controller for the VR device.
    private Vector3 pre_pos;

    // Use mouse to select tooth and store its id here.
    // -1 means that the user doesn't select any tooth.
    private int selected_tooth_id = -1; 
    // Trnaslation and rotation with respect to now_axis.
    // non, v1, v2, v3, up, down, left, right. Default is non.
    private uint now_axis = 0;
    private const uint AXIS_NUM = 8;    // We have 8 axes in total (include non).
    public enum AXIS { non, v1, v2, v3, up, down, left, right }

    // Only for 2D fallback debug.
    public bool is_pure_2d_debug;

    // Start is called before the first frame update
    void Start()
    {
        src_dir_path = Application.dataPath + src_dir_path;
        final_dir_path = Application.dataPath + final_dir_path;
        teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        // Import teeth.
        import.ImportInit();
        // Transform teeth to correct position.
        tooth_transform.Init();
        tooth_transform.SetCorrectPosition();
        // Initialize debug classes.
        bound_box.Init();
        debug_vec.Init();
    }

    // Update is called once per frame
    void Update() {
        /***********************************************************
         * Debug message
         **********************************************************/
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            // Draw bounding box.
            //bound_box.DrawBoundingBox((uint)i);   // Bounding box according to mesh.bounds
            bound_box.DrawBox((uint)i); // Bounding box according to up, down, left and right.

            // Draw v1, v2 of each tooth.
            debug_vec.DrawV1((uint)i);
            debug_vec.DrawV2((uint)i);
        }
        /***********************************************************
         * End Debug message
         **********************************************************/

        /***********************************************************
         * 2D Fallback event handler
         **********************************************************/
        if (SteamVR.active) {
            UpdateVRInput();
        }
        else {
            if (is_pure_2d_debug) {
                KeyboardEvent();
                MouseEvent();
            }
            else {
                // Left
                is_trigger_click[LEFT] = Input.GetMouseButtonDown(0);
                is_trackpad_press[LEFT] = Input.GetKey("f");
                is_grip_click[LEFT] = Input.GetKeyDown("z");
                // Right
                is_trigger_press[RIGHT] = Input.GetKey("1");
                is_trackpad_click[RIGHT] = Input.GetKeyDown("j");
                is_grip_click[RIGHT] = Input.GetKeyDown("x");
            }
        }
        /***********************************************************
         * End 2D Fallback event hanedler
         **********************************************************/

        if (!is_pure_2d_debug) {
            if (is_trackpad_press[LEFT]) {
                if (SteamVR.active) laser.SetActive(true);
                SelectTooth();
            }
            else {
                laser.SetActive(false);
                SwitchTooth();
                SwitchAction();
                ToothAction();
            }
        }
    }

    void UpdateVRInput() {
        // Actions
        for (int i = 0; i < 2; i++) {
            is_trigger_click[i] = trigger_action.GetStateDown(VR_hand[i]);
            is_trigger_press[i] = trigger_action.GetState(VR_hand[i]);
            is_trackpad_click[i] = trackpad_action.GetStateDown(VR_hand[i]);
            is_trackpad_press[i] = trackpad_action.GetState(VR_hand[i]);
            is_grip_click[i] = grip_action.GetStateDown(VR_hand[i]);
        }
        // Movements
        left_position = VR_pose[LEFT].transform.position;
        right_position = VR_pose[RIGHT].transform.position;
    }

    void SelectTooth() {
        if (is_trigger_click[LEFT] && is_trackpad_press[LEFT]) {
            RaycastHit hit = new RaycastHit();

            // Do we select a tooth?
            if (SteamVR.active) {
                if (Physics.Raycast(
                        VR_pose[LEFT].transform.position,
                        VR_pose[LEFT].transform.forward,
                        out hit))
                {
                    pre_pos = VR_pose[LEFT].transform.position;
                    selected_tooth_id = ToothNameToID(hit.transform.name);
                    now_axis = (uint)AXIS.non;
                }
                else selected_tooth_id = -1;
            }
            else {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                    mouse_start_t = Time.time;
                    pre_pos = Input.mousePosition;
                    selected_tooth_id = ToothNameToID(hit.transform.name);
                    now_axis = (uint)AXIS.non;
                }
                else selected_tooth_id = -1;
            }
        }
    }

    void SwitchAction() {
        if (is_trackpad_click[RIGHT]) {
            now_axis = (now_axis + 1) % AXIS_NUM;
        }
    }

    void SwitchTooth() {
        if (!IsValidToothID()) return;
        int tmp_id = selected_tooth_id;
       
        if (is_grip_click[LEFT]) {
            do {
                if (tmp_id >= 16) selected_tooth_id--;
                else selected_tooth_id++;
                if (selected_tooth_id < 0) selected_tooth_id += (int)Teeth.TOOTH_NUM;
                selected_tooth_id %= (int)Teeth.TOOTH_NUM;
            }
            while (!IsValidToothID());
            now_axis = (uint)AXIS.non;
        }
        else if (is_grip_click[RIGHT]) {
            do {
                if (tmp_id >= 16) selected_tooth_id++;
                else selected_tooth_id--;
                if (selected_tooth_id < 0) selected_tooth_id += (int)Teeth.TOOTH_NUM;
                selected_tooth_id %= (int)Teeth.TOOTH_NUM;
            }
            while (!IsValidToothID());
            now_axis = (uint)AXIS.non;
        }        
    }

    void ToothAction() {
        // Set now position.
        Vector3 now_pos;
        float hack = 1f;

        if (SteamVR.active) hack = 200f;
        if (SteamVR.active) now_pos = VR_pose[RIGHT].transform.localPosition;
        else now_pos = Input.mousePosition;

        // Translate the tooth while holding the left button.
        if (!is_trigger_press[RIGHT]) {
            if (IsValidAxis("translation") && IsValidToothID()) {
                tooth_transform.TranslateVx(
                    (uint)selected_tooth_id,
                    now_axis,
                    ActionSpeeed(now_pos, now_axis) * hack
                );
            }
        }
        // Rotate the tooth while holding the right button.
        else {
            if (IsValidAxis("rotation") && IsValidToothID()) {
                if (now_axis <= 2) {
                    tooth_transform.RotateVx(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(now_pos, now_axis) * hack
                    );
                }
                else {
                    tooth_transform.RotateBox(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(now_pos, now_axis) * hack
                    );
                }
            }
        }

        // Update position.
        if (SteamVR.active) pre_pos = now_pos;
        else pre_pos = now_pos;
    }

    // Only for 2D fallback.
    void KeyboardEvent() {
        // Choose axis.
        if (Input.GetKey("1")) {
            now_axis = (uint)AXIS.v1;
        }
        else if (Input.GetKey("2")) {
            now_axis = (uint)AXIS.v2;
        }
        else if (Input.GetKey("3")) {
            now_axis = (uint)AXIS.v3;
        }
        else if (Input.GetKey("w")) {
            now_axis = (uint)AXIS.up;
        }
        else if (Input.GetKey("s")) {
            now_axis = (uint)AXIS.down;
        }
        else if (Input.GetKey("a")) {
            if (selected_tooth_id < 16) now_axis = (uint)AXIS.left;
            else now_axis = (uint)AXIS.right;
        }
        else if (Input.GetKey("d")) {
            if (selected_tooth_id < 16) now_axis = (int)AXIS.right;
            else now_axis = (uint)AXIS.left;
        }
        else {
            now_axis = (int)AXIS.non;
        }

        // Choose tooth.
        if (Input.GetKey("left")) {
            while (!IsValidToothID())
                selected_tooth_id = (selected_tooth_id - 1) % (int)Teeth.TOOTH_NUM;
        }
        else if (Input.GetKey("right")) {
            while (!IsValidToothID())
                selected_tooth_id = (selected_tooth_id + 1) % (int)Teeth.TOOTH_NUM;
        }
    }

    // Only for 2D fallback.
    void MouseEvent() {
        // Select tooth.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            RaycastHit hit = new RaycastHit();

            // Do we select a tooth?
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                mouse_start_t = Time.time;
                pre_pos = Input.mousePosition;
                selected_tooth_id = ToothNameToID(hit.transform.name);
            }
            else selected_tooth_id = -1;
        }

        // Translate the tooth while holding the left button.
        if (Input.GetMouseButton(0)) {
            if (IsValidAxis("translation") && IsValidToothID()) {
                if (now_axis == (int)AXIS.non) {
                    // Choose v1 by default.
                    tooth_transform.TranslateVx(
                        (uint)selected_tooth_id,
                        (uint)AXIS.v1,
                        ActionSpeeed(Input.mousePosition, (uint)AXIS.v1)
                    );
                }
                else {
                    tooth_transform.TranslateVx(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionSpeeed(Input.mousePosition, now_axis)
                    );
                }
            }

            // Update mouse position.
            pre_pos = Input.mousePosition;
        }
        // Rotate the tooth while holding the right button.
        else if (Input.GetMouseButton(1)) {
            if (IsValidAxis("rotation") && IsValidToothID()) {
                if (now_axis <= 2) {
                    tooth_transform.RotateVx(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(Input.mousePosition, now_axis)
                    );
                }
                else {
                    tooth_transform.RotateBox(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(Input.mousePosition, now_axis)
                    );
                }
            }

            // Update mouse position.
            pre_pos = Input.mousePosition;
        }
    }

    int ToothNameToID(string t_name) {
        if (t_name.Length > 9) {
            return int.Parse(string.Concat(t_name[7], t_name[8]));
        }
        else {
            return int.Parse(t_name[7].ToString());
        }
    }

    bool IsValidAxis(string action) {
        switch (action) {
            case "translation":
                return (now_axis > 0 && now_axis <= 3);
            case "rotation":
                return (now_axis > 0 && now_axis <= 2) || (now_axis > 3 && now_axis <= 7);
            default:
                return false;
        }
    }

    bool IsValidToothID() {
        return selected_tooth_id >= 0 &&
            teeth.obj[selected_tooth_id].GetComponent<MeshFilter>().mesh.vertexCount != 0;
    }

    float ActionSpeeed(Vector3 pos, uint axis) {
        float retval;

        switch (axis) {
            case (uint)AXIS.v1:
                retval = (pos.y - pre_pos.y) * Time.deltaTime;
                break;
            case (uint)AXIS.v2:
                if (SteamVR.active) retval = (pos.z - pre_pos.z) * Time.deltaTime;
                else retval = (pos.x - pre_pos.x) * Time.deltaTime;
                break;
            case (uint)AXIS.v3:
                retval = (pos.x - pre_pos.x) * Time.deltaTime;
                break;
            default:
                return 1.0f * Time.deltaTime;
        }

        if ((axis == (uint)AXIS.v1 || axis == (uint)AXIS.v3) && selected_tooth_id >= 16) {
            return -retval;
        }
        else return retval;
    }

    float ActionDegree(Vector3 pos, uint axis) {
        float retval;

        switch (axis) {
            case (uint)AXIS.up:
            case (uint)AXIS.down:
                retval = (pos.y - pre_pos.y) * Time.deltaTime;
                break;
            case (uint)AXIS.v1:
            case (uint)AXIS.v2:
            case (uint)AXIS.left:
            case (uint)AXIS.right:
                retval = -(pos.x - pre_pos.x) * Time.deltaTime;
                break;
            default:
                return 1.0f * Time.deltaTime;
        }

        if (selected_tooth_id >= 16) {
            return -retval*10;
        }
        return retval*10;
    }

    public uint GetNowAxis() { return now_axis; }
    public int GetNowSelectTooth() { return selected_tooth_id; }

    public static void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
