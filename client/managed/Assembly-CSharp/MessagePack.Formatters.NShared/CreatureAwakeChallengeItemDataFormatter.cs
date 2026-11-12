using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureAwakeChallengeItemDataFormatter : IMessagePackFormatter<CreatureAwakeChallengeItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureAwakeChallengeItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendItemCode1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendItemCode2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendItemCode3, options);
	}

	public CreatureAwakeChallengeItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureAwakeChallengeItemData creatureAwakeChallengeItemData = new CreatureAwakeChallengeItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureAwakeChallengeItemData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				creatureAwakeChallengeItemData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				creatureAwakeChallengeItemData.ItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				creatureAwakeChallengeItemData.ItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				creatureAwakeChallengeItemData.TranscendItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureAwakeChallengeItemData.TranscendItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				creatureAwakeChallengeItemData.TranscendItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureAwakeChallengeItemData;
	}
}
