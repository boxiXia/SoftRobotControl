using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MockSystem : MonoBehaviour
{

    public bool simulate;
    public Muscle muscle;
    //public ForceCycleController controller;

    public FSE1001Mono fse1001MonoScript;
    public UDPTemperature udpTemperatureScript;
    public PWMControl pwmControlScript;
    // Use this for initialization
    private void Awake()
    {
        fse1001MonoScript = FindObjectOfType<FSE1001Mono>();
        udpTemperatureScript = FindObjectOfType<UDPTemperature>();
        pwmControlScript = FindObjectOfType<PWMControl>();
    }
    void Start()
    {
        muscle = new Muscle
        {
            a = 0.008f,
            b = 0.02f,
            eta = 1f,
            b_eta = 0.00002f,
            f0 = 1,
        };
    }

    // Update is called once per frame
    void Update()
    {
        byte u = pwmControlScript.motors[0].Value;
        muscle.OneStepAdvance(u);
        fse1001MonoScript.forceZ = (pwmControlScript.forceMode == PWMControl.ForceMode.Compression ? muscle.f:-muscle.f) + UnityEngine.Random.Range(-1f, 1f); ;
        udpTemperatureScript.temperatures[0] = muscle.T + UnityEngine.Random.Range(-0.1f, 0.1f);
    }

    [System.Serializable]
    public class Muscle
    {
        public float a;
        public float b;
        public float eta = 1f;
        public float b_eta;
        public float dt = 10e-2f;
        public float T = 25f;
        public float T0 = 25f;
        public float f0;
        /*
         * dT/dt = a*(GetPWM) - b*(T-T0)
         * */
        public void OneStepAdvance(byte u)
        {
            T = T + (a * u - b * (T - T0)) * dt;
            eta = eta - b_eta * (T - T0) * dt;
        }
        public float f
        {
            get { return eta * (Ethanol.VaporPressure(T) - Ethanol.VaporPressure(T0)) * f0; }
        }
    }

    public class Ethanol
    {
        private static readonly float A1 = 7.329073020132939f; //8.20417 + np.log10(101.325/760)
        private static readonly float B1 = 1642.89f;
        private static readonly float C1 = 230.3f;
        private static readonly float T1min = -57f;
        private static readonly float T1max = 80f;
        private static readonly float A2 = 6.806073020132939f; //7.68117 + np.log10(101.325/760)
        private static readonly float B2 = 1332.04f;
        private static readonly float C2 = 199.2f;
        private static readonly float T2min = 77f;
        private static readonly float T2max = 243f;
        public static float VaporPressure(float T)
        {
            /*Calculate saturated vapor pressure(kpa) given temerature T
            # P = 10**(A-B/(C+T))
            # ref:http://ddbonline.ddbst.com/AntoineCalculation/AntoineCalculationCGI.exe?component=Ethanol
            **/
            if (T1min < T && T < T2min)
            {
                return Mathf.Pow(10, A1 - B1 / (C1 + T));
            }
            else if (T < T1max)
            {
                return Mathf.Pow(10, 0.5f * (A1 - B1 / (C1 + T) + A2 - B2 / (C2 + T)));
            }
            else if(T<T2max)
            {
                return Mathf.Pow(10, A2 - B2 / (C2 + T));
            }
            else
            {
                Debug.LogError("Error:temperarure over maximum temperarture");
                return float.NegativeInfinity;
            }
        }
    }


}
