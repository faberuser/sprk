using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccessorySelectItemDataFormatter : IMessagePackFormatter<AccessorySelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccessorySelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.AccessoryIndex);
		writer.Write(value.AllOwnedRewardIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TargetHero, options);
	}

	public AccessorySelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AccessorySelectItemData accessorySelectItemData = new AccessorySelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				accessorySelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				accessorySelectItemData.AccessoryIndex = reader.ReadInt32();
				break;
			case 2:
				accessorySelectItemData.AllOwnedRewardIndex = reader.ReadInt32();
				break;
			case 3:
				accessorySelectItemData.TargetHero = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return accessorySelectItemData;
	}
}
