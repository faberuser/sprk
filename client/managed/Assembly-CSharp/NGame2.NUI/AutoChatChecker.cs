using System;
using NGame2.NChatting;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

public class AutoChatChecker : MonoBehaviour
{
	public float UpdateTime;

	public Transform Widget_Alert;

	private DateTime _lastAlertCheckTime = DateTime.UtcNow;

	private void OnEnable()
	{
		UIHelper.EnableGameObject(Widget_Alert, enable: false);
	}

	private void Update()
	{
		UpdateChat();
	}

	private void UpdateChat()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!((utcNow - _lastAlertCheckTime).TotalSeconds < 0.5))
		{
			_lastAlertCheckTime = utcNow;
			SetChatCount(GlobalAutoBehaviour<ChattingManager>.instance.GetTotalAlertCount(refresh: true));
		}
	}

	private void SetChatCount(int count)
	{
		bool enable = count > 0;
		UIHelper.EnableGameObject(Widget_Alert, enable);
	}
}
