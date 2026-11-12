using System.Collections;

namespace NGame2.NNotification;

public interface IRemoteNotificationReceiver
{
	void OnNotificationReceived(IDictionary value);

	void OnRegistration(string registerId, string error);

	void OnUnregistration(string error);
}
