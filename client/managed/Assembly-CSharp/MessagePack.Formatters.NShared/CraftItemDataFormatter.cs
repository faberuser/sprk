using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CraftItemDataFormatter : IMessagePackFormatter<CraftItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CraftItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(26);
		writer.Write(value.CraftIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ResultItemCount);
		writer.Write(value.ReqGold);
		writer.Write(value.ReqTime);
		writer.Write(value.Countable);
		writer.Write(value.IsOpen);
		writer.Write(value.IsRefund);
		resolver.GetFormatterWithVerify<ItemType>().Serialize(ref writer, value.ItemType, options);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode1, options);
		writer.Write(value.MaterialItemCount1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode2, options);
		writer.Write(value.MaterialItemCount2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode3, options);
		writer.Write(value.MaterialItemCount3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode4, options);
		writer.Write(value.MaterialItemCount4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode5, options);
		writer.Write(value.MaterialItemCount5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode6, options);
		writer.Write(value.MaterialItemCount6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode7, options);
		writer.Write(value.MaterialItemCount7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode8, options);
		writer.Write(value.MaterialItemCount8);
	}

	public CraftItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CraftItemData craftItemData = new CraftItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				craftItemData.CraftIndex = reader.ReadInt32();
				break;
			case 1:
				craftItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				craftItemData.ResultItemCount = reader.ReadInt32();
				break;
			case 3:
				craftItemData.ReqGold = reader.ReadInt32();
				break;
			case 4:
				craftItemData.ReqTime = reader.ReadInt32();
				break;
			case 5:
				craftItemData.Countable = reader.ReadBoolean();
				break;
			case 6:
				craftItemData.IsOpen = reader.ReadBoolean();
				break;
			case 7:
				craftItemData.IsRefund = reader.ReadBoolean();
				break;
			case 8:
				craftItemData.ItemType = resolver.GetFormatterWithVerify<ItemType>().Deserialize(ref reader, options);
				break;
			case 9:
				craftItemData.ShowIndex = reader.ReadInt32();
				break;
			case 10:
				craftItemData.MaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				craftItemData.MaterialItemCount1 = reader.ReadInt32();
				break;
			case 12:
				craftItemData.MaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				craftItemData.MaterialItemCount2 = reader.ReadInt32();
				break;
			case 14:
				craftItemData.MaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				craftItemData.MaterialItemCount3 = reader.ReadInt32();
				break;
			case 16:
				craftItemData.MaterialItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				craftItemData.MaterialItemCount4 = reader.ReadInt32();
				break;
			case 18:
				craftItemData.MaterialItemCode5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				craftItemData.MaterialItemCount5 = reader.ReadInt32();
				break;
			case 20:
				craftItemData.MaterialItemCode6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				craftItemData.MaterialItemCount6 = reader.ReadInt32();
				break;
			case 22:
				craftItemData.MaterialItemCode7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 23:
				craftItemData.MaterialItemCount7 = reader.ReadInt32();
				break;
			case 24:
				craftItemData.MaterialItemCode8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				craftItemData.MaterialItemCount8 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return craftItemData;
	}
}
