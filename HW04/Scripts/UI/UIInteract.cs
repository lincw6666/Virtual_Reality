using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable), typeof(Collider))]
public class UIInteract : MonoBehaviour
{
    public UIAction.UI_ACTION UI_action;

    private UserInput user_input;

    private void Start() {
        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();
    }

    private void HandHoverUpdate(Hand hand) {
        if (user_input.IsTrackpadClick(UserInput.HAND_ID.Left)
            || user_input.IsTrackpadClick(UserInput.HAND_ID.Right))
        {
            UIAction.DoAction(UI_action);
        }
    }
}
