using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowNowAxis : MonoBehaviour
{
    private Controller controller;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("/Controller").GetComponent<Controller>();
        text = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string axis;

        switch (controller.GetNowAxis()) {
            case (uint)Controller.AXIS.non:
                axis = "non";
                break;
            case (uint)Controller.AXIS.v1:
                axis = "v1";
                break;
            case (uint)Controller.AXIS.v2:
                axis = "v2";
                break;
            case (uint)Controller.AXIS.v3:
                axis = "v3";
                break;
            case (uint)Controller.AXIS.up:
                axis = "up";
                break;
            case (uint)Controller.AXIS.down:
                axis = "down";
                break;
            case (uint)Controller.AXIS.left:
                if (controller.GetNowSelectTooth() < 16) axis = "left";
                else axis = "right";
                break;
            case (uint)Controller.AXIS.right:
                if (controller.GetNowSelectTooth() < 16) axis = "right";
                else axis = "left";
                break;
            default:
                axis = "Error axis: " + controller.GetNowAxis().ToString();
                break;
        }

        text.text = "Now Axis: " + axis + "    Now step: " + controller.now_step;
    }
}
