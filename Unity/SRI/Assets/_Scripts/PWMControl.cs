using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgPack.Serialization;
using UIEventDelegate;
using System;
using System.Collections.Specialized;
using System.IO;
public class PWMControl : MonoBehaviour
{
    public UDPTemperature udpTemperatureScript;
    public FSE1001Mono fse1001MonoScript;
    public ForceTemperatureUIControl forceTemperatureUIControlScript;
    public Text[] temperatureUI;
    private Image[] sliderBackgroundImages;

    public float updateInterval = 0.01f;
    private float lastInterval = 0;

    [SerializeField]
    public Motor[] motors;
    public byte[] previousPWMs;
    //public static ControlInput targetObject;
    //public ReorderableEventList OnValueChanged;
    public int numMotors => motors.GetLength(0);
    //public static int repeatSendCount = 3 * 2;
    public static bool valueChanged = true;
    [Range(0, 255u)]
    public uint maxPWM = 255;
    [SerializeField]
    public float[] temperatureThreshold;
    UDPServerPWM udpServerPWMScript;
    
    public enum ForceMode
    {
        Compression = 0,
        Extension = 1
    };
    [Space(10)]
    [Header("--------------AutoControl Setup--------------")]
    [Tooltip("compression is for block force")]
    public ForceMode forceMode = ForceMode.Compression;
    public ForceCycleController controller;

    private void Awake()
    {
        fse1001MonoScript = FindObjectOfType<FSE1001Mono>();
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        udpServerPWMScript = FindObjectOfType<UDPServerPWM>();
        forceTemperatureUIControlScript= FindObjectOfType <ForceTemperatureUIControl>();
        temperatureUI = new Text[6];
        sliderBackgroundImages = new Image[6];
        previousPWMs = new byte[numMotors];
        for (int i = 0; i < numMotors; i++)
        {
            var temp = GameObject.Find(string.Format("Canvas/PWM Sliders/PWM Slider {0}", i));
            temperatureUI[i] = temp.transform.Find("Temperature").GetComponent<Text>();
            sliderBackgroundImages[i] = temp.transform.Find("Background").GetComponent<Image>();
            motors[i].sliderUI = temp.GetComponent<Slider>();
            motors[i].sliderUI.interactable = motors[i].On;
            motors[i].sliderUI.value = (float)motors[i].Value;
            motors[i].sliderUI.maxValue = (float)maxPWM;
            previousPWMs[i] = motors[i].Value;
        }
    }



    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float timeNow = Time.time;
        if (timeNow > lastInterval + updateInterval)
        {
            lastInterval = timeNow;

            if (controller.started)
            {
                for (int i = 0; i < numMotors; i++)
                {
                    
                    //// this is only for one motor
                    //motors[i].Value = controller.GetPWM(udpTemperatureScript.temperatures[0],
                    //    forceMode == ForceMode.Compression ? fse1001MonoScript.forceZ : -fse1001MonoScript.forceZ);
                    motors[i].Value = controller.GetPWM(forceTemperatureUIControlScript.temperatureFiltered[i],
    forceMode == ForceMode.Compression ? forceTemperatureUIControlScript.forceFiltered : -forceTemperatureUIControlScript.forceFiltered);
                }

            }
            valueChanged = false;
            for (int i = 0; i < numMotors; i++)
            {
                if (!motors[i].On)
                {
                    motors[i].Value = 0;
                }
                if (previousPWMs[i] != motors[i].Value)
                {
                    previousPWMs[i] = motors[i].Value;
                    motors[i].sliderUI.value = (float)motors[i].Value;
                    valueChanged = true;
                }
            }
            //if (valueChanged)
            //{
            //    if (OnValueChanged.List.Count > 0)
            //        EventDelegate.Execute(OnValueChanged.List);
            //}
            udpServerPWMScript.UpdatePWM(Array.ConvertAll(previousPWMs, item => (uint)item));
        }

    }

    public void ControlStop()
    {
        foreach (var motor in motors)
        {//grey out and reset sliderUI
            motor.ControlStop();
        }
    }

    public void ControlStart()
    {
        foreach (var motor in motors)
        {
            motor.ControlStart();
        }
    }

    public void OnSliderUIValueChanged(int motorNum)
    {
        motors[motorNum].Value = (byte)motors[motorNum].sliderUI.value;
    }


    public void OnSliderUIValueChanged(int motorNum, byte value)
    {
        motors[motorNum].Value = value;
    }

    public void OnTemperatureUpdateFn()
    {
        for (int i = 0; i < numMotors; i++)
        {
            float temperature_i = udpTemperatureScript.temperatures[i];
            temperatureUI[i].text = temperature_i.ToString("0.0");
            if (temperature_i < temperatureThreshold[0])
            {
                temperatureUI[i].color = Color.white;
            }
            else if (temperature_i < temperatureThreshold[1])
            {
                temperatureUI[i].color = Color.green;
            }
            else if (temperature_i < temperatureThreshold[2])
            {
                temperatureUI[i].color = Color.yellow;
            }
            else
            {
                temperatureUI[i].color = Color.red;
            }
            float t2 = (temperature_i - temperatureThreshold[0]) / (temperatureThreshold[temperatureThreshold.Length - 1] - temperatureThreshold[0]);
            sliderBackgroundImages[i].color = ColoarMap.GetColor(t2);
        }
    }



    [System.Serializable]
    public class Motor
    {
        public bool On;
        public byte Value;
        public Slider sliderUI;

        public void ControlStop()
        {
            //grey out and reset sliderUI
            sliderUI.interactable = false;
            sliderUI.value = 0;
            On = false;
        }

        public void ControlStart()
        {
            sliderUI.interactable = true;
            On = true;
            Value = 0;
        }
    }




    [System.Serializable]
    public class ForceCycleController
    {
        public bool started = false;
        private bool started_ = false;

        public CycleCriterion firstCycleCriterion;

        public Vector2 fLimit;
        private float fMin { get { return fLimit.x; } set { fLimit.x = value; } }
        private float fMax { get { return fLimit.y; } set { fLimit.y = value; } }
        public float MaxAllowableTemperature = 145f;
        public Vector2 TLimit;
        private float Tl { get { return TLimit.x; } set { TLimit.x = value; } }
        private float Th { get { return TLimit.y; } set { TLimit.y = value; } }
        public byte uMin = 0;
        public byte uMax = 255;

        [Space(10)]
        [Header("read-only: general")]
        public bool firstCycle = true;
        public bool timmerExpired = false;
        public bool heating = true;
        public byte u = 0;
        public int cycleCount = 0;
        [Header("read-only: First Cycle Action Based On Temperature")]
        public bool fMinLogged = false;
        public bool fMaxLogged = false;
        [Header("read-only: First Cycle Action Based On Force")]
        public bool TlLogged = false;
        public bool ThLogged = false;

        [Space(5)]
        [Header("read-only: time(s)")]
        public float timeAtStart = 0;
        public float time0Tl = 0;//tl0,first time at Tl
        public float time0Th = 0;//th0,first time at Th
        public float time1Tl = 0;//tl1,second time at Tl
        public float tFirstCycle = 0;
        public float tHeating0 = 0;
        public float tHeating = 0;
        public float timeCycleBegin = 0;
        public enum CycleCriterion
        {
            Force = 0,
            Temperature = 1
        };

        private void FirstCycleActionBasedOnTemperature(ref float T, ref float f)
        {
            if (!fMinLogged)
            {
                if (T > Tl)
                {
                    fMin = f;
                    fMinLogged = true;
                    time0Tl = Time.time;
                }
                u = uMax;
                heating = true;
            }
            else if (!fMaxLogged)
            {
                if (T > Th)
                {
                    fMax = f;
                    fMaxLogged = true;
                    time0Th = Time.time;
                    u = uMin;
                    heating = false;
                }
                else
                {
                    u = uMax;
                    heating = true;
                }
            }
            else if (f <= fMin)
            {
                FirstCycleExpiredMethod(ref T);
            }
            else
            {
                heating = false;
                u = uMin;
            }
        }

        private void FirstCycleActionBasedOnForce(ref float T, ref float f)
        {
            if (!TlLogged)
            {
                if (f > fMin)
                {
                    TlLogged = true;
                    time0Tl = Time.time;
                }
                u = PwmFn(ref T, ref heating);
                heating = true;
            }
            else if (!ThLogged)
            {
                if (f > fMax)
                {
                    ThLogged = true;
                    time0Th = Time.time;
                    heating = false;
                    u = PwmFn(ref T, ref heating);
                }
                else
                {
                    heating = true;
                    u = PwmFn(ref T, ref heating);
                }
            }
            else if (f <= fMin)
            {
                FirstCycleExpiredMethod(ref T);
            }
            else
            {
                heating = false;
                u = PwmFn(ref T, ref heating);
            }
        }


        void FirstCycleExpiredMethod(ref float T)
        {
            time1Tl = Time.time;
            tFirstCycle = time1Tl - time0Tl;
            tHeating0 = time0Th - time0Tl;
            firstCycle = false;
            cycleCount += 1;
            timeCycleBegin = time1Tl;
            tHeating = 0;
            heating = true;
            u = PwmFn(ref T, ref heating);
        }

        public byte GetPWM(float T, float f)
        {
            if (T > MaxAllowableTemperature)
            {
                Debug.LogWarning(string.Format("Temperature({0:0.0}°C) exceeds maximum temperature({1:0.0}°C)! Experiment Terminated", T, MaxAllowableTemperature));
                firstCycle = false;
                timmerExpired = true;
                started = false;
            }
            if (started_ == false && started == true)
            {
                timeAtStart = Time.time;
                timeCycleBegin = timeAtStart;
                started_ = true;
            }
            if (started)
            {

                if (firstCycle&& !timmerExpired)
                {
                    if (firstCycleCriterion == CycleCriterion.Temperature)
                    {
                        FirstCycleActionBasedOnTemperature(ref T, ref f);
                    }
                    else//firstCycleCriterion== CycleCriterion.Force
                    {
                        FirstCycleActionBasedOnForce(ref T, ref f);
                    }

                }
                else
                {
                    if (!timmerExpired)
                    {
                        if (heating)
                        {
                            //tHeating += Time.deltaTime * Time.timeScale;
                            tHeating = Time.time - timeCycleBegin;
                        }
                        else
                        {
                            tHeating = 0;
                        }
                        if (tHeating > 2 * tHeating0)
                        {
                            Debug.Log("timer expired: tHeating > 2 * tHeating0");
                            timmerExpired = true;
                            cycleCount += 1;
                        }
                        else if (f >= fMax)
                        {
                            heating = false;
                            u = uMin;
                        }
                        else if (f < fMin)
                        {
                            if (!heating)
                            {
                                cycleCount += 1;
                                timeCycleBegin = Time.time;
                            }
                            heating = true;
                            u = uMax;
                        }
                    }
                    else
                    {
                        heating = false;
                        u = uMin;
                    }
                }
                return u;
            }
            return 0;
        }

        public byte PwmFn(ref float T, ref bool heating)
        {// calculate the pwm output based on the temperatue
            //if(T < MaxAllowableTemperature)
            {
                if (heating)
                {
                    return uMax;
                }
                else
                {
                    return uMin;
                }
            }
            //else
            //{
            //    Debug.LogWarning(string.Format("Temperature({0:0.0}°C) exceeds maximum temperature({1:0.0}°C)! Experiment Terminated", T, MaxAllowableTemperature));
            //    firstCycle = false;
            //    timmerExpired = true;
            //    return 0;
            //}

        }

    }

}
