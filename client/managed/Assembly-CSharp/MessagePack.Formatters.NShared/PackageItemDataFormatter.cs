using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PackageItemDataFormatter : IMessagePackFormatter<PackageItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PackageItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(16);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.RewardRate1);
		writer.Write(value.RewardIndex1);
		writer.Write(value.RewardRate2);
		writer.Write(value.RewardIndex2);
		writer.Write(value.RewardRate3);
		writer.Write(value.RewardIndex3);
		writer.Write(value.RewardRate4);
		writer.Write(value.RewardIndex4);
		writer.Write(value.RewardRate5);
		writer.Write(value.RewardIndex5);
		writer.Write(value.MaxOpenCount);
		writer.Write(value.OpenWithdrawal);
		writer.Write(value.ShowContent);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WithdrawTextKey, options);
		writer.Write(value.HasEquip);
	}

	public PackageItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PackageItemData packageItemData = new PackageItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				packageItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				packageItemData.RewardRate1 = reader.ReadInt32();
				break;
			case 2:
				packageItemData.RewardIndex1 = reader.ReadInt32();
				break;
			case 3:
				packageItemData.RewardRate2 = reader.ReadInt32();
				break;
			case 4:
				packageItemData.RewardIndex2 = reader.ReadInt32();
				break;
			case 5:
				packageItemData.RewardRate3 = reader.ReadInt32();
				break;
			case 6:
				packageItemData.RewardIndex3 = reader.ReadInt32();
				break;
			case 7:
				packageItemData.RewardRate4 = reader.ReadInt32();
				break;
			case 8:
				packageItemData.RewardIndex4 = reader.ReadInt32();
				break;
			case 9:
				packageItemData.RewardRate5 = reader.ReadInt32();
				break;
			case 10:
				packageItemData.RewardIndex5 = reader.ReadInt32();
				break;
			case 11:
				packageItemData.MaxOpenCount = reader.ReadInt32();
				break;
			case 12:
				packageItemData.OpenWithdrawal = reader.ReadByte();
				break;
			case 13:
				packageItemData.ShowContent = reader.ReadByte();
				break;
			case 14:
				packageItemData.WithdrawTextKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				packageItemData.HasEquip = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return packageItemData;
	}
}
