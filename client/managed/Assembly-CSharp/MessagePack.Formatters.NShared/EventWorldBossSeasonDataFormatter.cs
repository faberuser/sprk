using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventWorldBossSeasonDataFormatter : IMessagePackFormatter<EventWorldBossSeasonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventWorldBossSeasonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PlayTimeBegin, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PlayTimeEnd, options);
		writer.Write(value.Immortal);
		writer.Write(value.DefaultWorldBossLevel);
	}

	public EventWorldBossSeasonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventWorldBossSeasonData eventWorldBossSeasonData = new EventWorldBossSeasonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventWorldBossSeasonData.Index = reader.ReadInt32();
				break;
			case 1:
				eventWorldBossSeasonData.PlayTimeBegin = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				eventWorldBossSeasonData.PlayTimeEnd = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				eventWorldBossSeasonData.Immortal = reader.ReadBoolean();
				break;
			case 4:
				eventWorldBossSeasonData.DefaultWorldBossLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventWorldBossSeasonData;
	}
}
