using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HelpListDataFormatter : IMessagePackFormatter<HelpListData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HelpListData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(29);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.Write(value.Page);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ScrollMovement, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MainTitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BackgroundTexturePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentName1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath1, options);
		writer.Write(value.TextureWidth1);
		writer.Write(value.TextureHeight1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentName2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath2, options);
		writer.Write(value.TextureWidth2);
		writer.Write(value.TextureHeight2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentName3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath3, options);
		writer.Write(value.TextureWidth3);
		writer.Write(value.TextureHeight3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentName4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath4, options);
		writer.Write(value.TextureWidth4);
		writer.Write(value.TextureHeight4);
	}

	public HelpListData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HelpListData helpListData = new HelpListData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				helpListData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				helpListData.Page = reader.ReadInt32();
				break;
			case 2:
				helpListData.ScrollMovement = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				helpListData.MainTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				helpListData.BackgroundTexturePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				helpListData.ComponentName1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				helpListData.TitleKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				helpListData.DescKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				helpListData.TexturePath1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				helpListData.TextureWidth1 = reader.ReadInt32();
				break;
			case 10:
				helpListData.TextureHeight1 = reader.ReadInt32();
				break;
			case 11:
				helpListData.ComponentName2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				helpListData.TitleKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				helpListData.DescKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				helpListData.TexturePath2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				helpListData.TextureWidth2 = reader.ReadInt32();
				break;
			case 16:
				helpListData.TextureHeight2 = reader.ReadInt32();
				break;
			case 17:
				helpListData.ComponentName3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				helpListData.TitleKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				helpListData.DescKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				helpListData.TexturePath3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				helpListData.TextureWidth3 = reader.ReadInt32();
				break;
			case 22:
				helpListData.TextureHeight3 = reader.ReadInt32();
				break;
			case 23:
				helpListData.ComponentName4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 24:
				helpListData.TitleKey4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				helpListData.DescKey4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 26:
				helpListData.TexturePath4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				helpListData.TextureWidth4 = reader.ReadInt32();
				break;
			case 28:
				helpListData.TextureHeight4 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return helpListData;
	}
}
