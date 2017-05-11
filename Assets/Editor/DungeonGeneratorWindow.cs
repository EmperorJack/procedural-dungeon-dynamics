using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using DungeonGeneration;

public class DungeonGeneratorWindow : EditorWindow {

	private string generateButton = "Generate";
    private string displayRoomsButton = "Display Rooms";
    private string hideRoomsButton = "Hide Rooms";
    private string displayPartitionsButton = "Display Partitions";
    private string hidePartitionsButton = "Hide Partitions";
	private string displayCorridorsButton = "Display Corridors";
	private string hideCorridorsButton = "Hide Corridors";
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

        if (GUILayout.Button(generateButton)) GetGenerator().Generate();

        if (GUILayout.Button(displayRoomsButton)) GetGenerator().DisplayRooms();
        if (GUILayout.Button(hideRoomsButton)) GetGenerator().HideRooms();

        if (GUILayout.Button(displayPartitionsButton)) GetGenerator().DisplayPartitions();
        if (GUILayout.Button(hidePartitionsButton)) GetGenerator().HidePartitions();

		if (GUILayout.Button(displayCorridorsButton)) GetGenerator ().DisplayCorridors();
		if (GUILayout.Button(hideCorridorsButton)) GetGenerator().HideCorridors();

        if (GUILayout.Button(clearButton)) GetGenerator().Clear();
    }

    private DungeonGenerator GetGenerator()
    {
        if (generatorObject != null)
        {
            DungeonGenerator generator = generatorObject.GetComponent<DungeonGenerator>();
            if (generator != null) return generator;
        }

        throw new Exception("Dungeon generator not found!");
    }
}
