namespace NGame2.NNotification;

public class DummyManager : IRemoteNotificationManager, ILocalNotificationManager
{
	private IRemoteNotificationReceiver _receiver;

	void IRemoteNotificationManager.Register(IRemoteNotificationReceiver receiver)
	{
		_receiver = receiver;
		_receiver.OnRegistration(null, null);
	}

	void IRemoteNotificationManager.Unregister()
	{
		_receiver.OnUnregistration(null);
		_receiver = null;
	}

	void IRemoteNotificationManager.Clear()
	{
	}

	void IRemoteNotificationManager.ChainUpdate()
	{
	}

	void ILocalNotificationManager.RegisterLocalNotification(int id, string title, string body, string ticker, long remainTimeMs, bool everyday)
	{
	}

	void ILocalNotificationManager.UnRegisterLocalNotification(int id, string title, string body, string ticker, long remainTimeMs, bool everyday)
	{
	}

	void ILocalNotificationManager.CheckNotification(int id, long maxTime)
	{
	}

	void ILocalNotificationManager.CancelAllLocalNotification()
	{
	}

	void ILocalNotificationManager.ClearAllLocalNotification()
	{
	}
}
