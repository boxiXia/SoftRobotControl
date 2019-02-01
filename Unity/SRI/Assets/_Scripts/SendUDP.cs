/*
 * ref:https://social.msdn.microsoft.com/Forums/en-US/92846ccb-fad3-469a-baf7-bb153ce2d82b/simple-udp-example-code?forum=netfxnetcom
 event manager: https://forum.unity.com/threads/custom-ui-event-system-revamped.353544/
    -----------------------
    UDP-Send
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
    // > gesendetes unter
    // 127.0.0.1 : 8050 empfangen
   
    // nc -lu 127.0.0.1 8050
 
        // todo: shutdown thread at the end
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using MsgPack;
using MsgPack.Serialization;
using UIEventDelegate;
using System.Linq;
public class SendUDP : MonoBehaviour
{
    public ReorderableEventList OnEnableEvents;
    //// read Thread
    //Thread thread;

    // udpclient object
    UdpClient client;
    public string FileName = "data";
    public string IP;  // define in Init
    public int port;  // define in Init
    //private byte[] send_buffer_ex=new byte[20];
    int count = 0;
    int maxRepeatSend = 5;
    public int repeatSendCount = 5;
    //[SerializeField]
    private byte[] send_buffer;

    // "connection" things
    IPEndPoint remoteEndPoint;

    // serializer option
    SerializationContext ctx;

    FileStream fstream;
    MessagePackSerializer serializer;
    BinaryWriter fwriter;
    public bool saveToFile = false;
    void OnEnable()
    {
        if (OnEnableEvents.List.Count > 0)
            EventDelegate.Execute(OnEnableEvents.List);
    }

    private void Awake()
    {
        //QualitySettings.vSyncCount = 0;
        //fstream.Close();
    }
    public void Start()
    {
        if (saveToFile)
        {
            string filePath = String.Format("{0}/{1}_{2}.msgpack", Application.persistentDataPath, FileName, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            print(String.Format("Saved to:{0}_{1}", filePath, Time.time));
            fstream = new FileStream(filePath, FileMode.Create);
            fwriter = new BinaryWriter(fstream);
        }

        ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Array };
        // 1. Create serializer instance.
        //var serializer = MessagePackSerializer.Get(targetObject.GetType(), ctx);
        serializer = MessagePackSerializer.Get<SRController.ControlInput>(ctx);

        //initalize the trackedObject
        // here targetObject initialized in SRController



        Init();
    }

    // Init
    public void Init()
    {
        // Endpoint to which the messages are sent.
        //print("SendUDP.Init()");
        //// define
        //IP = "127.0.0.1";
        //port = 8051;

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        // status
        print("Sending to " + IP + " : " + port);
    }




    private void LateUpdate()
    {
        if (SRController.valueChanged)
        {
            repeatSendCount = maxRepeatSend;
        }

        if (repeatSendCount > 0)
        {
            SendPacket();
            repeatSendCount--;
        }
    }




    void SendPacket()
    {
        MemoryStream mstream = new MemoryStream();
        //update targetObject//
        // 2. Serialize object to the specified mstream.
        serializer.Pack(mstream, SRController.targetObject);
        // Set position to head of the mstream to demonstrate deserialization.
        mstream.Position = 0;
        send_buffer = mstream.ToArray();
        if (saveToFile)
        {
            fwriter.Write(send_buffer);
            if (count > 20)
            {
                fstream.Flush();
                count = 0;
            }
            count++;
        }
        // send send_buffer through UDP
        try
        {
            if (send_buffer != null && send_buffer.Length > 0)
            {
                // Send the message to the remote client.
                client.Send(send_buffer, send_buffer.Length, remoteEndPoint);
            }
        }
        catch (Exception send_exception)
        {
            //exception_thrown = true;
            Debug.Log(string.Format("Exception {0}", send_exception.Message));
        }
    }


    void OnApplicationQuit()
    {
        //print(Time.time);
        try
        {
            client.Close();
            fstream.Close();
            fwriter.Close();

        }
        catch (NullReferenceException exception)
        {
            Debug.Log(string.Format("Exception {0}", exception.Message));
        }
    }



    public void printtest(bool test_value)
    {
        print(String.Format("event allert {0}", test_value));
    }

}


