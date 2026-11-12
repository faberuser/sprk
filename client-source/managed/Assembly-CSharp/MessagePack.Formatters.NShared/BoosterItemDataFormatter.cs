using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BoosterItemDataFormatter : IMessagePackFormatter<BoosterItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BoosterItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.BoosterItemIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Value);
		writer.Write(value.Duration);
		resolver.GetFormatterWithVerify<BattleType[]>().Serialize(ref writer, value.BattleTypes, options);
		writer.Write(value.IsOnetime);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnetimeBoosterTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnetimeBoosterDesc, options);
		writer.Write(value.IsHideAtBattle);
	}

	public BoosterItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BoosterItemData boosterItemData = new BoosterItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				boosterItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				boosterItemData.BoosterItemIndex = reader.ReadInt32();
				break;
			case 2:
				boosterItemData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				boosterItemData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				boosterItemData.Type = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 5:
				boosterItemData.Value = reader.ReadInt32();
				break;
			case 6:
				boosterItemData.Duration = reader.ReadInt32();
				break;
			case 7:
				boosterItemData.BattleTypes = resolver.GetFormatterWithVerify<BattleType[]>().Deserialize(ref reader, options);
				break;
			case 8:
				boosterItemData.IsOnetime = reader.ReadBoolean();
				break;
			case 9:
				boosterItemData.OnetimeBoosterTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				boosterItemData.OnetimeBoosterDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				boosterItemData.IsHideAtBattle = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return boosterItemData;
	}
}
