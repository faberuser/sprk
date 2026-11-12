namespace LitJson;

public enum JsonToken
{
	None,
	ObjectStart,
	PropertyName,
	ObjectEnd,
	ArrayStart,
	ArrayEnd,
	Long,
	Double,
	String,
	Boolean,
	Null
}
