using NGame2.NAccount;
using NGameAsset;
using NNguiExtension;
using NShared.NWorldBoss;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class WorldBossRankerComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/WorldBoss/WorldBossRankerComponent";

	public static PrefabAuto<WorldBossRankerComponent> PrefabAuto = new PrefabAuto<WorldBossRankerComponent>("Prefab2/Component/WorldBoss/WorldBossRankerComponent");

	public UITexture HeroTex;

	public UITexture HeroBattleTex;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel NameLabel;

	public UILabel PointLabel;

	private WorldBossRankerInfo rankerInfo;

	private Listener _Listener;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetData(WorldBossRankerInfo rankerInfo, Listener listener)
	{
		this.rankerInfo = rankerInfo;
		_Listener = listener;
		SetHeroImage();
		SetRank(rankerInfo.Rank + 1, rankerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		UIHelper.SetLabelText(NameLabel, rankerInfo.Nick);
		UIHelper.SetLabelText(PointLabel, rankerInfo.Score.ToString("#,0"));
	}

	private void SetHeroImage()
	{
		string iconPath = UIAvatarHelper.GetIconPath(rankerInfo.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, HeroTex, plainTexture);
		}
	}

	private void SetRank(int rank, bool isMe)
	{
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabel_Me, enable: false);
			UIHelper.EnableGameObject(RankLabel_Other, enable: false);
		}
		else
		{
			if (isMe)
			{
				UIHelper.SetLabelText(RankLabel_Me, rank);
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, rank);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, isMe);
			UIHelper.EnableGameObject(RankLabel_Other, !isMe);
		}
	}

	public void OnClickComponent()
	{
		if (_Listener != null)
		{
			_Listener.OnClickComponent(rankerInfo.AccountId);
		}
	}
}
