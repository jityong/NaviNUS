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

    // Use this for initialization
    void Start() {
        Input.location.Start();

        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        StartARKit();
        FeaturesVisualizer.EnablePointcloud();
        LibPlacenote.Instance.RegisterListener(this);
        //getBuildings();
        //Debug.Log("After get building in Start()");

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

    public void getBuildings()
    {        
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
                    }
                }                
            }
            Debug.Log("Exit getBuildings()");
        });
        Debug.Log("buildings: " + string.Join(", ", buildings.ToArray()));
    }

    public void getFloors()
    {
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
        });
        Debug.Log("floors: " + string.Join(", ", floors.ToArray()));
    }
    public void getDestination()
    {
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

        });
        Debug.Log("destinations: " + string.Join(", ", destinations.ToArray()));
    }


    public void handleUserInput(String input)
    {
        mapName += input + "/";
        Debug.Log("userinput: " + mapName);
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

    public void SetMapName(String mapName) {
        this.mapName += mapName;
        isNavReady = true;
        navigationPanel.SetActive(true);
    }
}
