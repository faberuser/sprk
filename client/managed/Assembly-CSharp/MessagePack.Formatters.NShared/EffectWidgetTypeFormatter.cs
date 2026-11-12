using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EffectWidgetTypeFormatter : IMessagePackFormatter<EffectWidgetType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EffectWidgetType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EffectWidgetType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EffectWidgetType)reader.ReadInt32();
	}
}
