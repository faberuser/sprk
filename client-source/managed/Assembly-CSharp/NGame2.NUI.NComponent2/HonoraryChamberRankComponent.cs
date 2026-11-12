using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HonoraryChamberRankComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClick(HonoraryChamberRankComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/HonoraryChamber/HonoraryChamberRankComponent";

	public static PrefabAuto<HonoraryChamberRankComponent> PrefabAuto = new PrefabAuto<HonoraryChamberRankComponent>("Prefab2/Component/HonoraryChamber/HonoraryChamberRankComponent");

	public UIToggle Toggle_Base;

	private IListener _listener;

	public UITableHorizontal TableInfo;

	public UISprite SpriteSelect;

	public UISpriteSelector SpriteSelectorMedal;

	public UITexture TextureCountry;

	public UILabel LabelRank;

	public UILabel LabelLv;

	public UILabel LabelName;

	private bool _changed;

	private bool _select;

	public RecordsOfHonorRankingInfo RankingInfo { get; private set; }

	public bool Select
	{
		get
		{
			return _select;
		}
		set
		{
			if (_select != value)
			{
				_select = value;
				_changed = true;
			}
		}
	}

	public void UpdateComponent()
	{
		if (_changed)
		{
			_changed = false;
			SetSelect(_select);
		}
	}

	public void Open(IListener listener, string name)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		UIHelper.SetLabelText(LabelName, name);
	}

	public void Open(IListener listener, RecordsOfHonorRankingInfo rankingInfo)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		if (rankingInfo == null)
		{
			VespaDebug.LogError("rankingInfo == null");
			return;
		}
		_listener = listener;
		RankingInfo = rankingInfo;
		SetRankerInfo(RankingInfo);
	}

	private void SetRankerInfo(RecordsOfHonorRankingInfo rankingInfo)
	{
		string text = (rankingInfo.Rank + 1).ToString();
		bool flag = SpriteSelectorMedal.Select(text);
		UIHelper.EnableGameObject(SpriteSelectorMedal, flag);
		UIHelper.SetLabelText(LabelRank, text);
		UIHelper.EnableGameObject(LabelRank, !flag);
		SetCountryTexture(rankingInfo);
		UIHelper.SetLabelText(LabelLv, "Lv." + rankingInfo.Level);
		UIHelper.SetLabelText(LabelName, rankingInfo.Name);
		TableInfo.repositionNow = true;
	}

	private void SetCountryTexture(RecordsOfHonorRankingInfo rankingInfo)
	{
		bool flag = rankingInfo.RankingType == RecordsOfHonorRankingType.World;
		UIHelper.EnableGameObject(TextureCountry, flag);
		if (flag)
		{
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(rankingInfo.CountryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, TextureCountry, plainTexture);
			}
		}
	}

	public void OnCreate()
	{
	}

	public void OnRelease()
	{
		UIHelper.ClearPlaneAssetTexture(TextureCountry);
	}

	private void SetSelect(bool select)
	{
		Toggle_Base.Set(select);
	}

	public void OnClick()
	{
		if (_listener != null && !(Toggle_Base == null))
		{
			bool value = Toggle_Base.value;
			UIHelper.EnableGameObject(SpriteSelect, value);
			if (value)
			{
				_listener.OnClick(this);
			}
		}
	}
}
