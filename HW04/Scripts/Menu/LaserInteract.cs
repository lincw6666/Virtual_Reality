using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class LaserInteract : MonoBehaviour
{
    // For VR hand interact with object.
    private Interactable interactable;
    private Hand.AttachmentFlags attachment_flags = Hand.defaultAttachmentFlags 
        & (~Hand.AttachmentFlags.SnapOnAttach) 
        & (~Hand.AttachmentFlags.DetachOthers) 
        & (~Hand.AttachmentFlags.VelocityMovement);

    // For laser beam.
    public GameObject laser;

    // For user input.
    private UserInput user_input;

    private void Start() {
        user_input = GameObject.Find("/User Input").GetComponent<UserInput>();
        interactable = this.GetComponent<Interactable>();
    }

    // Called every Update() while a Hand is hovering over this object.
    private void HandHoverUpdate(Hand hand) {
        /* No grabbing object. */
        if (interactable.attachedToHand == null) {
            // Start grabbing object.
            if (user_input.IsTriggerPress(UserInput.HandTOID(hand))) {
                GrabOBJ(hand);          // Grab the laser pointer.
                laser.SetActive(true);  // Show the laser beam.
            }
        }
        /* While grabbing object. */
        else {
            // Put down the laser pointer.
            if (!(user_input.IsTriggerPress(UserInput.HandTOID(hand)))) {
                ReleaseOBJ(hand);
                laser.SetActive(false);
            }
        }
    }

    private void GrabOBJ(Hand hand) {
        // Call this to continue receiving HandHoverUpdate messages,
        // and prevent the hand from hovering over anything else
        hand.HoverLock(interactable);

        // Attach this object to the hand
        hand.AttachObject(gameObject, GrabTypes.Trigger, attachment_flags);
    }

    private void ReleaseOBJ(Hand hand) {
        // Detach this object from the hand
        hand.DetachObject(gameObject);

        // Call this to undo HoverLock
        hand.HoverUnlock(interactable);
    }
}
