namespace NGame2.NUI.NWorldMap.NMapLayout;

public interface IObserverManager
{
	void AddObserver(IObserver observer);

	void RemoveObserver(IObserver observer);

	void ClearObserver();
}
