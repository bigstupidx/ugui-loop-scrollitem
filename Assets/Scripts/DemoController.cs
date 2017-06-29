using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//增加删除数据重新调用InitializeWithData方法
//ScrollableAreaController有些方法需要等待Awake完成，所以不要在Awake调用ScrollableAreaController中的方法
public class DemoController : MonoBehaviour
{

	public ScrollableAreaController scrollController;
	List<int> list = new List<int>();
	// Use this for initialization
	void Start()
	{
        
		for (int i = 0; i < 20; i++) {
			list.Add(i);
		}

		scrollController.InitializeWithData(list);
	}


	void Update()
	{
		if (Input.GetKeyUp(KeyCode.X)) {
			for (int i = 20; i < 40; i++) {
				list.Add(i);
			}
			scrollController.InitializeWithData(list);
		}
	}

	Vector3 v3;

	public void OnClick()
	{
		for (int i = 1; i < 2; i++) {
			list.Remove(i);
		}
		scrollController.InitializeWithData(list);
		v3 = scrollController.getContentLocalPostion();
	}

	public void OnClickPostion()
	{
		scrollController.setContentLocalPostion(v3);
	}
}
