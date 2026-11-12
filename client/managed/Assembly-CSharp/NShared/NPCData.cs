using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NPCData : IDataKeySelector<string>
{
	private List<EntryMenu> EntryMenus;

	private List<EntryMenu> BottomEntryMenus;

	private List<EntryMenuBuffIconInfo> BuffIconInfos;

	string IDataKeySelector<string>.Key => Code;

	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public string ModelCode { get; set; }

	[Key(2)]
	public int CreatureIndex { get; set; }

	[Key(3)]
	public int CreatureStar { get; set; }

	[Key(4)]
	public int HeroIndex { get; set; }

	[Key(5)]
	public bool ShowHeroBuffIcon { get; set; }

	[Key(6)]
	public string[] OnOpenEntryMenu { get; set; }

	[Key(7)]
	public string[] OnTouch { get; set; }

	[Key(8)]
	public string[] OnRecruit { get; set; }

	[Key(9)]
	public int[] NpcOffset { get; set; }

	[Key(10)]
	public int NpcAngle { get; set; }

	[Key(11)]
	public int NpcScale { get; set; }

	[Key(12)]
	public string NpcHudTextCode { get; set; }

	[Key(13)]
	public string[] NpcCondition { get; set; }

	[Key(14)]
	public int MiniGuideIndex { get; set; }

	[Key(15)]
	public string[] EntryMenuButton1 { get; set; }

	[Key(16)]
	public string EntryMenuCondition1 { get; set; }

	[Key(17)]
	public string[] EntryMenuEvent1 { get; set; }

	[Key(18)]
	public int[] ChildIndices1 { get; set; }

	[Key(19)]
	public int ParentIndex1 { get; set; }

	[Key(20)]
	public string[] EntryMenuButton2 { get; set; }

	[Key(21)]
	public string EntryMenuCondition2 { get; set; }

	[Key(22)]
	public string[] EntryMenuEvent2 { get; set; }

	[Key(23)]
	public int[] ChildIndices2 { get; set; }

	[Key(24)]
	public int ParentIndex2 { get; set; }

	[Key(25)]
	public string[] EntryMenuButton3 { get; set; }

	[Key(26)]
	public string EntryMenuCondition3 { get; set; }

	[Key(27)]
	public string[] EntryMenuEvent3 { get; set; }

	[Key(28)]
	public int[] ChildIndices3 { get; set; }

	[Key(29)]
	public int ParentIndex3 { get; set; }

	[Key(30)]
	public string[] EntryMenuButton4 { get; set; }

	[Key(31)]
	public string EntryMenuCondition4 { get; set; }

	[Key(32)]
	public string[] EntryMenuEvent4 { get; set; }

	[Key(33)]
	public int[] ChildIndices4 { get; set; }

	[Key(34)]
	public int ParentIndex4 { get; set; }

	[Key(35)]
	public string[] EntryMenuButton5 { get; set; }

	[Key(36)]
	public string EntryMenuCondition5 { get; set; }

	[Key(37)]
	public string[] EntryMenuEvent5 { get; set; }

	[Key(38)]
	public int[] ChildIndices5 { get; set; }

	[Key(39)]
	public int ParentIndex5 { get; set; }

	[Key(40)]
	public string[] EntryMenuButton6 { get; set; }

	[Key(41)]
	public string EntryMenuCondition6 { get; set; }

	[Key(42)]
	public string[] EntryMenuEvent6 { get; set; }

	[Key(43)]
	public int[] ChildIndices6 { get; set; }

	[Key(44)]
	public int ParentIndex6 { get; set; }

	[Key(45)]
	public string[] EntryMenuButton7 { get; set; }

	[Key(46)]
	public string EntryMenuCondition7 { get; set; }

	[Key(47)]
	public string[] EntryMenuEvent7 { get; set; }

	[Key(48)]
	public int[] ChildIndices7 { get; set; }

	[Key(49)]
	public int ParentIndex7 { get; set; }

	[Key(50)]
	public string[] EntryMenuButton8 { get; set; }

	[Key(51)]
	public string EntryMenuCondition8 { get; set; }

	[Key(52)]
	public string[] EntryMenuEvent8 { get; set; }

	[Key(53)]
	public int[] ChildIndices8 { get; set; }

	[Key(54)]
	public int ParentIndex8 { get; set; }

	[Key(55)]
	public string[] BottomEntryMenuButton1 { get; set; }

	[Key(56)]
	public string BottomEntryMenuCondition1 { get; set; }

	[Key(57)]
	public string[] BottomEntryMenuEvent1 { get; set; }

	[Key(58)]
	public string[] BottomEntryMenuButton2 { get; set; }

	[Key(59)]
	public string BottomEntryMenuCondition2 { get; set; }

	[Key(60)]
	public string[] BottomEntryMenuEvent2 { get; set; }

	[Key(61)]
	public string[] BottomEntryMenuButton3 { get; set; }

	[Key(62)]
	public string BottomEntryMenuCondition3 { get; set; }

	[Key(63)]
	public string[] BottomEntryMenuEvent3 { get; set; }

	[Key(64)]
	public string BuffIconName1 { get; set; }

	[Key(65)]
	public string BuffIconCondition1 { get; set; }

	[Key(66)]
	public string BuffIconName2 { get; set; }

	[Key(67)]
	public string BuffIconCondition2 { get; set; }

	[Key(68)]
	public string BuffIconName3 { get; set; }

	[Key(69)]
	public string BuffIconCondition3 { get; set; }

	[Key(70)]
	public string MenuIconType { get; set; }

	[Key(71)]
	public string MenuTitleKey { get; set; }

	[Key(72)]
	public string[] OnGetMissionReward { get; set; }

	[Key(73)]
	public string[] OnOpenMissionPopup { get; set; }

	[Key(74)]
	public string[] QuizCorrectAnswer { get; set; }

	[Key(75)]
	public string[] QuizWrongAnswer { get; set; }

	[Key(76)]
	public string[] StepRewardViewHero { get; set; }

	[Key(77)]
	public int CreatureCostumeIndex { get; set; }

	public override string ToString()
	{
		return "NPC Code=" + Code;
	}

	private void InitEntryMenu()
	{
		EntryMenus = new List<EntryMenu>();
		EntryMenus.Add(new EntryMenu(1, EntryMenuButton1, EntryMenuCondition1, EntryMenuEvent1, ChildIndices1, ParentIndex1));
		EntryMenus.Add(new EntryMenu(2, EntryMenuButton2, EntryMenuCondition2, EntryMenuEvent2, ChildIndices2, ParentIndex2));
		EntryMenus.Add(new EntryMenu(3, EntryMenuButton3, EntryMenuCondition3, EntryMenuEvent3, ChildIndices3, ParentIndex3));
		EntryMenus.Add(new EntryMenu(4, EntryMenuButton4, EntryMenuCondition4, EntryMenuEvent4, ChildIndices4, ParentIndex4));
		EntryMenus.Add(new EntryMenu(5, EntryMenuButton5, EntryMenuCondition5, EntryMenuEvent5, ChildIndices5, ParentIndex5));
		EntryMenus.Add(new EntryMenu(6, EntryMenuButton6, EntryMenuCondition6, EntryMenuEvent6, ChildIndices6, ParentIndex6));
		EntryMenus.Add(new EntryMenu(7, EntryMenuButton7, EntryMenuCondition7, EntryMenuEvent7, ChildIndices7, ParentIndex7));
		EntryMenus.Add(new EntryMenu(8, EntryMenuButton8, EntryMenuCondition8, EntryMenuEvent8, ChildIndices8, ParentIndex8));
		BottomEntryMenus = new List<EntryMenu>();
		BottomEntryMenus.Add(new EntryMenu(1, BottomEntryMenuButton1, BottomEntryMenuCondition1, BottomEntryMenuEvent1, null, 0));
		BottomEntryMenus.Add(new EntryMenu(2, BottomEntryMenuButton2, BottomEntryMenuCondition2, BottomEntryMenuEvent2, null, 0));
		BottomEntryMenus.Add(new EntryMenu(3, BottomEntryMenuButton3, BottomEntryMenuCondition3, BottomEntryMenuEvent3, null, 0));
		BuffIconInfos = new List<EntryMenuBuffIconInfo>();
		BuffIconInfos.Add(new EntryMenuBuffIconInfo
		{
			BuffIconName = BuffIconName1,
			BuffIconCondition = BuffIconCondition1
		});
		BuffIconInfos.Add(new EntryMenuBuffIconInfo
		{
			BuffIconName = BuffIconName2,
			BuffIconCondition = BuffIconCondition2
		});
		BuffIconInfos.Add(new EntryMenuBuffIconInfo
		{
			BuffIconName = BuffIconName3,
			BuffIconCondition = BuffIconCondition3
		});
	}

	public bool EntryMenuExist()
	{
		if (EntryMenuButton1 != null)
		{
			return EntryMenuButton1.Length != 0;
		}
		return false;
	}

	public EntryMenu GetEntryMenuButton(int menuIndex)
	{
		if (EntryMenus == null)
		{
			InitEntryMenu();
		}
		if (menuIndex < 1 || menuIndex > EntryMenus.Count)
		{
			return null;
		}
		return EntryMenus[menuIndex - 1];
	}

	public List<EntryMenu> GetSubEntryMenu(int parentIndex)
	{
		if (EntryMenus == null)
		{
			InitEntryMenu();
		}
		List<EntryMenu> list = new List<EntryMenu>();
		for (int i = 0; i < EntryMenus.Count; i++)
		{
			if (EntryMenus[i].IsChild && EntryMenus[i].ParentIndex == parentIndex)
			{
				list.Add(EntryMenus[i]);
			}
		}
		return list;
	}

	public EntryMenu GetBottomEntryMenu(int menuIndex)
	{
		if (BottomEntryMenus == null)
		{
			InitEntryMenu();
		}
		if (menuIndex < 1 || menuIndex > BottomEntryMenus.Count)
		{
			return null;
		}
		return BottomEntryMenus[menuIndex - 1];
	}

	public EntryMenuBuffIconInfo GetBuffIconInfo(int buffIndex)
	{
		if (BuffIconInfos == null)
		{
			InitEntryMenu();
		}
		if (buffIndex < 1 || buffIndex > BuffIconInfos.Count)
		{
			return null;
		}
		return BuffIconInfos[buffIndex - 1];
	}

	public bool GetStepRewardViewHero(out int angle, out int scale, out int x, out int y)
	{
		if (StepRewardViewHero == null || StepRewardViewHero.Length != 4)
		{
			angle = 0;
			scale = 0;
			x = 0;
			y = 0;
			return false;
		}
		angle = ArgParser.GetValueInt(StepRewardViewHero, 0);
		scale = ArgParser.GetValueInt(StepRewardViewHero, 1);
		x = ArgParser.GetValueInt(StepRewardViewHero, 2);
		y = ArgParser.GetValueInt(StepRewardViewHero, 3);
		return true;
	}
}
