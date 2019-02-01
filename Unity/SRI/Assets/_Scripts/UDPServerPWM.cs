using System.Collections;
using System.Collections.Generic;
using UDPCommunication;
using System.Net;
using MsgPack.Serialization;
using System.IO;
using System;
using UnityEngine;
public class UDPServerPWM : MonoBehaviour {
    public Parameters pars;


    public UDPServerThread udpServer;
    // Use this for initialization
    public string remoteIP { get { return pars.remoteIPAddress; } } // define in Init
    public int remoteport { get { return pars.remotePort; } } // define in Init
    private IPEndPoint remoteEndPoint;

    //private PWMControl pwmControlScript;
    public ControlInput targetObject;
    public byte[] command;

    // serializer option
    SerializationContext ctx;
    MessagePackSerializer serializer;


    private void Awake()
    {
        //pwmControlScript = FindObjectOfType<PWMControl>();
    }
    void Start () {

        var pars_list = FindObjectOfType<Parameters>().GetComponents<Parameters>();
        foreach (var item in pars_list)
        {
            if (item.enabled)
            {
                pars = item;
                break;
            }
        }





        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remoteport);
        udpServer = new UDPServerThread(remoteEndPoint);

        // initialize targetObject, which will be serialized and sent via UDP
        targetObject = new ControlInput();
        ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Array };
        // 1. Create serializer instance.
        serializer = MessagePackSerializer.Get<ControlInput>(ctx);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnApplicationQuit()
    {
        udpServer.StopThread();
    }


    public void UpdatePWM(uint [] pwm)
    {
        targetObject.PWM = (uint[])pwm.Clone();

        using (MemoryStream mstream = new MemoryStream())
        {
            serializer.Pack(mstream, targetObject);
            command = mstream.ToArray();
            udpServer.Write(command);//write to udp
            //udpServer.Write(command);//write to udp, write twice to ensure received
            //udpServer.Write(command);//write to udp, write twice to ensure received
        }
    }


    [System.Serializable]
    public class ControlInput
    {
        [MessagePackMember(0)]
        public int Msgtype;
        [MessagePackMember(1)]
        public uint[] PWM;

        public ControlInput()
        {
            //print(motors);
            Msgtype = 7;
            ////PWM = new Dictionary<int, int>();
            //PWM = new uint[motors.GetLength(0)];
            //for (int i = 0; i < motors.GetLength(0); i++)
            //{
            //    //PWM.Add((int)i, motors[i].On ? motors[i].Value : (int)0);
            //    PWM[i] = motors[i].On ? motors[i].Value : (uint)0;
            //}
        }
    }
}
