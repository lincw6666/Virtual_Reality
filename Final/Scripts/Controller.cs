using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Controller : MonoBehaviour
{
    // Teeth data path.
    public string src_dir_path;
    public string final_dir_path;
    // Transformation matrix file path.
    public string trans_mat_path;
    public string trans_pos_rot_path;
    public string trans_mat_human_readable_path;

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
    // Write transform matrix to file.
    private readonly FileIO wf_readable = new FileIO();     // For writing humane readable.
    private readonly FileIO f_trans_mat = new FileIO();     // For r/w pure transformation matrix.
    private readonly FileIO f_trans_pos_rot = new FileIO(); // For r/w position and rotation before and after transformation.
    public int now_step = 0;                // The step in the given video.
    private bool start_transform = false;   // Start transformation of each step.
    private bool end_transform = false;
    private int pre_decode_step = 0;
    private int now_decode_step = 0;       // The step we've decoded now.
    private float start_show_step_t = 0;    // Time we start to show a step.
    private const float show_step_time = 0.5f;  // Show each step for 0.5 sec.
    private Vector3[] decode_pos, decode_rot;
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
    private int pre_selected_tooth_id = -1;
    // Trnaslation and rotation with respect to now_axis.
    // non, v1, v2, v3, up, down, left, right. Default is non.
    private uint now_axis = 0;
    private const uint AXIS_NUM = 8;    // We have 8 axes in total (include non).
    public enum AXIS { non, v1, v2, v3, up, down, left, right }

    /* Public methods */
    public uint GetNowAxis() { return now_axis; }
    public int GetNowSelectTooth() { return selected_tooth_id; }
    public static void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

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
        if (SteamVR.active) tooth_transform.SetCorrectPosition();
        // Initivlize decoded pre position, position and rotation.
        decode_pos = new Vector3[Teeth.TOOTH_NUM];
        decode_rot = new Vector3[Teeth.TOOTH_NUM];
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
            // Left
            is_trigger_click[LEFT] = Input.GetMouseButtonDown(0);
            is_trackpad_press[LEFT] = Input.GetKey("f");
            is_grip_click[LEFT] = Input.GetKeyDown("z");
            // Right
            is_trigger_press[RIGHT] = Input.GetKey("1");
            is_trackpad_click[RIGHT] = Input.GetKeyDown("j");
            is_grip_click[RIGHT] = Input.GetKeyDown("x");

            if (start_transform) {
                if (now_step >= now_decode_step) {
                    if (end_transform) {
                        start_transform = false;
                        end_transform = false;
                        goto END;
                    }
                    else {
                        pre_decode_step = now_decode_step;
                        DecodePosRot();
                    }
                }
                if (Time.time - start_show_step_t > show_step_time) {
                    now_step++;
                    NowStepTransform();
                    start_show_step_t = Time.time;
                }
            END:;   // End transform.
            }
            else {
                // Update now_step.
                if (Input.GetKeyDown("0")) {
                    now_step++;
                }
                else if (Input.GetKeyDown("9")) {
                    now_step += 10;
                }
                // Write transformation matrix to file.
                if (Input.GetKeyDown("r")) {
                    WriteTransformMatrix();
                }
                // Moving to the final position.
                if (Input.GetKeyDown("t")) {
                    tooth_transform.SetCorrectPosition();
                    WriteTransformMatrix();
                }
                // Transform the tooth according to the file.
                if (Input.GetKeyDown("y")) {
                    start_transform = true;
                }
            }
        }
        /***********************************************************
         * End 2D Fallback event hanedler
         **********************************************************/

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

        // Change color on selected tooth.
        if (selected_tooth_id != pre_selected_tooth_id) {
            if (pre_selected_tooth_id >= 0) {
                teeth.obj[pre_selected_tooth_id].layer = 0;
            }
            teeth.obj[selected_tooth_id].layer = 5;
            pre_selected_tooth_id = selected_tooth_id;
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
        if (!IsValidToothID(selected_tooth_id)) return;
        int tmp_id = selected_tooth_id;
       
        if (is_grip_click[LEFT]) {
            do {
                if (tmp_id >= 16) selected_tooth_id--;
                else selected_tooth_id++;
                if (selected_tooth_id < 0) selected_tooth_id += (int)Teeth.TOOTH_NUM;
                selected_tooth_id %= (int)Teeth.TOOTH_NUM;
            }
            while (!IsValidToothID(selected_tooth_id));
            now_axis = (uint)AXIS.non;
        }
        else if (is_grip_click[RIGHT]) {
            do {
                if (tmp_id >= 16) selected_tooth_id++;
                else selected_tooth_id--;
                if (selected_tooth_id < 0) selected_tooth_id += (int)Teeth.TOOTH_NUM;
                selected_tooth_id %= (int)Teeth.TOOTH_NUM;
            }
            while (!IsValidToothID(selected_tooth_id));
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
            if (IsValidAxis("translation") && IsValidToothID(selected_tooth_id)
                 && (SteamVR.active || Input.GetKey("2")))
            {
                tooth_transform.TranslateVx(
                    (uint)selected_tooth_id,
                    now_axis,
                    ActionSpeeed(now_pos, now_axis) * hack
                );
            }
        }
        // Rotate the tooth while holding the right button.
        else {
            if (IsValidAxis("rotation") && IsValidToothID(selected_tooth_id)) {
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

    bool IsValidToothID(int t_id) {
        return t_id >= 0 && teeth.obj[t_id].GetComponent<MeshFilter>().mesh.vertexCount != 0;
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

    void WriteTransformMatrix() {
        string encode_mat = now_step.ToString();
        string pos_rot = now_step.ToString();       // Encoded previous and now position and rotation.

        wf_readable.Init(trans_mat_human_readable_path, false, false);
        f_trans_mat.Init(trans_mat_path, false, false);
        f_trans_pos_rot.Init(trans_pos_rot_path, false, false);
        wf_readable.WriteContent("********** Now step: " + now_step + " **********");
        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            if (!IsValidToothID(i)) continue;
            Vector3 now_R = tooth_transform.NowR((uint)i).eulerAngles;
            Matrix4x4 trans_mat = tooth_transform.GetTransformMatrix((uint)i);
            Vector3 tmp_pre_center = teeth.param[i].GetPreCenter();
            Vector3 tmp_now_center = teeth.param[i].GetCenter();

            // Write human readable info to file.
            wf_readable.WriteContent("ID: " + i);
            wf_readable.WriteContent("    Translation:");
            wf_readable.WriteContent("        From: ( " + tmp_pre_center + " )");
            wf_readable.WriteContent("        To  : ( " + tmp_now_center + " )");
            wf_readable.WriteContent("    Rotation: ( " + now_R.x + ", " + now_R.y + ", " + now_R.z + " )");
            // Write pure transformation matrix to file.
            encode_mat += " " + i;
            for (int j = 0; j < 4; j++) {
                encode_mat += " " + trans_mat[0, j] + " " + trans_mat[1, j] +
                    " " + trans_mat[2, j] + " " + trans_mat[3, j];
            }
            teeth.param[i].SetPre();
            // Write previous and now position and rotation to file.
            pos_rot += " " + i + " " + tmp_now_center.x + " " + tmp_now_center.y + " " +
                tmp_now_center.z + " " + now_R.x + " " + now_R.y + " " + now_R.z;
        }
        wf_readable.WriteContent("");
        f_trans_mat.WriteContent(encode_mat);
        f_trans_pos_rot.WriteContent(pos_rot);
    }

    /* Decode the previous and now position and rotation which are encoded in the file. */
    void DecodePosRot() {
        string[] decode_pos_rot;

        // Close the opened file for writing previous and now position and rotation.
        f_trans_pos_rot.Close();

        /********************************************************************************/
        /*************** Start Decode Pre_Position, Position and Rotation ***************/
        f_trans_pos_rot.Init(trans_pos_rot_path, true, true);
        // Read the transformation matrix.
        // Skip lines we have already read.
        for (int i = 0; i < f_trans_pos_rot.now_read_line; i++)
            f_trans_pos_rot.ReadContent();
        // Parse the encoded matrix.
        string encode_pos_rot = f_trans_pos_rot.ReadContent();
        // Check if we reach the end of file.
        if (encode_pos_rot == null) {
            Debug.Log("Read end of file.");     // No more transformation.
            end_transform = true;
            goto EOF;
        }
        decode_pos_rot = encode_pos_rot.Split(' ');
        now_decode_step = int.Parse(decode_pos_rot[0]);
        for (int i = 1, now_tid = 0; i < decode_pos_rot.Length; i += 7, now_tid++) {
            //Debug.Log(decode_pos_rot[i] + " " + decode_pos_rot[i + 1] + " " +
            //    decode_pos_rot[i + 2] + " " + decode_pos_rot[i + 3] + " " + decode_pos_rot[i + 4] +
            //    " " + decode_pos_rot[i + 5] + " " + decode_pos_rot[i + 6]);
            int t_id = int.Parse(decode_pos_rot[i]);

            // Set skip t_id to identity matrix.
            while (now_tid < t_id) {
                decode_pos[now_tid] = Vector3.zero;
                decode_rot[now_tid++] = Vector3.zero;
            }
            // Parse position after transform.
            decode_pos[now_tid] = new Vector3(
                float.Parse(decode_pos_rot[i + 1]),
                float.Parse(decode_pos_rot[i + 2]),
                float.Parse(decode_pos_rot[i + 3])
            );
            // Parse rotation.
            decode_rot[now_tid] = new Vector3(
                float.Parse(decode_pos_rot[i + 4]),
                float.Parse(decode_pos_rot[i + 5]),
                float.Parse(decode_pos_rot[i + 6])
            );
            //Debug.Log("ID: " + now_tid);
            //Debug.Log(decode_pos[now_tid] + "\n" + decode_rot[now_tid]);
        }
        // Update where we have read in the file.
        f_trans_pos_rot.now_read_line++;
    EOF:
        f_trans_pos_rot.Close();
        /***************  End Decode Pre_Position, Position and Rotation  ***************/
        /********************************************************************************/

        // Reopen the file for writing transformation matrix.
        f_trans_pos_rot.Init(trans_pos_rot_path, false, true);
    }

    void NowStepTransform() {
        if (now_step <= 0) return;
        // Interpolate weight.
        float base_w = (float)(now_decode_step - now_step +1);
        float pre_w = (float)(now_decode_step - now_step) / base_w;
        float now_w = 1f / base_w;
        float rot_w = 1f / (float)(now_decode_step - pre_decode_step);
        // Interpolate position and rotation.
        Vector3 inter_pos, inter_rot;
        // Transformation matrix build by inter_pos and inter_rot.
        Matrix4x4[] trans_mat = new Matrix4x4[Teeth.TOOTH_NUM];

        for (int i = 0; i < Teeth.TOOTH_NUM; i++) {
            // Skip missing tooth.
            if (decode_pos[i] == Vector3.zero && decode_rot[i] == Vector3.zero) continue;

            // Set interpolation position and rotation.
            inter_pos = teeth.param[i].GetCenter() * pre_w + decode_pos[i] * now_w;
            inter_rot = decode_rot[i];
            if (inter_rot.x >= 180) inter_rot.x -= 360;
            if (inter_rot.y >= 180) inter_rot.y -= 360;
            if (inter_rot.z >= 180) inter_rot.z -= 360;
            inter_rot *= rot_w;
            // Build transformation matrix
            trans_mat[i] = Matrix4x4.Translate(inter_pos) *
                Matrix4x4.Rotate(Quaternion.Euler(inter_rot)) *
                Matrix4x4.Translate(-teeth.param[i].GetCenter());
        }
        // Transform tooth.
        tooth_transform.Transform(trans_mat);
    }
}
