using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class MatchRewardInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchRewardInfo";

	public UIToggle GradeRewardToggle;

	public UIToggle FirstRewardToggle;

	public UIGrid Grid;

	public UIScrollGrid RewardGrid;

	private List<MatchTierRewardComponent> _componentList;

	public new void Open()
	{
		Open(RefreshRewardList, null);
	}

	public void Init()
	{
		RefreshRewardList();
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		ClearRewardList();
	}

	public void OnClickClose()
	{
		Close();
	}

	private void RefreshRewardList()
	{
		ClearRewardList();
		if (_componentList == null)
		{
			_componentList = new List<MatchTierRewardComponent>();
		}
		foreach (MatchTierType allTierType in GameRoot.Instance.MatchTierDataContainer.GetAllTierTypeList())
		{
			_ = allTierType;
			MatchTierRewardComponent item = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
			_componentList.Add(item);
		}
		MatchTierRewardComponent item2 = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
		_componentList.Add(item2);
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}
}
