using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(WholeObject))]
public class WholeObjectEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		//list.DoLayoutList();
		WholeObject objectScript = (WholeObject)target;
		if (GUILayout.Button ("Initialise Broken Geo")) {
			objectScript.initialize ();
		}
	}
}
