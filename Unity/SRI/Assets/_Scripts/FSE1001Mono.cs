using SerialCommunicationCsharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class FSE1001Mono : MonoBehaviour
{
    string portName { get { return pars.forceSensorPortName; } }

    private SerialThreadFSE1001 sensor;
    public Parameters pars;
    FSE1001 sample;
    public float forceZ = 0;
    // Use this for initialization
    void Start()
    {
        var pars_list = FindObjectOfType<Parameters>().GetComponents<Parameters>();
        foreach (var item in pars_list)
        {
            if (item.enabled)
            {
                pars = item;
                break;
            }
        }

        print(pars.forceSensorPortName);
        sensor = new SerialThreadFSE1001(portName: portName);
        sensor.InitializeSensor();
    }

    // Update is called once per frame
    void Update()
    {
        if (sensor.IsLooping())
        {
            while (sensor.receiveQueue.Count != 0)
            {
                sample = (FSE1001)sensor.receiveQueue.Dequeue();
                //print(String.Format("{0:0}: {1,5:0.0}", sample.Timestamp, sample.ForceZ));
                forceZ = sample.ForceZ;
            }
        }
        //Debug.Log(string.Format("{0},{1}", Screen.width, Screen.height));
    }
    private void OnApplicationQuit()
    {
        sensor.StopThread();
    }

}
