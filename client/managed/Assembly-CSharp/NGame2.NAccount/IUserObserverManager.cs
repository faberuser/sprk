namespace NGame2.NAccount;

public interface IUserObserverManager
{
	void AddObserver(IUserObserver observer);

	void RemoveObserver(IUserObserver observer);

	void ClearObserver();
}
