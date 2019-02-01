﻿using System;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using UnityEngine;
namespace SerialCommunicationCsharp
{
    public class SerialThread
    {
        // please refer to https://www.alanzucconi.com/2016/12/01/asynchronous-serial-communication/
        // This is a class that enables asynchronous serial communication, based on Alan Zucconi's article
        public SerialPort serialPort;
        public byte[] buffer;
        public string message;
        public Queue transmitQueue;  // queue to send command
        public Queue receiveQueue;   // queue to receive info
        public Thread thread;
        public bool looping = true;
        public SerialThread(string portName = "COM3", int readBufferSize = 12, int baudRate = 38400, bool startNow = true)
        {
            serialPort = new SerialPort
            {
                PortName = portName,
                ReadBufferSize = readBufferSize,
                BaudRate = baudRate,
                DataBits = 8,
                Handshake = Handshake.None,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 500,
                WriteTimeout = 500,
            };
            // Creates the thread
            transmitQueue = Queue.Synchronized(new Queue());
            receiveQueue = Queue.Synchronized(new Queue());
            thread = new Thread(ThreadLoop);
            //{
            //    IsBackground = true
            //};
            if (startNow)
            {
                StartThread();
            }
        }
        public void StartThread()
        {
            thread.Start();
        }
        public virtual void ThreadLoop()
        {
            // runs the looping
            // step 1: Opens the connection on the serial port
            serialPort.Open();
            // step 2: initialization(optional)

            // step 3: start looping
            while (IsLooping())
            {
                //do something here
                // e.g ReadSerial()
            }
            // step 4: close the serial port
            serialPort.Close();
        }

        // For status checking
        public bool IsLooping()
        {
            lock (this)
            {
                return looping;
            }
        }

        // Stopt the thread
        public void StopThread()
        {
            lock (this)
            {
                looping = false;
            }
        }
        public virtual void ReadSerial()
        {

        }
        public void Write<T>(T command)
        {
            transmitQueue.Enqueue(command);
        }
    }


    public class FSE1001
    {
        public UInt32 Timestamp { get; set; }
        public float ForceZ { get; set; }
        public FSE1001(UInt32 timestamp = 0, float forceZ = 0)
        {
            Timestamp = timestamp;
            ForceZ = forceZ;
        }
        public void Print()
        {
            //Console.WriteLine(String.Format("{0:0}: {1,5:0.0}", Timestamp, ForceZ));
            Debug.Log(String.Format("{0:0}: {1,5:0.0}", Timestamp, ForceZ));
        }
    }

    public class SerialThreadFSE1001 : SerialThread
    {
        public SerialThreadFSE1001(string portName = "COM3", int readBufferSize = 12, int baudRate = 38400, bool startNow = true) : base(portName, readBufferSize, baudRate, startNow)
        {
        }

        public void ReadSerial(ref bool success, ref uint timestamp, ref float forceZ)
        {
            success = false;
            try
            {
                //Initialize a buffer to hold the received data 
                buffer = new byte[serialPort.ReadBufferSize];
                int bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                if (bytesRead == 1)//check how many bytes are read
                {
                    bytesRead = serialPort.Read(buffer, 1, buffer.Length - 1);
                }
                if (buffer[0] == 0x0D && buffer[1] == 0x0C && buffer[11] == 0xFF) //check for data correctness
                {
                    timestamp = ((uint)buffer[3] << 24)
                                    | (((uint)buffer[4]) << 16)
                                    | (((uint)buffer[5]) << 8)
                                    | (((uint)buffer[6]));
                    forceZ = BitConverter.ToSingle(new byte[4] { buffer[10], buffer[9], buffer[8], buffer[7] }, 0);
                    //Console.WriteLine(String.Format("{0:0} - {1} : {2,5:0.0}", timestamp, ByteArrayToString(buffer), forceZ));
                    success = true;
                }
            }
            catch (TimeoutException)
            {
                // do nothing
            }
        }

        public override void ThreadLoop()
        {
            // Opens the connection on the serial port
            serialPort.Open();
            //serialPort.Write(new char[] { 'z' }, 0, 1); // initialize the force sensor
            bool success = false;
            uint timestamp = 0;
            float forceZ = 0;

            // Looping
            while (IsLooping())
            {
                // Send to Serial
                if (transmitQueue.Count != 0)
                {
                    var command = transmitQueue.Dequeue();
                    if (command is byte[])
                        serialPort.Write((byte[])command, 0, ((byte[])command).Length);
                    else if (command is char[])
                        serialPort.Write((char[])command, 0, ((char[])command).Length);
                    else
                        serialPort.Write((string)command);
                }
                // Read from Serial
                ReadSerial(ref success, ref timestamp, ref forceZ);
                if (success)
                {
                    //Console.WriteLine(String.Format("{0:0}: {1,5:0.0}", timestamp, forceZ));
                    receiveQueue.Enqueue(new FSE1001(timestamp, forceZ));
                }
                Thread.Sleep(5);
            }
            serialPort.Close();
        }

        public void InitializeSensor()
        {
            transmitQueue.Enqueue(new char[] { 'z' });// initialize the force sensor
            //Console.WriteLine("FSE1001 zeroed!");
            Debug.Log("FSE1001 zeroed!");
        }
    }

}
