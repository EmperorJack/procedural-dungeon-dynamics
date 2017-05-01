using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DungeonGeneratorWindow : EditorWindow {

	private string generateButton = "Generate";
    private string clearButton = "Clear";

    private GameObject generatorObject = null;

    [MenuItem("Window/Dungeon Generator")]
	static void OpenWindow() {
		DungeonGeneratorWindow window = (DungeonGeneratorWindow) GetWindow(typeof(DungeonGeneratorWindow));
		window.minSize = new Vector2 (100, 100);
		window.Show();
	}

	void OnEnable() {

	}

	void OnGUI() {
        generatorObject = (GameObject) EditorGUILayout.ObjectField("Dungeon Generator:", generatorObject, typeof(GameObject), true);

        if (GUILayout.Button(generateButton)) {
            if (generatorObject != null)
            {
                DungeonGenerator generator = generatorObject.GetComponent<DungeonGenerator>();
                if (generator != null) generator.Generate();
            }
        }

        if (GUILayout.Button(clearButton))
        {
            if (generatorObject != null)
            {
                DungeonGenerator generator = generatorObject.GetComponent<DungeonGenerator>();
                if (generator != null) generator.Clear();
            }
        }
    }
}
