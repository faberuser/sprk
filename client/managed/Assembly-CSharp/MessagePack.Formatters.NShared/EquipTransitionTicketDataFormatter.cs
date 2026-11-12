using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipTransitionTicketDataFormatter : IMessagePackFormatter<EquipTransitionTicketData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipTransitionTicketData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.ItemIndex);
		resolver.GetFormatterWithVerify<EquipTransitionType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SelectAbleDetailIndex, options);
		writer.Write(value.LeastStar);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.FixedGiveItem, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.FixedGetItem, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ExceptItemGroup1, options);
	}

	public EquipTransitionTicketData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipTransitionTicketData equipTransitionTicketData = new EquipTransitionTicketData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipTransitionTicketData.ItemIndex = reader.ReadInt32();
				break;
			case 1:
				equipTransitionTicketData.Type = resolver.GetFormatterWithVerify<EquipTransitionType>().Deserialize(ref reader, options);
				break;
			case 2:
				equipTransitionTicketData.SelectAbleDetailIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				equipTransitionTicketData.LeastStar = reader.ReadInt32();
				break;
			case 4:
				equipTransitionTicketData.FixedGiveItem = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				equipTransitionTicketData.FixedGetItem = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				equipTransitionTicketData.ExceptItemGroup1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipTransitionTicketData;
	}
}
