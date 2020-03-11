using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAttatchPlane : MonoBehaviour
{
    private Vector3 origin_position;
    private Quaternion origin_rotation;
    private bool flag;

    void Start() {
        origin_position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        origin_rotation = new Quaternion(
            transform.rotation.x,
            transform.rotation.y,
            transform.rotation.z,
            transform.rotation.w
        );
        flag = false;
    }

    // Update is called once per frame
    void Update() {
        if (SceneController.NowScene() == SceneController.SCENE_ID.Game) {
            if (GameController.GetGameSTAT() != GameController.GAME_STAT.GameOver) {
                transform.parent = GameObject.Find("Plane/Player Position").transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else {
                transform.parent = null;
                flag = true;
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (flag) {
            transform.position = origin_position;
            transform.rotation = origin_rotation;
            flag = false;
        }
    }
}
