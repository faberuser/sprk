namespace NGameAsset;

public class InvalidAsyncLoad<T> : IAsyncLoad<T>, IAsyncLoadBase where T : class
{
	private string _keyDesc;

	private string _loadMessage;

	string IAsyncLoadBase.KeyDesc => _keyDesc;

	bool IAsyncLoadBase.IsDone => true;

	bool IAsyncLoadBase.IsValid => false;

	string IAsyncLoadBase.LoadMessage => _loadMessage;

	T IAsyncLoad<T>.Value => null;

	public InvalidAsyncLoad(string keyDesc, string loadMessage)
	{
		_keyDesc = keyDesc;
		_loadMessage = loadMessage;
	}
}
