using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CrowdSim;

namespace CrowdSim
{
	public class SliderScript : MonoBehaviour
	{

		public Slider slider;
		public string action;
		public SimAccess simAccess;

		float minValue;
		float maxValue;

		public GameObject minText;
		public GameObject maxText;

		float value;


		// Use this for initialization
		void Start ()
		{
			if (slider != null) {

				minValue = slider.minValue;
				maxValue = slider.maxValue;

				setText ();

				value = slider.value;

				slider.onValueChanged.AddListener (delegate {
					ValueChangeCheck ();
				});

				simAccess.addSlider (this);
			}
		}

		void setText(){
			if (minText != null && maxText != null) {
				minText.GetComponent<Text>().text = minValue.ToString ("#.##");
				maxText.GetComponent<Text> ().text = maxValue.ToString ("#.##");
			}
		}

		public void reset ()
		{
			slider.minValue = minValue;
			slider.maxValue = maxValue;
			slider.value = value;

			setText ();
		}

		void ValueChangeCheck ()
		{
			simAccess.sliderAction (action, slider.value);

		}

		// Update is called once per frame
		void Update ()
		{
		
		}
	}
}

