using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotUI : MonoBehaviour
{
    UDPTemperature udpTemperatureScript;
    public RobotPWMControl robotPWMControlScript;
    RobotDataLogger dataLoggerScript;
    public GraphPlot[] temperaturePlots;
    public Slider[] pwmSliders;

    public Transform buttonFilterTransform;
    private StateButton buttonFilter;

    public Transform buttonStartStopTransform;
    private StateButton buttonStartStop;

    public Transform buttonDisableEnableTransform;
    private StateButton buttonDisableEnable;

    public Transform buttonCycleTransform;
    private StateButton buttonCycle;

    public bool overWriteRangeSetting = false;
    public float minValue = float.NegativeInfinity;
    public float maxValue = float.PositiveInfinity;


    public bool filterPlot = false;

    public bool ControlStarted { get { return robotPWMControlScript.controller.started; } set { robotPWMControlScript.controller.started = value; } }
    public bool Recording { get { return dataLoggerScript.recording; }set { dataLoggerScript.recording = value; } }
    private void Awake()
    {
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        robotPWMControlScript = FindObjectOfType<RobotPWMControl>();
        dataLoggerScript = FindObjectOfType<RobotDataLogger>();
    }
    // Use this for initialization
    void Start()
    {
        buttonFilter = new StateButton(buttonFilterTransform);
        buttonFilter.button.onClick.AddListener(ButtonFilterFcn);

        buttonStartStop = new StateButton(buttonStartStopTransform);
        buttonStartStop.button.onClick.AddListener(ButtonStartStopFcn);

        buttonDisableEnable = new StateButton(buttonDisableEnableTransform);
        buttonDisableEnable.button.onClick.AddListener(ButtonDisableEnableFcn);

        buttonCycle = new StateButton(buttonCycleTransform);
        buttonCycle.button.onClick.AddListener(ButtonCycleFcn);

        if (overWriteRangeSetting)
        {
            for (int i = 0; i < robotPWMControlScript.numMotors; i++)
            {
                temperaturePlots[i].maxValue = maxValue;
                temperaturePlots[i].minValue = minValue;
                temperaturePlots[i].useMaxMinAsRange = true;
            }
        }
        //float minValue = float.m
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < udpTemperatureScript.temperatures.GetLength(0); i++)
        {
            temperaturePlots[i].value = udpTemperatureScript.temperatures[i];
        }

        for (int i = 0; i < robotPWMControlScript.numMotors; i++)
        {
            if(robotPWMControlScript.pwm[i] == robotPWMControlScript.previousPWM[i])
            {
                robotPWMControlScript.pwm[i] = (uint)pwmSliders[i].value;
            }
            else if(pwmSliders[i].value != robotPWMControlScript.pwm[i])
            {
                pwmSliders[i].value = robotPWMControlScript.pwm[i];
            }
        }
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
        Recording = !Recording;
        // startStop button callbcak function
        if (Recording)
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
            ControlEnable(true);
            buttonDisableEnable.State(new Color32(251, 66, 52, 255),
                "Disable", debugMessage: "Heater Enabled");
        }
        else
        {
            ControlEnable(false);
            buttonDisableEnable.State(new Color32(83, 188, 117, 255),
                "Enable", debugMessage: "Heater Disabled");
        }
    }


    public void ControlEnable(bool enable)
    {
        foreach (var slider in pwmSliders)
        {
            slider.interactable = enable;
            slider.value = 0;
        }
    }

    [System.Serializable]
    public class StateButton
    {// helper class for two state button display
        public Transform transform;
        public Text text;
        public Image image;
        public Button button;
        public StateButton(Transform buttonTransform)
        {
            transform = buttonTransform;
            text = transform.GetComponentInChildren<Text>();
            image = transform.GetComponent<Image>();
            button = transform.GetComponent<Button>();
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
}
