using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowNowToothInfo : MonoBehaviour
{
    private Teeth teeth;
    private Controller controller;
    private Text text;

    private Vector3 pre_pos, pre_v1, pre_v3;
    int t_id;
    uint axis;

    // Start is called before the first frame update
    void Start() {
        teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        controller = GameObject.Find("/Controller").GetComponent<Controller>();
        text = this.GetComponent<Text>();
        pre_pos = Vector3.zero; pre_v1 = Vector3.zero; pre_v3 = Vector3.zero;
        t_id = -1;
        axis = (uint)Controller.AXIS.non;
    }

    // Update is called once per frame
    void Update()
    {
        int tmp_id = controller.GetNowSelectTooth();
        uint tmp_axis = controller.GetNowAxis();

        if (tmp_id != t_id || tmp_axis != axis) {
            // Reset pre_pos, pre_v1 and pre_v3.
            pre_pos = teeth.param[tmp_id].GetCenter();
            pre_v1 = teeth.param[tmp_id].GetV1();
            pre_v3 = teeth.param[tmp_id].GetV3();
        }
        t_id = tmp_id; axis = tmp_axis;
        if (t_id < 0) return;

        // Show translation.
        text.text = "Translation: " + (teeth.param[t_id].GetCenter() - pre_pos);
        // Show rotation.
        Quaternion rotate_change = Quaternion.FromToRotation(pre_v1, teeth.param[t_id].GetV1()).normalized;
        rotate_change = Quaternion.FromToRotation(rotate_change * pre_v3, teeth.param[t_id].GetV3()).normalized * rotate_change;
        text.text += "\nRotation: " + rotate_change.eulerAngles;
    }
}
