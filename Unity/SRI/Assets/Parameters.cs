using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour {
    [Tooltip("Identifier")]
    public string Name;
    [Tooltip("port name of the FSE1001")]
    public string forceSensorPortName;
    [Tooltip("remote ip address of the photon controller")]
    public string remoteIPAddress;
    [Tooltip("remote port of the photon controller")]
    public int remotePort;
    [Tooltip("local port of this computer where photon sends info to")]
    public int localPort;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
