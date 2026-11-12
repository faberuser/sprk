public sealed class SprkIllustrationReuse
{
	private readonly object sync = new object();

	private string key;

	private byte[] bytes;

	public byte[] Get(string identity)
	{
		lock (sync)
		{
			return (identity == key) ? bytes : null;
		}
	}

	public void Set(string identity, byte[] value)
	{
		lock (sync)
		{
			key = identity;
			bytes = value;
		}
	}

	public void Clear()
	{
		lock (sync)
		{
			key = null;
			bytes = null;
		}
	}
}
