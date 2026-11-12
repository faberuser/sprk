namespace NGameAsset;

public class ValidAsyncLoad<T> : IAsyncLoad<T>, IAsyncLoadBase where T : class
{
	private T _value;

	string IAsyncLoadBase.KeyDesc => string.Empty;

	bool IAsyncLoadBase.IsDone => true;

	bool IAsyncLoadBase.IsValid => true;

	string IAsyncLoadBase.LoadMessage => string.Empty;

	T IAsyncLoad<T>.Value => _value;

	public ValidAsyncLoad(T value)
	{
		_value = value;
	}
}
