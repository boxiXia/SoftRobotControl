using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;

namespace UDPCommunication
{
    
    public class UDPServerThread
    {
        public static int threadSleepTime = 5;//ms
        public bool looping = true;
        public IPEndPoint RemoteEndPoint;
        public UdpClient server;
        public Thread thread;
        public Queue transmitQueue;  // queue to send command

        public UDPServerThread(IPEndPoint remoteEndPoint, bool startNow = true)
        {

            server = new UdpClient();
            RemoteEndPoint = remoteEndPoint;
            Debug.Log(string.Format("UDPServerThread Sending to {0}:{1}", RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port.ToString()));
            // Creates the Thread
            transmitQueue = Queue.Synchronized(new Queue());
            thread = new Thread(ThreadLoop);
            if (startNow)
            {
                StartThread();
            }
        }

        public void StartThread()
        {
            thread.Start();
        }

        public void ThreadLoop()
        {
            // runs the looping
            while (IsLooping())
            {
                if (transmitQueue.Count != 0)
                {
                    var command = transmitQueue.Dequeue() as byte[];
                    server.Send(command, command.Length, RemoteEndPoint);
                }
                Thread.Sleep(threadSleepTime);
            }
            server.Close();
        }

        public bool IsLooping()
        {
            lock (this)
            {
                return looping;
            }
        }
        public void StopThread()
        {
            lock (this)
            {
                looping = false;
                // close the port
            }
        }
        public void Write(byte[] command)
        {
            transmitQueue.Enqueue(command);
        }
    }

    public class UDPClinetThread
    {
        public static int threadSleepTime = 5;//ms
        public bool looping = true;

        public IPEndPoint LocalEndPoint;
        public UdpClient client;
        public Thread thread;
        public Queue receiveQueue;   // queue to receive info


        public UDPClinetThread(IPEndPoint localEndPoint, bool startNow = true)
        {

            LocalEndPoint = localEndPoint;
            client = new UdpClient(localEndPoint);
            Debug.Log(string.Format("UDPClinetThread reciving at port {0}", LocalEndPoint.Port.ToString()));
            // Creates the Thread
            receiveQueue = Queue.Synchronized(new Queue());
            thread = new Thread(ThreadLoop)
            {
                IsBackground = true,
            };
            if (startNow)
            {
                StartThread();
            }
        }

        public void StartThread()
        {
            thread.Start();
        }

        public void ThreadLoop()
        {
            // runs the looping
            while (IsLooping())
            {
                try
                {
                    byte[] data = client.Receive(ref LocalEndPoint);
                    receiveQueue.Enqueue(data);
                    //Debug.Log(ByteArrayToString(data));
                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                }
                Thread.Sleep(threadSleepTime);
            }
            // close the port
            client.Close();
        }
        public bool IsLooping()
        {
            lock (this)
            {
                return looping;
            }
        }
        public void StopThread()
        {
            lock (this)
            {
                looping = false;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            // refer to: https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    public class FileWriterThread
    {
        public static int threadSleepTime = 5;//ms
        public bool looping = true;
        public FileStream fstream;
        public BinaryWriter fwriter;
        public Thread thread;
        public Queue queue;  // queue to send command
        private int writeCount = 0;
        private bool fileWritten = false;
        private string filePath;
        public FileWriterThread(string filePath, FileMode fileMode = FileMode.Create,
                                bool startNow = true)
        {
            this.filePath = filePath;
            fstream = new FileStream(filePath, fileMode);
            fwriter = new BinaryWriter(fstream);
            Debug.Log(string.Format("FileWriterThread writing to {0}", filePath));
            // Creates the Thread
            queue = Queue.Synchronized(new Queue());
            thread = new Thread(ThreadLoop);
            if (startNow)
            {
                StartThread();
            }
        }

        public void StartThread()
        {
            thread.Start();
        }

        public void ThreadLoop()
        {
            // runs the looping
            // step 1: Opens the connection on the serial port
            // step 2: initialization(optional)

            // step 3: start looping

            while (IsLooping())
            {
                if(queue.Count != 0)
                {
                    fileWritten = true;
                    break;
                }
                Thread.Sleep(threadSleepTime);
            }

            while (IsLooping())
            {
                if (queue.Count != 0)
                {
                    var command = queue.Dequeue() as byte[];
                    fwriter.Write(command);
                    writeCount += command.Length;
                }

                if (writeCount > 1024)
                {
                    fstream.Flush();
                    writeCount = 0;
                }
                Thread.Sleep(threadSleepTime);
            }
            // step 4: close fstream, fwriter

        }

        public bool IsLooping()
        {
            lock (this)
            {
                return looping;
            }
        }


        public void StopThread()
        {
            lock (this)
            {
                looping = false;
                try
                {
                    fstream.Flush();
                    fstream.Close();
                }
                catch (ObjectDisposedException)
                {
                    // do nothing here
                    Debug.Log("Stream has been closed.");
                }
                fwriter.Close();
                if (!fileWritten)
                {
                    File.Delete(filePath);
                }
            }
        }
        public void Write(byte[] command)
        {
            queue.Enqueue(command);
        }
    }
}
