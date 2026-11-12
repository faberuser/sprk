using UnityEngine;

namespace NGame2.NUI;

public class OpenAnimationController : MonoBehaviour
{
	[SerializeField]
	private float DelayTime;

	[SerializeField]
	private float EachDelayTime;

	[SerializeField]
	private float PlayListCount;

	public void Ready(bool isForward = true)
	{
		OpenAnimationClass[] componentsInChildren = GetComponentsInChildren<OpenAnimationClass>();
		int num = 0;
		OpenAnimationClass[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Ready(isForward);
			if (PlayListCount > 0f)
			{
				num++;
				if (PlayListCount <= (float)num)
				{
					break;
				}
			}
		}
	}

	public void Play(float addedDelayTime = 0f, bool isForward = true)
	{
		float num = 0f;
		OpenAnimationClass[] componentsInChildren = GetComponentsInChildren<OpenAnimationClass>();
		int num2 = 0;
		OpenAnimationClass[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play(DelayTime + addedDelayTime + num, isForward);
			num += EachDelayTime;
			if (PlayListCount > 0f)
			{
				num2++;
				if (PlayListCount <= (float)num2)
				{
					break;
				}
			}
		}
	}

	public void Finish()
	{
		OpenAnimationClass[] componentsInChildren = GetComponentsInChildren<OpenAnimationClass>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Finish();
		}
	}
}
