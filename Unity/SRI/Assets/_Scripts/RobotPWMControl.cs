using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class RobotPWMControl : MonoBehaviour
{
    UDPTemperature udpTemperatureScript;
    UDPServerPWM udpServerPWMScript;
    public float updateInterval = 0.01f;
    private float lastInterval = 0;
    [Range(0, 255)]
    [Header("pwm:[0,255]")]
    public uint[] pwm;
    [HideInInspector]
    public uint[] previousPWM;
    public int numMotors => pwm.GetLength(0);
    public bool valueChanged = true;
    public Controller controller;
    private void Awake()
    {
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        udpServerPWMScript = FindObjectOfType<UDPServerPWM>();
        previousPWM = new uint[numMotors];
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
            valueChanged = false;
            for (int i = 0; i < numMotors; i++)
            {
                if (previousPWM[i] != pwm[i])
                {
                    valueChanged = true;
                    previousPWM = (uint[])pwm.Clone();
                    break;
                }
            }
            //udpServerPWMScript.UpdatePWM(Array.ConvertAll(pwms, item => (uint)item));
            udpServerPWMScript.UpdatePWM(pwm);
        }
        //if (valueChanged)
        //{
        //    if (OnValueChanged.List.Count > 0)
        //        EventDelegate.Execute(OnValueChanged.List);
        //}


    }


    [System.Serializable]
    public class Controller
    {
        public bool started = false;
        private bool started_ = false;
    }
}