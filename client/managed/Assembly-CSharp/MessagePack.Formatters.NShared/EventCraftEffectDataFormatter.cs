using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventCraftEffectDataFormatter : IMessagePackFormatter<EventCraftEffectData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventCraftEffectData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.ItemIndex);
	}

	public EventCraftEffectData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EventCraftEffectData eventCraftEffectData = new EventCraftEffectData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventCraftEffectData.Index = reader.ReadInt32();
				break;
			case 1:
				eventCraftEffectData.ItemIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventCraftEffectData;
	}
}
