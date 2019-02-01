using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Filters;
using UnityEngine.UI;
public class ForceTemperatureUIControl : MonoBehaviour
{
    public bool filterPlot = false;
    public float updateInterval = 0.05f;
    private float lastInterval = 0;
    // for text display
    public float updateIntervalSlow = 0.05f;
    private float lastIntervalSlow = 0;

    public FSE1001Mono fse1001MonoScript;
    public UDPTemperature udpTemperatureScript;
    public PWMControl pwmControlScript;

    public RectTransform temperatureRectTransform;
    private GraphRect temperatureGraphRect;

    public RectTransform forceRectTransform;
    private GraphRect forceGraphRect;

    public RectTransform pwmRectTransform;
    private GraphRect pwm;

    public RectTransform ambientTemperatureTransform;
    private GraphRect ambientTemperature;




    ButterWorth butterWorthTamb;//butterworthFilter for ambient temperature;
    ButterWorth butterWorthT;//butterworthFilter for temperature;
    ButterWorth butterWorthF;//butterworthFilter for Force;
    //private float[] temperature;
    private float force;
    public float[] temperatureFiltered;
    public float forceFiltered;

    public Transform buttonFilterTransform;
    private StateButton buttonFilter;

    public Transform buttonStartStopTransform;
    private StateButton buttonStartStop;

    public Transform buttonDisableEnableTransform;
    private StateButton buttonDisableEnable;

    public Transform buttonCycleTransform;
    private StateButton buttonCycle;

    public bool ControlStarted { get { return pwmControlScript.controller.started; } set { pwmControlScript.controller.started = value; } }
    public bool TimerExpired { get { return pwmControlScript.controller.timmerExpired; } set { pwmControlScript.controller.timmerExpired = value; } }
    // Use this for initialization
    void Start()
    {
        temperatureFiltered = new float[2];
        pwm = new GraphRect(pwmRectTransform);
        ambientTemperature = new GraphRect(ambientTemperatureTransform);
        temperatureGraphRect = new GraphRect(temperatureRectTransform);
        forceGraphRect = new GraphRect(forceRectTransform);

        buttonFilter = new StateButton(buttonFilterTransform);
        buttonStartStop = new StateButton(buttonStartStopTransform);
        buttonDisableEnable = new StateButton(buttonDisableEnableTransform);
        buttonCycle = new StateButton(buttonCycleTransform);

        pwmControlScript = FindObjectOfType<PWMControl>();

        {   //butterworthFilter for temperature: cutoff = 0.1, n = 5
            float[] B = new float[] { 5.979578037000323e-05f, 0.00029897890185001614f, 0.0005979578037000323f, 0.0005979578037000323f, 0.00029897890185001614f, 5.979578037000323e-05f };
            float[] A = new float[] { 1.0f, -3.984543119612336f, 6.434867090275868f, -5.253615170352268f, 2.1651329097241323f, -0.35992824506355614f };
            butterWorthT = new ButterWorth(B, A);
            butterWorthTamb = new ButterWorth(B, A);
        }



        {   //butterworthFilter for force: cutoff = 0.08, n = 5
            float[] B = new float[] { 2.139615203813592e-05f, 0.00010698076019067959f, 0.00021396152038135919f, 0.00021396152038135919f, 0.00010698076019067959f, 2.139615203813592e-05f };
            float[] A = new float[] { 1.0f, -4.187300047864399f, 7.069722752792468f, -6.009958148187329f, 2.5704293025241007f, -0.4422091823996199f };
            butterWorthF = new ButterWorth(B, A);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (DataLogger.recording&&TimerExpired)
        {
            DataLogger.recording = false;
            TimerExpired = true;
            // startStop button callbcak function
            buttonStartStop.State(new Color32(83, 188, 117, 255),
                "Start", debugMessage: "Recording stopped");

            //if timer expired, end recording
        }
        float timeNow = Time.time;
        if (GraphManager.Graph != null && timeNow > lastInterval + updateInterval)
        {
            lastInterval = timeNow;

            force = fse1001MonoScript.forceZ;
            temperatureFiltered[0] = butterWorthT.Filtering(udpTemperatureScript.temperatures[0]);
            temperatureFiltered[1] = butterWorthTamb.Filtering(udpTemperatureScript.temperatures[1]);

            forceFiltered = butterWorthF.Filtering(force);
            //int pwm = (int)pwmControlScript.motors[0].Value;
            GraphManager.Graph.Plot("PWM", (int)pwmControlScript.motors[0].Value, Color.green, pwm.rect);



            if (filterPlot)//filter befor plotting
            {
                GraphManager.Graph.Plot("temperature", temperatureFiltered[0], Color.green, temperatureGraphRect.rect);
                GraphManager.Graph.Plot("ambientTemperature", temperatureFiltered[1], Color.green, ambientTemperature.rect);
                GraphManager.Graph.Plot("force", forceFiltered, Color.green, forceGraphRect.rect);

            }
            else
            {
                GraphManager.Graph.Plot("temperature", udpTemperatureScript.temperatures[0], Color.green, temperatureGraphRect.rect);
                GraphManager.Graph.Plot("ambientTemperature", udpTemperatureScript.temperatures[1], Color.green, ambientTemperature.rect);
                GraphManager.Graph.Plot("force", force, Color.green, forceGraphRect.rect);
            }
            if (timeNow > lastIntervalSlow + updateIntervalSlow)
            {
                lastIntervalSlow = timeNow;
                ambientTemperature.text.text = string.Format("T atm [°C]: {0,5:0.0}", temperatureFiltered[1]);
                temperatureGraphRect.text.text = string.Format("T muscle [°C]: {0,5:0.0}", temperatureFiltered[0]);
                forceGraphRect.text.text = string.Format("Force [N]: {0,5:0.0}", forceFiltered);
                pwm.text.text = string.Format("PWM: {0:0}", pwmControlScript.motors[0].Value);
            }

        }


    }


    Rect InitializePanelTransform(RectTransform panelTransform)
    {
        float xSize = panelTransform.sizeDelta.x * panelTransform.lossyScale.x;
        float ySize = panelTransform.sizeDelta.y * panelTransform.lossyScale.y;
        float xPos = panelTransform.anchoredPosition.x;
        float yPos = -panelTransform.anchoredPosition.y;
        return new Rect(xPos, yPos, xSize, ySize);
    }

    public void ButtonFilterFcn()
    {
        // filter button callbcak function
        buttonFilter.text.text = filterPlot ? "Filter" : "No Filter";
        filterPlot = !filterPlot;
        if (filterPlot)
        {
            Debug.Log("Filter Enabled");
        }
        else
        {
            Debug.Log("Filter disabled");
        }
    }





    public void ButtonStartStopFcn()
    {
        DataLogger.recording = !DataLogger.recording;
        // startStop button callbcak function
        if (DataLogger.recording)
        {
            buttonStartStop.State(new Color32(251, 66, 52, 255),
                "Stop", debugMessage: "Recording started");
        }
        else
        {
            buttonStartStop.State(new Color32(83, 188, 117, 255),
                "Start", debugMessage: "Recording stopped");
        }
    }


    public void ButtonCycleFcn()
    {
        ControlStarted = !ControlStarted;
        if (ControlStarted)
        {
            buttonCycle.State("Manual", debugMessage: "Cycle control started");
        }
        else
        {
            buttonCycle.State("Auto", debugMessage: "Cycle control stopped");
        }
    }

    public static bool on = true;
    public void ButtonDisableEnableFcn()
    {
        on = !on;
        // startStop button callbcak function
        if (on)
        {
            pwmControlScript.ControlStart();
            buttonDisableEnable.State(new Color32(251, 66, 52, 255),
                "Disable", debugMessage: "Heater Enabled");
        }
        else
        {
            pwmControlScript.ControlStop();
            buttonDisableEnable.State(new Color32(83, 188, 117, 255),
                "Enable", debugMessage: "Heater Disabled");
        }
    }

    [System.Serializable]
    public class StateButton
    {// helper class for two state button display
        public Transform transform;
        public Text text;
        public Image image;
        public StateButton(Transform buttonTransform)
        {
            transform = buttonTransform;
            text = transform.GetComponentInChildren<Text>();
            image = transform.GetComponent<Image>();
        }
        public void State(Color color, string name, string debugMessage)
        {
            image.color = color;
            text.text = name;
            Debug.Log(debugMessage);
        }
        public void State(string name, string debugMessage)
        {
            text.text = name;
            Debug.Log(debugMessage);
        }
    }

    [System.Serializable]
    public class GraphRect
    {// helper class for two state button display
        public RectTransform transform;
        public Text text;
        public Rect rect;
        public GraphRect(RectTransform panelTransform)
        {
            transform = panelTransform;
            text = transform.GetComponentInChildren<Text>();
            rect = InitializePanelTransform(transform);
        }

        Rect InitializePanelTransform(RectTransform panelTransform)
        {
            float xSize = panelTransform.sizeDelta.x * panelTransform.lossyScale.x;
            float ySize = panelTransform.sizeDelta.y * panelTransform.lossyScale.y;
            float xPos = panelTransform.anchoredPosition.x;
            float yPos = -panelTransform.anchoredPosition.y;
            return new Rect(xPos, yPos, xSize, ySize);
        }
    }
}
