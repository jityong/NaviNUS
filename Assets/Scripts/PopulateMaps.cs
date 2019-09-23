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

public class PopulateMaps : MonoBehaviour, PlacenoteListener {


    //change map name to "userinput" (hardcode first) 
    private string MAP_NAME = "";
	private List<string> buildings = new List<string>();
	private List<string> floors = new List<string>();
    private UnityARSessionNativeInterface mSession;
    private bool mARInit = false;

    private LibPlacenote.MapMetadataSettable mCurrMapDetails;

    string currMapID = String.Empty;

    private LibPlacenote.MapInfo mSelectedMapInfo;
    private string mSelectedMapId {
        get {
            return mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
        }
    }

   public void getBuildings()
	{
		//List<string> buildingsHere = new List<string>();
		LibPlacenote.Instance.ListMaps((LibPlacenote.MapInfo[] obj) => {
			foreach (LibPlacenote.MapInfo map in obj)
			{
				if (map.metadata.name != null)
				{
					Debug.Log(map.metadata.name);
					string[] buildingNameTemp = map.metadata.name.Split(new char[] { '/' });
					string buildingName = buildingNameTemp[0];
					//string buildingName = MAP_NAME.Substring(0, MAP_NAME.Length - 1);
					//Debug.Log(buildingName + " line 45");
					if (!buildings.Contains(buildingName))
					{
						buildings.Add(buildingName);
						Debug.Log("buildingName:" + buildingName);
					}
				}
				Debug.Log("test");
			}
		});
		Debug.Log("buildings: " + string.Join(", ", buildings.ToArray()));
	}

	public void getFloors()
	{
		//List<string> buildingsHere = new List<string>();
		LibPlacenote.Instance.ListMaps((LibPlacenote.MapInfo[] obj) => {
			foreach (LibPlacenote.MapInfo map in obj)
			{
				if (map.metadata.name != null && MAP_NAME != null)
				{
					Debug.Log(map.metadata.name);
                    Debug.Log(MAP_NAME);
					string[] buildingNameTemp = map.metadata.name.Split(new char[] { '/' });
					string buildingName = buildingNameTemp[0];
					string floorNum = buildingNameTemp[1];
					string[] currentBuildingArr = MAP_NAME.Split(new char[] { '/' });
					string currentBuilding = currentBuildingArr[0];
					//string buildingName = MAP_NAME.Substring(0, MAP_NAME.Length - 1);
					//Debug.Log(buildingName + " line 45");
					if (currentBuilding == buildingName && !floors.Contains(floorNum))
					{
						floors.Add(floorNum);
						Debug.Log("floorNum:" + floorNum);
					}
				}
				Debug.Log("test");
			}
		});
		Debug.Log("floors: " + string.Join(", ", floors.ToArray()));
	}


	public void handleUserInput(String input)
    {
        MAP_NAME += input + "/";
        Debug.Log("userinput: " + MAP_NAME);
		getFloors();
		Debug.Log("after findmap: 36");
    }
    
    //// Use this for initialization
    //void Start() {
    //    Input.location.Start();

    //    //mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
    //    FeaturesVisualizer.EnablePointcloud();
    //    LibPlacenote.Instance.RegisterListener(this);
    //}

    void OnDisable() {
    }

    // Update is called once per frame


    
    void FindMap() {
        //get metadata
        LibPlacenote.Instance.SearchMaps(MAP_NAME, (LibPlacenote.MapInfo[] obj) => {
			Debug.Log("findmap current obj: " + obj);
            foreach (LibPlacenote.MapInfo map in obj) {
				Debug.Log("findmap current map: " + map);
				//change to contains
				if (map.metadata.name == MAP_NAME) {
                    Debug.Log("IN findmap method:" + MAP_NAME);
                    mSelectedMapInfo = map;
                    Debug.Log("FOUND MAP: " + mSelectedMapInfo.placeId);
                    LoadMap();
                    return;
                }
				Debug.Log("cannot find in findmap:" + MAP_NAME);
            }
        });
    }

	void LoadMap()
	{
		ConfigureSession(false);

		LibPlacenote.Instance.LoadMap(mSelectedMapInfo.placeId,
			(completed, faulted, percentage) =>
			{
				if (completed)
				{
					Debug.Log("Loaded ID: " + mSelectedMapInfo.placeId + "...Starting session");

					LibPlacenote.Instance.StartSession();

				}
				else if (faulted)
				{
					Debug.Log("Failed to load ID: " + mSelectedMapInfo.placeId);
				}
				else
				{
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
            Debug.Log("Localized: " + mSelectedMapInfo.metadata.name);
            GetComponent<CustomShapeManager>().LoadShapesJSON(mSelectedMapInfo.metadata.userdata);
            FeaturesVisualizer.DisablePointcloud();
        } else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
            Debug.Log("Mapping");
        } else if (currStatus == LibPlacenote.MappingStatus.LOST) {
            Debug.Log("Searching for position lock");
        } else if (currStatus == LibPlacenote.MappingStatus.WAITING) {
            if (GetComponent<CustomShapeManager>().shapeObjList.Count != 0) {
				//GetComponent<CustomShapeManager>().ClearShapes();
			}
        }
    }
}
