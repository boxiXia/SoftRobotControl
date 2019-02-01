using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MsgPack;
using MsgPack.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UIEventDelegate;
using UDPCommunication;
public class InputUDP : MonoBehaviour
{
    private UDPClinetThread udpClinet;
    // port number
    public int port = 10001;

    private IPEndPoint localEndPoint;
    [SerializeField]
    public byte[] data;
    public float[] temperatures;
    private float[] _temperatures;
    public float temperature0 = 0;
    public ReorderableEventList onTemperatureUpdate;

    void Start()
    {
        Application.runInBackground = true;
        localEndPoint = new IPEndPoint(IPAddress.Any, port);
        udpClinet = new UDPClinetThread(localEndPoint);
        _temperatures = new float[8];
        //StartCoroutine(OnTemperatureUpdateCoroutine());
    }
    private void Update()
    {
        bool temperatureIsChanged = false;
        if (udpClinet.receiveQueue.Count != 0)
        {
            data = udpClinet.receiveQueue.Dequeue() as byte[];
            temperatureIsChanged = true;
            //Todo,instead of clearing the queue, do something
        }

        //DebugGraph.Log("Temperature",temperatures);
        if (temperatureIsChanged)
        {
            ReceiveData(data);
            for (int i = 0; i < temperatures.Length; i++)
            {
                _temperatures[i] = temperatures[i];
            }
            temperature0 = temperatures[0];

            if (onTemperatureUpdate.List.Count > 0)
                EventDelegate.Execute(onTemperatureUpdate.List);
        }

    }
    //private IEnumerator OnTemperatureUpdateCoroutine(float waitTime = 0.015f)
    //{
    //    while (onTemperatureUpdate.List.Count > 0)
    //    {
    //        yield return new WaitForSeconds(waitTime);
    //        EventDelegate.Execute(onTemperatureUpdate.List);
    //    }    
    //}
    // Unity Application Quit Function
    void OnApplicationQuit()
    {
        if (udpClinet.thread.IsAlive)
            udpClinet.StopThread();
    }


    // Unpack data
    private void ReceiveData(byte[] data)
    {
        try
        {
            using (var unpacker = Unpacker.Create(data))
            {
                //var unpacked_data = unpacker.ReadItem().Value.AsList();//simplify to:
                var unpacked_data = unpacker.ReadItemData().AsList();

                if ((bool)unpacked_data[0].IsTypeOf<uint>() && (uint)unpacked_data[0] == 6)
                {
                    if (unpacked_data[1].IsArray)
                    {
                        var temp = unpacked_data[1].AsList();
                        temperatures = new float[temp.Count];
                        for (int i = 0; i < temp.Count; i++)
                        {
                            temperatures[i] = (float)temp[i].AsDouble();
                        }
                    }
                }
                else
                {
                    print("expected uint(6)");
                }
            }
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
}
