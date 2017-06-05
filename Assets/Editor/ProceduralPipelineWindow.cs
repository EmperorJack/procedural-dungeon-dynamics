using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ProceduralPipelineWindow : EditorWindow {

    private string performButton = "Perform";
    private string resetButton = "Reset";
    private string displaySimpleLayoutButton = "Display Simple Layout";

	private string createSim = "Initialize Crowd Simulator";
	private string addAgent = "Add Agent";
	private string addGoal = "Add Goal";
	private string selectCell ="Select Cell";
	private string displaySim = "Display Sim";
    private string displayComplexLayoutButton = "Display Complex Layout";
    private string displayAnchorsButton = "Display Anchors";

    private GameObject pipelineObject = null;
	private GameObject simObject = null;

    [MenuItem("Window/Procedural Pipeline")]
    static void OpenWindow()
    {
        ProceduralPipelineWindow window = (ProceduralPipelineWindow) GetWindow(typeof(ProceduralPipelineWindow));
        window.minSize = new Vector2(100, 100);
        window.Show();
    }

    void OnGUI()
    {
        pipelineObject = (GameObject) EditorGUILayout.ObjectField("Procedural Pipeline:", pipelineObject, typeof(GameObject), true);

		simObject = (GameObject)EditorGUILayout.ObjectField ("Agent Object:", simObject, typeof(GameObject), true);

        if (GUILayout.Button(performButton)) GetPipeline().Perform();

        if (GUILayout.Button(resetButton)) GetPipeline().Reset();

        if (GUILayout.Button(displaySimpleLayoutButton)) GetPipeline().DisplaySimpleLayout();

		if (GUILayout.Button (createSim))
			GetPipeline ().createSim ();

		if (GUILayout.Button (addAgent)) 
			GetPipeline ().setAddAgent(simObject);
		

		if (GUILayout.Button (addGoal))
			GetPipeline ().setAddGoal();

		if (GUILayout.Button (selectCell))
			GetPipeline ().setSelect ();

		if (GUILayout.Button (displaySim))
			GetPipeline ().displaySim ();
        if (GUILayout.Button(displayComplexLayoutButton)) GetPipeline().DisplayComplexLayout();

        if (GUILayout.Button(displayAnchorsButton)) GetPipeline().DisplayAnchors();
    }

    private ProceduralPipeline GetPipeline()
    {
        if (pipelineObject != null)
        {
            ProceduralPipeline generator = pipelineObject.GetComponent<ProceduralPipeline>();
            if (generator != null) return generator;
        }

        throw new Exception("Procedural Pipeline not found!");
    }
}
