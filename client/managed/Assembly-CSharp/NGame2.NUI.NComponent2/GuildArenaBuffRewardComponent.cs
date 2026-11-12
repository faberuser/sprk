using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildArenaBuffRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaBuffRewardComponent";

	public static PrefabAuto<GuildArenaBuffRewardComponent> PrefabAuto = new PrefabAuto<GuildArenaBuffRewardComponent>(PrefabPath);

	public UILabel RankLabel;

	public Transform ItemIconRoot;

	private ItemIconComponent _component;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _component);
	}

	private string GetRankText(int rank)
	{
		return LocalizationManager.GetStringDirect("HONOR_RANK_NUM", rank);
	}

	public void Open(GuildArenaServerBuffRewardData data)
	{
		if (data.MinRank == data.MaxRank)
		{
			UIHelper.SetLabelText(RankLabel, GetRankText(data.MinRank));
		}
		else
		{
			UIHelper.SetLabelText(RankLabel, $"{GetRankText(data.MinRank)} ~ {GetRankText(data.MaxRank)}");
		}
		FactoryBase.SafeReleaseComponent(ref _component);
		CreateItemIcon(data.ItemIndex);
	}

	private void CreateItemIcon(int itemIndex)
	{
		FactoryBase.SafeReleaseComponent(ref _component);
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			_component = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
			if (!(_component == null))
			{
				_component.ResetPosition();
				_component.Open(data);
			}
		}
	}
}
