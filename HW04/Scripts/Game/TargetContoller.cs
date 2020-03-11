using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetContoller : MonoBehaviour
{
    public Transform plane_transform;

    private AudioSource explode_sound;
    private float end_t;
    private bool is_active;

    // Start is called before the first frame update
    void Start()
    {
        explode_sound = this.GetComponent<AudioSource>();
        HideObj();
        is_active = false;
        end_t = -5;
    }

    // Update is called once per frame
    void Update()
    {
        // Only work in the game.
        if (GameController.GetGameSTAT() != GameController.GAME_STAT.Game
            && GameController.GetGameSTAT() != GameController.GAME_STAT.Tutorial) {
            return;
        }

        if (!is_active && Time.time - end_t > 5.0f) {
            is_active = true;
            UpdatePos();
            ShowObj();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "laser") {
            explode_sound.Play();
            is_active = false;
            HideObj();
            end_t = Time.time;
            ShowScore.score++;
        }
        else if (other.tag == "Obstacles") {
            UpdatePos();
        }
    }

    void ShowObj() {
        this.GetComponent<MeshRenderer>().enabled = true;
        this.GetComponent<Collider>().enabled = true;
    }

    void HideObj() {
        this.GetComponent<MeshRenderer>().enabled = false;
        this.GetComponent<Collider>().enabled = false;
    }

    void UpdatePos() {
        Transform parent_t = transform.parent.transform;
        bool is_success = false;

        do {
            is_success = true;
            // Set new position.
            transform.position = new Vector3(
                Random.Range(-1500, 1500),
                Random.Range(50, 500),
                Random.Range(-1500, 1500)
            );
            // Check collision with other targets.
            for (int i = 0; i < parent_t.childCount; i++) {
                if (i != transform.GetSiblingIndex()) {
                    if (Vector3.Distance(transform.position, 
                        parent_t.GetChild(i).transform.position) < 30f)
                    {
                        is_success = false;
                        break;
                    }
                }
            }
            // Check collision with plane.
            if (Vector3.Distance(transform.position, plane_transform.position) < 30f) {
                is_success = false;
            }
        } while (!is_success);
    }
}
