using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform left_door, right_door;
    Vector3 open_pos_l, open_pos_r, close_pos_l, close_pos_r;
    bool is_open = false, is_close = false;

    // Start is called before the first frame update
    void Start()
    {
        open_pos_l = left_door.position + new Vector3(-1.99f, 0, 0);
        open_pos_r = right_door.position + new Vector3(1.99f, 0, 0);
        close_pos_l = left_door.position;
        close_pos_r = right_door.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_open) {
            left_door.position = Vector3.Slerp(left_door.position, open_pos_l, 0.1f);
            right_door.position = Vector3.Slerp(right_door.position, open_pos_r, 0.1f);
        }
        else if (is_close) {
            left_door.position = Vector3.Slerp(left_door.position, close_pos_l, 0.2f);
            right_door.position = Vector3.Slerp(right_door.position, close_pos_r, 0.2f);
        }
    }

    public void OpenDoors() {
        is_open = true;
    }

    public void CloseDoors() {
        is_close = true;
    }
}
