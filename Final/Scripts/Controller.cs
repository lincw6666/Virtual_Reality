using System.Collections;
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
        teeth = GameObject.Find("/Tooth").GetComponent<Teeth>();
        // Import teeth.
        import.ImportInit();
        // Transform teeth to correct position.
        tooth_transform.Init();
        tooth_transform.SetCorrectPosition();
        // Initialize debug classes.
        //bound_box.Init();
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

            // Draw v1, v2 of each tooth.
            debug_vec.DrawV1((uint)i);
            debug_vec.DrawV2((uint)i);
        }
        /***********************************************************
         * End Debug message
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
