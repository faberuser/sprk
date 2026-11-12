using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCDataFormatter : IMessagePackFormatter<NPCData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(78);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelCode, options);
		writer.Write(value.CreatureIndex);
		writer.Write(value.CreatureStar);
		writer.Write(value.HeroIndex);
		writer.Write(value.ShowHeroBuffIcon);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OnOpenEntryMenu, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OnTouch, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OnRecruit, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.NpcOffset, options);
		writer.Write(value.NpcAngle);
		writer.Write(value.NpcScale);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcHudTextCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NpcCondition, options);
		writer.Write(value.MiniGuideIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices1, options);
		writer.Write(value.ParentIndex1);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices2, options);
		writer.Write(value.ParentIndex2);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices3, options);
		writer.Write(value.ParentIndex3);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition4, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices4, options);
		writer.Write(value.ParentIndex4);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton5, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition5, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent5, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices5, options);
		writer.Write(value.ParentIndex5);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton6, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition6, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent6, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices6, options);
		writer.Write(value.ParentIndex6);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton7, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition7, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent7, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices7, options);
		writer.Write(value.ParentIndex7);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuButton8, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition8, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EntryMenuEvent8, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ChildIndices8, options);
		writer.Write(value.ParentIndex8);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuButton1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BottomEntryMenuCondition1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuEvent1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuButton2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BottomEntryMenuCondition2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuEvent2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuButton3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BottomEntryMenuCondition3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BottomEntryMenuEvent3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconName1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconCondition1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconName2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconCondition2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconName3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconCondition3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MenuIconType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MenuTitleKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OnGetMissionReward, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OnOpenMissionPopup, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.QuizCorrectAnswer, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.QuizWrongAnswer, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.StepRewardViewHero, options);
		writer.Write(value.CreatureCostumeIndex);
	}

	public NPCData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NPCData nPCData = new NPCData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				nPCData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				nPCData.ModelCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				nPCData.CreatureIndex = reader.ReadInt32();
				break;
			case 3:
				nPCData.CreatureStar = reader.ReadInt32();
				break;
			case 4:
				nPCData.HeroIndex = reader.ReadInt32();
				break;
			case 5:
				nPCData.ShowHeroBuffIcon = reader.ReadBoolean();
				break;
			case 6:
				nPCData.OnOpenEntryMenu = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				nPCData.OnTouch = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				nPCData.OnRecruit = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				nPCData.NpcOffset = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				nPCData.NpcAngle = reader.ReadInt32();
				break;
			case 11:
				nPCData.NpcScale = reader.ReadInt32();
				break;
			case 12:
				nPCData.NpcHudTextCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				nPCData.NpcCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				nPCData.MiniGuideIndex = reader.ReadInt32();
				break;
			case 15:
				nPCData.EntryMenuButton1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				nPCData.EntryMenuCondition1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				nPCData.EntryMenuEvent1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 18:
				nPCData.ChildIndices1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 19:
				nPCData.ParentIndex1 = reader.ReadInt32();
				break;
			case 20:
				nPCData.EntryMenuButton2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 21:
				nPCData.EntryMenuCondition2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				nPCData.EntryMenuEvent2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 23:
				nPCData.ChildIndices2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 24:
				nPCData.ParentIndex2 = reader.ReadInt32();
				break;
			case 25:
				nPCData.EntryMenuButton3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 26:
				nPCData.EntryMenuCondition3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				nPCData.EntryMenuEvent3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 28:
				nPCData.ChildIndices3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 29:
				nPCData.ParentIndex3 = reader.ReadInt32();
				break;
			case 30:
				nPCData.EntryMenuButton4 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 31:
				nPCData.EntryMenuCondition4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 32:
				nPCData.EntryMenuEvent4 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 33:
				nPCData.ChildIndices4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 34:
				nPCData.ParentIndex4 = reader.ReadInt32();
				break;
			case 35:
				nPCData.EntryMenuButton5 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 36:
				nPCData.EntryMenuCondition5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 37:
				nPCData.EntryMenuEvent5 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 38:
				nPCData.ChildIndices5 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 39:
				nPCData.ParentIndex5 = reader.ReadInt32();
				break;
			case 40:
				nPCData.EntryMenuButton6 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 41:
				nPCData.EntryMenuCondition6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 42:
				nPCData.EntryMenuEvent6 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 43:
				nPCData.ChildIndices6 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 44:
				nPCData.ParentIndex6 = reader.ReadInt32();
				break;
			case 45:
				nPCData.EntryMenuButton7 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 46:
				nPCData.EntryMenuCondition7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 47:
				nPCData.EntryMenuEvent7 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 48:
				nPCData.ChildIndices7 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 49:
				nPCData.ParentIndex7 = reader.ReadInt32();
				break;
			case 50:
				nPCData.EntryMenuButton8 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 51:
				nPCData.EntryMenuCondition8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 52:
				nPCData.EntryMenuEvent8 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 53:
				nPCData.ChildIndices8 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 54:
				nPCData.ParentIndex8 = reader.ReadInt32();
				break;
			case 55:
				nPCData.BottomEntryMenuButton1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 56:
				nPCData.BottomEntryMenuCondition1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 57:
				nPCData.BottomEntryMenuEvent1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 58:
				nPCData.BottomEntryMenuButton2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 59:
				nPCData.BottomEntryMenuCondition2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 60:
				nPCData.BottomEntryMenuEvent2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 61:
				nPCData.BottomEntryMenuButton3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 62:
				nPCData.BottomEntryMenuCondition3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 63:
				nPCData.BottomEntryMenuEvent3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 64:
				nPCData.BuffIconName1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 65:
				nPCData.BuffIconCondition1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 66:
				nPCData.BuffIconName2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 67:
				nPCData.BuffIconCondition2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 68:
				nPCData.BuffIconName3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 69:
				nPCData.BuffIconCondition3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 70:
				nPCData.MenuIconType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 71:
				nPCData.MenuTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 72:
				nPCData.OnGetMissionReward = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 73:
				nPCData.OnOpenMissionPopup = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 74:
				nPCData.QuizCorrectAnswer = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 75:
				nPCData.QuizWrongAnswer = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 76:
				nPCData.StepRewardViewHero = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 77:
				nPCData.CreatureCostumeIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return nPCData;
	}
}
