using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventCraftDataFormatter : IMessagePackFormatter<EventCraftData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventCraftData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(22);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ShowGroup, options);
		writer.Write(value.RewardIndex);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode1, options);
		writer.Write(value.MaterialItemCount1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode2, options);
		writer.Write(value.MaterialItemCount2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode3, options);
		writer.Write(value.MaterialItemCount3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode4, options);
		writer.Write(value.MaterialItemCount4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode5, options);
		writer.Write(value.MaterialItemCount5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode6, options);
		writer.Write(value.MaterialItemCount6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode7, options);
		writer.Write(value.MaterialItemCount7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode8, options);
		writer.Write(value.MaterialItemCount8);
		writer.Write(value.CraftCount);
	}

	public EventCraftData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventCraftData eventCraftData = new EventCraftData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventCraftData.Index = reader.ReadInt32();
				break;
			case 1:
				eventCraftData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				eventCraftData.ShowGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				eventCraftData.RewardIndex = reader.ReadInt32();
				break;
			case 4:
				eventCraftData.ShowIndex = reader.ReadInt32();
				break;
			case 5:
				eventCraftData.MaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				eventCraftData.MaterialItemCount1 = reader.ReadInt32();
				break;
			case 7:
				eventCraftData.MaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				eventCraftData.MaterialItemCount2 = reader.ReadInt32();
				break;
			case 9:
				eventCraftData.MaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				eventCraftData.MaterialItemCount3 = reader.ReadInt32();
				break;
			case 11:
				eventCraftData.MaterialItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				eventCraftData.MaterialItemCount4 = reader.ReadInt32();
				break;
			case 13:
				eventCraftData.MaterialItemCode5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				eventCraftData.MaterialItemCount5 = reader.ReadInt32();
				break;
			case 15:
				eventCraftData.MaterialItemCode6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				eventCraftData.MaterialItemCount6 = reader.ReadInt32();
				break;
			case 17:
				eventCraftData.MaterialItemCode7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				eventCraftData.MaterialItemCount7 = reader.ReadInt32();
				break;
			case 19:
				eventCraftData.MaterialItemCode8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				eventCraftData.MaterialItemCount8 = reader.ReadInt32();
				break;
			case 21:
				eventCraftData.CraftCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventCraftData;
	}
}
