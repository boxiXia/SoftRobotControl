using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationSettings : MonoBehaviour {
    public bool runInBackground = true;
    public int targetFrameRate;
    private void Awake()
    {
        //QualitySettings.vSyncCount = 0;

        Application.runInBackground = runInBackground;
        if (targetFrameRate > 0)
        {
            Application.targetFrameRate = targetFrameRate;
        }

    }
}
