namespace JsonBin;

public enum JsonType : byte
{
	None = 0,
	Boolean = 10,
	Long = 23,
	ULong = 27,
	Float = 40,
	Double = 41,
	String = 50,
	Array = 60,
	Object = 70,
	Error = byte.MaxValue
}
