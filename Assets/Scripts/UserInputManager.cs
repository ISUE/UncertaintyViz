using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserInputManager : MonoBehaviour {
    public bool manipulationEnabled = false;
    public bool hidePreviousTimeSteps = false;

    public List<GameObject> assetPrefab;
    public List<GameObject> casePrefab;

    public int selectedAsset = 0;
    public int selectedTarget = 0;


    public string[] assetNames = new string[] { "Yacht", "SmallBoat", "RIB", "JetSki", "PartyLocation", "RoughWeatherRegion" };
    public string[] caseNames = new string[] { "Food", "Alcohol", "Misc.", "RivalYacht", "RivalSmallBoat", "RivalJetski", "BoatAF", "BoatAM", "BoatFM" };

    public float[] assetScales;
    public float[] caseScales;

    public GameObject csOverlay;

    GameObject canvas;
    private bool isDragging = false;
    private OnlineMapsMarker3D dragged;

    private bool isRotating = false;
    private GameObject rotated;

    private bool isFileWindowOpen = false;
    private bool doFinalizeLoad = false;
    private bool doFinalizeLoad2 = false;
    private List<Quaternion> rotations = new List<Quaternion>();

    private List<string> types = new List<string>();
    private List<float> scales = new List<float>();
    private List<int> zooms = new List<int>();

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<float> randomNoise = new List<float>();

    const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int num_yacht = 0, num_food = 0, num_misc = 0, num_alcohol = 0;

    private float sliderDownTime = 0;
    private float sliderDelta = 0;
    private float sliderAbsoluteDelta = 0;

    private float timeStep = 0;
    public bool isAnimating = false;

    // Use this for initialization
    void Start() {
        canvas = GameObject.FindGameObjectWithTag("CanvasRoot");
        UniFileBrowser.use.SendWindowCloseMessage(FileWindowClosed);
        assetScales = new float[] { 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f };
        caseScales = new float[] { 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f };

        CheckMapCompleteLoaded.OnMapCompleteLoaded += OnMapCompleteLoaded;
        OnlineMaps.instance.OnChangeZoom += OnChangeZoom;

        OnlineMapsControlBase3D.instance.OnMapDrag += OnMapDrag;
        OnlineMapsControlBase3D.instance.OnMapRelease += OnMapDrag;
        OnlineMapsControlBase3D.instance.OnMapClick += OnMapClick;
        OnlineMapsControlBase3D.instance.OnMapDoubleClick += OnMapClick;

        resetRandom();
    }

    // Update is called once per frame
    void Update() {
        if (doFinalizeLoad)
        {
            if (!doFinalizeLoad2)
                doFinalizeLoad2 = true;
            else
            {
                int count = 0;
                for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
                {
                    var isAsset = false;
                    var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;

                    foreach (var assetP in assetPrefab)
                    {
                        if (prefabType.name == assetP.name)
                            isAsset = true;
                    }
                    if (!isAsset)
                    {
                        //Appropriately rename for running the study
                        if (SceneManager.GetActiveScene().name.Contains("Study") && StudyFlowControl.visualizationType.Contains("line"))
                        {
                            switch (types[count])
                            {
                                case "FanLowComponentLowUncertainty":
                                    types[count] = "LineCloudLow";
                                    break;
                                case "FanLowComponentMidUncertainty":
                                    types[count] = "LineCloudMid";
                                    break;
                                case "FanLowComponentHighUncertainty":
                                    types[count] = "LineCloudHigh";
                                    break;
                            }
                        }

                        var temp = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponent<ConeHelper>().ChangePrefabToType(types[count], scales[count], zooms[count]);
                        temp.transform.rotation = rotations[count];
                        count++;
                    }
                    /*
                    foreach (Transform child in OnlineMapsControlBase3D.instance.markers3D[ii].transform)
                    {
                        print(child.name);
                        if (!child.name.Contains("Cylinder"))
                        {
                            child.rotation = rotations[ii];
                            OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponent<ConeHelper>().ChangePrefabToType(types[ii]);
                            break;
                        }
                    }*/
                }
                rotations.Clear();
                types.Clear();

                doFinalizeLoad = false;
                doFinalizeLoad2 = false;
            }
        }

        if (isAnimating)
        {
            GameObject.Find("TimeSlider").GetComponent<Slider>().value += 2 * Time.deltaTime;
            if (GameObject.Find("TimeSlider").GetComponent<Slider>().value >= GameObject.Find("TimeSlider").GetComponent<Slider>().maxValue)
            {
                ResetTimeInformation();
                LogHelper.AnimationLooped(StudyFlowControl.TrueQuestionID);
            }
            else
            {
                TimeStepChangedLerp();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        if (!manipulationEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Home))
        {
            SaveScreenshot();
        }

        if (isDragging)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                dragged = null;
            }
            else
            {
                OnMarkerDrag(dragged);
            }
        }

        if (isRotating)
        {
            if (Input.GetMouseButtonUp(0) || rotated == null)
            {
                isRotating = false;
                rotated = null;
            }
            else
            {
                OnRotateCone(rotated);
            }
        }
        if (Input.GetMouseButtonDown(0) && !isDragging && !isRotating && !isFileWindowOpen)
        {
            RaycastHit[] hits;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
            var bestDistance = float.MaxValue;
            OnlineMapsMarker3D bestMarker = null;
            if (hits.Length > 0)
            {
                foreach (OnlineMapsMarker3D marker in OnlineMapsControlBase3D.instance.markers3D)
                {
                    var distance = CheckHitMarker(hits, marker.instance.gameObject);
                    if (distance >= 0 && distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestMarker = marker;
                    }
                }
                if (bestMarker != null)
                {
                    OnlineMapsControlBase3D.instance.allowUserControl = false;
                    isDragging = true;
                    dragged = bestMarker;
                    OnMarkerDrag(dragged);
                }
                if (!isDragging)
                {
                    foreach (GameObject cone in GameObject.FindGameObjectsWithTag("Uncertainty"))
                    {
                        if (CheckHit(hits, cone))
                        {
                            OnlineMapsControlBase3D.instance.allowUserControl = false;
                            isRotating = true;
                            rotated = cone;
                            OnRotateCone(rotated);
                            break;
                        }
                    }
                }
            }
        }
        if (!isDragging && !isRotating && !isFileWindowOpen)
            OnlineMapsControlBase3D.instance.allowUserControl = true;
    }

    void resetRandom()
    {
        randomNoise = new List<float>();
        for (int ii = 0; ii < 100; ii++)
            randomNoise.Add(UnityEngine.Random.Range(-1f, 1f));
    }

    float CheckHitMarker(RaycastHit[] check, GameObject target)
    {
        foreach (RaycastHit rh in check)
        {
            var temp = rh.collider.gameObject;
            while (temp != null)
            {
                if (temp.tag == "Uncertainty")
                    return -1;
                if (temp == target)
                    return rh.distance;
                if (temp.transform.parent)
                    temp = temp.transform.parent.gameObject;
                else temp = null;
            }
        }


        return -1;
    }

    bool CheckHit(RaycastHit[] check, GameObject target)
    {
        foreach (RaycastHit rh in check)
        {
            var temp = rh.collider.gameObject;
            while (temp != null)
            {
                if (temp == target)
                    return true;
                if (temp.transform.parent)
                    temp = temp.transform.parent.gameObject;
                else temp = null;
            }
        }


        return false;
    }

    private void OnMarkerDrag(OnlineMapsMarker3D obj)
    {
        var position = OnlineMapsControlBase3D.instance.GetCoords();
        obj.SetPosition(position.x, position.y);

    }

    private void OnRotateCone(GameObject cone)
    {
        //Get the Screen positions of the object
        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(cone.transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //Ta Daaa
        cone.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, angle + 90f));
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    //Screenshot handling code
    public void SaveScreenshot()
    {
        StartCoroutine(CaptureScreen());
    }

    public IEnumerator CaptureScreen()
    {
        // Wait till the last possible moment before screen rendering to hide the UI
        yield return null;
        canvas.SetActive(false);

        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();

        Directory.CreateDirectory(Application.dataPath + "/screens/");
        string filename = Application.dataPath + "/screens/" + DateTime.Now.Ticks + ".png";
        ScreenCapture.CaptureScreenshot(filename, 4);
        yield return null;
        print("Image written to " + filename);

        // Show UI after we're done
        canvas.SetActive(true);
    }

    public void SaveClicked()
    {
        isFileWindowOpen = true;
        UniFileBrowser.use.SaveFileWindow(SaveFile);
        //disable online maps controls here
        OnlineMaps.instance.control.allowUserControl = false;
    }

    public void LoadClicked()
    {
        isFileWindowOpen = true;
        UniFileBrowser.use.OpenFileWindow(OpenFile);
        //disable online maps controls here
        OnlineMaps.instance.control.allowUserControl = false;
    }

    public void ClearClicked()
    {
        num_yacht = num_misc = num_food = num_alcohol = 0;
        OnlineMapsControlBase3D.instance.RemoveAllMarker3D();
    }

    public void AddCaseClicked()
    {
        string suffix = "";
        if (selectedTarget == 0)
        {
            suffix = "_" + Digits[num_food % 26];
            num_food++;
        }
        if (selectedTarget == 1)
        {
            suffix = "_" + Digits[num_alcohol % 26];
            num_alcohol++;
        }
        if (selectedTarget == 2)
        {
            suffix = "_" + Digits[num_misc % 26];
            num_misc++;
        }
        var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(OnlineMaps.instance.position, casePrefab[selectedTarget]);
        newMarker.scale = caseScales[selectedTarget];
        if (suffix != "")
            newMarker.label = caseNames[selectedTarget] + suffix;
    }

    public void AddAssetClicked()
    {
        string suffix = "";
        if (selectedAsset == 0)
        {
            suffix = "_" + Digits[num_yacht % 26];
            num_yacht++;
        }
        var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(OnlineMaps.instance.position, assetPrefab[selectedAsset]);
        newMarker.scale = assetScales[selectedAsset];
        //if (suffix != "")
        //    newMarker.label = assetNames[selectedAsset] + suffix;                
    }

    public void RemoveClicked()
    {
        if (OnlineMapsControlBase3D.instance.markers3D.Length == 0)
            return;
        OnlineMapsMarker3D closest = OnlineMapsControlBase3D.instance.markers3D[0];
        var minDistance = float.PositiveInfinity;
        foreach (OnlineMapsMarker3D marker in OnlineMapsControlBase3D.instance.markers3D)
        {
            var distance = (OnlineMaps.instance.position - marker.position).magnitude;
            if (distance < minDistance)
            {
                closest = marker;
                minDistance = distance;
            }
        }
        if (closest != null && closest.instance.activeInHierarchy)
            OnlineMapsControlBase3D.instance.RemoveMarker3D(closest);
    }



    public void ResetCameraClicked()
    {
        OnlineMapsControlBase3D.instance.cameraDistance = 400;
        if (OnlineMapsControlBase3D.instance.cameraRotation.x == 45 && OnlineMapsControlBase3D.instance.cameraRotation.y == 0)
            OnlineMapsControlBase3D.instance.cameraRotation = new Vector2(0, 0);
        else
            OnlineMapsControlBase3D.instance.cameraRotation = new Vector2(45, 0);
    }

    public void ZoomInClicked()
    {
        OnlineMaps.instance.zoom++;
    }

    public void ZoomOutClicked()
    {
        OnlineMaps.instance.zoom--;
    }

    void FileWindowClosed()
    {
        isFileWindowOpen = false;
        //enable online maps controls here
        OnlineMaps.instance.control.allowUserControl = true;
    }

    public void OpenFile(string path)
    {
        try
        {
            StreamReader reader = new StreamReader(path);

            var markers = OnlineMapsControlBase3D.instance.markers3D;
            var cameraZoom = OnlineMaps.instance.zoom;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                string[] tokens = line.Split();
                switch (tokens[0])
                {
                    case "TimeStep:":
                        timeStep = int.Parse(tokens[1]);
                        break;
                    case "CameraPosition:":
                        OnlineMaps.instance.position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                        break;
                    case "CameraOrientation:":
                        OnlineMapsControlBase3D.instance.cameraRotation = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                        break;
                    case "CameraZoom:":
                        //OnlineMaps.instance.zoom = int.Parse(tokens[1]);
                        break;
                    case "OverallDefaultZoom:":
                        ScaleFix.SetOverallDefaultZoom(int.Parse(tokens[1]));
                        break;
                    case "Markers:":
                        OnlineMapsControlBase3D.instance.RemoveAllMarker3D();
                        int count = int.Parse(tokens[1]);
                        for (int ii = 0; ii < count; ii++)
                        {
                            line = reader.ReadLine();
                            tokens = line.Split();
                            if (tokens[0] == "Asset")
                            {
                                var position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                                var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(position, assetPrefab[int.Parse(tokens[4])]);
                                newMarker.scale = float.Parse(tokens[3]);

                                if (int.Parse(tokens[4]) == 0)
                                {
                                    num_yacht++;
                                }
                                newMarker.label = "";
                            }
                            else if (tokens[0] == "Case")
                            {
                                var position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                                var cone_name = tokens[3];
                                var cone_rotation = new Quaternion(float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6]), float.Parse(tokens[7]));
                                var cone_zoom = int.Parse(tokens[8]);
                                var cone_scale = float.Parse(tokens[9]);
                                var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(position, casePrefab[int.Parse(tokens[11])]);
                                newMarker.scale = float.Parse(tokens[10]);

                                //Depending on what type of case it is, we want to add a label to the object.
                                var index = int.Parse(tokens[11]);
                                if (index < 3)
                                {
                                    newMarker.label = tokens[12];
                                    switch (index)
                                    {
                                        case 0:
                                            num_food++;
                                            break;
                                        case 1:
                                            num_alcohol++;
                                            break;
                                        case 2:
                                            num_misc++;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                rotations.Add(cone_rotation);
                                types.Add(cone_name);
                                zooms.Add(cone_zoom);
                                scales.Add(cone_scale);

                                /*foreach (Transform child in newMarker.transform)
                                {
                                    print(child.name);
                                    if (!child.name.Contains("Cylinder"))
                                    {
                                        child.rotation = cone_rotation;
                                        newMarker.transform.gameObject.GetComponent<ConeHelper>().ChangePrefabToType(cone_name);
                                        break;
                                    }
                                }*/
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            doFinalizeLoad = true;
            reader.Close();
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    public void OpenStream(string instream)
    {
        try
        {
            StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(instream)));
            var markers = OnlineMapsControlBase3D.instance.markers3D;
            var cameraZoom = OnlineMaps.instance.zoom;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                string[] tokens = line.Split();
                switch (tokens[0])
                {
                    case "TimeStep:":
                        timeStep = int.Parse(tokens[1]);
                        break;
                    case "CameraPosition:":
                        OnlineMaps.instance.position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                        break;
                    case "CameraOrientation:":
                        OnlineMapsControlBase3D.instance.cameraRotation = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                        break;
                    case "CameraZoom:":
                        //OnlineMaps.instance.zoom = int.Parse(tokens[1]);
                        break;
                    case "OverallDefaultZoom:":
                        ScaleFix.SetOverallDefaultZoom(int.Parse(tokens[1]));
                        break;
                    case "Markers:":
                        OnlineMapsControlBase3D.instance.RemoveAllMarker3D();
                        int count = int.Parse(tokens[1]);
                        for (int ii = 0; ii < count; ii++)
                        {
                            line = reader.ReadLine();
                            tokens = line.Split();
                            if (tokens[0] == "Asset")
                            {
                                var position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                                var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(position, assetPrefab[int.Parse(tokens[4])]);
                                newMarker.scale = float.Parse(tokens[3]);

                                if (int.Parse(tokens[4]) == 0)
                                {
                                    num_yacht++;
                                }
                                newMarker.label = "";
                                //Some hacking here for labels on certain questions
                                if ((StudyFlowControl.TrueQuestionID == 17 || StudyFlowControl.TrueQuestionID == 18) && int.Parse(tokens[4]) == 0)
                                    newMarker.label = tokens[5];

                                if (Camera.main.GetComponent<StudyFlowControl>() != null &&
                                    StudyFlowControl.TrueQuestionID <= 10 &&
                                    StudyFlowControl.TrueQuestionID > 0 &&
                                    int.Parse(tokens[4]) < 4)
                                    newMarker.label = tokens[5];
                            }
                            else if (tokens[0] == "Case")
                            {
                                var position = new Vector2(float.Parse(tokens[1]), float.Parse(tokens[2]));
                                var cone_name = tokens[3];
                                var cone_rotation = new Quaternion(float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6]), float.Parse(tokens[7]));
                                var cone_zoom = int.Parse(tokens[8]);
                                var cone_scale = float.Parse(tokens[9]);
                                var newMarker = OnlineMapsControlBase3D.instance.AddMarker3D(position, casePrefab[int.Parse(tokens[11])]);
                                newMarker.scale = float.Parse(tokens[10]);
                                
                                //Depending on what type of case it is, we want to add a label to the object.
                                var index = int.Parse(tokens[11]);
                                if (index < 3)
                                {
                                    newMarker.label = tokens[12];
                                    switch (index)
                                    {
                                        case 0:
                                            num_food++;
                                            break;
                                        case 1:
                                            num_alcohol++;
                                            break;
                                        case 2:
                                            num_misc++;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                rotations.Add(cone_rotation);
                                types.Add(cone_name);
                                zooms.Add(cone_zoom);
                                scales.Add(cone_scale);

                                /*foreach (Transform child in newMarker.transform)
                                {
                                    print(child.name);
                                    if (!child.name.Contains("Cylinder"))
                                    {
                                        child.rotation = cone_rotation;
                                        newMarker.transform.gameObject.GetComponent<ConeHelper>().ChangePrefabToType(cone_name);
                                        break;
                                    }
                                }*/
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            doFinalizeLoad = true;
            reader.Close();
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    //Saving scenario to file for future loading.
    public void SaveFile(string path)
    {
        var markers = OnlineMapsControlBase3D.instance.markers3D;
        var cameraPosition = OnlineMaps.instance.position;
        var cameraZoom = OnlineMaps.instance.zoom;
        var cameraOrientation = OnlineMapsControlBase3D.instance.cameraRotation;
        var overallDefaultZoom = ScaleFix.GetOverallDefaultZoom();

        StreamWriter fw = new StreamWriter(path);
        string[] separators = { ",", ".", "!", "?", ";", ":", " ", "(", ")" };
        fw.WriteLine("TimeStep: " + timeStep);
        fw.WriteLine("CameraPosition: " + cameraPosition.x + " " + cameraPosition.y);
        fw.WriteLine("CameraOrientation: " + cameraOrientation.x + " " + cameraOrientation.y);
        fw.WriteLine("CameraZoom: " + cameraZoom);
        fw.WriteLine("OverallDefaultZoom: " + overallDefaultZoom);

        fw.WriteLine("Markers: " + markers.Length);

        foreach (var marker in markers)
        {
            string type = "null";
            int index = assetPrefab.IndexOf(marker.prefab);
            var position = marker.position;
            if (index != -1)
            {
                type = "Asset";
                var label = "";
                if (marker.label != "")
                    label = marker.label;
                else
                    label = assetNames[index];
                fw.WriteLine(type + " " + position.x + " " + position.y + " " + marker.scale + " " + index + " " + label);
            }
            else
            {
                index = casePrefab.IndexOf(marker.prefab);

                type = "Case";
                Quaternion cone_rotation = new Quaternion();
                var cone_name = "";
                int cone_zoom = 1;
                float cone_scale = 20;
                foreach (Transform child in marker.transform)
                {
                    if (!child.name.Contains("Cylinder") && !child.name.Contains("yatch") && !child.name.Contains("scp_sb") && !child.name.Contains("Hamburger"))
                    {
                        cone_name = child.name.Split(separators, StringSplitOptions.RemoveEmptyEntries)[0];
                        cone_rotation = child.rotation;
                        print(cone_name);
                        cone_zoom = child.GetComponent<ConeScale>().defaultZoom;
                        cone_scale = child.GetComponent<ConeScale>().defaultScale;
                    }
                }

                var label = "";
                if (marker.label != "")
                    label = marker.label;
                else
                    label = caseNames[index];
                fw.WriteLine(type + " " + position.x + " " + position.y + " " + cone_name + " " +
                    cone_rotation.x + " " + cone_rotation.y + " " + cone_rotation.z + " " + cone_rotation.w + " " + cone_zoom + " " + cone_scale
                    + " " + caseScales[index] + " " + index + " " + label);
            }

        }
        fw.Flush();
        fw.Close();
    }


    //DEPRECATED whenever the timestep changes, update all positions appropriately.
    public void TimeStepChanged()
    {
        var newTimeStep = GameObject.Find("TimeSlider").GetComponent<Slider>().value;

        var delta = newTimeStep - timeStep;
        sliderDelta += delta;
        sliderAbsoluteDelta += Mathf.Abs(delta);

        if (newTimeStep == 0 && originalPositions.Count > 0)
        {
            //print("RESETING POSTIONS");
            resetRandom();
            for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
            {
                var isAsset = false;
                var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;
                foreach (var assetP in assetPrefab)
                {
                    if (prefabType.name == assetP.name)
                        isAsset = true;
                }
                if (isAsset)
                    continue;
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.position = originalPositions[ii];
                var cone = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponentInChildren<ConeHelper>().getCone();
                cone.transform.localPosition = Vector3.zero;
            }
            originalPositions.Clear();
        }

        if (timeStep == 0 && newTimeStep != 0)
        {
            //print("STORING POSTIONS");

            originalPositions = new List<Vector3>();
            for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
            {
                originalPositions.Add(OnlineMapsControlBase3D.instance.markers3D[ii].transform.position);
            }
        }

        timeStep = newTimeStep;
        for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
        {
            var isAsset = false;
            var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;

            foreach (var assetP in assetPrefab)
            {
                if (prefabType.name == assetP.name)
                    isAsset = true;
            }
            if (isAsset)
                continue;


            var cone = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponentInChildren<ConeHelper>().getCone();

            if (newTimeStep != 0 && !isAnimating)
                cone.transform.parent.GetComponent<ConeHelper>().setTransparent(true);
            else
            {
                cone.transform.parent.GetComponent<ConeHelper>().setTransparent(false);
            }
            var oldChildPosition = cone.transform.position;

            //The visualizations fit a normal distributions, so we calculate movement based on these distributions.
            if (cone.name.Contains("Left"))
            {
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.up * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * 10);
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(cone.transform.right * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (Mathf.Floor(timeStep) * 2.15f));

            }
            else if (cone.name.Contains("Right"))
            {
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.up * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * 10);
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.right * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (Mathf.Floor(timeStep) * 2.15f));

            }
            else if (cone.name.Contains("LowU") || cone.name.Contains("LineCloudLow"))
            {
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.up * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (9.5f + randomNoise[(ii * ii) % 100]));
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(cone.transform.right * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (Mathf.Floor(timeStep) * .9f * randomNoise[ii % 100]));

            }
            else if (cone.name.Contains("MidU") || cone.name.Contains("LineCloudMid"))
            {
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.up * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (9.5f + randomNoise[(ii * ii) % 100]));
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(cone.transform.right * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (Mathf.Floor(timeStep) * 1.8f * randomNoise[ii % 100]));

            }
            else
            {
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(-cone.transform.up * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (9.5f + randomNoise[(ii * ii) % 100]));
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.Translate(cone.transform.right * delta * OnlineMapsControlBase3D.instance.markers3D[ii].scale * (Mathf.Floor(timeStep) * 2.6f * randomNoise[ii % 100]));

            }
            cone.transform.position = oldChildPosition;

        }
    }
    
    //whenever the timestep changes, update all positions appropriately.
    public void TimeStepChangedLerp()
    {
        var newTimeStep = GameObject.Find("TimeSlider").GetComponent<Slider>().value;

        var delta = newTimeStep - timeStep;

        sliderDelta += delta;
        sliderAbsoluteDelta += Mathf.Abs(delta);

        if (newTimeStep == 0 && originalPositions.Count > 0)
        {
            //print("RESETING POSTIONS");
            resetRandom();
            for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
            {
                var isAsset = false;
                var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;
                foreach (var assetP in assetPrefab)
                {
                    if (prefabType.name == assetP.name)
                        isAsset = true;
                }
                if (isAsset)
                    continue;
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.position = originalPositions[ii];
                var cone = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponentInChildren<ConeHelper>().getCone();
                cone.transform.localPosition = Vector3.zero;
            }
        }


        if ((timeStep == 0 && newTimeStep != 0))
        {
            //print("STORING POSTIONS");

            originalPositions = new List<Vector3>();
            for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
            {
                originalPositions.Add(OnlineMapsControlBase3D.instance.markers3D[ii].transform.position);
            }
        }

        timeStep = newTimeStep;
        for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
        {
            var isAsset = false;
            var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;

            foreach (var assetP in assetPrefab)
            {
                if (prefabType.name == assetP.name)
                    isAsset = true;
            }
            if (isAsset)
                continue;

            var cone = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponentInChildren<ConeHelper>().getCone();

            if (newTimeStep != 0 && !isAnimating)
                cone.transform.parent.GetComponent<ConeHelper>().setTransparent(true);
            else
            {
                cone.transform.parent.GetComponent<ConeHelper>().setTransparent(false);
            }
            var oldChildPosition = cone.transform.position;

            List<Vector3> path;
            if (cone.name.Contains("LowU") || cone.name.Contains("LineCloudLow"))
            {
                path = generatePath(originalPositions[ii], -cone.transform.up, cone.transform.right, OnlineMapsControlBase3D.instance.markers3D[ii].scale, 7, ii, 9.5f, 0.5f);                
            }
            else if (cone.name.Contains("MidU") || cone.name.Contains("LineCloudMid"))
            {
                path = generatePath(originalPositions[ii], -cone.transform.up, cone.transform.right, OnlineMapsControlBase3D.instance.markers3D[ii].scale, 7, ii, 9.5f, 1.0f);                
            }
            else
            {
                path = generatePath(originalPositions[ii], -cone.transform.up, cone.transform.right, OnlineMapsControlBase3D.instance.markers3D[ii].scale, 7, ii, 9.5f, 1.5f);                
            }
            OnlineMapsControlBase3D.instance.markers3D[ii].transform.position = MultiLerp(timeStep, path);
            cone.transform.position = oldChildPosition;
        }
    }

    public Vector3 MultiLerp(float t, List<Vector3> path)
    {
        var idx1 = Mathf.FloorToInt(t);
        var idx2 = Mathf.CeilToInt(t);
        return Vector3.Lerp(path[idx1], path[idx2], (t - idx1));
    }

    public List<Vector3> generatePath(Vector3 initialPoint, Vector3 direction_up, Vector3 direction_right, float scale, int totalPoints, int index, float distance_traveled, float cone_width)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(initialPoint);
        int count = 1;
        while(path.Count <= totalPoints)
        {
            path.Add(initialPoint + (direction_up * scale * count * (distance_traveled + randomNoise[(index * index) % 100])) +
                                   (direction_right * scale * cone_width * count * (count-1) * randomNoise[(index) % 100]));
            count++;
        }

        return path;
    }

    //Whenever we reset time to 0, we want all the objects and cases to return to their original positions
    private void ResetTimeInformation()
    {
        if (timeStep > 0)
        {
            timeStep = 0;
            GameObject.Find("TimeSlider").GetComponent<Slider>().value = 0;

            for (int ii = 0; ii < OnlineMapsControlBase3D.instance.markers3D.Length; ii++)
            {
                var isAsset = false;
                var prefabType = OnlineMapsControlBase3D.instance.markers3D[ii].prefab;

                foreach (var assetP in assetPrefab)
                {
                    if (prefabType.name == assetP.name)
                        isAsset = true;
                }
                if (isAsset)
                    continue;

                var cone = OnlineMapsControlBase3D.instance.markers3D[ii].transform.gameObject.GetComponentInChildren<ConeHelper>().getCone();
                if(!isAnimating)
                    cone.transform.parent.GetComponent<ConeHelper>().setTransparent(false);

                cone.transform.localPosition = Vector3.zero;
                OnlineMapsControlBase3D.instance.markers3D[ii].transform.position = originalPositions[ii];
            }
        }
    }


    //Below here are various animation related tools.
    public void AnimateClicked()
    {
        if(isAnimating)
        {
            isAnimating = false;
            GameObject.Find("PlaySimulation").GetComponentInChildren<Text>().text = "Animate";
            ResetTimeInformation();
            LogHelper.SetTimeAnimating(StudyFlowControl.TrueQuestionID, false);
        }
        else
        {
            LogHelper.SetTimeToFirstManipulation(StudyFlowControl.TrueQuestionID);
            LogHelper.UsedDynamicVisualization(StudyFlowControl.TrueQuestionID);
            LogHelper.SetTimeAnimating(StudyFlowControl.TrueQuestionID, true);
            isAnimating = true;
            GameObject.Find("PlaySimulation").GetComponentInChildren<Text>().text = "Stop Animation";
            resetRandom();
            ResetTimeInformation();
        }
    }

    public void IncrementAnimateClicked()
    {
        LogHelper.IncrementAnimateClickCount(StudyFlowControl.TrueQuestionID);
    }

    public void NextClicked()
    {
        if (csOverlay.activeSelf)
        {
            LogHelper.ClosedCheatSheet(StudyFlowControl.TrueQuestionID);
            csOverlay.SetActive(false);
        }
        if (isAnimating)
            AnimateClicked();
    }

    public void TimeSliderUsed()
    {
        LogHelper.SetTimeToFirstManipulation(StudyFlowControl.TrueQuestionID);
        LogHelper.UsedDynamicVisualization(StudyFlowControl.TrueQuestionID);
    }

    public void TimeSliderPressed()
    {
        sliderDownTime = Time.timeSinceLevelLoad;
        sliderDelta = 0;
        sliderAbsoluteDelta = 0;
    }

    public void TimeSliderReleased()
    {
        LogHelper.FinishedManipulatingSlider(StudyFlowControl.TrueQuestionID, Time.timeSinceLevelLoad - sliderDownTime, sliderDelta, sliderAbsoluteDelta);
    }

    public void AssetDropdownChanged()
    {     
        selectedAsset = GameObject.Find("AssetDropdown").GetComponent<Dropdown>().value;
        Debug.Log(selectedAsset);
    }

    public void CaseDropdownChanged()
    {
        selectedTarget = GameObject.Find("CaseDropdown").GetComponent<Dropdown>().value;
    }

    private void OnMapCompleteLoaded()
    {
        if (StudyFlowControl.dynamicVisualizationType == "none")
            return;
        GameObject.Find("TimeSlider").GetComponent<Slider>().interactable = true;
        if (StudyFlowControl.dynamicVisualizationType == "movie")

            GameObject.Find("PlaySimulation").GetComponent<Button>().interactable = true;
    }

    public void ToggleLabels(bool state)
    {
        if (state)
            OnlineMaps.instance.showMarkerTooltip = OnlineMapsShowMarkerTooltip.always;
        else
            OnlineMaps.instance.showMarkerTooltip = OnlineMapsShowMarkerTooltip.none;
    }

    //Animating and allowing map control are weird with OpenMaps, so we are limiting them for now.
    private void OnMapDrag()
    {
        if (StudyFlowControl.dynamicVisualizationType == "none")
            return;
        if (isAnimating)
            AnimateClicked();
        ResetTimeInformation();
    }

    private void OnMapClick()
    {
        if (StudyFlowControl.dynamicVisualizationType == "none")
            return;
        ResetTimeInformation();
    }

    private void OnChangeZoom()
    {
        if (StudyFlowControl.dynamicVisualizationType == "none")
            return;
        GameObject.Find("TimeSlider").GetComponent<Slider>().interactable = false;
        GameObject.Find("PlaySimulation").GetComponent<Button>().interactable = false;
        if (isAnimating)
            AnimateClicked();
        ResetTimeInformation();
    }

    //Specific to the study survey code, used for logging and UI.
    public void CheatSheetClicked(string imName)
    {

        string currentImageString = csOverlay.GetComponent<Image>().sprite.name;
        if (csOverlay.activeSelf && (currentImageString == imName))
        {
            csOverlay.SetActive(false);
            LogHelper.ClosedCheatSheet(StudyFlowControl.TrueQuestionID);
            ToggleLabels(true);
        }
        else
        {
            csOverlay.SetActive(true);
            LogHelper.UsedCheatSheet(StudyFlowControl.TrueQuestionID);
            ToggleLabels(false);
        }
    }
}
