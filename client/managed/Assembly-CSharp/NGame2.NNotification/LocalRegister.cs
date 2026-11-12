using NVespa;
using UnityEngine;

namespace NGame2.NNotification;

public class LocalRegister : SingletonAuto<LocalRegister>
{
	public const int StaminaNotificationId = 1001;

	private ILocalNotificationManager _localNotificationManager;

	public LocalRegister()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			_ = Application.platform;
			_ = 8;
		}
		_localNotificationManager = _localNotificationManager ?? new DummyManager();
	}

	private void RegisterTest()
	{
		for (int i = 0; i < 10; i++)
		{
			int num = i + 1;
			int num2 = 60000 * num;
			Register(1000000 + i, "noti test", "remained min: " + num, "ticker " + i, num2, everyday: false);
		}
	}

	public void RegisterSchedule()
	{
	}

	public void Register(int id, string title, string body, string ticker, long remainTimeMs, bool everyday)
	{
		Debug.Log("[Notification] LocalRegister.Register. id=" + id + " title=" + title + " body=" + body + " ticker=" + ticker + " remainTimeMs=" + remainTimeMs + " everyday=" + everyday);
		_localNotificationManager.RegisterLocalNotification(id, title, body, ticker, remainTimeMs, everyday);
	}

	public void UnRegister(int id, string title, string body, string ticker, long remainTimeMs, bool everyday)
	{
		Debug.Log("[Notification] LocalRegister.UnRegister. id=" + id + " title=" + title + " body=" + body + " ticker=" + ticker + " remainTimeMs=" + remainTimeMs + " everyday=" + everyday);
		_localNotificationManager.UnRegisterLocalNotification(id, title, body, ticker, remainTimeMs, everyday);
	}

	public void CheckNotification(int id, long maxTime)
	{
		Debug.Log("[Notification] LocalRegister.CheckNotification. index=" + id);
		_localNotificationManager.CheckNotification(id, maxTime);
	}

	public void Cancel()
	{
		Debug.Log("[Notification] LocalRegister.Cancel");
		_localNotificationManager.CancelAllLocalNotification();
	}

	public void Clear()
	{
		Debug.Log("[Notification] LocalRegister.Clear");
		_localNotificationManager.ClearAllLocalNotification();
	}
}
