using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WeaponUniqueSelectItemData : IItemSubData, IDataKeySelector<string>
{
	[CompilerGenerated]
	private int[] _003CItemIndices_003Ek__BackingField;

	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int[] ItemIndices
	{
		[CompilerGenerated]
		get
		{
			if (ItemCode == "ARTIFACT_SELECT_ITEM_EVENT_NORMAL")
			{
				return new int[45]
				{
					150001, 150002, 150003, 150004, 150006, 150007, 150008, 150009, 150010, 150011,
					150012, 150013, 150014, 150015, 150016, 150018, 150019, 150020, 150021, 150022,
					150027, 150028, 150029, 150005, 150032, 150033, 150030, 150034, 150035, 150037,
					150031, 150036, 150038, 150047, 150048, 150049, 150063, 150064, 150065, 150066,
					150067, 150068, 150900, 150901, 150902
				};
			}
			if (ItemCode == "ARTIFACT_SELECT_ITEM_ALL")
			{
				return new int[91]
				{
					150001, 150002, 150003, 150004, 150005, 150006, 150007, 150008, 150009, 150010,
					150011, 150012, 150013, 150014, 150015, 150016, 150018, 150019, 150020, 150021,
					150022, 150024, 150025, 150026, 150023, 150027, 150028, 150029, 150030, 150031,
					150032, 150033, 150034, 150035, 150036, 150037, 150038, 150040, 150041, 150042,
					150039, 150043, 150044, 150045, 150046, 150047, 150048, 150049, 150051, 150052,
					150053, 150055, 150056, 150057, 150058, 150059, 150060, 150061, 150062, 150063,
					150064, 150065, 150066, 150067, 150068, 150501, 150502, 150503, 150520, 150504,
					150505, 150506, 150507, 150508, 150509, 150510, 150511, 150512, 150513, 150514,
					150516, 150517, 150518, 150519, 150521, 150522, 150523, 150524, 150900, 150901,
					150902
				};
			}
			return _003CItemIndices_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CItemIndices_003Ek__BackingField = value;
		}
	}

	[Key(2)]
	public string TitleKey { get; set; }

	[Key(3)]
	public int SelectOptionCount { get; set; }

	[Key(4)]
	public int Star { get; set; }

	[Key(5)]
	public int Level { get; set; }

	[Key(6)]
	public bool SelectUniqueOption { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
