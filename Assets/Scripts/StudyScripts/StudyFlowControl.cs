using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class StudyFlowControl : MonoBehaviour {

    public enum VisualizationType { Line, Cone };
    public enum DynamicVisualizationType { None, Movie, Slider};

    public GameObject Answer1;
    public GameObject Answer2;
    public GameObject Answer3;
    public GameObject Answer4;
    public GameObject QuestionText;
    public GameObject UIOverlay;
    public GameObject NextButton;
    public GameObject TimeSlider;
    public GameObject ResetCamera;
    public GameObject TilesetMap;
    public GameObject Likert;
    public GameObject MetaCogTextBox;
    public GameObject InstructionOverlay;

    //The unique identifier for this participant
    public string UserIdentifier = "TestUser";

    //Which static visualization to use for the scenarios
    public VisualizationType StaticVisualizationType;

    //Which Animation type to present to the user.
    public DynamicVisualizationType AnimationControlType;

    bool is_intro = true;
    bool is_premeta = false;
    bool is_practice = false;
    bool is_highlight = false;
    bool is_postmeta = false;
    bool is_question = false;
    bool is_map = false;

    private List<string> finfo;
    private List<string> random_finfo;
    private int question_id = 0;
    private int selected = -1;

    private int confidenceVal = -1;

    public static string userID; //should be carried over from qualtrics
    public static string visualizationType; //line or cone
    public static string dynamicVisualizationType; // none or movie or slider
    private static int true_question_id = -1;

    public static int TrueQuestionID
    {
        get
        {
            return true_question_id;
        }

        set
        {
            true_question_id = value;
        }
    }

    // Use this for initialization
    void Start () {
        ParseURL(Application.absoluteURL);
        SetQuestionOverlay(true);

        //Maybe this will work
        Screen.fullScreen = true;

        if (dynamicVisualizationType == "none")
        {
            GameObject.Find("PlaySimulation").SetActive(false);
            TimeSlider.SetActive(false);
        }
        else if (dynamicVisualizationType == "slider")
        {
            GameObject.Find("PlaySimulation").SetActive(false);
        }
        else //default to movie?
        {
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (dynamicVisualizationType == "movie")
        {
            TimeSlider.GetComponent<Slider>().interactable = false;        
        }
        if (is_intro && !Screen.fullScreen && !Application.isEditor)
        {
            QuestionText.GetComponent<Text>().text = "Please press the blue Fullscreen button at in the bottom right to allow the study to continue.";
            NextButton.GetComponent<Button>().interactable = false;
        }
        else if (is_intro && (Screen.fullScreen || Application.isEditor))
        {
            QuestionText.GetComponent<Text>().text = "In the following section, you will be presented with a series of scenarios and a set of questions about the scenarios. " +
                "For this set of questions, you will be provided with several possible answers.\n\n" +
                "-There may be more than one correct option\n" +
                "-Pick the BEST solution for each scenario\n";
            NextButton.GetComponent<Button>().interactable = true;

        }
    }

    private void ParseURL(string url)
    {
        int pm = Application.absoluteURL.IndexOf("?");
        if (pm != -1)
        {
            var toBeParsed = url.Split(new string[] { "?", "&" }, StringSplitOptions.None);
            foreach (string temp in toBeParsed)
            {
                if (temp.StartsWith("user"))
                    userID = temp.Split('=')[1];
                else if (temp.StartsWith("static"))
                    visualizationType = temp.Split('=')[1];
                else if (temp.StartsWith("dynamic"))
                    dynamicVisualizationType = temp.Split('=')[1];
            }            
        }
        else
        {
            userID = UserIdentifier;
            visualizationType = StaticVisualizationType.ToString().ToLower();
            dynamicVisualizationType = AnimationControlType.ToString().ToLower();
        }
    }

    public void OnNextClicked()
    {
        if (is_intro)
        {
            is_intro = false;
            LoadSampleQuestion();
            InstructionOverlay.SetActive(true);
        }
        else if (is_practice)
        {
            HighlightCorrectAnswer();
            is_practice = false;
        }
        else if (is_highlight)
        {
            LoadCognitionQuestion(false);
            Answer1.GetComponentInChildren<Button>().interactable = true;
            Answer2.GetComponentInChildren<Button>().interactable = true;
            Answer3.GetComponentInChildren<Button>().interactable = true;
            Answer4.GetComponentInChildren<Button>().interactable = true;

            is_highlight = false;
        }
        else if (is_premeta)
        {
            MetaCogTextBox.GetComponent<InputField>().text = "";
            MetaCogTextBox.SetActive(false);

            //DirectoryInfo dir = new DirectoryInfo("ScenarioFiles");
            //finfo = new List<FileInfo>(dir.GetFiles("*.*"));
            /*finfo = new List<string> {Scenario01,
            Scenario02,
            Scenario03,
            Scenario04,
            Scenario05,
            Scenario06,
            Scenario07,
            Scenario08,
            Scenario09,
            Scenario10};*/

            finfo = new List<string> {EasyLowCoordScenario1,
            EasyLowCoordScenario2,
            EasyLowCoordScenario3,
            EasyLowCoordScenario4,
            EasyLowCoordScenario5,
            HardLowCoordScenario1,
            HardLowCoordScenario2,
            HardLowCoordScenario3,
            HardLowCoordScenario4,
            HardLowCoordScenario5,
            EasyHighCoordScenario1,
            EasyHighCoordScenario2,
            EasyHighCoordScenario3,
            EasyHighCoordScenario4,
            EasyHighCoordScenario5,
            HardHighCoordScenario1,
            HardHighCoordScenario2,
            HardHighCoordScenario3,
            HardHighCoordScenario4,
            HardHighCoordScenario5};

            random_finfo = finfo.OrderBy(a => Guid.NewGuid()).ToList();
            //random_finfo = finfo;

            LoadNextQuestion();

            is_intro = false;
            is_practice = false;
            is_premeta = false;
        }
        else if(is_question)
        {
            LogHelper.AnswerSubmitted(true_question_id, selected, confidenceVal);

            LoadNextQuestion();
        }
        else if(is_postmeta)
        {
            MetaCogTextBox.SetActive(false);
            QuestionText.GetComponent<Text>().text = "If the window is still in full screen, press ESC and click next to continue with the survey.";           

            NextButton.GetComponentInChildren<Text>().text = "Next";
            NextButton.GetComponent<Button>().interactable = false;
            NextButton.SetActive(false);
            LogHelper.StudyCompleted(userID, visualizationType, dynamicVisualizationType);


            Screen.fullScreen = false;
            //Shouldn't need this anymore?        
            //StartCoroutine(LogHelper.StudyCompleted(userID,visualizationType,dynamicVisualizationType));
        }
    }

    public void LoadNextQuestion()
    {
        question_id++;
        //print("HERE: " + question_id);
        if (question_id - 1 >= finfo.Count)
        {            
            Answer1.SetActive(false);
            Answer2.SetActive(false);
            Answer3.SetActive(false);
            Answer4.SetActive(false);

            Likert.SetActive(false);
            is_question = false;

            LoadCognitionQuestion(true);
        }
        else
        {
            selected = -1;
            confidenceVal = -1;

            is_question = true;
            TimeSlider.GetComponent<Slider>().value = 0;
            //SetQuestionOverlay(false);

            updateQuestionText(question_id);
            GetComponent<UserInputManager>().OpenStream(random_finfo[question_id - 1]);

            SetQuestionOverlay(true);
        }
    }

    public void LoadSampleQuestion()
    {      
        //do load up the training overlay here
        selected = -1;
        confidenceVal = -1;
        is_practice = true;

        TimeSlider.GetComponent<Slider>().value = 0;
        //SetQuestionOverlay(false);
        GetComponent<UserInputManager>().OpenStream(SampleScenario);

        //MAY OR MAY NOT NEED THIS
        RandomizeAnswerOrder();

        QuestionText.GetComponent<Text>().text = "Complete the following practice question: Choose the BEST solution to get supplies in the scenario shown. Remember, there may be more than one correct solution.";
        Answer1.SetActive(true);
        Answer2.SetActive(true);
        Answer3.SetActive(true);
        Answer4.SetActive(true);
        Answer1.GetComponentInChildren<Image>().color = Color.white;
        Answer2.GetComponentInChildren<Image>().color = Color.white;
        Answer3.GetComponentInChildren<Image>().color = Color.white;
        Answer4.GetComponentInChildren<Image>().color = Color.white;
        NextButton.GetComponent<Button>().interactable = false;

        Likert.SetActive(true);
        Likert.GetComponent<ToggleGroup>().SetAllTogglesOff();

        Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Loaded Boat FM";
        Answer2.GetComponentInChildren<Text>().text = "Send Cruiser to Miscellaneous_B then Loaded Boat FM";        
        Answer3.GetComponentInChildren<Text>().text = "Send Cruiser to Loaded Boat FM then Miscellaneous_B";
        Answer4.GetComponentInChildren<Text>().text = "Send Jet Ski to Loaded Boat FM; send Cruiser to Miscellaneous_B";        

        SetQuestionOverlay(true);
    }

    public void HighlightCorrectAnswer()
    {
        //do load up the training overlay here
        is_highlight = true;
        QuestionText.GetComponent<Text>().text = "The correct answer to the practice question is displayed below. Click next to continue.";

        Answer1.GetComponentInChildren<Button>().interactable = false;
        Answer2.GetComponentInChildren<Button>().interactable = false;
        Answer3.GetComponentInChildren<Button>().interactable = false;
        Answer4.GetComponentInChildren<Button>().interactable = false;

        switch (selected)
        {
            case 1:
                Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Loaded Boat FM - Correct!";
                Answer1.GetComponentInChildren<Image>().color = Color.green;
                break;
            case 2:
                Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Loaded Boat FM - Correct answer";
                Answer1.GetComponentInChildren<Image>().color = Color.green;
                Answer2.GetComponentInChildren<Text>().text = "Send Cruiser to Miscellaneous_B then Loaded Boat FM - Incorrect";
                Answer2.GetComponentInChildren<Image>().color = Color.red;
                break;
            case 3:
                Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Loaded Boat FM - Correct answer";
                Answer1.GetComponentInChildren<Image>().color = Color.green;
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser to Loaded Boat FM then Miscellaneous_B - Incorrect";
                Answer3.GetComponentInChildren<Image>().color = Color.red;
                break;
            case 4:
                Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Loaded Boat FM - Correct answer";
                Answer1.GetComponentInChildren<Image>().color = Color.green;
                Answer4.GetComponentInChildren<Text>().text = "Send Jet Ski to Loaded Boat FM; send Cruiser to Miscellaneous_B - Incorrect";
                Answer4.GetComponentInChildren<Image>().color = Color.red;
                break;
        }
        NextButton.GetComponent<Button>().interactable = true;

        Likert.SetActive(false);

        SetQuestionOverlay(true);
    }

    private void LoadCognitionQuestion(bool isPost)
    {
        if (!isPost)
        {
            QuestionText.GetComponent<Text>().text = "Before you begin, based upon how well you think you understand the training, how many of the" +
                " 20 scenarios do you think you'll find the best solution for?\n\n" +
                "In the box below, type the number of scenarios you will find the best solution for (out of 20 scenarios total).";
            is_premeta = true;
        }
        else
        {
            QuestionText.GetComponent<Text>().text = "Based upon how well you think you just did on these 20 questions, how many scenarios do you think you found" +
                " the best solution for?\n\n" + 
                "In the box below, type the number of scenarios you think you found the best solution for (out of 20 scenarios total).";
            is_postmeta = true;

        }
        Answer1.SetActive(false);
        Answer2.SetActive(false);
        Answer3.SetActive(false);
        Answer4.SetActive(false);

        Likert.SetActive(false);

        NextButton.GetComponentInChildren<Text>().text = "Next";
        NextButton.GetComponent<Button>().interactable = false;

        MetaCogTextBox.SetActive(true);
    }

    private void updateQuestionText(int question_id)
    {
        RandomizeAnswerOrder();
        if (question_id > 0 && question_id <= 20)
        {
            QuestionText.GetComponent<Text>().text = "Choose the BEST solution to get supplies in the scenario shown. Remember, there may be more than one correct solution.";
            Answer1.SetActive(true);
            Answer2.SetActive(true);
            Answer3.SetActive(true);
            Answer4.SetActive(true);
            Answer1.GetComponentInChildren<Image>().color = Color.white;
            Answer2.GetComponentInChildren<Image>().color = Color.white;
            Answer3.GetComponentInChildren<Image>().color = Color.white;
            Answer4.GetComponentInChildren<Image>().color = Color.white;
            NextButton.GetComponent<Button>().interactable = false;

            Likert.SetActive(true);
            Likert.GetComponent<ToggleGroup>().SetAllTogglesOff();
        }


        true_question_id = finfo.IndexOf(random_finfo[question_id-1]) + 1;

        //print(true_question_id + " " + question_id + " " +  random_finfo[question_id - 1]);
        switch (true_question_id)
        {
            // TODO FIX ME
            case 1:
                Answer1.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Alcohol_B 1; send Cruiser 2 to Alcohol_B 2";
                Answer2.GetComponentInChildren<Text>().text = "Send Cruiser 2 to Alcohol_B 2 then Alcohol_B 1";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Alcohol_B 1 then Alcohol_B 2";
                Answer4.GetComponentInChildren<Text>().text = "Send Cruiser 2 to Alcohol_B 1";
                break;

            case 2:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1; send Yacht 2 to Alcohol_A 2";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1; send Yacht 2 to Alcohol A 3";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 2 then Alcohol A 3";
                Answer4.GetComponentInChildren<Text>().text = "Send Yacht 2 to Alcohol_A 2";
                break;

            case 3:
                Answer1.GetComponentInChildren<Text>().text = "Send Jetski 1 to Food_B 1; send Jetski 2 to Food_B 2";
                Answer2.GetComponentInChildren<Text>().text = "Send Jetski 1 to Food_B 1; send Jetski 2 to Food_B 3";
                Answer3.GetComponentInChildren<Text>().text = "Send Jetski 1 to Food_B 1 then Food_B 2; send Jetski 2 to Food_B 3";
                Answer4.GetComponentInChildren<Text>().text = "Send Jetski 1 to Food_B 1 then Food_B 2 then Food_B 3";
                break;

            case 4:
                Answer1.GetComponentInChildren<Text>().text = "Send RIB 1 to Miscellaneous_A 1; send RIB 2 to Miscellaneous_A 2";
                Answer2.GetComponentInChildren<Text>().text = "Send RIB 1 to Miscellaneous_A 1; send RIB 2 to Miscellaneous_A 3";
                Answer3.GetComponentInChildren<Text>().text = "Send RIB 1 to Miscellaneous_A 1 then Miscellaneous_A 2";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB 1 to Miscellaneous_A 2 then Miscellaneous_A 3";
                break;

            case 5:
                Answer1.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Miscellaneous_A 1; send Cruiser 2 to Miscellaneous_A 2";
                Answer2.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Miscellaneous_A 1 then Miscellaneous_A 2";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Miscellaneous_A 1";
                Answer4.GetComponentInChildren<Text>().text = "Do Nothing";
                break;

            case 6:
                Answer1.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Alcohol_B 1; send Cruiser 2 to Alcohol_B 2; send Cruiser 3 to Alcohol_B 3";
                Answer2.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Alcohol_B 1 then Alcohol_B 2; Send Cruiser 2 to Alcohol_B 4 then Alcohol_B 3";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser 2 to Alcohol_B 2 then Alcohol_B 3 then Alcohol B 4; send Cruiser 1 to Alcohol_B 1";
                Answer4.GetComponentInChildren<Text>().text = "Send Cruiser 1 to Alcohol_B 2 then Alcohol_B 3";
                break;

            case 7:
                Answer1.GetComponentInChildren<Text>().text = "Send JetSki 1 to Alcohol_C 1; send JetSki 2 to Alcohol_C 2; send JetSki 3 to Alcohol_C 3";
                Answer2.GetComponentInChildren<Text>().text = "Send JetSki 1 to Alcohol_C 1 then Alcohol_C 2; Send JetSki 2 to Alcohol_C 3 then Alcohol_C 4";
                Answer3.GetComponentInChildren<Text>().text = "Send JetSki 1 to Alcohol_C 1 then Alcohol_C 2 then Alcohol_C 3; send JetSki 2 to Alcohol_C 4";
                Answer4.GetComponentInChildren<Text>().text = "Send JetSki 1 to Alcohol_C 1 then Alcohol_C 2";
                break;

            case 8:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1; send Yacht 2 to Alcohol_A 3; send Yacht 3 to Alcohol_A 4";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1 then Alcohol_A 3; Send Yacht 2 to Alcohol_A 4 then Alcohol_A 2";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1 then Alcohol_A 3";
                Answer4.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_A 1 then Alcohol_A 3 then Alcohol_A 4; send Yacht 2 to Alcohol_A 2";
                break;

            case 9:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht 1 to Miscellaneous_A 4; send Yacht 2 to Miscellaneous_A 3; send Yacht 3 to Miscellaneous_A 2 then Miscellaneous_A 1";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht 1 to Miscellaneous_A 4 then Miscellaneous_A 3 then Miscellaneous_A 2 then Miscellaneous_A 1";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht 1 to Miscellaneous_A 4 then Miscellaneous_A 3; send Yacht 2 to Miscellaneous_A 2 then Miscellaneous_A 1";
                Answer4.GetComponentInChildren<Text>().text = "Send Yacht 1 to Miscellaneous_A 3 then Miscellaneous_A 1";
                break;

            case 10:
                Answer1.GetComponentInChildren<Text>().text = "Send RIB 1 to Alcohol_D 1 then Alcohol_D 2; send RIB 2 to Alcohol_D 3; send RIB 3 to Alcohol_D 4";
                Answer2.GetComponentInChildren<Text>().text = "Send RIB 1 to Alcohol_D 1 then Alcohol_D 2; Send RIB 2 to Alcohol_D 3 then Alcohol_D 4";
                Answer3.GetComponentInChildren<Text>().text = "Send RIB 3 to Alcohol_D 4 then Alcohol_D 3 then Alcohol_D 2; send RIB 2 to Alcohol_D 1";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB 1 to Alcohol_D 4 then Alcohol_D 3 then Alcohol_D 2 then Alcohol_D 1";
                break;


            case 11:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht to Food_A; send Cruiser to Miscellaneous_A";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht to Miscellaneous_A then Food_A";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht to Food_A then Miscellaneous_A";
                Answer4.GetComponentInChildren<Text>().text = "Send Cruiser to Food_A; send Yacht to Miscellaneous_A";
                break;

            case 12:
                Answer1.GetComponentInChildren<Text>().text = "Send Cruiser to Loaded Boat FM; send RIB to Loaded Boat AM";
                Answer2.GetComponentInChildren<Text>().text = "Send Cruiser to Loaded Boat FM; send RIB to Alcohol_D";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser to Loaded Boat AM then Alcohol_D";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Loaded Boat AM; send Jet Ski to Alcohol_D";
                break;

            case 13:
                Answer1.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_D then Alcohol_C; send Jet Ski to Miscellaneous_B";
                Answer2.GetComponentInChildren<Text>().text = "Send RIB to Miscellaneous_B then Alcohol_D then Alcohol_C";
                Answer3.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_C then Alcohol_D then Miscellaneous_B";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Miscellaneous_B then Alcohol_C; send Jet Ski to Alcohol_D";
                break;

            case 14:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht to pick up Alcohol_A then Alcohol_B; send RIB to Food_B then Alcohol_C";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht to Alcohol_B then Alcohol_A; send RIB to Alcohol_C then Food_B";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht to Alcohol_A then Alcohol_B; send RIB to Alcohol_C then Food_B";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_A then Food_B";
                break;

            case 15:
                Answer1.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B then Food_B; send Cruiser to Miscellaneous_A";
                Answer2.GetComponentInChildren<Text>().text = "Send Jet Ski to Food_B then Miscellaneous_B; send Cruiser to Miscellaneous_A";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser to Miscellaneous_A then Food_B then Miscellaneous_B";
                Answer4.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_A; send Cruiser to Miscellaneous_B then Food_B";
                break;

            case 16:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht to Alcohol_A; send Cruiser to Miscellaneous_A; send Jet Ski to Food_B";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht to Alcohol_A then Food_B; send Cruiser to Miscellaneous_A";
                Answer3.GetComponentInChildren<Text>().text = "Send Jet Ski to Food_B; send Yacht to Food_A; send Cruiser to Miscellaneous_A";
                Answer4.GetComponentInChildren<Text>().text = "Send Jet Ski to Food_A then Alcohol_A; send Cruiser to Miscellaneous_A";
                break;

            case 17:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht 1 to Loaded Boat AF; send Yacht 2 to Alcohol_A then Alcohol_B; send RIB to Food_B";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_B then Alcohol_A then Food_B; send Yacht 2 to Loaded Boat AF";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht 1 to Food_B then Loaded Boat AF; send Yacht 2 to Alcohol_B then Alcohol_A";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Loaded Boat AF; send Yacht 1 to Food_B then Alcohol_A then Alcohol_B";
                break;

            case 18:
                Answer1.GetComponentInChildren<Text>().text = "Send Yacht 1 to Food_A; send Yacht 2 to Alcohol_A then Alcohol_B; send RIB to Food_B";
                Answer2.GetComponentInChildren<Text>().text = "Send Yacht 1 to Food_B then Food_A; send Yacht 2 to Alcohol_B then Alcohol_A";
                Answer3.GetComponentInChildren<Text>().text = "Send Yacht 1 to Alcohol_B then Alcohol_A then Food_B; send Yacht 2 to Food_A";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Food_A; send Yacht 1 to Food_B then Alcohol_A then Alcohol_B";
                break;

            case 19:
                Answer1.GetComponentInChildren<Text>().text = "Send Cruiser to Alcohol_B; send RIB to Miscellaneous_B then Alcohol_D; send Jet Ski to Alcohol_C";
                Answer2.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Alcohol_B; send RIB to Alcohol_C then Alcohol_D";
                Answer3.GetComponentInChildren<Text>().text = "Send Jet Ski to Miscellaneous_B; send Cruiser to Alcohol_C then Alcohol_D";
                Answer4.GetComponentInChildren<Text>().text = "Send Jet Ski to Alcohol_B; send RIB to Miscellaneous B then Alcohol_D; send Cruiser to Alcohol_C";
                break;

            case 20:
                Answer1.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_D then Alcohol_C; send Cruiser to Alcohol_B";
                Answer2.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_C then Alcohol_D; send Cruiser to Alcohol_B";
                Answer3.GetComponentInChildren<Text>().text = "Send Cruiser to Alcohol_D; send RIB to Alcohol_C";
                Answer4.GetComponentInChildren<Text>().text = "Send RIB to Alcohol_B; send Cruiser to Alcohol_C then Alcohol_D";
                break;
        }

        LogHelper.QuestionLoaded(true_question_id);
    }

    public void OnAnswerClicked(int choice)
    {
        selected = choice;
        Answer1.GetComponentInChildren<Image>().color = Color.white;
        Answer2.GetComponentInChildren<Image>().color = Color.white;
        Answer3.GetComponentInChildren<Image>().color = Color.white;
        Answer4.GetComponentInChildren<Image>().color = Color.white;

        if (selected > 0 && confidenceVal > 0)
        {
            NextButton.GetComponent<Button>().interactable = true;
        }

        switch (choice)
        {
            case 1:
                Answer1.GetComponentInChildren<Image>().color = Color.yellow;
                break;
            case 2:
                Answer2.GetComponentInChildren<Image>().color = Color.yellow;
                break;
            case 3:
                Answer3.GetComponentInChildren<Image>().color = Color.yellow;
                break;
            case 4:
                Answer4.GetComponentInChildren<Image>().color = Color.yellow;
                break;
        }

        LogHelper.AnswerChanged(true_question_id);
    }

    public void OnToggleClicked()
    {
        var toggles = Likert.GetComponent<ToggleGroup>().ActiveToggles();
        if (toggles.Count() == 1)
        {
            //print(toggles.First().name);
            confidenceVal = Convert.ToInt32(toggles.First().name);
            if (selected > 0 && confidenceVal > 0)
            {
                NextButton.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void OnMetaCogEndEdit()
    {
        var textbox = GameObject.FindGameObjectWithTag("MetaCogInput").GetComponent<InputField>();

        var text = textbox.text;
        int val;
        bool isValid = Int32.TryParse(text, out val);
        if (isValid)
        {
            if (val < 0)
                val = 0;
            if (val > 20)
                val = 20;

            LogHelper.SetExpectedCorrect(val, is_postmeta);
            textbox.text = val.ToString();
            //enable next button, set metacog response 
            NextButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            NextButton.GetComponent<Button>().interactable = false;
        }
    }


    public void SetQuestionOverlay(bool visible)
    {
        if (visible)
        {
            //ShowQuestion.SetActive(false);
            //ResetCamera.SetActive(true);
            //TilesetMap.SetActive(false);
            //OnlineMaps.instance.control.allowUserControl = true;
            OnlineMaps.instance.showMarkerTooltip = OnlineMapsShowMarkerTooltip.always;
            //OnlineMapsControlBase3D.instance.allowCameraControl = true;
            if (StudyFlowControl.dynamicVisualizationType == "none")
                return;
            TimeSlider.SetActive(true);
        }

        else
        {
            //ShowQuestion.SetActive(true);
            //ResetCamera.SetActive(false);
            //TilesetMap.SetActive(true);
            //OnlineMaps.instance.control.allowUserControl = false;
            OnlineMaps.instance.showMarkerTooltip = OnlineMapsShowMarkerTooltip.none;
            //OnlineMapsControlBase3D.instance.allowCameraControl = false;

            TimeSlider.SetActive(false);
        }
    }

    public void RandomizeAnswerOrder()
    {
        List<Vector3> AnswerPositions = new List<Vector3>();
        AnswerPositions.Add(new Vector3(10, -40, 0));
        AnswerPositions.Add(new Vector3(10, -70, 0));
        AnswerPositions.Add(new Vector3(10, -100, 0));
        AnswerPositions.Add(new Vector3(10, -130, 0));
        AnswerPositions = AnswerPositions.OrderBy(a => UnityEngine.Random.Range(0, 300)).ToList();

        Answer1.GetComponent<RectTransform>().anchoredPosition3D = AnswerPositions[0];
        Answer2.GetComponent<RectTransform>().anchoredPosition3D = AnswerPositions[1];
        Answer3.GetComponent<RectTransform>().anchoredPosition3D = AnswerPositions[2];
        Answer4.GetComponent<RectTransform>().anchoredPosition3D = AnswerPositions[3];
    }

    public static string EasyLowCoordScenario1 = "TimeStep: 0\nCameraPosition: -81.24525 19.38077\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 5\nAsset -81.271 19.37468 2.5 4 Party_Location\nAsset -81.259 19.38412 2.5 1 Cruiser_1\nAsset -81.2505 19.37218 2.5 1 Cruiser_2\nCase -81.23893 19.39709 FanLowComponentLowUncertainty 0.6993894 0.104185 -0.104185 0.6993894 12 20 2.5 1 Alcohol_B_1\nCase -81.22606 19.38578 FanLowComponentLowUncertainty 0.5409579 0.455373 -0.455373 0.5409579 12 20 2.5 1 Alcohol_B_2";
    public static string EasyLowCoordScenario2 = "TimeStep: 0\nCameraPosition: -81.24161 19.38552\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.271 19.37468 2.5 4 Party_Location\nAsset -81.27195 19.39266 2.5 0 Yacht_1\nAsset -81.24855 19.37099 2.5 0 Yacht_2\nCase -81.29896 19.42184 FanLowComponentLowUncertainty 0.5760783 -0.4100413 0.4100413 0.5760783 12 20 2.5 1 Alcohol_A_1\nCase -81.20564 19.39717 FanLowComponentLowUncertainty 0.6976072 0.115517 -0.115517 0.6976072 12 20 2.5 1 Alcohol_A_2\nCase -81.20823 19.38254 FanLowComponentHighUncertainty 0.5193071 0.4799168 -0.4799168 0.5193071 12 20 2.5 1 Alcohol_A_3";
    public static string EasyLowCoordScenario3 = "TimeStep: 0\nCameraPosition: -81.24703 19.41183\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.27119 19.37344 2.5 4 Party_Location\nAsset -81.24588 19.37209 2.5 3 Jetski_2\nAsset -81.26513 19.38625 2.5 3 Jetski_1\nCase -81.29086 19.41296 FanLowComponentLowUncertainty 0.5510766 -0.443074 0.443074 0.5510766 12 20 2.5 0 Food_B_1\nCase -81.24879 19.42285 FanLowComponentLowUncertainty 0.6657796 -0.2381965 0.2381965 0.6657796 12 20 2.5 0 Food_B_2\nCase -81.22087 19.41478 FanLowComponentHighUncertainty 0.6600416 0.2536635 -0.2536635 0.6600416 12 20 2.5 0 Food_B_3";
    public static string EasyLowCoordScenario4 = "TimeStep: 0\nCameraPosition: -81.244 19.39642\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.27126 19.37471 2.5 4 Party_Location\nAsset -81.25816 19.38949 2.5 2 RIB_1\nAsset -81.23679 19.37083 2.5 2 RIB_2\nCase -81.28588 19.42467 FanLowComponentMidUncertainty -0.5528712 0.4408327 -0.4408327 -0.5528712 12 20 2.5 2 Miscellaneous_A_1\nCase -81.2248 19.41811 FanLowComponentLowUncertainty -0.6924919 0.143021 -0.143021 -0.6924919 12 20 2.5 2 Miscellaneous_A_2\nCase -81.18993 19.40554 FanLowComponentHighUncertainty 0.6807385 0.1912985 -0.1912985 0.6807385 12 20 2.5 2 Miscellaneous_A_3";
    public static string EasyLowCoordScenario5 = "TimeStep: 0\nCameraPosition: -81.22191 19.39058\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 5\nAsset -81.271 19.37468 2.5 4 Party_Location\nAsset -81.28886 19.374 2.5 1 Cruiser_1\nAsset -81.25111 19.37115 2.5 1 Cruiser_2\nCase -81.28863 19.39559 FanLowComponentLowUncertainty 0.6511787 -0.2756199 0.2756199 0.6511787 12 20 2.5 2 Miscellaneous_A_1\nCase -81.22191 19.39058 FanLowComponentLowUncertainty 0.5955689 0.3811793 -0.3811793 0.5955689 12 20 2.5 2 Miscellaneous_A_2";
    public static string HardLowCoordScenario1 = "TimeStep: 0\nCameraPosition: -81.24073 19.37335\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.271 19.37468 2.5 4 Party_Location\nAsset -81.259 19.38412 2.5 1 Cruiser_1\nAsset -81.2505 19.37218 2.5 1 Cruiser_2\nAsset -81.23658 19.36381 2.5 1 Cruiser_3\nCase -81.23893 19.39709 FanLowComponentLowUncertainty 0.6993894 0.104185 -0.104185 0.6993894 12 20 2.5 1 Alcohol_B_2\nCase -81.22606 19.38578 FanLowComponentMidUncertainty 0.5409579 0.455373 -0.455373 0.5409579 12 20 2.5 1 Alcohol_B_3\nCase -81.26778 19.40145 FanLowComponentLowUncertainty 0.6771851 -0.20352 0.20352 0.6771851 12 20 2.5 1 Alcohol_B_1\nCase -81.22287 19.37322 FanLowComponentHighUncertainty 0.5329686 0.4646983 -0.4646983 0.5329686 12 20 2.5 1 Alcohol_B_4";
    public static string HardLowCoordScenario2 = "TimeStep: 0\nCameraPosition: -81.2595 19.41862\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.27119 19.37344 2.5 4 Party_Location\nAsset -81.24588 19.37209 2.5 3 Jetski_3\nAsset -81.28762 19.37653 2.5 3 Jetski_1\nAsset -81.25792 19.38111 2.5 3 Jetski_2\nCase -81.30099 19.42426 FanLowComponentLowUncertainty 0.5544948 -0.4387887 0.4387887 0.5544948 12 20 2.5 1 Alcohol_C_1\nCase -81.26655 19.45795 FanLowComponentLowUncertainty 0.5012743 -0.4987225 0.4987225 0.5012743 12 20 2.5 1 Alcohol_C_2\nCase -81.22659 19.40205 FanLowComponentMidUncertainty 0.6864409 0.1697022 -0.1697022 0.6864409 12 20 2.5 1 Alcohol_C_3\nCase -81.20549 19.37898 FanLowComponentHighUncertainty 0.6778581 0.201267 -0.201267 0.6778581 12 20 2.5 1 Alcohol_C_4";
    public static string HardLowCoordScenario3 = "TimeStep: 0\nCameraPosition: -81.25122 19.39177\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.271 19.37468 2.5 4 Party_Location\nAsset -81.27999 19.39092 2.5 0 Yacht_1\nAsset -81.22308 19.36294 2.5 0 Yacht_3\nAsset -81.24709 19.38515 2.5 0 Yacht_2\nCase -81.29896 19.42184 FanLowComponentLowUncertainty 0.5760783 -0.4100413 0.4100413 0.5760783 12 20 2.5 1 Alcohol_A_1\nCase -81.20564 19.39717 FanLowComponentLowUncertainty 0.6976072 0.115517 -0.115517 0.6976072 12 20 2.5 1 Alcohol_A_3\nCase -81.20823 19.38254 FanLowComponentLowUncertainty 0.5193071 0.4799168 -0.4799168 0.5193071 12 20 2.5 1 Alcohol_A_4\nCase -81.25691 19.42271 FanLowComponentHighUncertainty 0.7070931 -0.004408919 0.004408919 0.7070931 12 20 2.5 1 Alcohol_A_2";
    public static string HardLowCoordScenario4 = "TimeStep: 0\nCameraPosition: -81.2533 19.38179\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.27248 19.37178 2.5 4 Party_Location\nAsset -81.2932 19.38126 2.5 0 Yacht_1\nAsset -81.26351 19.39091 2.5 0 Yacht_2\nAsset -81.23058 19.3672 2.5 0 Yacht_3\nCase -81.20707 19.38862 FanLowComponentLowUncertainty 0.5966478 0.3794884 -0.3794884 0.5966478 12 20 2.5 2 Miscellaneous_A_4\nCase -81.23913 19.41993 FanLowComponentLowUncertainty 0.6635187 0.2444239 -0.2444239 0.6635187 12 20 2.5 2 Miscellaneous_A_3\nCase -81.27921 19.45022 FanLowComponentMidUncertainty 0.1899254 -0.6811229 0.6811229 0.1899254 12 20 2.5 2 Miscellaneous_A_2\nCase -81.29799 19.43408 FanLowComponentMidUncertainty 0.3366838 -0.6218071 0.6218071 0.3366838 12 20 2.5 2 Miscellaneous_A_1";
    public static string HardLowCoordScenario5 = "TimeStep: 0\nCameraPosition: -81.22642 19.40953\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.27126 19.37471 2.5 4 Party_Location\nAsset -81.23772 19.36569 2.5 2 RIB_3\nAsset -81.24606 19.38703 2.5 2 RIB_2\nAsset -81.26215 19.39359 2.5 2 RIB_1\nCase -81.20613 19.40118 FanLowComponentLowUncertainty -0.4303205 -0.561092 0.561092 -0.4303205 12 20 2.5 1 Alcohol_D_4\nCase -81.22444 19.42735 FanLowComponentLowUncertainty 0.7053628 0.04963232 -0.04963232 0.7053628 12 20 2.5 1 Alcohol_D_2\nCase -81.21464 19.41619 FanLowComponentLowUncertainty 0.5792167 0.4055958 -0.4055958 0.5792167 12 20 2.5 1 Alcohol_D_3\nCase -81.28096 19.4362 FanLowComponentHighUncertainty 0.6781326 0.2003402 -0.2003402 0.6781326 12 20 2.5 1 Alcohol_D_1";

    public static string EasyHighCoordScenario1 = "TimeStep: 0\nCameraPosition: -81.2382 19.41912\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.25083 19.37785 2.5 0 Yacht\nAsset -81.2355 19.36762 2.5 1 Cruiser\nCase -81.21477 19.40968 FanLowComponentLowUncertainty 0.6782379 0.1999833 -0.1999833 0.6782379 11 20 2.5 0 Food_A\nCase -81.24934 19.43634 FanLowComponentMidUncertainty -0.4969133 0.5030678 -0.5030678 -0.4969133 11 20 2.5 2 Miscellaneous_A\nAsset -81.39784 19.44628 2.5 5 Rough_Weather_Region\nAsset -81.26976 19.36897 2.5 4 Party_Location";
    public static string EasyHighCoordScenario2 = "TimeStep: 0\nCameraPosition: -81.22221 19.38432\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 8\nAsset -81.26167 19.37947 2.5 3 Jetski\nAsset -81.24854 19.37076 2.5 2 RIB\nAsset -81.23901 19.36163 2.5 1 Cruiser\nAsset -81.27163 19.37236 2.5 4 Party_Location\nCase -81.209 19.37673 FanLowComponentMidUncertainty 0.5797381 0.4048503 -0.4048503 0.5797381 12 20 2.5 7 Boat_Alcohol_Misc\nCase -81.21906 19.3915 FanLowComponentHighUncertainty 0.7064686 -0.03003541 0.03003541 0.7064686 12 20 2.5 8 Boat_Food_Misc\nAsset -81.23644 19.49437 2.5 5 Rough_Weather_Region\nCase -81.19906 19.36101 FanLowComponentLowUncertainty 0.5945908 0.3827032 -0.3827032 0.5945908 12 20 2.5 1 Alcohol_D";
    public static string EasyHighCoordScenario3 = "TimeStep: 0\nCameraPosition: -81.21456 19.37445\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.26956 19.37117 2.5 4 Party_Location\nAsset -81.24542 19.37622 2.5 2 RIB\nAsset -81.21875 19.36396 2.5 3 Jetski\nCase -81.23116 19.44279 FanLowComponentLowUncertainty 0.6006532 0.3731162 -0.3731162 0.6006532 11 20 2.5 1 Alcohol_C\nCase -81.19367 19.42417 FanLowComponentMidUncertainty -0.3812723 -0.5955094 0.5955094 -0.3812723 11 20 2.5 1 Alcohol_D\nCase -81.21456 19.37845 FanLowComponentHighUncertainty 0.5973303 0.3784131 -0.3784131 0.5973303 11 20 2.5 2 Miscellaneous_B";
    public static string EasyHighCoordScenario4 = "TimeStep: 0\nCameraPosition: -81.21521 19.40801\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 7\nAsset -81.25597 19.37533 2.5 0 Yacht\nAsset -81.22282 19.36058 2.5 2 RIB\nCase -81.28008 19.46006 FanLowComponentMidUncertainty -0.4762481 0.5226737 -0.5226737 -0.4762481 12 20 2.5 0 Food_B\nCase -81.14143 19.42994 FanLowComponentHighUncertainty -0.4373023 -0.5556678 0.5556678 -0.4373023 12 20 2.5 1 Alcohol_A\nCase -81.19088 19.43814 FanLowComponentMidUncertainty 0.6420029 0.2963651 -0.2963651 0.6420029 12 20 2.5 1 Alcohol_B\nCase -81.2595 19.47056 FanLowComponentLowUncertainty 0.6260041 -0.3288144 0.3288144 0.6260041 12 20 2.5 1 Alcohol_C\nAsset -81.27163 19.37236 2.5 4 Party_Location";
    public static string EasyHighCoordScenario5 = "TimeStep: 0\nCameraPosition: -81.21342 19.39842\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.23303 19.3588 2.5 3 Jetski\nAsset -81.2527 19.36962 2.5 1 Cruiser\nCase -81.1994 19.39123 FanLowComponentLowUncertainty 0.7030551 0.07558814 -0.07558814 0.7030551 11 20 2.5 0 Food_B\nCase -81.23669 19.42711 FanLowComponentMidUncertainty -0.6797493 0.1947843 -0.1947843 -0.6797493 11 20 2.5 2 Miscellaneous_A\nCase -81.18028 19.38142 FanLowComponentHighUncertainty 0.591758 0.387069 -0.387069 0.591758 11 20 2.5 2 Miscellaneous_B\nAsset -81.27163 19.37236 2.5 4 Party_Location";
    public static string HardHighCoordScenario1 = "TimeStep: 0\nCameraPosition: -81.2469804 19.4104214\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 9\nAsset -81.24635 19.38061 2.5 0 Yacht\nAsset -81.21902 19.36619 2.5 1 Cruiser\nAsset -81.28236 19.36207 2.5 3 Jetski\nAsset -81.08108 19.43819 2.5 5 Rough_Weather_Region\nAsset -81.27073 19.37293 2.5 4 Party_Location\nCase -81.27893 19.50026 FanLowComponentLowUncertainty -0.1766341 -0.68469 0.68469 -0.1766341 12 20 2.5 1 Alcohol_A\nCase -81.24046 19.49115 FanLowComponentMidUncertainty -0.3742138 -0.5999701 0.5999701 -0.3742138 12 20 2.5 2 Miscellaneous_A\nCase -81.32135 19.44357 FanLowComponentHighUncertainty -0.5892783 0.3908337 -0.3908337 -0.5892783 12 20 2.5 0 Food_B\nCase -81.20753 19.52588 FanLowComponentMidUncertainty -0.3020799 -0.6393338 0.6393338 -0.3020799 12 20 2.5 0 Food_A";
    public static string HardHighCoordScenario2 = "TimeStep: 0\nCameraPosition: -81.2499866 19.41366\nCameraOrientation: 45 0\nCameraZoom: 11\nOverallDefaultZoom: 12\nMarkers: 9\nAsset -81.27361 19.38483 1.25 0 Yacht_2\nAsset -81.24477 19.37055 1.25 2 RIB\nAsset -81.27113 19.36802 1.25 4 Party_Location\nAsset -81.29276 19.36735 1.25 0 Yacht_1\nCase -81.24766 19.43286 FanLowComponentHighUncertainty 0.5724248 0.4151263 -0.4151263 0.5724248 12 20 2.5 0 Food_B\nCase -81.25081 19.45682 FanLowComponentHighUncertainty 0.7070788 0.006291297 -0.006291297 0.7070788 12 20 2.5 1 Alcohol_A\nCase -81.28576 19.44579 FanLowComponentLowUncertainty 0.6429034 -0.2944064 0.2944064 0.6429034 12 20 2.5 1 Alcohol_B\nCase -81.21865 19.37754 FanLowComponentLowUncertainty -0.4919465 -0.5079258 0.5079258 -0.4919465 12 20 2.5 6 Boat_Alcohol_Food\nAsset -81.08545 19.46504 2.5 5 Rough_Weather_Region";
    public static string HardHighCoordScenario3 = "TimeStep: 0\nCameraPosition: -81.2505 19.41962\nCameraOrientation: 45 0\nCameraZoom: 11\nOverallDefaultZoom: 12\nMarkers: 9\nAsset -81.27361 19.38483 1.25 0 Yacht_2\nAsset -81.24477 19.37055 1.25 2 RIB\nAsset -81.27113 19.36802 1.25 4 Party_Location\nAsset -81.29276 19.36735 1.25 0 Yacht_1\nCase -81.23545 19.40592 FanLowComponentLowUncertainty -0.384049 -0.5937225 0.5937225 -0.384049 12 20 2.5 0 Food_A\nCase -81.24766 19.43286 FanLowComponentHighUncertainty 0.5724248 0.4151263 -0.4151263 0.5724248 12 20 2.5 0 Food_B\nCase -81.25081 19.45682 FanLowComponentHighUncertainty 0.7070788 0.006291297 -0.006291297 0.7070788 12 20 2.5 1 Alcohol_A\nCase -81.28576 19.44579 FanLowComponentLowUncertainty 0.6429034 -0.2944064 0.2944064 0.6429034 12 20 2.5 1 Alcohol_B\nAsset -81.08192 19.47361 2.5 5 Rough_Weather_Region";
    public static string HardHighCoordScenario4 = "TimeStep: 0\nCameraPosition: -81.186958 19.4148787\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 9\nAsset -81.27293 19.3744 2.5 4 Party_Location\nAsset -81.22563 19.35926 2.5 2 RIB\nAsset -81.19791 19.36156 2.5 1 Cruiser\nAsset -81.21875 19.36406 2.5 3 Jetski\nCase -81.15582 19.40181 FanLowComponentLowUncertainty 0.434446 0.5579038 -0.5579038 0.434446 12 20 2.5 1 Alcohol_B\nCase -81.14268 19.4342 FanLowComponentHighUncertainty 0.473477 0.5251853 -0.5251853 0.473477 12 20 2.5 1 Alcohol_C\nCase -81.14349 19.46709 FanLowComponentHighUncertainty 0.6856416 0.1729033 -0.1729033 0.6856416 12 20 2.5 1 Alcohol_D\nCase -81.20152 19.50382 FanLowComponentMidUncertainty 0.5090681 0.4907644 -0.4907644 0.5090681 12 20 2.5 4 Rival_Cruiser\nCase -81.17028 19.48142 FanLowComponentHighUncertainty 0.591758 0.387069 -0.387069 0.591758 11 20 2.5 2 Miscellaneous_B";
    public static string HardHighCoordScenario5 = "TimeStep: 0\nCameraPosition: -81.1982725 19.41687825\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 7\nAsset -81.27293 19.3744 2.5 4 Party_Location\nAsset -81.22563 19.35926 2.5 2 RIB\nAsset -81.19791 19.36156 2.5 1 Cruiser\nCase -81.15582 19.40181 FanLowComponentLowUncertainty 0.434446 0.5579038 -0.5579038 0.434446 11 20 2.5 1 Alcohol_B\nCase -81.14268 19.4342 FanLowComponentMidUncertainty -0.4453648 -0.5492269 0.5492269 -0.4453648 11 20 2.5 1 Alcohol_C\nCase -81.14349 19.46709 FanLowComponentHighUncertainty 0.3104229 -0.6353248 0.6353248 0.3104229 11 20 2.5 1 Alcohol_D\nCase -81.23241 19.51345 FanLowComponentMidUncertainty -0.4936072 -0.5063121 0.5063121 -0.4936072 11 20 2.5 4 Rival_Cruiser";

    public static string SampleScenario = "TimeStep: 0\nCameraPosition: -81.23402 19.39164\nCameraOrientation: 45 0\nCameraZoom: 12\nOverallDefaultZoom: 12\nMarkers: 6\nAsset -81.29793 19.39118 2.5 1 Cruiser\nCase -81.22998 19.41105 FanLowComponentMidUncertainty 0.6395245 -0.3016762 0.3016762 0.6395245 12 20 2.5 2 Miscellaneous_B\nAsset -81.34115 19.38161 2.5 3 Jetski\nCase -81.37633 19.51238 FanLowComponentMidUncertainty 0.3707853 -0.6020949 0.6020949 0.3707853 12 20 2.5 8 Boat_Food_Misc\nAsset -81.42116 19.48264 2.5 5 Rough_Weather_Region\nAsset -81.26874 19.36763 2.5 4 Party_Location";

}
