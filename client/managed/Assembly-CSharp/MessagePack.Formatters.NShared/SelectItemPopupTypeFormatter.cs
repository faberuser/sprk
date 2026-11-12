using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SelectItemPopupTypeFormatter : IMessagePackFormatter<SelectItemPopupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SelectItemPopupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SelectItemPopupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SelectItemPopupType)reader.ReadInt32();
	}
}
