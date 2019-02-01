using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UDPCommunication;
using System;
using MsgPack.Serialization;
using System.IO;
public class DataLogger : MonoBehaviour {
    FileWriterThread fileWriterThread;
    public static bool recording = false;//whether or not to record the data
    public float updateInterval = 0.01f;
    private float lastInterval = 0;
    ForceTemperatureLog targetObject;
    public FSE1001Mono fse1001MonoScript;
    public UDPTemperature udpTemperatureScript;
    public PWMControl pwmControlScript;

    // serializer option
    SerializationContext ctx;
    MessagePackSerializer serializer;

    public float r_wire;
    public float r_coil;
    public float Muscle_Diameter;

    float temperature { get { return udpTemperatureScript.temperatures[0]; } }
    float temperature_env { get { return udpTemperatureScript.temperatures[1]; } }
    uint pwm { get { return (uint)pwmControlScript.motors[0].Value; } }
    float force { get { return fse1001MonoScript.forceZ; } }
    // Use this for initialization
    void Start () {
        fse1001MonoScript = FindObjectOfType<FSE1001Mono>();
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        pwmControlScript = FindObjectOfType<PWMControl>();

        string fileNamePrefix = "data";
        string folderPath = "D:\\Google Drive\\Soft Robot\\Logs\\Force Temperature";
        string filePath = string.Format("{0}/{1}_{2}.msgpack", 
            folderPath, fileNamePrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        fileWriterThread = new FileWriterThread(filePath);
        //fileWriterThread.Write(new byte[] { 0x01, 0x02, 0x03 });
        targetObject = new ForceTemperatureLog();

        ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Map };
        // 1. Create serializer instance.
        serializer = MessagePackSerializer.Get<ForceTemperatureLog>(ctx);
    }
	
	// Update is called once per frame
	void Update () {
        float timeNow = Time.time;
        if (recording && timeNow > lastInterval + updateInterval)
        {
            targetObject.Update(temperature,temperature_env, force,pwm,r_wire,r_coil, Muscle_Diameter);
            //Debug.Log(string.Format("{0:0.000},{1:0.00},{2:0.00}", targetObject.TimeStamp, targetObject.Temperature, targetObject.Force));
            using(MemoryStream mstream = new MemoryStream())
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
    public class ForceTemperatureLog
    {
        [MessagePackMember(0)]
        public int Msgtype { get { return 2; } }
        [MessagePackMember(1)]
        public float F { set; get; } // force
        [MessagePackMember(2)]
        public float T { set; get; } // temperature of the muscle
        [MessagePackMember(3)]
        public float T0 { set; get; } // Environment temperature
        [MessagePackMember(4)]
        public uint PWM { get; set; }
        [MessagePackMember(5)]
        public float Rw { set; get; }
        [MessagePackMember(6)]
        public float Rc { set; get; }
        [MessagePackMember(7)]
        public float MDia { set; get; }
        [MessagePackMember(8)]
        public float TimeStamp { get {return Time.time; } }

        public ForceTemperatureLog(float temperature=0,float temperatrue_env = 0, float force = -300f,uint pwm = 0,float r_wire=-1, float r_coil=-1, float Muscle_Diameter = -1)
        {
            Update(temperature, temperatrue_env, force, pwm,r_wire,r_coil, Muscle_Diameter);
            Debug.Log("ForceTemperatureLog instantialized");
        }
        public void Update(float temperature, float temperatrue_env,float force, uint pwm, float r_wire,float r_coil, float Muscle_Diameter)
        {
            T = temperature;
            T0 = temperatrue_env;
            F = force;
            PWM = pwm;
            Rw = r_wire;
            Rc = r_coil;
            MDia = Muscle_Diameter;
        }

    }
}
