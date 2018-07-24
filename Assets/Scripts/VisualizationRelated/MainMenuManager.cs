using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void LoadBuilder()
    {
        SceneManager.LoadScene("BuilderScene2", LoadSceneMode.Single);
    }

    public void LoadTestScene()
    {
        SceneManager.LoadScene("ExperimentScene2", LoadSceneMode.Single);
    }
}
