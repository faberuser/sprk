using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccessoryCostumeDataFormatter : IMessagePackFormatter<AccessoryCostumeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccessoryCostumeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(21);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.UnableHero, options);
		resolver.GetFormatterWithVerify<AccessoryPartType>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Serialize(ref writer, value.PivotType, options);
		writer.WriteNil();
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ProductDescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		writer.Write(value.ReqBuyGem);
		writer.Write(value.Mileage);
		writer.Write(value.IsOpen);
		writer.Write(value.IsBuy);
		writer.Write(value.New);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ProductImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPortraitPath, options);
		writer.Write(value.PreviewableWhenOwned);
		writer.Write(value.OpenDate);
		writer.Write(value.IsShow);
	}

	public AccessoryCostumeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AccessoryCostumeData accessoryCostumeData = new AccessoryCostumeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				accessoryCostumeData.Index = reader.ReadInt32();
				break;
			case 1:
				accessoryCostumeData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				accessoryCostumeData.UnableHero = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				accessoryCostumeData.PartType = resolver.GetFormatterWithVerify<AccessoryPartType>().Deserialize(ref reader, options);
				break;
			case 4:
				accessoryCostumeData.PivotType = resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Deserialize(ref reader, options);
				break;
			case 7:
				accessoryCostumeData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				accessoryCostumeData.ProductDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				accessoryCostumeData.Category = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				accessoryCostumeData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				accessoryCostumeData.ReqBuyGem = reader.ReadInt32();
				break;
			case 12:
				accessoryCostumeData.Mileage = reader.ReadInt32();
				break;
			case 13:
				accessoryCostumeData.IsOpen = reader.ReadBoolean();
				break;
			case 14:
				accessoryCostumeData.IsBuy = reader.ReadBoolean();
				break;
			case 15:
				accessoryCostumeData.New = reader.ReadByte();
				break;
			case 16:
				accessoryCostumeData.ProductImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				accessoryCostumeData.IconPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				accessoryCostumeData.PreviewableWhenOwned = reader.ReadBoolean();
				break;
			case 19:
				accessoryCostumeData.OpenDate = reader.ReadInt32();
				break;
			case 20:
				accessoryCostumeData.IsShow = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return accessoryCostumeData;
	}
}
