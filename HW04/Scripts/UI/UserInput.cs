using UnityEngine;
using System.Collections;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class UserInput : MonoBehaviour
{
    /* VR sources */
    public SteamVR_Action_Boolean trigger;
    public SteamVR_Action_Boolean trackpad;
    public SteamVR_Action_Boolean grip;
    public const int DEV_NUM = 2;  // Number of devices.
    public enum HAND_ID { Non, Left, Right }
    public SteamVR_Input_Sources[] vr_src;
    public SteamVR_Behaviour_Pose[] hand_pose;

    /* User inputs */
    private bool[] is_trigger_click = new bool[DEV_NUM];
    private bool[] is_trigger_press = new bool[DEV_NUM];
    private bool[] is_trackpad_click = new bool[DEV_NUM];
    private bool[] is_trackpad_press = new bool[DEV_NUM];
    private bool[] is_grip_click = new bool[DEV_NUM];
    private bool[] is_grip_press = new bool[DEV_NUM];
    private Vector3[] hand_position = new Vector3[DEV_NUM];

    /* Public method */
    public bool IsTriggerClick(HAND_ID hand_id) { return is_trigger_click[(int)hand_id]; }
    public bool IsTriggerPress(HAND_ID hand_id) { return is_trigger_press[(int)hand_id]; }
    public bool IsTrackpadClick(HAND_ID hand_id) { return is_trackpad_click[(int)hand_id]; }
    public bool IsTrackpadPress(HAND_ID hand_id) { return is_trackpad_press[(int)hand_id]; }
    public bool IsGripClick(HAND_ID hand_id) { return is_grip_click[(int)hand_id]; }
    public bool IsGripPress(HAND_ID hand_id) { return is_grip_press[(int)hand_id]; }
    public Vector3 HandPosition(HAND_ID hand_id) { return hand_position[(int)hand_id]; }

    // Use this for initialization
    void Start() {
        for (int i = 0; i < DEV_NUM; i++) {
            is_trigger_click[i] = false;
            is_trigger_press[i] = false;
            is_trackpad_click[i] = false;
            is_trackpad_press[i] = false;
            is_grip_click[i] = false;
            is_grip_press[i] = false;
            hand_position[i] = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update() {
        /* VR inputs */
        if (SteamVR.active) {
            for (int i = 0; i < DEV_NUM; i++) {
                is_trigger_click[i] = trigger.GetStateDown(vr_src[i]);
                is_trigger_press[i] = trigger.GetState(vr_src[i]);
                is_trackpad_click[i] = trackpad.GetStateDown(vr_src[i]);
                is_trackpad_press[i] = trackpad.GetState(vr_src[i]);
                is_grip_click[i] = grip.GetStateDown(vr_src[i]);
                is_grip_press[i] = grip.GetState(vr_src[i]);
                hand_position[i] = hand_pose[i].transform.localPosition;
            }
        }
        /* 2D fallback */
        else {
            if (GameController.GetGameSTAT() == GameController.GAME_STAT.Menu) {
                // Left hand
                is_trigger_click[(int)HAND_ID.Left] = Input.GetMouseButtonDown(0);
                is_trigger_press[(int)HAND_ID.Left] = Input.GetMouseButton(0);
                // Right hand
                is_trigger_click[(int)HAND_ID.Right] = Input.GetMouseButtonDown(0);
                is_trigger_press[(int)HAND_ID.Right] = Input.GetMouseButton(0);
            }
            else {
                // Left hand
                is_trigger_click[(int)HAND_ID.Left] = Input.GetKeyDown("c");
                is_trigger_press[(int)HAND_ID.Left] = Input.GetKey("c");
                // Right hand
                is_trigger_click[(int)HAND_ID.Right] = Input.GetKeyDown("v");
                is_trigger_press[(int)HAND_ID.Right] = Input.GetKey("v");
            }
            // Left hand
            is_trackpad_click[(int)HAND_ID.Left] = Input.GetKeyDown("f");
            is_trackpad_press[(int)HAND_ID.Left] = Input.GetKey("f");
            is_grip_click[(int)HAND_ID.Left] = Input.GetKeyDown("z");
            is_grip_press[(int)HAND_ID.Left] = Input.GetKey("z");
            hand_position[(int)HAND_ID.Left] = Input.mousePosition;
            hand_position[(int)HAND_ID.Left].z = hand_position[(int)HAND_ID.Left].y;
            // Right hand
            is_trackpad_click[(int)HAND_ID.Right] = Input.GetKeyDown("f");
            is_trackpad_press[(int)HAND_ID.Right] = Input.GetKey("f");
            is_grip_click[(int)HAND_ID.Right] = Input.GetKeyDown("x"); 
            is_grip_press[(int)HAND_ID.Right] = Input.GetKey("x"); 
            hand_position[(int)HAND_ID.Right] = Input.mousePosition;
            hand_position[(int)HAND_ID.Right].z = hand_position[(int)HAND_ID.Right].y;
        }
    }

    // Translate from hand to HAND_ID
    public static HAND_ID HandTOID(Hand hand) {
        if (hand.name == "LeftHand") return HAND_ID.Left;
        else if (hand.name == "RightHand") return HAND_ID.Right;
        return HAND_ID.Non;
    }
}
