using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemGradeDataFormatter : IMessagePackFormatter<ItemGradeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemGradeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Grade);
		writer.Write(value.Score);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GetEffect, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameColor, options);
	}

	public ItemGradeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemGradeData itemGradeData = new ItemGradeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemGradeData.Grade = reader.ReadInt32();
				break;
			case 1:
				itemGradeData.Score = reader.ReadByte();
				break;
			case 2:
				itemGradeData.GetEffect = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				itemGradeData.NameColor = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemGradeData;
	}
}
