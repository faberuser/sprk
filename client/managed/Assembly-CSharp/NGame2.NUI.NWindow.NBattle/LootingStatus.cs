using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NBattle;

public class LootingStatus : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/LootingStatus";

	public UILabel CoinLabel;

	public UILabel ItemLabel;

	public TweenScale CoinTween;

	public TweenScale ItemTween;

	private int _coin;

	private int _box;

	public UISimpleGrid BoosterGrid;

	private List<BoosterComponent> _BoosterList = new List<BoosterComponent>();

	public new void Open()
	{
		Open(delegate
		{
			SetupBoosters();
		}, null);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_BoosterList);
	}

	public void AddCoin(int i_coin)
	{
		_coin += i_coin;
		UIHelper.SetLabelText(CoinLabel, _coin.ToString());
		CoinTween.PlayForwardReset();
	}

	public void AddBox(int i_box)
	{
		_box += i_box;
		UIHelper.SetLabelText(ItemLabel, _box.ToString());
		ItemTween.PlayForwardReset();
	}

	private void SetupBoosters()
	{
	}
}
