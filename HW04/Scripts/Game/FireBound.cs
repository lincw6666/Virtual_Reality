using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBound : MonoBehaviour
{
    public Transform plane_transform;

    private void LateUpdate() {
        transform.position = plane_transform.position;
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "laser") {
            Destroy(other.gameObject);
        }
    }
}
