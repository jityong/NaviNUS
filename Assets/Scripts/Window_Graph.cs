//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//public class Window_Graph : MonoBehaviour
//{
//    [SerializeField] private Sprite circleSprite;
//    private RectTransform graphContainer;

//    List<Vector2> valueList = new List<Vector2>() { new Vector2((float)-1.88385177, -2),
//            new Vector2(-2,-5),
//            new Vector2(3,2) };

//    // Start is called before the first frame update
//    void Start()
//    {
//        graphContainer = GetComponent<RectTransform>();
//        showGraph(valueList);
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    void Awake()
//    {
//    }

//    private GameObject CreateCircle(Vector2 anchoredPosition)
//    {
//        GameObject gameObject = new GameObject("circle", typeof(Image));
//        gameObject.transform.SetParent(graphContainer, false);
//        gameObject.GetComponent<Image>().sprite = circleSprite;
//        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
//        rectTransform.anchoredPosition = anchoredPosition;
//        rectTransform.sizeDelta = new Vector2(11, 11);
//        rectTransform.anchorMin = new Vector2(0, 0);
//        rectTransform.anchorMax = new Vector2(0, 0);
//        return gameObject;
//    }

//    private void showGraph(List<Vector2> valueList)
//    {
//        float graphHeight = graphContainer.sizeDelta.y;
//        float graphWidth = graphContainer.sizeDelta.x;
//        float[] yArr = new float[valueList.Count];
//        float[] xArr = new float[valueList.Count];
//        for (int i = 0; i < valueList.Count; i++)
//        {
//            yArr[i] = valueList[i].y; // TODO how to extract y value from the Node 
//            xArr[i] = valueList[i].x; // TODO how to extract x vaue from the Node
//        }
//        float yMaximum = yArr.Max();
//        float yMinimum = yArr.Min();
//        float xMaximum = xArr.Max();
//        float xMinimum = xArr.Min();

//        GameObject lastCircleGameObject = null;
//        for (int i = 0; i < valueList.Count; i++)
//        {
//            float xPosition = (xArr[i] - xMinimum) / (xMaximum - xMinimum) * graphWidth;
//            float yPosition = (yArr[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
//            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
//            if (lastCircleGameObject != null)
//            {
//                joinLinesForPath(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
//                                             circleGameObject.GetComponent<RectTransform>().anchoredPosition);
//            }
//            lastCircleGameObject = circleGameObject;
//        }
//    }

//    private void joinLinesForPath(Vector2 dotPositionA, Vector2 dotPositionB)
//    {
//        GameObject gameObject = new GameObject("Lines", typeof(Image));
//        gameObject.transform.SetParent(graphContainer, false);
//        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
//        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
//        Vector2 dir = (dotPositionB - dotPositionA).normalized;
//        float distance = Vector2.Distance(dotPositionA, dotPositionB);
//        rectTransform.anchorMin = new Vector2(0, 0);
//        rectTransform.anchorMax = new Vector2(0, 0);
//        rectTransform.sizeDelta = new Vector2(distance, 3f); //?
//        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
//        rectTransform.localEulerAngles = new Vector3(0, 0,
//                    (float)Math.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

//    }
//}
