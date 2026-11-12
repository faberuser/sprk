namespace NGame2.NAccount.NNotice;

public interface IChecker
{
	void Set(ICheckerListener listener);

	void Close();

	void AddObserver();

	void RemoveObserver();
}
