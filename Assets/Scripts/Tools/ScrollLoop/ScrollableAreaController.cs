using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScrollableAreaController : MonoBehaviour
{
	public enum AdapationType
	{
		Default,
		ModifyColumns,
		Scale
	}

	public enum ItemAlignment
	{
		Left,
		Center
	}

	public ItemAlignment itemAlignment = ItemAlignment.Left;

	[SerializeField]
	private ScrollableCell cellPrefab;
	[SerializeField]
	private int NUMBER_OF_COLUMNS = 1;
	//表示并排显示几个，比如是上下滑动，当此处为2时表示一排有两个cell
	[SerializeField]
	private RectTransform canvasRec;


	public float cellWidth = 30.0f;
	public float cellHeight = 25.0f;
	public Vector2 cellOffset;
	public Vector2 cellPadding;
	public AdapationType adapationType;

	private RectTransform content;
	private int visibleCellsTotalCount = 0;
	private int visibleCellsRowCount = 0;
	private LinkedList<GameObject> localCellsPool = new LinkedList<GameObject>();
	private LinkedList<GameObject> cellsInUse = new LinkedList<GameObject>();
	private ScrollRect rect;

	private IList allCellsData;
	private int previousInitialIndex = 0;
	private int initialIndex = 0;
	private float initpostion = 0;
	private float adjustSize;
	private Vector3 contentPostion;
	private CanvasScaler canvasScaler;


	private float adaptationScale = 1.0f;

	void Start()
	{
		canvasScaler = canvasRec.gameObject.GetComponent<CanvasScaler>();
		rect = this.GetComponent<ScrollRect>();
		content = rect.content;

		if (horizontal) {
			visibleCellsRowCount = Mathf.CeilToInt(rect.viewport.GetComponent<RectTransform>().rect.width / cellWidth);
			ChangeModel();
		} else {
			visibleCellsRowCount = Mathf.CeilToInt(rect.viewport.GetComponent<RectTransform>().rect.height / cellHeight);
			ChangeModel();
		}

		visibleCellsTotalCount = visibleCellsRowCount + 1;
		visibleCellsTotalCount *= NUMBER_OF_COLUMNS;
		contentPostion = content.localPosition;

		this.CreateCellPool();
	}

	void ChangeModel()
	{
		switch (adapationType) {
			case AdapationType.ModifyColumns:
				ModifyColumnsModel();
				break;
			case AdapationType.Scale:
				ScaleModel();
				break;
		}
	}

	void ModifyColumnsModel()
	{
		int preNumber = NUMBER_OF_COLUMNS;
		if (horizontal) {
			NUMBER_OF_COLUMNS = Mathf.FloorToInt(rect.viewport.GetComponent<RectTransform>().rect.height / cellHeight);
			float deuceScale = rect.viewport.GetComponent<RectTransform>().rect.height / (canvasScaler.referenceResolution.y * NUMBER_OF_COLUMNS / preNumber);
			float preCellHeight = cellHeight;
			cellHeight = rect.viewport.GetComponent<RectTransform>().rect.height / NUMBER_OF_COLUMNS;
			cellOffset.y = (cellHeight - (preCellHeight - cellOffset.y * 2)) / 2;

			cellOffset.y *= deuceScale;
			cellHeight *= deuceScale;
		} else {
			NUMBER_OF_COLUMNS = Mathf.FloorToInt(rect.viewport.GetComponent<RectTransform>().rect.width / cellWidth);
			float preCellWidth = cellWidth;
			cellWidth = rect.viewport.GetComponent<RectTransform>().rect.width / NUMBER_OF_COLUMNS;
			cellOffset.x = (cellWidth - (preCellWidth - cellOffset.x * 2)) / 2;
		}
	}

	void ScaleModel()
	{
		float proportionWidth = Screen.width / canvasScaler.referenceResolution.x;
		float proportionHeight = Screen.height / canvasScaler.referenceResolution.y;
		if (proportionWidth > proportionHeight) {
			adaptationScale = proportionWidth / proportionHeight;
			cellWidth *= adaptationScale;
			cellHeight *= adaptationScale;
		}
	}

	public void Update()
	{
		if (allCellsData == null)
			return;
		previousInitialIndex = initialIndex;
		CalculateCurrentIndex();
		InternalCellsUpdate();
	}

	private void InternalCellsUpdate()
	{
		if (previousInitialIndex != initialIndex) {
			bool scrollingPositive = previousInitialIndex < initialIndex;
			int indexDelta = Mathf.Abs(previousInitialIndex - initialIndex);

			int deltaSign = scrollingPositive ? +1 : -1;

			for (int i = 1; i <= indexDelta; i++)
				this.UpdateContent(previousInitialIndex + i * deltaSign, scrollingPositive);
		}
	}

	private void CalculateCurrentIndex()
	{
		if (!horizontal)
			initialIndex = Mathf.FloorToInt((content.localPosition.y - initpostion) / cellHeight);
		else {
			initialIndex = -(int)((content.localPosition.x - initpostion) / cellWidth);
		}
		int limit = Mathf.CeilToInt((float)allCellsData.Count / (float)NUMBER_OF_COLUMNS) - visibleCellsRowCount;
		if (initialIndex < 0)
			initialIndex = 0;
		if (initialIndex >= limit)
			initialIndex = limit - 1;
	}

	private bool horizontal {
		get { return rect.horizontal; }
	}

	private void FreeCell(bool scrollingPositive)
	{
		LinkedListNode<GameObject> cell = null;
		// Add this GameObject to the end of the list
		if (scrollingPositive) {
			cell = cellsInUse.First;
			cellsInUse.RemoveFirst();
			localCellsPool.AddLast(cell);
		} else {
			cell = cellsInUse.Last;
			cellsInUse.RemoveLast();
			localCellsPool.AddFirst(cell);
		}
	}

	private void UpdateContent(int cellIndex, bool scrollingPositive)
	{
		int index = scrollingPositive ? ((cellIndex - 1) * NUMBER_OF_COLUMNS) + (visibleCellsTotalCount) : (cellIndex * NUMBER_OF_COLUMNS);
		LinkedListNode<GameObject> tempCell = null;

		int currentDataIndex = 0;
		for (int i = 0; i < NUMBER_OF_COLUMNS; i++) {
			this.FreeCell(scrollingPositive);
			tempCell = GetCellFromPool(scrollingPositive);
			currentDataIndex = index + i;

			PositionCell(tempCell.Value, index + i);
			ScrollableCell scrollableCell = tempCell.Value.GetComponent<ScrollableCell>();
			if (currentDataIndex >= 0 && currentDataIndex < allCellsData.Count) {
				scrollableCell.Init(this, allCellsData [currentDataIndex], currentDataIndex);
			} else
				scrollableCell.Init(this, null, currentDataIndex);

			scrollableCell.ConfigureCell();
		}
	}

	void setContentSize()
	{
		int cellOneWayCount = (int)Math.Ceiling((float)allCellsData.Count / NUMBER_OF_COLUMNS);
		if (horizontal) {
			content.sizeDelta = new Vector2(cellOneWayCount * cellWidth, content.sizeDelta.y);
		} else {
			content.sizeDelta = new Vector2(content.sizeDelta.x, cellOneWayCount * cellHeight);
		}

	}

	private void PositionCell(GameObject go, int index)
	{
		int rowMod = index % NUMBER_OF_COLUMNS;
		if (!horizontal) {
			go.transform.localPosition = firstCellPostion
			+ new Vector3(cellWidth * (rowMod), -(index / NUMBER_OF_COLUMNS) * cellHeight);
		} else {
			go.transform.localPosition = firstCellPostion
			+ new Vector3((index / NUMBER_OF_COLUMNS) * cellWidth, -cellHeight * (rowMod));
		}
	}


	private Vector3 firstCellPostion;

	private Vector3 FirstCellPosition {
		get {
			return (itemAlignment == ItemAlignment.Left ? 
				new Vector3(-rect.viewport.GetComponent<RectTransform>().rect.width / 2 + cellWidth / 2 + cellOffset.x, -cellOffset.y) : 
				new Vector3(-cellWidth * NUMBER_OF_COLUMNS / 2 + cellWidth / 2, -cellOffset.y)) * adaptationScale;
		}
	}

	private void CreateCellPool()
	{
		GameObject tempCell = null;
		for (int i = 0; i < visibleCellsTotalCount; i++) {
			tempCell = this.InstantiateCell(i);
			localCellsPool.AddLast(tempCell);
		}
		content.gameObject.SetActive(false);
	}

	private GameObject InstantiateCell(int index)
	{
		Debug.Log("===== " + adaptationScale);
		GameObject cellTempObject = Instantiate(cellPrefab.gameObject) as GameObject;
		cellTempObject.layer = this.gameObject.layer;
		cellTempObject.name = "Cell" + index;
		cellTempObject.transform.SetParent(content.transform, false);
		cellTempObject.transform.localScale = cellPrefab.transform.localScale * adaptationScale;
		cellTempObject.transform.localPosition = cellPrefab.transform.localPosition;
		cellTempObject.transform.localRotation = cellPrefab.transform.localRotation;
		cellTempObject.SetActive(false);
		return cellTempObject;
	}

	private LinkedListNode<GameObject> GetCellFromPool(bool scrollingPositive)
	{
		if (localCellsPool.Count == 0)
			return null;

		LinkedListNode<GameObject> cell = localCellsPool.First;
		localCellsPool.RemoveFirst();

		if (scrollingPositive)
			cellsInUse.AddLast(cell);
		else
			cellsInUse.AddFirst(cell);
		return cell;
	}

	public void InitializeWithData(IList cellDataList)
	{
		if (cellsInUse.Count > 0) {
			foreach (var cell in cellsInUse) {
				localCellsPool.AddLast(cell);
			}
			cellsInUse.Clear();
		} else {
			if (horizontal) {
				initpostion = content.localPosition.x;
			} else {
				initpostion = content.localPosition.y;
			}
		}

		previousInitialIndex = 0;
		initialIndex = 0;
		content.gameObject.SetActive(true);
		LinkedListNode<GameObject> tempCell = null;
		allCellsData = cellDataList;

		setContentSize();
		firstCellPostion = FirstCellPosition;
        

		int currentDataIndex = 0;
		for (int i = 0; i < visibleCellsTotalCount; i++) {
			tempCell = GetCellFromPool(true);
			if (tempCell == null || tempCell.Value == null)
				continue;
			currentDataIndex = i + initialIndex * NUMBER_OF_COLUMNS;

			PositionCell(tempCell.Value, currentDataIndex);
			tempCell.Value.SetActive(true);
			ScrollableCell scrollableCell = tempCell.Value.GetComponent<ScrollableCell>();
			if (currentDataIndex < cellDataList.Count)
				scrollableCell.Init(this, cellDataList [i], currentDataIndex);
			else
				scrollableCell.Init(this, null, currentDataIndex);
			scrollableCell.ConfigureCell();
		}
	}

	public void setContentLocalPostion(Vector3 postion)
	{
		content.localPosition = postion;
	}

	public Vector3 getContentLocalPostion()
	{
		return content.localPosition;
	}
}
