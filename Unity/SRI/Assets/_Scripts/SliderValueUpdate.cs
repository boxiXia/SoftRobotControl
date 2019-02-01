using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIEventDelegate;

public class SliderValueUpdate : MonoBehaviour {
    private Slider slider;
    private Text value;
    //public ReorderableEventList OnValueChanged;
    private void Awake()
    {
        slider = GetComponent<Slider>();
        value = transform.Find("Value").GetComponent<Text>();
    }


    public void Start()
    {
        //slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
    }
    private void Update()
    {
        //if (valueChanged)
        //{
        //    if (OnValueChanged.List.Count > 0)
        //        EventDelegate.Execute(OnValueChanged.List);
        //}
    }
    public void OnSliderValueChanged()
    {
        value.text = slider.value.ToString("0");
        //if (OnValueChanged.List.Count > 0)
        //    EventDelegate.Execute(OnValueChanged.List);
    }
}
