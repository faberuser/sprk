namespace NGame2.NNotification;

public interface IRemoteNotificationManager
{
	void Register(IRemoteNotificationReceiver receiver);

	void Unregister();

	void Clear();

	void ChainUpdate();
}
