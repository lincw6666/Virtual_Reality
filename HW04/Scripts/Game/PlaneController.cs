using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlaneController : MonoBehaviour
{
    /* Control movement */
    public StickController stick_ctrl;

    /* Speed control */
    float origin_z;
    float pre_speed, speed;

    // For user input.
    private UserInput user_input;

    // For machine gun sound.
    private AudioSource fire_sound;

    /* Public method */
    public float SpeedChange() { return speed - pre_speed; }

    private void Start() {
        origin_z = 0;
        pre_speed = 60f; speed = 60f;

        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();

        fire_sound = this.GetComponent<AudioSource>();
        fire_sound.loop = true;
    }

    // Update is called once per frame
    void Update() {
        // Plane can only exist in the game scene.
        if (SceneController.NowScene() == SceneController.SCENE_ID.Menu) {
            Destroy(gameObject);
        }
        // Only work in the game.
        if (GameController.GetGameSTAT() != GameController.GAME_STAT.Game
            && GameController.GetGameSTAT() != GameController.GAME_STAT.Tutorial) {
            return;
        }

        //---------------------------------------------------------------------
        // Control speed
        //---------------------------------------------------------------------
        if (user_input.IsTrackpadClick(UserInput.HAND_ID.Left)) {
            origin_z = user_input.HandPosition(UserInput.HAND_ID.Left).x;
            pre_speed = speed;
        }
        if (user_input.IsTrackpadPress(UserInput.HAND_ID.Left)) {
            float speed_change = user_input.HandPosition(UserInput.HAND_ID.Left).x - origin_z;

            if (!(speed < 20f && speed_change < 0f) 
                && !(speed > 180f && speed_change > 0f)) 
            {
                speed = pre_speed + speed_change * (SteamVR.active ? 1200f : 1f);
            }
        }
        //---------------------------------------------------------------------

        //---------------------------------------------------------------------
        // Fire
        //---------------------------------------------------------------------
        if (user_input.IsTriggerPress(UserInput.HAND_ID.Left)) {
            PlaneFire.Fire(
                Time.time,
                true,
                transform.TransformPoint(new Vector3(-2.5f / transform.localScale.x, 0, 0)),
                transform.rotation
            );
        }
        if (user_input.IsTriggerPress(UserInput.HAND_ID.Right)) {
            PlaneFire.Fire(
                Time.time,
                false,
                transform.TransformPoint(new Vector3(2.5f / transform.localScale.x, 0, 0)),
                transform.rotation
            );
        }
        
        if (user_input.IsTriggerPress(UserInput.HAND_ID.Left)
            || user_input.IsTriggerPress(UserInput.HAND_ID.Right))
        {
            if (!fire_sound.isPlaying) fire_sound.Play();
        }
        else {
            fire_sound.Stop();
        }
        //---------------------------------------------------------------------

        // Hit the ground.
        if (transform.position.y <= 38.8f) {
            GameController.SendSIG(GameController.GAME_SIG.GameOver);
        }
    }

    /* Control plane movement. */
    private void LateUpdate() {
        // Only work in the game.
        if (GameController.GetGameSTAT() != GameController.GAME_STAT.Game
            && GameController.GetGameSTAT() != GameController.GAME_STAT.Tutorial) {
            return;
        }

        /* Rotation */
        // How we are going to rotate the plane.
        Vector3 rotate_angle = GeneralizedEularAngle.Generalized(stick_ctrl.NowRotate());
        // The plane rotation now.
        Vector3 plane_rotate = GeneralizedEularAngle.Generalized(transform.rotation);

        // x axis
        if (rotate_angle.x > 0 && plane_rotate.x > 80f) rotate_angle.x = 0;
        // y axis
        float rotate_y = plane_rotate.z / 2f;
        if (plane_rotate.z * rotate_angle.z < 0) rotate_y /= 2f;
        if (user_input.IsGripPress(UserInput.HAND_ID.Left)) rotate_y += 2;
        if (user_input.IsGripPress(UserInput.HAND_ID.Right)) rotate_y -= 2;
        // Apply rotation.
        transform.Rotate(new Vector3(rotate_angle.x, rotate_angle.y - rotate_y, rotate_angle.z) * Time.deltaTime);

        // Move forward.
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        // Falling.
        if (speed < 30f) {
            transform.Translate(Vector3.down * (30f - speed) * 2f * Time.deltaTime, Space.World);
            if (plane_rotate.x < 80f) transform.Rotate(new Vector3((30f - speed), 0, 0) * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other) {
        // Plane hits an object.
        if (other.tag == "Obstacles" 
            && GameController.GetGameSTAT() != GameController.GAME_STAT.GameOver)
        {
            GameController.SendSIG(GameController.GAME_SIG.GameOver);
        }
    }
}
