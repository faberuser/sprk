using UnityEngine;

public class TweenListPlayer : MonoBehaviour
{
	public UITweener[] TwList;

	public bool ActivateOnEnable;

	public void Awake()
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(TwList); i < num; i++)
		{
			UITweener uITweener = TwList[i];
			if (!(uITweener == null))
			{
				uITweener.enabled = false;
				uITweener.ResetToBeginning();
			}
		}
	}

	public void OnEnable()
	{
		if (ActivateOnEnable)
		{
			PlayTween();
		}
	}

	public void PlayTween()
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(TwList); i < num; i++)
		{
			UITweener uITweener = TwList[i];
			if (!(uITweener == null))
			{
				uITweener.enabled = true;
				uITweener.ResetToBeginning();
				uITweener.PlayForward();
			}
		}
	}
}
