using UnityEngine;
using System.Collections;

public class ScrollableCell : MonoBehaviour
{
	protected ScrollableAreaController controller = null;
	protected System.Object dataObject = null;
	private int dataIndex;
	protected float cellHeight;
	protected float cellWidth;
	protected bool deactivateIfNull = true;
	protected ScrollableCell parentCell;

	public System.Object DataObject {
		get { return dataObject; }
		set {
			dataObject = value;
			ConfigureCellData();
		}
	}

	public int DataIndex {
		get { return dataIndex; }
	}

	public virtual void Init(ScrollableAreaController controller, System.Object data, int index,
	                         float cellHeight = 0.0f, float cellWidth = 0.0f, ScrollableCell parentCell = null)
	{
		this.controller = controller;
		this.dataObject = data;
		this.dataIndex = index;

		this.cellHeight = cellHeight;
		this.cellWidth = cellWidth;
		this.parentCell = parentCell;

		if (deactivateIfNull) {
			if (data == null)
				this.gameObject.SetActive(false);
			else
				this.gameObject.SetActive(true);
		}
	}


	public void ConfigureCell()
	{
		this.ConfigureCellData();
	}

	public virtual void ConfigureCellData()
	{ 
	}
}
