using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlay : MonoBehaviour {

    public List<GameObject> stencils;
    public GameObject tutorialText;
    public GameObject darkenPane;

    private int clickCount = 0;

    // Use this for initialization
    void Start () {
        tutorialText.GetComponent<Text>().text = "We will begin with a brief overview of the controls.\n\nClick anywhere to continue.";
        Camera.main.GetComponent<UserInputManager>().ToggleLabels(false);
        tutorialText.transform.SetParent(darkenPane.transform);
    }	

    public void OnOverlayClick()
    {
        foreach (var stencil in stencils)
            stencil.SetActive(false);

        switch (clickCount)
        {
            case 0:
                darkenPane.SetActive(false);
                tutorialText.GetComponent<Text>().text = "Click these buttons to display the cheat sheets, click the same button again to hide the cheat sheet.\n\nClick anywhere to continue.";
                stencils[clickCount].SetActive(true);
                tutorialText.transform.SetParent(stencils[clickCount].transform);
                break;

            case 1:
                if (StudyFlowControl.dynamicVisualizationType == "none")
                {
                    clickCount++;
                    stencils[3].SetActive(true);
                    tutorialText.GetComponent<Text>().text = "This is the area where you will provide your answers. Make sure to select an answer to both questions. Once you are ready to submit your answers to both questions, click the Next button to confirm your selections.\n\nClick anywhere to continue.";
                    tutorialText.transform.SetParent(stencils[3].transform);
                    break;
                }
                else
                {
                    if(StudyFlowControl.dynamicVisualizationType == "movie")
                    {
                        tutorialText.GetComponent<Text>().text = "Click the Animate button to start and stop the simulation.\n\nClick anywhere to continue.";
                        stencils[1].SetActive(true);
                        tutorialText.transform.SetParent(stencils[1].transform);
                    }
                    else
                    {
                        tutorialText.GetComponent<Text>().text = "Click and drag the slider to view the simulation at different times. Return all the way to the left side of the slider to view different uncertainty projections.\n\nClick anywhere to continue.";
                        stencils[2].SetActive(true);
                        tutorialText.transform.SetParent(stencils[2].transform);
                    }
                }

                break;

            case 2:
                stencils[3].SetActive(true);
                tutorialText.GetComponent<Text>().text = "This is the area where you will provide your answers. Make sure to select an answer to both questions. Once you are ready to submit your answers to both questions, click the Next button to confirm your selections.\n\nClick anywhere to continue.";
                tutorialText.transform.SetParent(stencils[3].transform);
                break;

            default:
                Camera.main.GetComponent<UserInputManager>().ToggleLabels(true);
                Destroy(this.gameObject);
                break;
        }
        clickCount++;

    }
}
