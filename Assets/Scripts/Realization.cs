using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Realization : ScrollableCell
{
	[SerializeField]
	private Text text;

	public override void ConfigureCellData()
	{
		if (dataObject == null)
			return;
		text.text = ((int)dataObject).ToString();
	}
}
