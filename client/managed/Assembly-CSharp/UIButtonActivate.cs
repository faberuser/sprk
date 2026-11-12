using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Activate")]
public class UIButtonActivate : MonoBehaviour, UICamera.IOnClickListener
{
	public GameObject target;

	public bool state = true;

	public void OnClick()
	{
		if (target != null)
		{
			NGUITools.SetActive(target, state);
		}
	}
}
