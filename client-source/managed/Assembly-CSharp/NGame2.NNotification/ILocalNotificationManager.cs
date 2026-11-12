namespace NGame2.NNotification;

public interface ILocalNotificationManager
{
	void RegisterLocalNotification(int id, string title, string body, string ticker, long remainTimeMs, bool everyday);

	void UnRegisterLocalNotification(int id, string title, string body, string ticker, long remainTimeMs, bool everyday);

	void CheckNotification(int id, long maxTime);

	void CancelAllLocalNotification();

	void ClearAllLocalNotification();
}
