using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class StickController : MonoBehaviour
{
    // For mesh operations.
    public static readonly int root_of_control_stick = 682;
    public static readonly int base_id = 640;
    public static readonly int limit_id = 711;
    public static readonly int cube_pos_id = 695;

    // For stick movement.
    private Interactable interactable;
    private Vector3 hand_pre_pos, pre_rotate;
    private Quaternion now_rotate;
    private bool is_move_valid;

    // For user input.
    private UserInput user_input;

    /* Public method */
    public Quaternion NowRotate() { return now_rotate; }
    public bool IsHoldStick() { return is_move_valid; }

    // Start is called before the first frame update
    private void Start() {
        hand_pre_pos = Vector3.zero; pre_rotate = Vector3.zero;
        now_rotate = Quaternion.identity;
        is_move_valid = false;
        interactable = this.GetComponent<Interactable>();
        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();
    }

    private void Update() {
        // Only work in the game.
        if (GameController.GetGameSTAT() != GameController.GAME_STAT.Game
            && GameController.GetGameSTAT() != GameController.GAME_STAT.Tutorial)
        {
            return;
        }

        // Update is_move_valid.
        is_move_valid &= user_input.IsTrackpadPress(UserInput.HAND_ID.Right);
        // Move the stick.
        if (is_move_valid) {
            /* Get user input movement. */
            float move_weight = SteamVR.active ? 60f : 0.1f;
            float mov_x = (user_input.HandPosition(UserInput.HAND_ID.Right).z - hand_pre_pos.z) * move_weight;
            float mov_z = (hand_pre_pos.x - user_input.HandPosition(UserInput.HAND_ID.Right).x) * move_weight;

            /* Update now rotaion. */
            // Update x rotation.
            float rotate_x = pre_rotate.x + mov_x;
            if (rotate_x > 30f) rotate_x = 30f;
            else if (rotate_x < -30f) rotate_x = -30f;
            // Update z rotation.
            float rotate_z = pre_rotate.z + mov_z;
            if (rotate_z > 30f) rotate_z = 30f;
            else if (rotate_z < -30f) rotate_z = -30f;
            // Update rotation.
            now_rotate = Quaternion.Euler(rotate_x, pre_rotate.y, rotate_z);
        }
    }

    private void LateUpdate() {
        // The stick will move. We need to attach the interactable object to the stick.
        // So that we can control the stick the next time.
        transform.localPosition = transform.parent.GetComponent<MeshFilter>().mesh.vertices[cube_pos_id];
        transform.localRotation = now_rotate;
    }

    private void HandHoverUpdate(Hand hand) {
        // Start moving the control stick.
        if (user_input.IsTrackpadClick(UserInput.HAND_ID.Right)) {
            hand_pre_pos = user_input.HandPosition(UserInput.HAND_ID.Right);
            pre_rotate = GeneralizedEularAngle.Generalized(now_rotate);
            is_move_valid = true;
        }
    }
}

public class GeneralizedEularAngle
{
    public static Vector3 Generalized(Vector3 angle) { return __Generalized(angle); }
    public static Vector3 Generalized(Quaternion angle) { return __Generalized(angle.eulerAngles); }

    private static Vector3 __Generalized(Vector3 angle) {
        angle.x %= 360; angle.y %= 360; angle.z %= 360;
        if (angle.x > 180) angle.x -= 360;
        if (angle.y > 180) angle.y -= 360;
        if (angle.z > 180) angle.z -= 360;
        return angle;
    }
}
