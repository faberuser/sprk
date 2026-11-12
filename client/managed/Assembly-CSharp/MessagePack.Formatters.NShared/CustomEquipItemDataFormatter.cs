using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CustomEquipItemDataFormatter : IMessagePackFormatter<CustomEquipItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CustomEquipItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(39);
		writer.Write(value.Index);
		writer.Write(value.SlotIndex);
		writer.Write(value.ItemIndex);
		writer.Write(value.Star);
		writer.Write(value.Level);
		writer.Write(value.Exp);
		writer.Write(value.OptionIndex1);
		writer.Write(value.OptionStep1);
		writer.Write(value.OptionRenewCount1);
		writer.Write(value.IsRenewedOption1);
		writer.Write(value.OptionIndex2);
		writer.Write(value.OptionStep2);
		writer.Write(value.OptionRenewCount2);
		writer.Write(value.IsRenewedOption2);
		writer.Write(value.OptionIndex3);
		writer.Write(value.OptionStep3);
		writer.Write(value.OptionRenewCount3);
		writer.Write(value.IsRenewedOption3);
		writer.Write(value.OptionIndex4);
		writer.Write(value.OptionStep4);
		writer.Write(value.OptionRenewCount4);
		writer.Write(value.IsRenewedOption4);
		writer.Write(value.OptionIndex5);
		writer.Write(value.OptionStep5);
		writer.Write(value.OptionRenewCount5);
		writer.Write(value.IsRenewedOption5);
		writer.Write(value.RuneSlotCount);
		writer.Write(value.RuneItemIndex1);
		writer.Write(value.RuneItemIndex2);
		writer.Write(value.RuneItemIndex3);
		writer.Write(value.UpgradeStarFailBonus);
		writer.Write(value.Locked);
		writer.Write(value.EnchantOptionIndex1);
		writer.Write(value.EnchantOptionStep1);
		writer.Write(value.EnchantOptionIndex2);
		writer.Write(value.EnchantOptionStep2);
		writer.Write(value.EnchantOptionIndex3);
		writer.Write(value.EnchantOptionStep3);
		writer.Write(value.RenewEnchantOptionSlotIndex);
	}

	public CustomEquipItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CustomEquipItemData customEquipItemData = new CustomEquipItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				customEquipItemData.Index = reader.ReadInt32();
				break;
			case 1:
				customEquipItemData.SlotIndex = reader.ReadInt32();
				break;
			case 2:
				customEquipItemData.ItemIndex = reader.ReadInt32();
				break;
			case 3:
				customEquipItemData.Star = reader.ReadInt32();
				break;
			case 4:
				customEquipItemData.Level = reader.ReadInt32();
				break;
			case 5:
				customEquipItemData.Exp = reader.ReadInt32();
				break;
			case 6:
				customEquipItemData.OptionIndex1 = reader.ReadInt32();
				break;
			case 7:
				customEquipItemData.OptionStep1 = reader.ReadInt32();
				break;
			case 8:
				customEquipItemData.OptionRenewCount1 = reader.ReadInt32();
				break;
			case 9:
				customEquipItemData.IsRenewedOption1 = reader.ReadInt32();
				break;
			case 10:
				customEquipItemData.OptionIndex2 = reader.ReadInt32();
				break;
			case 11:
				customEquipItemData.OptionStep2 = reader.ReadInt32();
				break;
			case 12:
				customEquipItemData.OptionRenewCount2 = reader.ReadInt32();
				break;
			case 13:
				customEquipItemData.IsRenewedOption2 = reader.ReadInt32();
				break;
			case 14:
				customEquipItemData.OptionIndex3 = reader.ReadInt32();
				break;
			case 15:
				customEquipItemData.OptionStep3 = reader.ReadInt32();
				break;
			case 16:
				customEquipItemData.OptionRenewCount3 = reader.ReadInt32();
				break;
			case 17:
				customEquipItemData.IsRenewedOption3 = reader.ReadInt32();
				break;
			case 18:
				customEquipItemData.OptionIndex4 = reader.ReadInt32();
				break;
			case 19:
				customEquipItemData.OptionStep4 = reader.ReadInt32();
				break;
			case 20:
				customEquipItemData.OptionRenewCount4 = reader.ReadInt32();
				break;
			case 21:
				customEquipItemData.IsRenewedOption4 = reader.ReadInt32();
				break;
			case 22:
				customEquipItemData.OptionIndex5 = reader.ReadInt32();
				break;
			case 23:
				customEquipItemData.OptionStep5 = reader.ReadInt32();
				break;
			case 24:
				customEquipItemData.OptionRenewCount5 = reader.ReadInt32();
				break;
			case 25:
				customEquipItemData.IsRenewedOption5 = reader.ReadInt32();
				break;
			case 26:
				customEquipItemData.RuneSlotCount = reader.ReadInt32();
				break;
			case 27:
				customEquipItemData.RuneItemIndex1 = reader.ReadInt32();
				break;
			case 28:
				customEquipItemData.RuneItemIndex2 = reader.ReadInt32();
				break;
			case 29:
				customEquipItemData.RuneItemIndex3 = reader.ReadInt32();
				break;
			case 30:
				customEquipItemData.UpgradeStarFailBonus = reader.ReadInt32();
				break;
			case 31:
				customEquipItemData.Locked = reader.ReadByte();
				break;
			case 32:
				customEquipItemData.EnchantOptionIndex1 = reader.ReadInt32();
				break;
			case 33:
				customEquipItemData.EnchantOptionStep1 = reader.ReadInt32();
				break;
			case 34:
				customEquipItemData.EnchantOptionIndex2 = reader.ReadInt32();
				break;
			case 35:
				customEquipItemData.EnchantOptionStep2 = reader.ReadInt32();
				break;
			case 36:
				customEquipItemData.EnchantOptionIndex3 = reader.ReadInt32();
				break;
			case 37:
				customEquipItemData.EnchantOptionStep3 = reader.ReadInt32();
				break;
			case 38:
				customEquipItemData.RenewEnchantOptionSlotIndex = reader.ReadByte();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return customEquipItemData;
	}
}
