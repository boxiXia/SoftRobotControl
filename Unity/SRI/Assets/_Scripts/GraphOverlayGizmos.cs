using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphOverlayGizmos : MonoBehaviour {
    private Rect GraphRect;
    public RectTransform panelDisplayTransform;
    public Material mat;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        //if (!mat)
        //{
        //    Debug.LogError("Please Assign a material on the inspector");
        //    return;
        //}
        //float xSize = panelDisplayTransform.sizeDelta.x;
        //float ySize = panelDisplayTransform.sizeDelta.y;
        //float xPos = panelDisplayTransform.anchoredPosition.x;
        //float yPos = -panelDisplayTransform.anchoredPosition.y;

        //Vector3 topL = new Vector3(xPos, yPos, 0.0f);
        //Vector3 topR = new Vector3(xPos + xSize, yPos, 0.0f);

        //Vector3 bottomL = new Vector3(xPos, yPos + ySize, 0.0f);
        //Vector3 bottomR = new Vector3(xPos + xSize, yPos + ySize, 0.0f);

        //GL.PushMatrix();
        //mat.SetPass(0);
        //GL.LoadOrtho();
        //GL.Begin(GL.LINES);
        //GL.Vertex(topL);
        //GL.Vertex(topR);
        //GL.Vertex(bottomR);
        //GL.Vertex(bottomL);
        //GL.End();
        //GL.PopMatrix();


        //GL.PushMatrix();
        //mat.SetPass(0);
        //GL.LoadOrtho();
        //GL.Begin(GL.QUADS);
        //GL.Color(Color.red);
        //GL.Vertex3(0, 0.5F, 0);
        //GL.Vertex3(0.5F, 1, 0);
        //GL.Vertex3(1, 0.5F, 0);
        //GL.Vertex3(0.5F, 0, 0);
        //GL.Color(Color.cyan);
        //GL.Vertex3(0, 0, 0);
        //GL.Vertex3(0, 0.25F, 0);
        //GL.Vertex3(0.25F, 0.25F, 0);
        //GL.Vertex3(0.25F, 0, 0);
        //GL.End();
        //GL.LoadOrtho();
        //GL.Begin(GL.LINES);
        //GL.Vertex3(0.1f, 0.1f, 0);
        //GL.Vertex3(0.9f, 0.1F, 0);
        //GL.Vertex3(0.9f, 0.1F, 0);
        //GL.Vertex3(0.9F, 0.9F, 0);
        //GL.Vertex3(0.9F, 0.9F, 0);
        //GL.Vertex3(0.1F, 0.9f, 0);
        //GL.Vertex3(0.1F, 0.9f, 0);
        //GL.Vertex3(0.1f, 0.1f, 0);
        //GL.End();
        //GL.PopMatrix();
    }

}
