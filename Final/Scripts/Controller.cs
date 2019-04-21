using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Teeth datas' path.
    public string src_dir_path;
    public string final_dir_path;

    private Teeth teeth;
    private readonly ImportSTL import = new ImportSTL();
    private readonly Tooth.ToothTransform tooth_transform = new Tooth.ToothTransform();
    // For debugging.
    private readonly ToothDebug.BoundingBox bound_box = new ToothDebug.BoundingBox();
    private readonly ToothDebug.DrawVectors debug_vec = new ToothDebug.DrawVectors();
    // For user events.
    private float mouse_start_t = 0;
    private Vector3 mouse_pos;
    private int selected_tooth_id = -1; // Use mouse to select tooth and store its id here.
                                        // -1 means that the user doesn't select any tooth.
    // Trnaslation and rotation with respect to now_axis.
    private uint now_axis = 0;   // v1, v2, v3, up, down, left, right. Default is v1.
    public enum AXIS { non, v1, v2, v3, up, down, left, right }
    private const float mouse_click_threshold = 0.3f;   // Threshold for checking are we
                                                        // clicking mouse or hold it.

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
            //bound_box.DrawBoundingBox((uint)i);
            bound_box.DrawBox((uint)i);

            // Draw v1, v2 of each tooth.
            debug_vec.DrawV1((uint)i);
            debug_vec.DrawV2((uint)i);
        }
        /***********************************************************
         * End Debug message
         **********************************************************/

        /***********************************************************
         * Keyboard Event
         **********************************************************/

        KeyboardEvent();

        /***********************************************************
         * End Keyboard Event
         **********************************************************/

        /***********************************************************
         * Mouse Event
         **********************************************************/

        // Select tooth.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            RaycastHit hit = new RaycastHit();

            // Do we select a tooth?
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                mouse_start_t = Time.time;
                mouse_pos = Input.mousePosition;
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
                        ActionSpeeed((uint)AXIS.v1)
                    );
                }
                else {
                    tooth_transform.TranslateVx(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionSpeeed(now_axis)
                    );
                }
            }

            // Update mouse position.
            mouse_pos = Input.mousePosition;
        }
        // Rotate the tooth while holding the right button.
        else if (Input.GetMouseButton(1)) {
            if (IsValidAxis("rotation") && IsValidToothID()) {
                if (now_axis <= 2) {
                    tooth_transform.RotateVx(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(now_axis)
                    );
                }
                else {
                    tooth_transform.RotateBox(
                        (uint)selected_tooth_id,
                        now_axis,
                        ActionDegree(now_axis)
                    );
                }
            }

            // Update mouse position.
            mouse_pos = Input.mousePosition;
        }

        /***********************************************************
         * End Mouse Event
         **********************************************************/
    }

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
                return now_axis <= 3;
            case "rotation":
                return (now_axis > 0 && now_axis <= 2) || (now_axis > 3 && now_axis <= 7);
            default:
                return false;
        }
    }

    bool IsValidToothID() {
        return selected_tooth_id >= 0 && teeth.obj[selected_tooth_id].GetComponent<MeshFilter>().mesh;
    }

    float ActionSpeeed(uint axis) {
        float retval;

        switch (axis) {
            case (uint)AXIS.v1:
                retval = (Input.mousePosition.y - mouse_pos.y) * Time.deltaTime;
                break;
            case (uint)AXIS.v2:
            case (uint)AXIS.v3:
                retval = (Input.mousePosition.x - mouse_pos.x) * Time.deltaTime;
                break;
            default:
                return 1.0f * Time.deltaTime;
        }

        if ((axis == (uint)AXIS.v1 || axis == (uint)AXIS.v3) && selected_tooth_id >= 16) {
            return -retval;
        }
        else return retval;
    }

    float ActionDegree(uint axis) {
        float retval;

        switch (axis) {
            case (uint)AXIS.up:
            case (uint)AXIS.down:
                retval = (Input.mousePosition.y - mouse_pos.y) * Time.deltaTime;
                break;
            case (uint)AXIS.v1:
            case (uint)AXIS.v2:
            case (uint)AXIS.left:
            case (uint)AXIS.right:
                retval = -(Input.mousePosition.x - mouse_pos.x) * Time.deltaTime;
                break;
            default:
                return 1.0f * Time.deltaTime;
        }

        if (selected_tooth_id >= 16) {
            return -retval*10;
        }
        return retval*10;
    }

    public void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
