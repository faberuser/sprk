using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UpgradeBookItemDataFormatter : IMessagePackFormatter<UpgradeBookItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UpgradeBookItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Grade);
	}

	public UpgradeBookItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		UpgradeBookItemData upgradeBookItemData = new UpgradeBookItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				upgradeBookItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				upgradeBookItemData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				upgradeBookItemData.Grade = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return upgradeBookItemData;
	}
}
