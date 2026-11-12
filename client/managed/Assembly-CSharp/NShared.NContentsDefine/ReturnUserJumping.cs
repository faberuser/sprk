using System.Collections.Generic;

namespace NShared.NContentsDefine;

[JsonSerializable]
public class ReturnUserJumping : ContentsDefine.IDefine
{
	[JsonSerializable]
	public class MissionCategoryKey : Tuple<int, int>
	{
		public int Main
		{
			get
			{
				return Item1;
			}
			set
			{
				Item1 = value;
			}
		}

		public int Sub
		{
			get
			{
				return Item2;
			}
			set
			{
				Item2 = value;
			}
		}

		public MissionCategoryKey()
			: base(0, 0)
		{
		}

		public MissionCategoryKey(int main, int sub)
			: base(main, sub)
		{
		}
	}

	[JsonSerializable]
	public class ExtraWidget_in_MissionNodeWindowV3
	{
		public MissionCategoryKey MissionCategory { get; set; }

		public bool Show_Button_ReceiveAll { get; set; }

		public bool Disable_NotiPopup { get; set; }

		public string Linked_PlayerAnyMisc { get; set; }

		public int PayShop_Product_Index { get; set; }

		public MissionCategoryKey Prerequisite_MissionCategory { get; set; }

		public ExtraWidget_in_MissionNodeWindowV3()
		{
			Show_Button_ReceiveAll = true;
			MissionCategory = new MissionCategoryKey();
			Prerequisite_MissionCategory = new MissionCategoryKey();
		}
	}

	private HashSet<MissionCategoryKey> _cacheForceUnlockMission_in_MissionSubCategoryComponent;

	private Dictionary<MissionCategoryKey, ExtraWidget_in_MissionNodeWindowV3> _cacheExtraWidgets_in_MissionNodeWindowV3;

	public int[] Jumping_PayShop_Product_Indices { get; set; }

	public List<MissionCategoryKey> ForceUnlockMission_in_MissionSubCategoryComponent { get; set; }

	public string Localize_Need_Purchase_PayShop_Product { get; set; }

	public string Localize_Prerequisite_MissionCategory_Not_Open { get; set; }

	public string Localize_Prerequisite_MissionCategory_Not_Open_Popup { get; set; }

	public List<ExtraWidget_in_MissionNodeWindowV3> ExtraWidgets_in_MissionNodeWindowV3 { get; set; }

	public ReturnUserJumping()
	{
		ForceUnlockMission_in_MissionSubCategoryComponent = new List<MissionCategoryKey>();
		ForceUnlockMission_in_MissionSubCategoryComponent.Add(new MissionCategoryKey(98, 2));
		Localize_Need_Purchase_PayShop_Product = "@@ Localize_Need_Purchase_PayShop_Product @@";
		ExtraWidgets_in_MissionNodeWindowV3 = new List<ExtraWidget_in_MissionNodeWindowV3>();
		ExtraWidgets_in_MissionNodeWindowV3.Add(new ExtraWidget_in_MissionNodeWindowV3
		{
			MissionCategory = new MissionCategoryKey(98, 1),
			PayShop_Product_Index = 9876
		});
	}

	public bool IsForceUnlockMission_in_MissionSubCategoryComponent(int mainCategoryIndex, int subCategoryIndex)
	{
		if (_cacheForceUnlockMission_in_MissionSubCategoryComponent == null)
		{
			_cacheForceUnlockMission_in_MissionSubCategoryComponent = new HashSet<MissionCategoryKey>();
			__Extension_Method_ICollection__.ADDRANGE(_cacheForceUnlockMission_in_MissionSubCategoryComponent, ForceUnlockMission_in_MissionSubCategoryComponent);
		}
		MissionCategoryKey item = new MissionCategoryKey(mainCategoryIndex, subCategoryIndex);
		return _cacheForceUnlockMission_in_MissionSubCategoryComponent.Contains(item);
	}

	public ExtraWidget_in_MissionNodeWindowV3 GetExtraWidget_in_MissionNodeWindowV3(int mainCategoryIndex, int subCategoryIndex)
	{
		if (_cacheExtraWidgets_in_MissionNodeWindowV3 == null)
		{
			_cacheExtraWidgets_in_MissionNodeWindowV3 = new Dictionary<MissionCategoryKey, ExtraWidget_in_MissionNodeWindowV3>();
			foreach (ExtraWidget_in_MissionNodeWindowV3 item in ExtraWidgets_in_MissionNodeWindowV3)
			{
				_cacheExtraWidgets_in_MissionNodeWindowV3.Add(item.MissionCategory, item);
			}
		}
		MissionCategoryKey key = new MissionCategoryKey(mainCategoryIndex, subCategoryIndex);
		if (_cacheExtraWidgets_in_MissionNodeWindowV3.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}
}
