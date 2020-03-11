using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeriodHighlight : MonoBehaviour
{
    float start_t = 0;
    bool enable = true;

    // Start is called before the first frame update
    void Start()
    {
        start_t = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - start_t > 0.5f) {
            enable = !enable;
            this.GetComponent<MeshRenderer>().enabled = enable;
            start_t = Time.time;
        }
    }
}
