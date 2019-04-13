﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Teeth datas' path.
    public string src_dir_path;
    public string final_dir_path;

    private Teeth teeth;
    private readonly ImportSTL import = new ImportSTL();
    private readonly Tooth.ToothTransform tooth_transform = new Tooth.ToothTransform();
    // For debugging.
    private readonly ToothDebug.BoundingBox bound_box = new ToothDebug.BoundingBox();
    private readonly ToothDebug.DrawVectors debug_vec = new ToothDebug.DrawVectors();

    // Start is called before the first frame update
    void Start()
    {
        src_dir_path = Application.dataPath + src_dir_path;
        final_dir_path = Application.dataPath + final_dir_path;
        teeth = GameObject.Find("/Teeth").GetComponent<Teeth>();
        // Import teeth.
        import.ImportInit();
        // Transform teeth to correct position.
        tooth_transform.Init();
        tooth_transform.SetCorrectPosition();
        // Initialize debug classes.
        bound_box.Init();
        debug_vec.Init();
    }

    // Update is called once per frame
    void Update() {
        /***********************************************************
         * Debug message
         **********************************************************/
        for (int i = 0; i < teeth.TOOTH_NUM; i++) {
            // Draw bounding box.
            //bound_box.DrawBoundingBox((uint)i);
            bound_box.DrawBox((uint)i);

            // Draw v1, v2 of each tooth.
            debug_vec.DrawV1((uint)i);
            debug_vec.DrawV2((uint)i);
                    }
        /***********************************************************
         * End Debug message
         **********************************************************/

        /***********************************************************
         * Keyboard Event
         **********************************************************/

        if (Input.GetKey("r")) {
            float degree = 30 * Time.deltaTime;
            if (Input.GetKey("-")) degree = -degree;
            if (Input.GetKey("1")) tooth_transform.RotateVx(2, 1, degree);
            if (Input.GetKey("2")) tooth_transform.RotateVx(2, 2, degree);
        }
        if (Input.GetKey("t")) {
            float speed = 3 * Time.deltaTime;
            if (Input.GetKey("-")) speed = -speed;
            if (Input.GetKey("1")) tooth_transform.TranslateVx(2, 1, speed);
            if (Input.GetKey("2")) tooth_transform.TranslateVx(2, 2, speed);
            if (Input.GetKey("3")) tooth_transform.TranslateVx(2, 3, speed);
        }
        if (Input.GetKey("s")) {
            float degree = 30 * Time.deltaTime;
            if (Input.GetKey("-")) degree = -degree;
            if (Input.GetKey("up")) tooth_transform.RotateBox(2, "up", degree);
            if (Input.GetKey("down")) tooth_transform.RotateBox(2, "down", degree);
            if (Input.GetKey("left")) tooth_transform.RotateBox(2, "left", degree);
            if (Input.GetKey("right")) tooth_transform.RotateBox(2, "right", degree);
        }

        /***********************************************************
         * Keyboard Event
         **********************************************************/
    }

    public void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
