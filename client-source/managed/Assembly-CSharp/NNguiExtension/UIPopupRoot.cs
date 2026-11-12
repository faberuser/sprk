using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUI.NWindow.NBattle;
using NVespa;
using UnityEngine;

namespace NNguiExtension;

[UIEvent(AllowDuringPatchDownloading = true)]
public class UIPopupRoot : MonoBehaviour
{
	public static UIPopupRoot working;

	public UIWidget RootWidget;

	public bool HideAfterClick = true;

	public float AlphaAnimationTime = 0.3f;

	public bool IsSecondaryMainPopup;

	[HideInInspector]
	public List<EventDelegate> onToggle = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> onShow = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> onShowEnd = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> onHide = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> onHideEnd = new List<EventDelegate>();

	private bool _isShow;

	private bool _isAnimate;

	public bool IsShow => _isShow;

	public bool IsAnimate => _isAnimate;

	public static void OnNativePressedEvent(GameObject eventGo, bool pressed)
	{
		if (working == null || !working.HideAfterClick)
		{
			return;
		}
		if (pressed)
		{
			GameObject gameObject = eventGo;
			while (gameObject != null)
			{
				if (gameObject == working.gameObject)
				{
					return;
				}
				Transform parent = gameObject.transform.parent;
				if (parent == null)
				{
					break;
				}
				gameObject = parent.gameObject;
			}
		}
		working.HidePopup();
	}

	private void OnEnable()
	{
		if (!IsSecondaryMainPopup)
		{
			HidePopup(immediate: true);
		}
	}

	private void OnDisable()
	{
		HidePopup(immediate: true);
	}

	private void Start()
	{
		if (!IsSecondaryMainPopup)
		{
			HidePopup(immediate: true);
		}
	}

	public void TogglePopup()
	{
		AutoNextBattle.StopWithClickButton();
		if (!_isAnimate)
		{
			EventDelegate.Execute(onToggle);
			if (_isShow)
			{
				HidePopup();
			}
			else
			{
				ShowPopup();
			}
		}
	}

	public void ShowPopup()
	{
		if (working != null && working != this)
		{
			working.HidePopup();
		}
		working = this;
		_isShow = true;
		_isAnimate = true;
		if (RootWidget != null)
		{
			RootWidget.gameObject.SetActive(value: true);
			RootWidget.alpha = 0f;
		}
		EventDelegate.Execute(onShow);
	}

	public void HidePopup(bool immediate = false)
	{
		if (working == this)
		{
			working = null;
		}
		_isShow = false;
		EventDelegate.Execute(onHide);
		if (immediate)
		{
			_isAnimate = false;
			RootWidget.gameObject.SetActive(value: false);
			EventDelegate.Execute(onHideEnd);
		}
		else
		{
			_isAnimate = true;
		}
	}

	private void Update()
	{
		if (!_isAnimate || RootWidget == null || !RootWidget.gameObject.activeSelf)
		{
			return;
		}
		float num = (float)UnitySystemTimer.unscaledDeltaTick * 0.001f / Vespa_Float_Extension_Method.NoneZero(AlphaAnimationTime);
		if (!_isShow)
		{
			float num2 = RootWidget.alpha - num;
			if (num2 <= 0f)
			{
				RootWidget.alpha = 0f;
				RootWidget.gameObject.SetActive(value: false);
				_isAnimate = false;
				EventDelegate.Execute(onHideEnd);
			}
			else
			{
				RootWidget.alpha = num2;
			}
		}
		else
		{
			float num3 = RootWidget.alpha + num;
			if (num3 > 1f)
			{
				RootWidget.alpha = 1f;
				_isAnimate = false;
				EventDelegate.Execute(onShowEnd);
			}
			else
			{
				RootWidget.alpha = num3;
			}
		}
	}
}
