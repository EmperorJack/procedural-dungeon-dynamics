using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DungeonGeneration;

public class InterfaceControl : MonoBehaviour {

    // User set fields
    public InputField inputField;
    public Slider slider;
    public PipelineComponent target;
    public string targetField;

	void Start () {
        if (target == null) return;

        if (inputField != null)
        {
            if (inputField.contentType == InputField.ContentType.DecimalNumber ||
                inputField.contentType == InputField.ContentType.IntegerNumber)
            {
                inputField.onValueChanged.AddListener(delegate {
                    target.ChangeValue(targetField, float.Parse(inputField.text));
                });
            }     
        }
        else if (slider != null)
        {
            slider.onValueChanged.AddListener(delegate {
                target.ChangeValue(targetField, slider.value);
            });
        }


    }
}
