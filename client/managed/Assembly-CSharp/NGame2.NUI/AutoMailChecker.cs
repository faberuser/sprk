using System;
using NGame2.NUI.NManager;
using UnityEngine;

namespace NGame2.NUI;

public class AutoMailChecker : MonoBehaviour
{
	public float UpdateTime;

	public Transform Widget_Count;

	public UILabel Label_MailCount;

	private DateTime _lastAlertCheckTime = DateTime.UtcNow;

	private void OnEnable()
	{
		UIHelper.EnableGameObject(Widget_Count, enable: false);
	}

	private void Update()
	{
		UpdateMail();
	}

	private void UpdateMail()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!((utcNow - _lastAlertCheckTime).TotalSeconds < 0.5))
		{
			_lastAlertCheckTime = utcNow;
			SetMailCount(MailManagement.MailCount);
		}
	}

	private void SetMailCount(int count)
	{
		bool enable = count > 0;
		UIHelper.EnableGameObject(Widget_Count, enable);
		UIHelper.SetLabelText(Label_MailCount, count);
	}
}
