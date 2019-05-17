using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileIO
{
    StreamReader r;
    StreamWriter w;
    bool rw;    // True: read, False: write
    bool is_open = false;
    public int now_read_line = 0;  // Start reading the file from now_read_line.

    public void Init(string path, bool i_rw, bool mode) {
        if (is_open) return;
        rw = i_rw;
        if (i_rw) r = new StreamReader(Application.dataPath + path, mode);
        else w = new StreamWriter(Application.dataPath + path, mode);
        is_open = true;
    }

    public string ReadContent() {
        if (!rw || !is_open) return "";
        return r.ReadLine();
    }

    public void WriteContent(string content) {
        if (rw || !is_open) return;
        w.WriteLine(content);
        w.Flush();
    }

    public void Close() {
        if (!is_open) return;
        if (rw) r.Close();
        else w.Close();
        is_open = false;
    }
}
