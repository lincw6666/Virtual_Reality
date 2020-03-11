using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowScore : MonoBehaviour
{
    public static int score = 0;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string leading_zero = "";

        if (score < 10) leading_zero = "00";
        else if (score < 100) leading_zero = "0";

        text.text = "Score: " + leading_zero + score.ToString();
    }
}
