using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    enum TutorialState {
        idle, 
        hold_control_stick,
        rotation,
        speed_control,
        fire,
        pause_button,
        end
    };

    enum RotateState { roll, pitch, yaw };
    enum SpeedState { acceleration, deceleration };

    TutorialState tutorial_state = TutorialState.idle;
    RotateState rotate_state = RotateState.roll;
    SpeedState speed_state = SpeedState.acceleration;

    /* Hints */
    // Hint objects
    public GameObject hint, end;
    public GameObject stick_hint_cube;
    Text text;
    // Hint text
    string roll = "< Hint >\nMove the stick left or right to roll.";
    string pitch = "< Hint >\nMove the stick forward or backward to pitch.";
    string yaw = "< Hint >\nPress grip button on your left hand or right hand to yaw to the left or to the right.";
    string accelerate = "< Hint >\nPress the trackpad on your left controller.\nMove it forward to accelerate.";
    string decelerate = "< Hint >\nPress the trackpad on your left controller.\nMove it backward to decelerate.";
    string fire = "\n\n< Hint >\nPress the trigger to fire.";
    string pause = "< Hint >\nMove your left controller over the pause button. Then press the trackpad to pause.";
    // Hint boolean action
    bool is_stick_hold = false;
    bool is_rolling = false;
    bool is_pitching = false;
    bool is_yawing = false;
    bool is_accelerating = false;
    bool is_decelerating = false;
    bool is_fire = false;
    bool is_pause_press = false;

    float start_t = 0;

    public PlaneController plane_ctrl;
    public StickController stick_ctrl;

    // For user input.
    private UserInput user_input;

    private void Start() {
        text = hint.transform.Find("Text").GetComponent<Text>();
        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (tutorial_state) {
            case TutorialState.idle:
                // Do nothing.
                break;

            case TutorialState.hold_control_stick:
                stick_hint_cube.SetActive(true);
                // Goto rotation hint after hold the control stick.
                if (is_stick_hold) {
                    stick_hint_cube.SetActive(false);
                    start_t = Time.time;
                    tutorial_state = TutorialState.rotation;
                }
                break;

            case TutorialState.rotation:
                RotateHintHandler();
                break;

            case TutorialState.speed_control:
                SpeedHintHandler();
                break;

            case TutorialState.fire:
                // Show fire hint.
                text.text = fire;
                // Goto pause button after fire.
                if (is_fire) {
                    tutorial_state = TutorialState.pause_button;
                }
                break;

            case TutorialState.pause_button:
                // Show pause button hint.
                text.text = pause;
                // Goto end after pause.
                if (is_pause_press) {
                    tutorial_state = TutorialState.end;
                }
                break;

            case TutorialState.end:
                end.SetActive(true);
                tutorial_state = TutorialState.idle;
                hint.SetActive(false);
                break;

            default:
                Debug.Log("Tutorial Error!! Invalid tutorial state: " + tutorial_state.ToString());
                break;
        }

        UpdateHintBool();
    }

    public void StartTutorial() {
        tutorial_state = TutorialState.hold_control_stick;
        hint.SetActive(true);
    }

    void RotateHintHandler() {
        switch (rotate_state) {
            case RotateState.roll:
                // Show roll hint.
                text.text = roll;
                // Goto pitch hint after rolling.
                if (is_rolling && Time.time - start_t > 3f) {
                    start_t = Time.time;
                    rotate_state = RotateState.pitch;
                }
                break;

            case RotateState.pitch:
                // Show pitch hint.
                text.text = pitch;
                // Goto yaw hint after pitching.
                if (is_pitching && Time.time - start_t > 3f) {
                    start_t = Time.time;
                    rotate_state = RotateState.yaw;
                }
                break;

            case RotateState.yaw:
                // Show yaw hint.
                text.text = yaw;
                // Go back to roll hint after yawing.
                if (is_yawing && Time.time - start_t > 3f) {
                    start_t = Time.time;
                    rotate_state = RotateState.roll;
                    tutorial_state = TutorialState.speed_control;
                }
                break;

            default:
                Debug.Log("Rotate state Error!! Invalid rotate state: " + rotate_state.ToString());
                break;
        }
    }

    void SpeedHintHandler() {
        switch (speed_state) {
            case SpeedState.acceleration:
                // Show acceleration hint.
                text.text = accelerate;
                // Goto deceleration hint after accelerating.
                if (is_accelerating && Time.time - start_t > 3f) {
                    start_t = Time.time;
                    speed_state = SpeedState.deceleration;
                }
                break;

            case SpeedState.deceleration:
                // Show deceleration hint.
                text.text = decelerate;
                // Go back to acceleration hint after decelerating.
                if (is_decelerating && Time.time - start_t > 3f) {
                    speed_state = SpeedState.acceleration;
                    tutorial_state = TutorialState.fire;
                }
                break;

            default:
                Debug.Log("Speed state Error!! Invalid speed state: " + speed_state.ToString());
                break;
        }
    }

    void UpdateHintBool() {
        is_stick_hold = stick_ctrl.IsHoldStick();
        is_rolling = stick_ctrl.IsHoldStick()
            && (Mathf.Abs(stick_ctrl.NowRotate().z) > 10f);
        is_pitching = stick_ctrl.IsHoldStick()
            && (Mathf.Abs(stick_ctrl.NowRotate().x) > 10f);
        is_yawing = user_input.IsGripPress(UserInput.HAND_ID.Left)
            || user_input.IsGripPress(UserInput.HAND_ID.Right);
        is_accelerating = user_input.IsTrackpadPress(UserInput.HAND_ID.Left) 
            && (plane_ctrl.SpeedChange() > 15f);
        is_decelerating = user_input.IsTrackpadPress(UserInput.HAND_ID.Left)
            && (plane_ctrl.SpeedChange() < -15f);
        is_fire = user_input.IsTriggerPress(UserInput.HAND_ID.Left)
            || user_input.IsTriggerPress(UserInput.HAND_ID.Right);
        is_pause_press = GameController.GetGameSTAT() == GameController.GAME_STAT.Pause;
    }
}
