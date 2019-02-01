using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SliderUIScript : MonoBehaviour {
    public Slider slider;
    public Text valueText;
    public Text temperatureText;
    public Text motorNumText;
    public int motorNum;
    public SRController srController;

    private void Awake()
    {
        motorNum = (int)(transform.name[transform.name.Length - 1]) - '0';
        srController = GameObject.Find("SRController").GetComponent<SRController>();
    }

    // Use this for initialization
    void Start () {
        //initalize the slider
        slider = gameObject.GetComponent<Slider>();

        valueText = transform.Find("Value").GetComponent<Text>();
        valueText.text = slider.value.ToString();

        //temperatureText = transform.Find("Temperature").GetComponent<Text>();

        motorNumText = transform.Find("MotorNum").GetComponent<Text>();
        motorNumText.text = string.Format("M{0}", transform.name[transform.name.Length - 1]);

        //Adds a listener to the main slider and invokes a method when the value changes.
        slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
    }
	
	// Update is called once per frame
	void Update () {
        //i f(srController.motors[motorNum].Value!= (byte)slider.value)
        //{
        //    slider.value = (float)srController.motors[motorNum].Value;
        //}
        

    }

    void OnSliderValueChanged()
    {
        valueText.text = slider.value.ToString();
        //notify the SRController of value change
        srController.OnSliderUIValueChanged(motorNum, (byte)slider.value);
    }

}
