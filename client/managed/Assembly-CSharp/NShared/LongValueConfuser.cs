namespace NShared;

public class LongValueConfuser : IValueConfuser<long>
{
	private readonly int _confusion;

	public LongValueConfuser(int confusion)
	{
		_confusion = confusion;
	}

	long IValueConfuser<long>.Enc(long original)
	{
		return original ^ _confusion;
	}

	long IValueConfuser<long>.Dec(long confused)
	{
		return confused ^ _confusion;
	}
}
