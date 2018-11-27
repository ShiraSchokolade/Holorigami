using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class OrigamiManager : MonoBehaviour {

    // prefabs to instantiate
    public GameObject workingArea;
    public GameObject startButton;
    public List<GameObject> origamiStepPrefabs;

    [Tooltip("Vertical distance of objects to working plane")]
    public float distanceToTable = 0.1f;

    /// <summary>
    /// null if only the start button is shown,
    /// current instruction model otherwise
    /// </summary>
    private GameObject currentShownModel;
    private MeshRenderer workingAreaMesh;
    private TapToPlace tapToPlace;

    private Vector3 objectPosition;
    private int currentModelID = 0;
    private bool startButtonClicked = false; 

    // true if workingare has been moved to a new position  
    // -> pos. of UI and 3D models have to be moved too
    private bool changedPosition = true;

    void Start () {

        if (origamiStepPrefabs.Count == 0)
        {
            Debug.Log("No prefabs defined");
        }

        // instantiate woking Area
        if(workingArea != null)
        {
            workingArea = Instantiate(workingArea);
            objectPosition = new Vector3(workingArea.transform.position.x, workingArea.transform.position.y + distanceToTable, workingArea.transform.position.z);
        }
        else
        {
            Debug.Log("No working area defined");
        }

        // instantiate start button
        startButton = Instantiate(startButton, objectPosition, Quaternion.identity);
        SetButtonEvent();
        startButton.SetActive(false);

        tapToPlace = workingArea.transform.Find("WorkingPlane").gameObject.GetComponent<TapToPlace>();
        workingAreaMesh = workingArea.transform.Find("Cube").gameObject.GetComponent<MeshRenderer>();
	}
	
	void Update () {

        // placement mode
        if (tapToPlace.IsBeingPlaced)
        {
            // just switched to placement mode
            // -> show workingArea boundings, hide model and button
            if (!changedPosition)
            {
                changedPosition = true;
                workingAreaMesh.enabled = true;

                if (currentShownModel != null)
                    currentShownModel.SetActive(false);

                if (startButton != null)
                    startButton.SetActive(false);
            }
        }
        // static mode
        else
        {   
            // just switched to static mode
            if (changedPosition)
            {
                // hide workingArea boundings, save new position
                workingAreaMesh.enabled = false;
                objectPosition = new Vector3(workingArea.transform.position.x, workingArea.transform.position.y + distanceToTable, workingArea.transform.position.z);

                if (currentShownModel != null)
                {
                    // move current model to new position/rotation and show it again
                    currentShownModel.SetActive(true);
                    currentShownModel.transform.position = objectPosition;
                    currentShownModel.transform.rotation = workingArea.transform.rotation;
                }
                // start button has not been clicked yet
                else
                {
                    startButton.SetActive(true);
                    startButton.transform.position = objectPosition;
                }
                changedPosition = false;
            }
        }	
	}

    /// <summary>
    /// Instantiate next instruction model, destroy the last
    /// </summary>
    public void ShowNext()
    {
        if (startButtonClicked)
        {
            if (currentModelID >= origamiStepPrefabs.Count)
                currentModelID = 0;

            if (currentShownModel != null)
                Destroy(currentShownModel);

            currentShownModel = Instantiate(origamiStepPrefabs[currentModelID], objectPosition, workingArea.transform.rotation);
            currentModelID++;
        }
    }

    /// <summary>
    /// hide start button, call method to show first instruction model
    /// </summary>
    public void StartGame()
    {
        startButtonClicked = true;
        startButton.SetActive(false);
        ShowNext();
    }

    /// <summary>
    /// reset to first instruction model
    /// </summary>
    public void ResetGame()
    {
        currentModelID = 0;
        ShowNext();
    }

    private void SetButtonEvent()
    {
        UnityEvent e = new UnityEvent();
        e.AddListener(StartGame);
        startButton.transform.Find("Button").gameObject.GetComponent<InteractiveButton>().OnSelectEvents = e;
    }
}
