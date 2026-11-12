using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PurchaseDungeonDataFormatter : IMessagePackFormatter<PurchaseDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PurchaseDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Buyable);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		writer.Write(value.Price);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonPortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonRewardPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BoosterItemCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnterCondition, options);
	}

	public PurchaseDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PurchaseDungeonData purchaseDungeonData = new PurchaseDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				purchaseDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				purchaseDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				purchaseDungeonData.Buyable = reader.ReadBoolean();
				break;
			case 3:
				purchaseDungeonData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 4:
				purchaseDungeonData.Price = reader.ReadInt32();
				break;
			case 5:
				purchaseDungeonData.DungeonPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				purchaseDungeonData.DungeonRewardPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				purchaseDungeonData.DungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				purchaseDungeonData.RewardLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				purchaseDungeonData.BoosterItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				purchaseDungeonData.EnterCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return purchaseDungeonData;
	}
}
