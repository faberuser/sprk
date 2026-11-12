namespace NShared;

public class DelegateReadWriter<K> : IReadWriter<K>, IReadWriter
{
	public delegate K ReaderDelegate(string value);

	public delegate bool ReadTrierDelegate(string value, out K key);

	public delegate string WriterDelegate(K key);

	private readonly ReadTrierDelegate _readTrier;

	private readonly ReaderDelegate _reader;

	private readonly WriterDelegate _writer;

	public DelegateReadWriter(ReaderDelegate reader, WriterDelegate writer)
	{
		_reader = reader;
		_writer = writer;
	}

	public DelegateReadWriter(ReadTrierDelegate readTrier, WriterDelegate writer)
	{
		_readTrier = readTrier;
		_writer = writer;
	}

	string IReadWriter<K>.ToString(K key)
	{
		if (_writer != null)
		{
			return _writer(key);
		}
		return key.ToString();
	}

	K IReadWriter<K>.FromString(string value)
	{
		if (_readTrier != null)
		{
			_readTrier(value, out var key);
			return key;
		}
		if (_reader != null)
		{
			return _reader(value);
		}
		return default(K);
	}
}
