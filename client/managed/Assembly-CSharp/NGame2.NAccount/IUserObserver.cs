namespace NGame2.NAccount;

public interface IUserObserver
{
	bool IsGlobal();
}
public interface IUserObserver<T> : IUserObserver
{
	void OnChanged(T changedInfo);
}
