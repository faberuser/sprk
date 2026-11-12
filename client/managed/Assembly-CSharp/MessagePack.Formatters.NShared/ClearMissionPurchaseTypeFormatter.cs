using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClearMissionPurchaseTypeFormatter : IMessagePackFormatter<ClearMissionPurchaseType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClearMissionPurchaseType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ClearMissionPurchaseType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ClearMissionPurchaseType)reader.ReadInt32();
	}
}
