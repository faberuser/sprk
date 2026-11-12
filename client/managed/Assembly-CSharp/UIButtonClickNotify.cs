using UnityEngine;

public class UIButtonClickNotify : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnDragOutNoArgListener
{
	public GameObject[] NotifyList;

	public void OnDragOut()
	{
		NotifyPress(pressed: false);
	}

	public void OnPress(bool pressed)
	{
		NotifyPress(pressed);
	}

	private void NotifyPress(bool pressed)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(NotifyList); i++)
		{
			NotifyList[i].GetComponent<UICamera.IOnPressListener>()?.OnPress(pressed);
		}
	}
}
