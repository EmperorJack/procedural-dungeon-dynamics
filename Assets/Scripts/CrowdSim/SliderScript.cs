using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CrowdSim;

 namespace CrowdSim{
public class SliderScript : MonoBehaviour
{

	public Slider slider;
	public string action;
	public SimAccess simAccess;

	float minValue;
	float maxValue;

	public float increment;

	// Use this for initialization
	void Start ()
	{
			if (slider != null) {
				slider.onValueChanged.AddListener (delegate {
					ValueChangeCheck ();
				});
			}

		minValue = slider.minValue;
		maxValue = slider.maxValue;

		simAccess.addSlider (this);
	}

	public void reset(){
		slider.minValue = minValue;
		slider.maxValue = maxValue;
	}

	void ValueChangeCheck ()
	{
		if (slider.value % increment == 0) {
			simAccess.sliderAction (action, slider.value);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
	}
}

