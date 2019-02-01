using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UDPCommunication;
using System;
using MsgPack.Serialization;
using System.IO;
public class RobotDataLogger : MonoBehaviour
{
    FileWriterThread fileWriterThread;
    public bool recording = false;//whether or not to record the data
    public float updateInterval = 0.01f;
    private float lastInterval = 0;
    RobotLog targetObject;
    public UDPTemperature udpTemperatureScript;
    public RobotPWMControl robotPWMControlScript;

    // serializer option
    SerializationContext ctx;
    MessagePackSerializer serializer;

    float[] temperatures { get { return udpTemperatureScript.temperatures; } }
    uint[] pwm { get { return robotPWMControlScript.pwm; } }
    // Use this for initialization
    private void Awake()
    {
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        robotPWMControlScript = FindObjectOfType<RobotPWMControl>();
    }
    void Start()
    {

        string fileNamePrefix = "data";
        string folderPath = "D:\\Google Drive\\Soft Robot\\Logs\\Robot";
        string filePath = string.Format("{0}/{1}_{2}.msgpack",
            folderPath, fileNamePrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        fileWriterThread = new FileWriterThread(filePath);
        //fileWriterThread.Write(new byte[] { 0x01, 0x02, 0x03 });
        targetObject = new RobotLog();

        ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Map };
        // 1. Create serializer instance.
        serializer = MessagePackSerializer.Get<RobotLog>(ctx);
    }

    // Update is called once per frame
    void Update()
    {
        float timeNow = Time.time;
        if (recording && timeNow > lastInterval + updateInterval)
        {
            targetObject.Update(temperatures, pwm);
            //Debug.Log(string.Format("{0:0.000},{1:0.00},{2:0.00}", targetObject.TimeStamp, targetObject.Temperature, targetObject.Force));
            using (MemoryStream mstream = new MemoryStream())
            {
                serializer.Pack(mstream, targetObject);
                fileWriterThread.Write(mstream.ToArray());
            }

            lastInterval = timeNow;
        }
    }
    private void OnDisable()
    {
        if (fileWriterThread.thread.IsAlive)
            fileWriterThread.StopThread();
    }
    private void OnApplicationQuit()
    {
        OnDisable();
    }

    [System.Serializable]
    public class RobotLog
    {
        [MessagePackMember(0)]
        public int Msgtype { get { return 3; } }
        [MessagePackMember(1)]
        public float[] T { set; get; } // temperature of the muscle
        [MessagePackMember(2)]
        public uint[] PWM { get; set; }
        [MessagePackMember(3)]
        public float TimeStamp { get { return Time.time; } }

        public RobotLog(/*float[] temperatures, uint[] pwm*/)
        {
            //Update(temperatures,pwm);
            Debug.Log("ForceTemperatureLog instantialized");
        }
        public void Update(float[] temperatures, uint[] pwm)
        {
            T = (float[])temperatures.Clone();
            PWM = (uint[])pwm.Clone();
        }

    }
}
