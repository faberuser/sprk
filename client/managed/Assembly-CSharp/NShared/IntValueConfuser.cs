namespace NShared;

public class IntValueConfuser : IValueConfuser<int>
{
	private readonly int _confusion;

	public IntValueConfuser(int confusion)
	{
		_confusion = confusion;
	}

	int IValueConfuser<int>.Enc(int original)
	{
		return original ^ _confusion;
	}

	int IValueConfuser<int>.Dec(int confused)
	{
		return confused ^ _confusion;
	}
}
