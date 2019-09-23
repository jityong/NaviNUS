using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class NavController : MonoBehaviour {
    
    public AStar AStar;
    private Transform destination;
    private bool _initialized = false;
    private bool _initializedComplete = false;
    private List<Node> path = new List<Node>();
    private int currNodeIndex = 0;
    private float maxDistance = 1.1f;

    [SerializeField] private Sprite circleSprite; //added
    public Sprite targetSprite; //added
    public GameObject graphPanel;
    public GameObject graphContainerPanel; //added
    private RectTransform graphContainer; //added

    private void Start() {
#if UNITY_EDITOR
        InitializeNavigation();
#endif
    }

    /// <summary>
    /// Returns the closest node to the given position.
    /// </summary>
    /// <returns>The closest node.</returns>
    /// <param name="point">Point.</param>
    Node ReturnClosestNode(Node[] nodes, Vector3 point) {
        float minDist = Mathf.Infinity;
        Node closestNode = null;
        foreach (Node node in nodes) {
            float dist = Vector3.Distance(node.pos, point);
            if (dist < minDist) {
                closestNode = node;
                minDist = dist;
            }
        }
        return closestNode;
    }

    public void InitializeNavigation() {
        StopAllCoroutines();
        StartCoroutine(DelayNavigation());
    }

    IEnumerator DelayNavigation() {
        while(FindObjectOfType<DiamondBehavior>() == null){
            yield return new WaitForSeconds(.5f);
            //Debug.Log("waiting for shapes to load...");
        }
        InitNav();
    }

    void InitNav(){
        if (!_initialized) {
            _initialized = true;
            Debug.Log("INITIALIZING NAVIGATION!!!");
            Node[] allNodes = FindObjectsOfType<Node>();
            Debug.Log("NODES: " + allNodes.Length);
            Node closestNode = ReturnClosestNode(allNodes, transform.position);
            Debug.Log("closest: " + closestNode.gameObject.name);
            Node target = FindObjectOfType<DiamondBehavior>().GetComponent<Node>();
            Debug.Log("target: " + target.gameObject.name);
            //set neighbor nodes for all nodes
            foreach (Node node in allNodes) {
                node.FindNeighbors(maxDistance);
            }

            //get path from A* algorithm
            path = AStar.FindPath(closestNode, target, allNodes);

            if (path == null) {
                //increase search distance for neighbors
                maxDistance += .1f;
                Debug.Log("Increasing search distance: " + maxDistance);
                _initialized = false;
                InitNav();
                return;
            }

            //set next nodes 
            for (int i = 0; i < path.Count - 1; i++) {
                path[i].NextInList = path[i + 1];
            }
            //activate first node
            path[0].Activate(true);
            _initializedComplete = true;

            // setPanel to be active
            Debug.Log("Setting to be true");
            //graphPanel.SetActive(true);

            // Populate windowGraph
            Debug.Log("binding component...");
            graphContainer = graphContainerPanel.GetComponent<RectTransform>();
            Debug.Log("binding done");
            Debug.Log("calling showGraph");
            showGraphWithTargetAndAllNodes(path, target);
            Debug.Log("showGraph done");

        }
    }

    // Takes in the path and the Dest Node. Creates Sprite for them and draws path.
    private void showGraphWithTargetAndAllNodes(List<Node> path, Node target)
    {
        Debug.Log("Starting showGraph fn...");
        int totalNumberOfNodes = path.Count + 1; //because of target ndoe
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        float[] yArr = new float[totalNumberOfNodes]; //for tgt Node
        float[] xArr = new float[totalNumberOfNodes]; //for tgt Node

        for (int i = 0; i < totalNumberOfNodes; i++) //-1 becasue target is already coded in.
        {
            if (i == (totalNumberOfNodes - 1))
            {
                yArr[i] = (float)target.pos.z;
                xArr[i] = (float)target.pos.x;
            }
            else
            {
                yArr[i] = (float)path[i].pos.z;
                xArr[i] = (float)path[i].pos.x;
            }
        }
        float yMaximum = yArr.Max();
        float yMinimum = yArr.Min();
        float xMaximum = xArr.Max();
        float xMinimum = xArr.Min();

        GameObject lastGameObject = null;
        for (int i = 0; i < totalNumberOfNodes; i++)
        {
            float xPosition = (xArr[i] - xMinimum) / (xMaximum - xMinimum) * graphWidth;
            float yPosition = (yArr[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
            if (i == (totalNumberOfNodes - 1)) // for the last, Dest Node
            {
                GameObject targetGameObject = CreateTarget(new Vector2(xPosition, yPosition));
                Debug.Log("Created targetGameObject");
                Debug.Log("xPosition is " + xPosition + " yPosition is" + yPosition);
                joinLinesForPath(lastGameObject.GetComponent<RectTransform>().anchoredPosition,
                                             targetGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            else
            {
                GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
                Debug.Log("Created circleGameObject");
                Debug.Log("xPosition is " + xPosition + " yPosition is" + yPosition);
                if (lastGameObject != null)
                {
                    joinLinesForPath(lastGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                 circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                }
                lastGameObject = circleGameObject;
            }
        }

    }

    //Creates and returns the Target Sprite with the relative position calculated
    private GameObject CreateTarget(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("target", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = targetSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    // Creates and returns the Circle Sprite with relative position calculated
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    // Calculates and draws the correct Line angle to connect Nodes.
    private void joinLinesForPath(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("Lines", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance , 3f); //?
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, 
                    (float) Math.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

    }

    private void OnTriggerEnter(Collider other) {
        if (_initializedComplete && other.CompareTag("waypoint")) {
            currNodeIndex = path.IndexOf(other.GetComponent<Node>());
            if (currNodeIndex < path.Count - 1) {
                path[currNodeIndex + 1].Activate(true);
            }
        }
    }

    public void toggleMap() {
        if (graphPanel.activeSelf)
        {
            graphPanel.SetActive(false);
        }
        else
        {
            graphPanel.SetActive(true);
        }
    }
    
}
