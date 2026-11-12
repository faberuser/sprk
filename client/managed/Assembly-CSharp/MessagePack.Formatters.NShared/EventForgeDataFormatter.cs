using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventForgeDataFormatter : IMessagePackFormatter<EventForgeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventForgeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.WriteNil();
		writer.Write(value.Index);
		writer.Write(value.EnchatItemGroup);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.EnchantLevel);
		writer.Write(value.MaxEnchantLevel);
		writer.Write(value.IsHighEnchant);
		writer.Write(value.SuccessRatio);
		writer.Write(value.FailRatio);
		writer.Write(value.BreakRatio);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BreakItemCode, options);
		writer.Write(value.IsBreakPopup);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SuccessfulItemCode, options);
		writer.Write(value.SuccessfulItemCount);
		writer.Write(value.ExchangeRewardIndex);
		writer.Write(value.RequireGoldCost);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RequireItem, options);
		writer.Write(value.RequireItemCount);
	}

	public EventForgeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventForgeData eventForgeData = new EventForgeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 1:
				eventForgeData.Index = reader.ReadInt32();
				break;
			case 2:
				eventForgeData.EnchatItemGroup = reader.ReadInt32();
				break;
			case 3:
				eventForgeData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				eventForgeData.EnchantLevel = reader.ReadInt32();
				break;
			case 5:
				eventForgeData.MaxEnchantLevel = reader.ReadInt32();
				break;
			case 6:
				eventForgeData.IsHighEnchant = reader.ReadBoolean();
				break;
			case 7:
				eventForgeData.SuccessRatio = reader.ReadInt32();
				break;
			case 8:
				eventForgeData.FailRatio = reader.ReadInt32();
				break;
			case 9:
				eventForgeData.BreakRatio = reader.ReadInt32();
				break;
			case 10:
				eventForgeData.BreakItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				eventForgeData.IsBreakPopup = reader.ReadBoolean();
				break;
			case 12:
				eventForgeData.SuccessfulItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				eventForgeData.SuccessfulItemCount = reader.ReadInt32();
				break;
			case 14:
				eventForgeData.ExchangeRewardIndex = reader.ReadInt32();
				break;
			case 15:
				eventForgeData.RequireGoldCost = reader.ReadInt32();
				break;
			case 16:
				eventForgeData.RequireItem = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				eventForgeData.RequireItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventForgeData;
	}
}
