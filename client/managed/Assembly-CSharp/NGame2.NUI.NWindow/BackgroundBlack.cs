using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BackgroundBlack : MonoBehaviour, IWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/BackgroundBlack";

	public UIWidget RootWidget;

	public UIPanel Panel;

	private HashSet<IBackgroundBlackRequester> _requester = new HashSet<IBackgroundBlackRequester>();

	private WindowAnimation _animation;

	EDepthOrder IWindow.DepthOrder => EDepthOrder.Normal;

	public void Open(IBackgroundBlackRequester requester)
	{
		if (requester != null && !_requester.Contains(requester))
		{
			_requester.Add(requester);
			UpdateDepth();
			Show();
		}
	}

	void IWindow.ReserveCache(UiPreloader preloader)
	{
	}

	void IWindow.SetBasePanelDepth(int depth)
	{
	}

	public bool IsOpen()
	{
		return !__Extension_Method_ICollection__.IsNullOrEmpty(_requester);
	}

	public void Close(IBackgroundBlackRequester requester)
	{
		if (_requester.Remove(requester))
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_requester))
			{
				Hide();
			}
			else
			{
				UpdateDepth();
			}
		}
	}

	void IWindow.Close()
	{
	}

	public void UpdateDepth()
	{
		int num = int.MaxValue;
		foreach (IBackgroundBlackRequester item in _requester)
		{
			num = Mathf.Min(item.BaseDepth, num);
		}
		if ((bool)Panel)
		{
			Panel.depth = num - 1;
		}
	}

	private void Show()
	{
		UIHelper.EnableGameObject(RootWidget, enable: true);
		if ((bool)_animation)
		{
			_animation.PlayAnimation(reverse: false);
		}
	}

	private void Hide()
	{
		UIHelper.EnableGameObject(RootWidget, enable: false);
		if ((bool)_animation)
		{
			_animation.PlayAnimation(reverse: true);
		}
	}

	private void Awake()
	{
		_animation = GetComponent<WindowAnimation>();
		if ((bool)_animation)
		{
			_animation.SetRoot(RootWidget.transform);
		}
		Hide();
	}
}
