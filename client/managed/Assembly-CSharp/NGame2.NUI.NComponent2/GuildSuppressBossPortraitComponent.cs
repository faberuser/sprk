using System.Collections.Generic;
using NGameAsset;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressBossPortraitComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface IClickListener
	{
		void OnClickSelect(int index);
	}

	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressBossPortraitComponent";

	public static PrefabAuto<GuildSuppressBossPortraitComponent> PrefabAuto = new PrefabAuto<GuildSuppressBossPortraitComponent>("Prefab2/Component/GuildSuppress/GuildSuppressBossPortraitComponent");

	public UITexture BossPortraitTexture;

	public UILabel BossPrefixLabel;

	public UILabel BossNameLabel;

	public UIScrollGrid BanScrollGrid;

	private int _index;

	private IClickListener _listener;

	private List<GuildSuppressBanComponent> _listBanComponent = new List<GuildSuppressBanComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseBanComponent();
	}

	public void Open(IClickListener listener, int index)
	{
	}

	private void SetBossPortrait(string path)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(path);
		UIHelper.ResetPlaneAssetTextureAsync(this, BossPortraitTexture, plainTexture);
	}

	private void SetBossPrefixLabel(string prefix)
	{
		UIHelper.SetLabelText(BossPrefixLabel, prefix);
	}

	private void SetBossNameLabel(string name)
	{
		UIHelper.SetLabelText(BossNameLabel, name);
	}

	private void SetBanComponent()
	{
	}

	private void ReleaseBanComponent()
	{
		FactoryBase.ReleaseAndClearList(_listBanComponent);
	}

	public void OnClickSelect()
	{
		if (_listener != null)
		{
			_listener.OnClickSelect(_index);
		}
	}
}
