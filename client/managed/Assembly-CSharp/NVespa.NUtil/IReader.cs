namespace NVespa.NUtil;

internal interface IReader
{
	bool ReadBool();

	byte ReadByte(byte defaultValue = 0);

	short ReadShort();

	int ReadInt();

	long ReadLong();

	double ReadDouble();

	string ReadString();

	bool CanRead(int byteCount);
}
