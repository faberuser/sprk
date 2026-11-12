public interface ILineReader
{
	bool canRead { get; }

	string ReadLine();

	string ReadLine(bool skipEmptyLines);
}
