using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgPack.Serialization;
using UIEventDelegate;
using System;
using System.Collections.Specialized;
public class SRController : MonoBehaviour
{
    [SerializeField]
    public Motor[] motors;
    public static ControlInput targetObject;
    public ReorderableEventList OnValueChanged;
    private static int numMotors;
    public static int repeatSendCount = 3 * 2;
    public static bool valueChanged = true;
    public Cycle cycle;
    public bool CycleControl = false;
    [Range(0, 255u)]
    public uint maxPWM = 255;
    [SerializeField]
    [Range(0,1f)]
    public float[] normalizedMotorPhase;
    public Text[] temperatureUI;
    private Image[] sliderBackgroundImages;
    public float[] temperatureThreshold;
    public InputUDP inputUDP;
    public bool useTemperatureFeedback = false;
    public float[] temperatureCorrections;
    void OnEnable()
    {
        if (OnValueChanged.List.Count > 0)
            EventDelegate.Execute(OnValueChanged.List);
    }


    private void Awake()
    {
        inputUDP= GameObject.Find(string.Format("UDPconnection")).GetComponent<InputUDP>();
        temperatureUI = new Text[6];
        sliderBackgroundImages = new Image[6];
        numMotors = motors.GetLength(0);
        for (int i = 0; i < numMotors; i++)
        {
            var temp = GameObject.Find(string.Format("Canvas/Panel/SliderPanel/Slider{0}", i));
            temperatureUI[i] = temp.transform.Find("Temperature").GetComponent<Text>();
            sliderBackgroundImages[i]=temp.transform.Find("Background").GetComponent<Image>();
            motors[i].sliderUI = temp.GetComponent<Slider>();
            motors[i].sliderUI.interactable = motors[i].On;
            //motors[i].Value = (byte)motors[i].sliderUI.GetComponent<Slider>().value;
            motors[i].sliderUI.value = (float)motors[i].Value;
            motors[i].sliderUI.maxValue = (float)maxPWM;
        }
        // initialize targetObject, which will be serialized and sent via UDP
        targetObject = new ControlInput(motors);
    }



    // Use this for initialization
    void Start()
    {
        print(useTemperatureFeedback ? "Use Temperature feedback for PWM." : "Open loop Control.");
    }

    // Update is called once per frame
    void Update()
    {
        //if (reverse)
        //{
        //    motors[1].Value--;
        //    if (motors[1].Value < 1)
        //    {
        //        reverse = false;
        //    }
        //}
        //else
        //{
        //    motors[1].Value++;
        //    if (motors[1].Value > 254)
        //    {
        //        reverse = true;
        //    }
        //}

        if (CycleControl)
        {
            for (int i = 0; i < 4; i++)
            {
                motors[i].Value = (byte)cycle.Getpwm(Time.time + normalizedMotorPhase[i] * cycle.T, useTemperatureFeedback,
                    inputUDP.temperatures[i], temperatureCorrections[i]);
            }

        }


        valueChanged = false;
        for (int i = 0; i < numMotors; i++)
        {
            if (targetObject.PWM[i] != motors[i].Value)
            {
                targetObject.PWM[i] = motors[i].On ? (uint)motors[i].Value : 0;
                motors[i].sliderUI.value = motors[i].On ? (float)motors[i].Value : 0;
                //print("value changed");
                valueChanged = true;
            }
        }
        if (valueChanged)
        {
            if (OnValueChanged.List.Count > 0)
                EventDelegate.Execute(OnValueChanged.List);
        }
    }

    private void LateUpdate()
    {
        DebugGraph.Log("PWM", targetObject.PWM);
        DebugGraph.Log("Temperature", inputUDP.temperatures);
    }



    public void ControlStop()
    {
        foreach (var motor in motors)
        {

            //grey out and reset sliderUI
            motor.sliderUI.interactable = false;
            motor.sliderUI.value = 0;
            motor.On = false;
        }
    }

    public void ControlStart()
    {
        foreach (var motor in motors)
        {
            motor.sliderUI.interactable = true;
            motor.On = true;
            motor.Value = 0;
        }
    }

    public void OnSliderUIValueChanged(int motorNum, byte value)
    {
        motors[motorNum].Value = value;
    }

    public void OnTemperatureUpdateFn()
    {
        for (int i = 0; i < inputUDP.temperatures.Length; i++)
        {
            float temperature_i = inputUDP.temperatures[i];
            //float temperature_i = mock_temperatures[i];

            temperatureUI[i].text = temperature_i.ToString("0.0");

            if (temperature_i < temperatureThreshold[0])
            {
                temperatureUI[i].color = Color.white;
            }
            else if (temperature_i < temperatureThreshold[1])
            {
                //float t = (temperature_i - temperatureThreshold[0]) / (temperatureThreshold[0] - temperatureThreshold[0]);
                //temperatureUI[i].color = Color.Lerp(Color.blue, Color.green, t);
                temperatureUI[i].color = Color.green;

            }
            else if (temperature_i < temperatureThreshold[2])
            {
                //float t = (temperature_i - temperatureThreshold[1]) / (temperatureThreshold[2] - temperatureThreshold[1]);
                //temperatureUI[i].color = Color.Lerp(Color.green, Color.red, t);
                temperatureUI[i].color = Color.yellow;
            }
            else
            {
                temperatureUI[i].color = Color.red;
            }
            float t2 = (temperature_i - temperatureThreshold[0]) / (temperatureThreshold[temperatureThreshold.Length - 1] - temperatureThreshold[0]);
            //temperatureUI[i].color = ColoarMap.GetColor(t);
            sliderBackgroundImages[i].color = ColoarMap.GetColor(t2);
        }
    }



    [System.Serializable]
    public class Motor
    {
        public bool On;
        public byte Value;
        public Slider sliderUI;
    }

    [System.Serializable]
    public class ControlInput
    {
        [MessagePackMember(0)]
        public int Msgtype { set; get; }
        //public int[] head { set; get; }
        [MessagePackMember(1)]
        //public Dictionary<int, int> PWM { get; set; }
        public uint[] PWM { get; set; }
        public ControlInput(Motor[] motors)
        {
            //print(motors);
            Msgtype = 7;
            //PWM = new Dictionary<int, int>();
            PWM = new uint[motors.GetLength(0)];
            for (int i = 0; i < motors.GetLength(0); i++)
            {
                //PWM.Add((int)i, motors[i].On ? motors[i].Value : (int)0);
                PWM[i] = motors[i].On ? motors[i].Value : (uint)0;
            }
        }
    }

    [System.Serializable]
    public class Cycle
    {
        [Tooltip("x:normalized time, y:normalized voltage, z:temperature")]
        public Vector3[] nTimeVolatges;//normalized time and voltage and temperature threshold
        public float T; // period
        public uint pwm=255; // Voltage
        //return the pwm
        public uint Getpwm(float t,bool useTemperatureFeedback=false, float temperature=-1000,float temperatureCorrection = 1f)
        {
            //float t_r = Mathf.Repeat(t,T);//remainder
            float t_r = t / T - Mathf.Floor(t/T);
            if (useTemperatureFeedback)
            {

            }
            int i;
            for (i = 0; i < nTimeVolatges.Length-1; i++)
            {
                if (t_r <= nTimeVolatges[i].x)
                {
                    break;
                }
            }
            uint pwmNow = (uint)(nTimeVolatges[i].y * (float)pwm);

            return useTemperatureFeedback? pwmNow*(uint)(temperature> nTimeVolatges[i].z* temperatureCorrection? 0:1) : pwmNow;
        }





    }

}
