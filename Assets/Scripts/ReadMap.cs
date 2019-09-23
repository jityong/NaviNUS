using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class ReadMap : MonoBehaviour, PlacenoteListener {
    

    private List<string> buildings = new List<string>();
    private List<string> floors = new List<string>();
    private List<string> destinations = new List<string>();
    private string mapName = "";

    private UnityARSessionNativeInterface mSession;
    private bool mARInit = false;
    private bool isNavReady = false;
    private bool isNavigating = false;

    private LibPlacenote.MapMetadataSettable mCurrMapDetails;

    string currMapID = String.Empty;

    private LibPlacenote.MapInfo mSelectedMapInfo;
    private string mSelectedMapId {
        get {
            return mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
        }
    }

    public GameObject navigationPanel;
    public GameObject navigationPanelText;
    public GameObject buildingButton1;
    public GameObject buildingButton2;
    public GameObject buildingButton3;
    public GameObject buildingButton4;
    public GameObject buildingButton5;

    public GameObject floorButton1;
    public GameObject floorButton2;
    public GameObject floorButton3;
    public GameObject floorButton4;
    public GameObject floorButton5;

    public GameObject destinationButton1;
    public GameObject destinationButton2;
    public GameObject destinationButton3;
    public GameObject destinationButton4;
    public GameObject destinationButton5;




    public GameObject building1;  
    public GameObject building2;
    public GameObject building3;
    public GameObject building4;
    public GameObject building5;

    public GameObject floor1;
    public GameObject floor2;
    public GameObject floor3;
    public GameObject floor4;
    public GameObject floor5;

    public GameObject destination1;
    public GameObject destination2;
    public GameObject destination3;
    public GameObject destination4;
    public GameObject destination5;

    public GameObject loadingText;
    public GameObject navigationButton;

    //public List<GameObject> buildingList = new List<GameObject>();

    // Use this for initialization
    void Start() {
        Input.location.Start();

        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        StartARKit();
        FeaturesVisualizer.EnablePointcloud();
        LibPlacenote.Instance.RegisterListener(this);        

    }

    void OnDisable() {
    }

    // Update is called once per frame
    void Update() {
        if (!mARInit && LibPlacenote.Instance.Initialized())
        {
            getBuildings();
            Debug.Log("After get building in Start()");

            Debug.Log("Ready to Start!!!!!!2");
            mARInit = true;            
        }

        if (isNavReady)
        {            
            FindMap();
            isNavReady = false;
        }
    }

    private void getBuildings()
    {
        loadingText.SetActive(true);
        LibPlacenote.Instance.ListMaps((LibPlacenote.MapInfo[] obj) => {
            foreach (LibPlacenote.MapInfo map in obj)
            {
                if (map.metadata.name != null)
                {
                    Debug.Log(map.metadata.name);
                    string[] buildingNameTemp = map.metadata.name.Split(new char[] { '/' });
                    string buildingName = buildingNameTemp[0];
                    if (!buildings.Contains(buildingName))
                    {
                        buildings.Add(buildingName);
                        Debug.Log("buildingName:" + buildingName);
                        Debug.Log("BuildingName0 " + buildings[0]);
                    }
                }                
            }
            Debug.Log("Exit getBuildings()");
            Debug.Log("buildings: " + string.Join(", ", buildings.ToArray()));
            Debug.Log("number of buildings1:" + buildings.Count);
            buildings.Sort();
            loadingText.SetActive(false);
            if (buildings.Count >= 1)
            {
                buildingButton1.SetActive(true);
                building1.GetComponent<Text>().text = buildings[0];
                Debug.Log("Building button 1 is assigned with " + buildings[0]);
                Debug.Log("number of buildings2:" + buildings.Count);
            }
            if (buildings.Count >= 2)
            {
                buildingButton2.SetActive(true);
                building2.GetComponent<Text>().text = buildings[1];
            }
            if (buildings.Count >= 3)
            {
                buildingButton3.SetActive(true);
                building3.GetComponent<Text>().text = buildings[2];
            }
            if (buildings.Count >= 4)
            {
                buildingButton4.SetActive(true);
                building4.GetComponent<Text>().text = buildings[3];
            }
            if (buildings.Count >= 5)
            {
                buildingButton5.SetActive(true);
                building5.GetComponent<Text>().text = buildings[4];
            }
        });        
 
    }

    public void getFloors()
    {
        loadingText.SetActive(true);
        //List<string> buildingsHere = new List<string>();
        LibPlacenote.Instance.ListMaps((LibPlacenote.MapInfo[] obj) => {
            foreach (LibPlacenote.MapInfo map in obj)
            {
                if (map.metadata.name != null && mapName != null)
                {
                    Debug.Log("metadataname:" + map.metadata.name);
                    Debug.Log(mapName);
                    string[] buildingNameTemp = map.metadata.name.Split(new char[] { '/' });

                    if (buildingNameTemp.Length > 1)
                    {
                        string buildingName = buildingNameTemp[0];
                        string floorNum = buildingNameTemp[1];
                        string[] currentBuildingArr = mapName.Split(new char[] { '/' });
                        string currentBuilding = currentBuildingArr[0];
                        if (currentBuilding == buildingName && !floors.Contains(floorNum))
                        {
                            floors.Add(floorNum);
                            Debug.Log("floorNum:" + floorNum);
                        }
                    }
                }                
            }
            Debug.Log("exit getFloors()");

            Debug.Log("floors: " + string.Join(", ", floors.ToArray()));
            floors.Sort();
            loadingText.SetActive(false);
            if (floors.Count >= 1)
            {

                floor1.GetComponent<Text>().text = floors[0];
                floorButton1.SetActive(true);
            }
            if (floors.Count >= 2)
            {
                floor2.GetComponent<Text>().text = floors[1];
                floorButton2.SetActive(true);
            }
            if (floors.Count >= 3)
            {
                floor3.GetComponent<Text>().text = floors[2];
                floorButton3.SetActive(true);
            }
            if (floors.Count >= 4)
            {
                floor4.GetComponent<Text>().text = floors[3];
                floorButton4.SetActive(true);
            }
            if (floors.Count >= 5)
            {
                floor5.GetComponent<Text>().text = floors[4];
                floorButton5.SetActive(true);
            }
        });
        
    }
    public void getDestination()
    {
        loadingText.SetActive(true);
        //List<string> buildingsHere = new List<string>();
        LibPlacenote.Instance.ListMaps((LibPlacenote.MapInfo[] obj) => {
            foreach (LibPlacenote.MapInfo map in obj)
            {
                if (map.metadata.name != null && mapName != null)
                {
                    Debug.Log("metadataname:" + map.metadata.name);
                    Debug.Log(mapName);
                    string[] buildingNameTemp = map.metadata.name.Split(new char[] { '/' });

                    if (buildingNameTemp.Length > 2)
                    {
                        string buildingName = buildingNameTemp[0];
                        string floorNum = buildingNameTemp[1];
                        string destination = buildingNameTemp[2];
                        string[] currentBuildingArr = mapName.Split(new char[] { '/' });
                        string currentBuilding = currentBuildingArr[0];
                        string currentFloor = currentBuildingArr[1];
                        if (currentBuilding == buildingName && currentFloor == floorNum && !destinations.Contains(destination) )
                        {
                            destinations.Add(destination);
                            Debug.Log("destination :" + destination);
                        }
                    }
                }
            }
            Debug.Log("exit getDestination()");
            Debug.Log("destinations: " + string.Join(", ", destinations.ToArray()));
            destinations.Sort();
            loadingText.SetActive(false);
            if (destinations.Count >= 1)
            {

                destination1.GetComponent<Text>().text = destinations[0];
                destinationButton1.SetActive(true);
            }
            if (destinations.Count >= 2)
            {
                destination2.GetComponent<Text>().text = destinations[1];
                destinationButton2.SetActive(true);
            }
            if (destinations.Count >= 3)
            {
                destination3.GetComponent<Text>().text = destinations[2];
                destinationButton3.SetActive(true);
            }
            if (destinations.Count >= 4)
            {
                destination4.GetComponent<Text>().text = destinations[3];
                destinationButton4.SetActive(true);
            }
            if (destinations.Count >= 5)
            {
                destination5.GetComponent<Text>().text = destinations[4];
                destinationButton5.SetActive(true);
            }

        });
        
    }


    public void handleBuildingUserInput(int input)
    {
        Debug.Log("IM HERE IN ");
        string current = buildings[input - 1];
        mapName += current + "/";
        Debug.Log("appending buildingName to mapName: " + current);
    }
    public void handleFloorUserInput(int input)
    {
        string current = floors[input - 1];
        mapName += current + "/";
        Debug.Log("appending floorName to mapName: " + current);
    }    

    void FindMap() {
        //get metadata
        LibPlacenote.Instance.SearchMaps(mapName, (LibPlacenote.MapInfo[] obj) => {
            foreach (LibPlacenote.MapInfo map in obj) {
                //change to contains                
                if (map.metadata.name == mapName) {
                    mSelectedMapInfo = map;
                    Debug.Log("FOUND MAP: " + mSelectedMapInfo.placeId);
                    LoadMap();
                    return;
                }
            }
        });
    }

    void LoadMap() {
        ConfigureSession(false);

        LibPlacenote.Instance.LoadMap(mSelectedMapInfo.placeId,
            (completed, faulted, percentage) => {
                if (completed) {
                    Debug.Log("Loaded ID: " + mSelectedMapInfo.placeId + "...Starting session");

                    LibPlacenote.Instance.StartSession();

                } else if (faulted) {
                    Debug.Log("Failed to load ID: " + mSelectedMapInfo.placeId);
                } else {
                    Debug.Log("Map Download: " + percentage.ToString("F2") + "/1.0");
                }
            }
        );
    }

    private void StartARKit() {
        Debug.Log("Initializing ARKit");
        Application.targetFrameRate = 60;
        ConfigureSession(false);
    }

    private void ConfigureSession(bool clearPlanes) {
#if !UNITY_EDITOR
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.None;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig (config);
#endif
    }

    public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

    public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus) {
        Debug.Log("prevStatus: " + prevStatus.ToString() + " currStatus: " + currStatus.ToString());
        if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) {
            //Debug.Log("Localized: " + mSelectedMapInfo.metadata.name);
            isNavigating = true;
            string[] mapNameArray = mapName.Split('/');
            navigationPanelText.GetComponent<Text>().text =
                "Navigating to " + mapNameArray[0] + ", Floor " + mapNameArray[1] + ", " + mapNameArray[2];
            navigationButton.SetActive(true);
            GetComponent<CustomShapeManager>().LoadShapesJSON(mSelectedMapInfo.metadata.userdata);
            FeaturesVisualizer.DisablePointcloud();
        } else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
            Debug.Log("Mapping");
        } else if (currStatus == LibPlacenote.MappingStatus.LOST) {
            if (!isNavigating)
            {
                navigationPanelText.GetComponent<Text>().text = "Locating...";
            }
            //Debug.Log("Searching for position lock");
        } else if (currStatus == LibPlacenote.MappingStatus.WAITING) {
            if (GetComponent<CustomShapeManager>().shapeObjList.Count != 0) {
                //GetComponent<CustomShapeManager>().ClearShapes();
            }
        }
    }

    public void SetMapName(int index) {
        string current = destinations[index-1];
        this.mapName += current;
        Debug.Log("final mapName:" + this.mapName);
        isNavReady = true;
        navigationPanel.SetActive(true);
    }
}
