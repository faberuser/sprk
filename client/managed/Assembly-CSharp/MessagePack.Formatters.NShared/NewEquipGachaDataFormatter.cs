using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewEquipGachaDataFormatter : IMessagePackFormatter<NewEquipGachaData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewEquipGachaData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(45);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EquipGachaType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		writer.Write(value.CardType);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Durational);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BeginTime, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EndTime, options);
		writer.Write(value.FreeCoolTime);
		writer.Write(value.Bid);
		writer.Write(value.BonusBid);
		writer.Write(value.OpenGold);
		writer.Write(value.OpenGem);
		writer.Write(value.Mileage);
		writer.Write(value.DailyMaxFree);
		writer.Write(value.ShowProbability);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PickupHeroIndices, options);
		writer.Write(value.PickupHeroCount);
		resolver.GetFormatterWithVerify<EquipGachaSubType>().Serialize(ref writer, value.GachaSubType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GachaCategorySelectTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GachaSceneDesc, options);
		writer.Write(value.GachaShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PickupGachaExpiredMessage, options);
		writer.Write(value.GachaCategoryGroupIndex);
		writer.Write(value.UniqueAwake);
		writer.Write(value.DailyMaxSale);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StepUpHeroIndices, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StepUpHeroCostumeIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		writer.Write(value.TicketOnly);
		writer.Write(value.StarterMileage);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AppearNodeTree, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReactionNodeTree, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NodeTree, options);
		writer.Write(value.OpenDate);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.MileageAmountForReward);
		writer.Write(value.StepUpAccessoryIndex);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.StepUpAccessoryOffset, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.StepUpAccessoryRotation, options);
		writer.Write(value.StepUpAccessoryScale);
		writer.Write(value.AllowExpiredTicket);
		writer.Write(value.PurchaseRestrictProductIndex);
	}

	public NewEquipGachaData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewEquipGachaData newEquipGachaData = new NewEquipGachaData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newEquipGachaData.Index = reader.ReadInt32();
				break;
			case 1:
				newEquipGachaData.Type = resolver.GetFormatterWithVerify<EquipGachaType>().Deserialize(ref reader, options);
				break;
			case 2:
				newEquipGachaData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newEquipGachaData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				newEquipGachaData.CardType = reader.ReadInt32();
				break;
			case 5:
				newEquipGachaData.Category = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				newEquipGachaData.Durational = reader.ReadInt32();
				break;
			case 7:
				newEquipGachaData.BeginTime = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				newEquipGachaData.EndTime = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				newEquipGachaData.FreeCoolTime = reader.ReadInt32();
				break;
			case 10:
				newEquipGachaData.Bid = reader.ReadByte();
				break;
			case 11:
				newEquipGachaData.BonusBid = reader.ReadByte();
				break;
			case 12:
				newEquipGachaData.OpenGold = reader.ReadInt32();
				break;
			case 13:
				newEquipGachaData.OpenGem = reader.ReadInt32();
				break;
			case 14:
				newEquipGachaData.Mileage = reader.ReadInt32();
				break;
			case 15:
				newEquipGachaData.DailyMaxFree = reader.ReadInt32();
				break;
			case 16:
				newEquipGachaData.ShowProbability = reader.ReadBoolean();
				break;
			case 17:
				newEquipGachaData.PickupHeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				newEquipGachaData.PickupHeroCount = reader.ReadInt32();
				break;
			case 19:
				newEquipGachaData.GachaSubType = resolver.GetFormatterWithVerify<EquipGachaSubType>().Deserialize(ref reader, options);
				break;
			case 20:
				newEquipGachaData.GachaCategorySelectTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				newEquipGachaData.GachaSceneDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				newEquipGachaData.GachaShowIndex = reader.ReadInt32();
				break;
			case 23:
				newEquipGachaData.PickupGachaExpiredMessage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 24:
				newEquipGachaData.GachaCategoryGroupIndex = reader.ReadInt32();
				break;
			case 25:
				newEquipGachaData.UniqueAwake = reader.ReadBoolean();
				break;
			case 26:
				newEquipGachaData.DailyMaxSale = reader.ReadInt32();
				break;
			case 27:
				newEquipGachaData.StepUpHeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 28:
				newEquipGachaData.StepUpHeroCostumeIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 29:
				newEquipGachaData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 30:
				newEquipGachaData.TicketOnly = reader.ReadBoolean();
				break;
			case 31:
				newEquipGachaData.StarterMileage = reader.ReadInt32();
				break;
			case 32:
				newEquipGachaData.RewardIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 33:
				newEquipGachaData.AppearNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 34:
				newEquipGachaData.ReactionNodeTree = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 35:
				newEquipGachaData.NodeTree = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 36:
				newEquipGachaData.OpenDate = reader.ReadInt32();
				break;
			case 37:
				newEquipGachaData.OpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 38:
				newEquipGachaData.MileageAmountForReward = reader.ReadInt32();
				break;
			case 39:
				newEquipGachaData.StepUpAccessoryIndex = reader.ReadInt32();
				break;
			case 40:
				newEquipGachaData.StepUpAccessoryOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 41:
				newEquipGachaData.StepUpAccessoryRotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 42:
				newEquipGachaData.StepUpAccessoryScale = reader.ReadSingle();
				break;
			case 43:
				newEquipGachaData.AllowExpiredTicket = reader.ReadBoolean();
				break;
			case 44:
				newEquipGachaData.PurchaseRestrictProductIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newEquipGachaData;
	}
}
