using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject crowdSimulationPanel;
    public GameObject dungeonGenerationPanel;

    private bool showCrowdSimulationPanel = false;
    private bool showDungeonGenerationPanel = false;

    public void Start()
    {
        if (crowdSimulationPanel == null || dungeonGenerationPanel == null) return;

        showCrowdSimulationPanel = false;
        showDungeonGenerationPanel = false;

        crowdSimulationPanel.SetActive(showCrowdSimulationPanel);
        dungeonGenerationPanel.SetActive(showDungeonGenerationPanel);
    }

    public void ToggleCrowdSimulationUI()
    {
        if (crowdSimulationPanel == null) return;

        showCrowdSimulationPanel = !showCrowdSimulationPanel;
        crowdSimulationPanel.SetActive(showCrowdSimulationPanel);
    }

    public void ToggleDungeonGenerationUI()
    {
        if (dungeonGenerationPanel == null) return;

        showDungeonGenerationPanel = !showDungeonGenerationPanel;
        dungeonGenerationPanel.SetActive(showDungeonGenerationPanel);
    }
}
