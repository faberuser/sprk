using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CostumeDataFormatter : IMessagePackFormatter<CostumeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CostumeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(43);
		writer.Write(value.CostumeIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CostumeCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		resolver.GetFormatterWithVerify<EEquipCondition>().Serialize(ref writer, value.EquipCondition, options);
		writer.Write(value.HeroIndex);
		writer.Write(value.HeroStar);
		writer.Write(value.HeroTranscended);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceNameKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NameParam, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceObtainKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ObtainParam, options);
		writer.Write(value.ReqBuyGem);
		writer.Write(value.ReqBuyGold);
		writer.Write(value.ReqBuyMileage);
		writer.Write(value.Mileage);
		writer.Write(value.IsOpen);
		writer.Write(value.IsBuy);
		writer.Write(value.IsDefault);
		writer.Write(value.PreviewableWhenOwned);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BodyPrefix, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WeaponPrefix, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattlePortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VoicePostfix, options);
		writer.Write(value.New);
		writer.Write(value.Hit);
		resolver.GetFormatterWithVerify<CostumeAbilityType>().Serialize(ref writer, value.AbilityType1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AbilityValue1, options);
		resolver.GetFormatterWithVerify<CostumeAbilityType>().Serialize(ref writer, value.AbilityType2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AbilityValue2, options);
		resolver.GetFormatterWithVerify<CostumeAbilityType>().Serialize(ref writer, value.AbilityType3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AbilityValue3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IllustPath, options);
		writer.Write(value.MainIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.BonusCostumeIndices, options);
		writer.Write(value.OpenDate);
		resolver.GetFormatterWithVerify<EHeroCostumeType>().Serialize(ref writer, value.CostumeType, options);
		writer.Write(value.ProductIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattlePortraitEffectPath, options);
		writer.Write(value.IsShow);
	}

	public CostumeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CostumeData costumeData = new CostumeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				costumeData.CostumeIndex = reader.ReadInt32();
				break;
			case 1:
				costumeData.CostumeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				costumeData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				costumeData.EquipCondition = resolver.GetFormatterWithVerify<EEquipCondition>().Deserialize(ref reader, options);
				break;
			case 4:
				costumeData.HeroIndex = reader.ReadInt32();
				break;
			case 5:
				costumeData.HeroStar = reader.ReadInt32();
				break;
			case 6:
				costumeData.HeroTranscended = reader.ReadInt32();
				break;
			case 8:
				costumeData.ReplaceNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				costumeData.NameParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				costumeData.ReplaceObtainKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				costumeData.ObtainParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 13:
				costumeData.ReqBuyGem = reader.ReadInt32();
				break;
			case 14:
				costumeData.ReqBuyGold = reader.ReadInt32();
				break;
			case 15:
				costumeData.ReqBuyMileage = reader.ReadInt32();
				break;
			case 16:
				costumeData.Mileage = reader.ReadInt32();
				break;
			case 17:
				costumeData.IsOpen = reader.ReadBoolean();
				break;
			case 18:
				costumeData.IsBuy = reader.ReadBoolean();
				break;
			case 19:
				costumeData.IsDefault = reader.ReadBoolean();
				break;
			case 20:
				costumeData.PreviewableWhenOwned = reader.ReadBoolean();
				break;
			case 21:
				costumeData.BodyPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				costumeData.WeaponPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 23:
				costumeData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 24:
				costumeData.IconPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				costumeData.BattlePortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 26:
				costumeData.VoicePostfix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				costumeData.New = reader.ReadByte();
				break;
			case 28:
				costumeData.Hit = reader.ReadByte();
				break;
			case 29:
				costumeData.AbilityType1 = resolver.GetFormatterWithVerify<CostumeAbilityType>().Deserialize(ref reader, options);
				break;
			case 30:
				costumeData.AbilityValue1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 31:
				costumeData.AbilityType2 = resolver.GetFormatterWithVerify<CostumeAbilityType>().Deserialize(ref reader, options);
				break;
			case 32:
				costumeData.AbilityValue2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 33:
				costumeData.AbilityType3 = resolver.GetFormatterWithVerify<CostumeAbilityType>().Deserialize(ref reader, options);
				break;
			case 34:
				costumeData.AbilityValue3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 35:
				costumeData.IllustPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 36:
				costumeData.MainIndex = reader.ReadInt32();
				break;
			case 37:
				costumeData.BonusCostumeIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 38:
				costumeData.OpenDate = reader.ReadInt32();
				break;
			case 39:
				costumeData.CostumeType = resolver.GetFormatterWithVerify<EHeroCostumeType>().Deserialize(ref reader, options);
				break;
			case 40:
				costumeData.ProductIndex = reader.ReadInt32();
				break;
			case 41:
				costumeData.BattlePortraitEffectPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 42:
				costumeData.IsShow = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return costumeData;
	}
}
