using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static int algorithm = 0; // [ Global PSO, Local PSO, UPSO, CLPSO, ELPSO ]
    public static int flockSize = 300;
    public static float maxVelocity = 20f;
    public static float c1 = 2f;
    public static float c2 = 2f;
    public static int n = 10;
    public static float u = 0.5f;
    public static float w = 0.729f;
    public static int exampleSetSize = 50;

    public Transform settingsPanel;
    public Transform options;

    private bool settingsOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleSettings() {
        settingsOpen = !settingsOpen;
        settingsPanel.position = new Vector3(
            settingsPanel.position.x + (settingsOpen ? -200 : 200),
            settingsPanel.position.y
        );
        settingsPanel.GetChild(0).GetChild(0).gameObject.SetActive(!settingsOpen);
        settingsPanel.GetChild(0).GetChild(1).gameObject.SetActive(settingsOpen);
    }

    public void PlayPause(bool play) {
        Time.timeScale = play ? 1f : 0f;
    }

    public void UpdateAlgorithm(Dropdown dropdown) {
        algorithm = dropdown.value;
        options.GetChild(1).gameObject.SetActive(algorithm == 0);
        options.GetChild(2).gameObject.SetActive(algorithm == 1);
        options.GetChild(3).gameObject.SetActive(algorithm == 2);
        options.GetChild(4).gameObject.SetActive(algorithm == 3);
        options.GetChild(5).gameObject.SetActive(algorithm == 4);

        // Set c1 and c2 to their default values for the different algorithms
        if (algorithm < 3) {
            GameObject.Find("C1").transform.GetChild(1).GetComponent<Slider>().value = 2f;
            GameObject.Find("C2").transform.GetChild(1).GetComponent<Slider>().value = 2f;
        } else {
            GameObject.Find("C1").transform.GetChild(1).GetComponent<Slider>().value = 1.49445f;
            if (algorithm == 4) GameObject.Find("C2").transform.GetChild(1).GetComponent<Slider>().value = 1.49445f;
        }
    }

    public void UpdateFlockSize(Slider slider) {
        flockSize = (int)slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Flock Size: " + flockSize.ToString();
    }

    public void UpdateMaxVelocity(Slider slider) {
        maxVelocity = slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Maximum Velocity: " + maxVelocity.ToString("F1") + " m/s";
    }

    public void UpdateC1(Slider slider) {
        c1 = slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "C 1: " + c1.ToString("F3");
    }

    public void UpdateC2(Slider slider) {
        c2 = slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "C 2: " + c2.ToString("F3");
    }

    public void UpdateNeighborhoodSize(Slider slider) {
        n = (int)slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Neighborhood Size: " + n.ToString();
    }

    public void UpdateUnificationFactor(Slider slider) {
        u = slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Unification Factor: " + u.ToString("F2");
    }

    public void UpdateInertiaWeight(Slider slider) {
        w = slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Initeria Weight: " + u.ToString("F2");
    }

    public void UpdateExampleSetSize(Slider slider) {
        exampleSetSize = (int)slider.value;
        slider.transform.parent.GetChild(0).GetComponent<Text>().text = "Example Set Size: " + exampleSetSize.ToString();
    }
}
