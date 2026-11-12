using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FlaskItemDataFormatter : IMessagePackFormatter<FlaskItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FlaskItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<FlaskType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<FlaskSourceType>().Serialize(ref writer, value.SourceType, options);
		resolver.GetFormatterWithVerify<FlaskRewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.FlaskExp);
		writer.Write(value.PotionItemIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UseDescLocal, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EffectiveSkillIndex, options);
		writer.Write(value.IsUnEquip);
		writer.Write(value.ShowIndex);
	}

	public FlaskItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		FlaskItemData flaskItemData = new FlaskItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				flaskItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				flaskItemData.Type = resolver.GetFormatterWithVerify<FlaskType>().Deserialize(ref reader, options);
				break;
			case 2:
				flaskItemData.SourceType = resolver.GetFormatterWithVerify<FlaskSourceType>().Deserialize(ref reader, options);
				break;
			case 3:
				flaskItemData.RewardType = resolver.GetFormatterWithVerify<FlaskRewardType>().Deserialize(ref reader, options);
				break;
			case 4:
				flaskItemData.FlaskExp = reader.ReadInt32();
				break;
			case 5:
				flaskItemData.PotionItemIndex = reader.ReadInt32();
				break;
			case 6:
				flaskItemData.UseDescLocal = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				flaskItemData.EffectiveSkillIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 8:
				flaskItemData.IsUnEquip = reader.ReadBoolean();
				break;
			case 9:
				flaskItemData.ShowIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return flaskItemData;
	}
}
