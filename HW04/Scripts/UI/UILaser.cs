using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UILaser : MonoBehaviour
{
    public UIAction.UI_ACTION UI_action;

    private UserInput user_input;

    private void Start() {
        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();
    }

    private void OnTriggerStay(Collider other) {
        if (user_input.IsTrackpadClick(UserInput.HAND_ID.Left)
            || user_input.IsTrackpadClick(UserInput.HAND_ID.Right))
        {
            UIAction.DoAction(UI_action);
        }
    }
}
