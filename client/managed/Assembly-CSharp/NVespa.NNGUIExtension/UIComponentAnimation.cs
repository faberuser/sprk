using NGame2.NUI.NWindow;
using NVespa.NNGUIExtension.NAnimation;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIComponentAnimation : MonoBehaviour, IInspectorGuiDrawer
{
	public BaseWindow Window;

	[HideInInspector]
	public UIComponetAnimationPlayData PlayData;

	public UIAnimationPlayer Player;

	public void PlayForward()
	{
		if (!(Player == null) && !(Window == null))
		{
			PlayData.IsReverse = false;
			Player.Setup(Window, PlayData);
		}
	}

	public void PlayBackWard()
	{
		if (!(Player == null) && !(Window == null))
		{
			PlayData.IsReverse = true;
			Player.Setup(Window, PlayData);
		}
	}

	private void Awake()
	{
		SingletonAuto<LinkedTransform<UIComponentAnimation>>.instance.OnAwake(this);
	}

	private void OnDestroy()
	{
		SingletonAuto<LinkedTransform<UIComponentAnimation>>.instance.OnDestroy(this);
	}
}
