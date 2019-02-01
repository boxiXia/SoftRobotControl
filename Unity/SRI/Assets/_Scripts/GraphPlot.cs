using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphPlot : MonoBehaviour {
    public float value;
    private GraphRect graphRect;
    public Rect rect;
    public float updateInterval = 0.05f;
    public float updateIntervalSlow = 0.1f;
    private string plotName;
    public Text valueText;
    [Header("Range Setting")]
    public float minValue = float.NegativeInfinity;
    public float maxValue = float.PositiveInfinity;
    public bool useMaxMinAsRange = false;
    // Use this for initialization
    private void Awake()
    {
        graphRect = new GraphRect(transform);
        plotName = transform.parent.name;
        valueText = transform.Find("Value").GetComponent<Text>();
    }
    void Start () {
        InvokeRepeating("UpdatePlot", 0f, updateInterval);
        InvokeRepeating("UpdateText", 0f, updateIntervalSlow);
    }
	void UpdatePlot()
    {
        if (GraphManager.Graph != null)
        {
            if (useMaxMinAsRange)
            {
                GraphManager.Graph.Plot(plotName, value, Color.green, graphRect.rect,minValue,maxValue);
            }
            else
            {
                GraphManager.Graph.Plot(plotName, value, Color.green, graphRect.rect);
            }
            
        }
    }
    void UpdateText()
    {
        valueText.text = value.ToString("0.0");
    }



    [System.Serializable]
    public class GraphRect
    {// helper class for two state button display
        public Text text;
        public Rect rect;
        public GraphRect(Transform transform)
        {
            text = transform.GetComponentInChildren<Text>();
            rect = InitializePanelTransform(transform);
        }

        Rect InitializePanelTransform(Transform transform)
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 tmp;
            if (transform.parent.GetComponent<Canvas>() != null)
            {
                tmp = rectTransform.anchoredPosition;
            }
            else
            {
                RectTransform paretRectTransform = transform.parent.GetComponent<RectTransform>();
                tmp = rectTransform.anchoredPosition + paretRectTransform.anchoredPosition;
            }
            return new Rect(tmp.x, -tmp.y, rectTransform.rect.width, rectTransform.rect.height);
        }
    }
}
