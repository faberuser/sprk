using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulStoneRestoreDataFormatter : IMessagePackFormatter<SoulStoneRestoreData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulStoneRestoreData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode, options);
		writer.Write(value.MaterialItemCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SoulStoneItemGroup, options);
	}

	public SoulStoneRestoreData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulStoneRestoreData soulStoneRestoreData = new SoulStoneRestoreData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulStoneRestoreData.MaterialItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				soulStoneRestoreData.MaterialItemCount = reader.ReadInt32();
				break;
			case 2:
				soulStoneRestoreData.SoulStoneItemGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulStoneRestoreData;
	}
}
