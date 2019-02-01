using UnityEngine;
using System.Collections;
//Author: Dave Hampson
//Here is a FPS Display script I created.
//It doesn't require a GUIText element and it also shows milliseconds, 
//so just drop it into a GameObject and you should be ready to go. 
public class FPSDisplay : MonoBehaviour
{
    public float xPos =0.5f;
    public float yPos =0f;
    float deltaTime = 0.0f;
    int w = Screen.width, h = Screen.height;
    GUIStyle style = new GUIStyle();
    Rect rect;
    float msec, fps;

    //[Tooltip("Set targetFrameRate<0 to disable it")]
    //public int targetFrameRate = 120;

    private void Start()
    {
        //if (targetFrameRate >0)
        //{
        //    Application.targetFrameRate = targetFrameRate;
        //}


        rect = new Rect(w* xPos, h* yPos, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        //style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        style.normal.textColor = Color.white;
    }
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.01f;

    }

    void OnGUI()
    {
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;
        string text = string.Format("{00:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}