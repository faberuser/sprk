using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchBanPickCostumeRewardView : BaseWindow, HeroPortraitComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/MatchBanPickCostumeRewardView";

	[Header("[View Base]")]
	public UILabel Label_Title;

	public UIScrollGrid PortraitGrid;

	private ItemData<CostumeSelectItemData> _itemData;

	private List<CostumeData> _costumeDataList = new List<CostumeData>();

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public void Open(ItemData<CostumeSelectItemData> itemData)
	{
		_itemData = itemData;
		base.Open();
		Init();
	}

	public void Init()
	{
		int[] costumeIndices = _itemData.SubData.CostumeIndices;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(costumeIndices))
		{
			Close();
			return;
		}
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("GLORY_COSTUME_NAME"));
		int[] array = costumeIndices;
		foreach (int key in array)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(key);
			if (data != null)
			{
				CreatePortrait(data);
				_costumeDataList.Add(data);
			}
		}
		PortraitGrid.RepositionGrid();
	}

	public override void Close()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		_itemData = null;
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_costumeDataList);
		base.Close();
	}

	private void CreatePortrait(CostumeData costumeData)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(costumeData.HeroIndex);
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(PortraitGrid, HeroPortraitComponent.PrefabAuto);
		heroPortraitComponent.Open(data, this, HeroPortraitComponent.EPortraitMode.BanPick, 0, 0, costumeData.CostumeIndex);
		heroPortraitComponent.ShowName(costumeData.CostumeName);
		heroPortraitComponent.ShowStar(show: false);
		heroPortraitComponent.EnableRewardEffectRoot(isActive: true);
		UIHelper.EnableGameObject(heroPortraitComponent, enable: true);
		_heroPortraitList.Add(heroPortraitComponent);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		int currentCostumeArrayIndex = _heroPortraitList.FindIndex((HeroPortraitComponent x) => x == heroPortrait);
		SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.OpenSeasonRewardIllustBook(currentCostumeArrayIndex, _costumeDataList);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}
}
