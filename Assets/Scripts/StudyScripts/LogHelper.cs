using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LogHelper : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void SendResults(string json_message);

    public static int question_ct = 20;

    private static List<int> submitted_answer; 
    private static List<int> confidence; 

    private static List<float> time_to_submit; 
    private static List<float> time_to_first_answer; 
    private static List<float> time_to_last_answer; 
    private static List<float> time_at_start_of_question; 
    private static List<float> time_to_first_manipulation; 
    private static List<float> time_to_last_manipulation; 

    private static List<List<float>> time_manipulatingSlider; 
    private static List<List<float>> delta_manipulatingSlider; 
    private static List<List<float>> absolutedelta_manipulatingSlider; 

    private static List<int> answer_changed;
    private static List<int> answer_changed_after_manipulation;

    private static List<int> animation_loops; 
    private static List<int> animation_button_clicked_count;
    private static List<float> time_animating;

    private static List<float> time_at_start_of_animation;

    private static List<int> used_dynamic_visualization;

    private static List<int> used_cheat_sheet;
    private static List<float> time_at_first_cheat_sheet;
    private static List<float> time_using_cheat_sheet;

    private static float total_time;
    private static int precog_num;
    private static int postcog_num;

    // Use this for initialization
    void Start () {
        submitted_answer = new List<int>(new int[question_ct]);
        confidence = new List<int>(new int[question_ct]);
        answer_changed = new List<int>(new int[question_ct]);
        answer_changed_after_manipulation = new List<int>(new int[question_ct]);
        used_dynamic_visualization = new List<int>(new int[question_ct]);
        animation_loops = new List<int>(new int[question_ct]);

        time_to_submit = new List<float>(new float[question_ct]);
        time_to_first_answer = new List<float>(new float[question_ct]);
        time_to_last_answer = new List<float>(new float[question_ct]);
        time_at_start_of_question = new List<float>(new float[question_ct]);
        time_to_first_manipulation = new List<float>(new float[question_ct]);
        time_to_last_manipulation = new List<float>(new float[question_ct]);

        animation_button_clicked_count = new List<int>(new int[question_ct]);
        time_animating = new List<float>(new float[question_ct]);
        time_at_start_of_animation = new List<float>(new float[question_ct]);

        time_manipulatingSlider = new List<List<float>>();
        delta_manipulatingSlider = new List<List<float>>();
        absolutedelta_manipulatingSlider = new List<List<float>>();
        for (int ii = 0; ii < question_ct; ii++)
        {
            time_manipulatingSlider.Add(new List<float>());
            delta_manipulatingSlider.Add(new List<float>());
            absolutedelta_manipulatingSlider.Add(new List<float>());
        }

        used_cheat_sheet = new List<int>(new int[question_ct]);
        time_at_first_cheat_sheet = new List<float>(new float[question_ct]);
        time_using_cheat_sheet = new List<float>(new float[question_ct]);
    }
	
	// Update is called once per frame
	void Update () {
    }

    public static void QuestionLoaded(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        time_at_start_of_question[index - 1] = Time.timeSinceLevelLoad;
    }

    public static void AnimationLooped(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        animation_loops[index - 1]++;
    }

    public static void AnswerChanged(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        answer_changed[index-1]++;

        if (((Time.timeSinceLevelLoad - time_at_start_of_question[index - 1]) > time_to_last_manipulation[index - 1]) && (time_to_last_manipulation[index - 1] > time_to_last_answer[index - 1]))
        {
            answer_changed_after_manipulation[index - 1]++;
        }

        if (time_to_first_answer[index-1] <= float.Epsilon)
        {
            time_to_first_answer[index-1] = Time.timeSinceLevelLoad - time_at_start_of_question[index-1];
        }        
        time_to_last_answer[index-1] = Time.timeSinceLevelLoad - time_at_start_of_question[index-1];
        
    }

    public static void AnswerSubmitted(int index, int selected_answer, int selected_confidence)
    {
        if (index < 0 || index > question_ct)
            return;

        submitted_answer[index-1] = selected_answer;
        confidence[index - 1] = selected_confidence;
        time_to_submit[index-1] = Time.timeSinceLevelLoad - time_at_start_of_question[index-1];
    }

    public static void UsedDynamicVisualization(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        used_dynamic_visualization[index-1]++;
    }

    public static void FinishedManipulatingSlider(int index, float time, float delta, float absolute_delta)
    {
        if (index < 0 || index > question_ct)
            return;

        time_manipulatingSlider[index-1].Add(time);
        delta_manipulatingSlider[index - 1].Add(delta);
        absolutedelta_manipulatingSlider[index - 1].Add(absolute_delta);
    }

    public static void IncrementAnimateClickCount(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        animation_button_clicked_count[index-1]++;
    }

    public static void SetTimeToFirstManipulation(int index)
    {
        if (index < 0 || index > question_ct)
            return;

        time_to_last_manipulation[index - 1] = Time.timeSinceLevelLoad - time_at_start_of_question[index - 1];
        if (time_to_first_manipulation[index-1] <= float.Epsilon)
        {
            time_to_first_manipulation[index - 1] = time_to_last_manipulation[index - 1];
        }        
    }

    public static void SetTimeAnimating(int index, bool isAnimating)
    {
        if (index < 0 || index > question_ct)
            return;

        if (!isAnimating)
        {
            time_animating[index - 1] += Time.timeSinceLevelLoad - time_at_start_of_question[index - 1] - time_at_start_of_animation[index - 1];
        }
        else
        {
            time_at_start_of_animation[index - 1] = Time.timeSinceLevelLoad - time_at_start_of_question[index - 1];            
        }
    }

    public static void SetExpectedCorrect(int prediction, bool is_post)
    {
        if (is_post)
            postcog_num = prediction;
        else
            precog_num = prediction;
    }

    static float last_cheat_sheet_activate = 0;   
    public static void UsedCheatSheet(int index)
    {
        if (index < 0 || index > question_ct)
            return;
        if (time_at_first_cheat_sheet[index - 1] <= float.Epsilon) // if the time hasn't been set yet
            time_at_first_cheat_sheet[index - 1] = Time.timeSinceLevelLoad - time_at_start_of_question[index - 1];

        if (last_cheat_sheet_activate <= 0)
        {
            last_cheat_sheet_activate = Time.timeSinceLevelLoad;
        }

        used_cheat_sheet[index - 1]++;
    }

    public static void ClosedCheatSheet(int index)
    {
        if (index < 0 || index > question_ct)
            return;
        // Do something with the timer here.
        time_using_cheat_sheet[index - 1] += (Time.timeSinceLevelLoad - last_cheat_sheet_activate);
        last_cheat_sheet_activate = 0;
    }

    public static float Mean(List<float> input)
    {
        float sum = 0;
        foreach(float f in input)
        {
            sum += f;
        }

        return input.Count > 0 ? sum / input.Count : 0;
    }


    public static string PrintList(List<float> input)
    {
        string outstring = "";
        foreach (float f in input)
        {
            outstring = outstring + f + ",";
        }

        return outstring + "";
    }

    public static void StudyCompleted(string userIdentity, string viz_type, string dynamic_viz_type)
    {
        print("Study Complete, pushing to database");
        total_time = Time.timeSinceLevelLoad;

        var results = new StudyResults();
        results.UserIdentity = userIdentity;
        results.VisualizationType = viz_type;
        results.DynamicVisualizationType = dynamic_viz_type;
        results.TotalTime = total_time;

        var TimeText = new List<string>();
        var DeltaText = new List<string>();
        var AbsoluteDeltaText = new List<string>();

        var MeanTime = new List<float>();
        var MeanDelta = new List<float>();
        var MeanAbsoluteDelta = new List<float>();

        for(int ii = 0; ii < question_ct; ii++)
        {
            TimeText.Add(PrintList(time_manipulatingSlider[ii]));
            DeltaText.Add(PrintList(delta_manipulatingSlider[ii]));
            AbsoluteDeltaText.Add(PrintList(absolutedelta_manipulatingSlider[ii]));
            MeanTime.Add(Mean(time_manipulatingSlider[ii]));
            MeanDelta.Add(Mean(delta_manipulatingSlider[ii]));
            MeanAbsoluteDelta.Add(Mean(absolutedelta_manipulatingSlider[ii]));
        }

        results.MeanTimeManipulatingSlider = MeanTime;
        results.MeanDeltaManipulatingSlider = MeanDelta;
        results.MeanAbsoluteDeltaManipulatingSlider = MeanAbsoluteDelta;

        results.TimeManipulatingSlider = TimeText;
        results.DeltaManipulatingSlider = DeltaText;
        results.AbsoluteDeltaManipulatingSlider = AbsoluteDeltaText;        

        results.AnimationLoops = animation_loops;
        results.AnimationButtonClickedCount = animation_button_clicked_count;
        results.TimeAnimating = time_animating; //Might need to divide this by the number of distinct animation activation clicks (should be used_dynamic_visualization)

        results.AnswerChanged = answer_changed;
        results.AnswerChangedAfterManipulation = answer_changed_after_manipulation;
        results.SubmittedAnswer = submitted_answer;
        results.SubmittedConfidence = confidence;

        results.TimeToFirstManipulation = time_to_first_manipulation;
        results.TimeAtStartOfQuestion = time_at_start_of_question;
        results.TimeToFirstAnswer = time_to_first_answer;
        results.TimeToLastAnswer = time_to_last_answer;
        results.TimeToSubmit = time_to_submit;
        results.UsedDynamicVisualization = used_dynamic_visualization;

        results.PreMetacognitionEstimate = precog_num;
        results.PostMetacognitionEstimate = postcog_num;

        results.CheatSheetUsed = used_cheat_sheet;
        results.TimeToFirstCheatSheet = time_at_first_cheat_sheet;
        results.TotalTimeLookingAtCheatSheet = time_using_cheat_sheet;

        var package = JsonUtility.ToJson(results) ?? "";
        print("Package contents in LogHelper: " + package);
        SendResults(package);
    }


    public void ConfirmWorking()
    {
        print("Displaying Properly");
        total_time = Time.timeSinceLevelLoad;

        var results = new StudyResults();
        results.UserIdentity = "confirm";
        results.VisualizationType = "none";
        results.DynamicVisualizationType = "none";
        results.TotalTime = total_time;

        var TimeText = new List<string>();
        var DeltaText = new List<string>();
        var AbsoluteDeltaText = new List<string>();

        var MeanTime = new List<float>();
        var MeanDelta = new List<float>();
        var MeanAbsoluteDelta = new List<float>();

        for (int ii = 0; ii < question_ct; ii++)
        {
            TimeText.Add(PrintList(time_manipulatingSlider[ii]));
            DeltaText.Add(PrintList(delta_manipulatingSlider[ii]));
            AbsoluteDeltaText.Add(PrintList(absolutedelta_manipulatingSlider[ii]));
            MeanTime.Add(Mean(time_manipulatingSlider[ii]));
            MeanDelta.Add(Mean(delta_manipulatingSlider[ii]));
            MeanAbsoluteDelta.Add(Mean(absolutedelta_manipulatingSlider[ii]));
        }

        results.MeanTimeManipulatingSlider = MeanTime;
        results.MeanDeltaManipulatingSlider = MeanDelta;
        results.MeanAbsoluteDeltaManipulatingSlider = MeanAbsoluteDelta;

        results.TimeManipulatingSlider = TimeText;
        results.DeltaManipulatingSlider = DeltaText;
        results.AbsoluteDeltaManipulatingSlider = AbsoluteDeltaText;

        results.AnimationLoops = animation_loops;
        results.AnimationButtonClickedCount = animation_button_clicked_count;
        results.TimeAnimating = time_animating; //Might need to divide this by the number of distinct animation activation clicks (should be used_dynamic_visualization)

        results.AnswerChanged = answer_changed;
        results.AnswerChangedAfterManipulation = answer_changed_after_manipulation;
        results.SubmittedAnswer = submitted_answer;
        results.SubmittedConfidence = confidence;

        results.TimeToFirstManipulation = time_to_first_manipulation;
        results.TimeAtStartOfQuestion = time_at_start_of_question;
        results.TimeToFirstAnswer = time_to_first_answer;
        results.TimeToLastAnswer = time_to_last_answer;
        results.TimeToSubmit = time_to_submit;
        results.UsedDynamicVisualization = used_dynamic_visualization;

        results.PreMetacognitionEstimate = precog_num;
        results.PostMetacognitionEstimate = postcog_num;

        results.CheatSheetUsed = used_cheat_sheet;
        results.TimeToFirstCheatSheet = time_at_first_cheat_sheet;
        results.TotalTimeLookingAtCheatSheet = time_using_cheat_sheet;

        var package = JsonUtility.ToJson(results) ?? "";
        print("Package contents in LogHelper: " + package);
        SendResults(package);   
    }
}

[Serializable]
public class StudyResults
{
    public string UserIdentity;
    public string VisualizationType;
    public string DynamicVisualizationType;
    public float TotalTime;
    public int PreMetacognitionEstimate;
    public int PostMetacognitionEstimate;

    public List<int> SubmittedAnswer;
    public List<int> SubmittedConfidence;  
    public List<float> TimeToSubmit; 
    public List<float> TimeToFirstAnswer; 
    public List<float> TimeToLastAnswer;
    public List<float> TimeAtStartOfQuestion;
    public List<float> TimeToFirstManipulation;

    public List<int> AnimationLoops;
    public List<int> AnimationButtonClickedCount;
    public List<float> TimeAnimating;

    public List<string> TimeManipulatingSlider;
    public List<string> DeltaManipulatingSlider;
    public List<string> AbsoluteDeltaManipulatingSlider;

    public List<float> MeanTimeManipulatingSlider; 
    public List<float> MeanDeltaManipulatingSlider; 
    public List<float> MeanAbsoluteDeltaManipulatingSlider;
    

    public List<int> AnswerChanged; 
    public List<int> AnswerChangedAfterManipulation;
    
    public List<int> UsedDynamicVisualization;

    public List<int> CheatSheetUsed;
    public List<float> TimeToFirstCheatSheet;
    public List<float> TotalTimeLookingAtCheatSheet;
}
