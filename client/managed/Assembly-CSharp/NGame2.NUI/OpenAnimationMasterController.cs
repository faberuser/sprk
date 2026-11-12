using UnityEngine;

namespace NGame2.NUI;

public class OpenAnimationMasterController : MonoBehaviour
{
	public float DelayTime;

	public void Ready(bool isForward = true)
	{
		OpenAnimationController[] componentsInChildren = GetComponentsInChildren<OpenAnimationController>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Ready(isForward);
		}
	}

	public void Play(bool isForward = true)
	{
		OpenAnimationController[] componentsInChildren = GetComponentsInChildren<OpenAnimationController>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play(DelayTime, isForward);
		}
	}

	public void Finish()
	{
		OpenAnimationController[] componentsInChildren = GetComponentsInChildren<OpenAnimationController>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Finish();
		}
	}
}
