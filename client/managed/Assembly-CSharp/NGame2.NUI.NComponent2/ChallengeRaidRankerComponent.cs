using NGame2.NAccount;
using NGameAsset;
using NNguiExtension;
using NShared.NRaid;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class ChallengeRaidRankerComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/ChallengeRaidRankerComponent";

	public static PrefabAuto<ChallengeRaidRankerComponent> PrefabAuto = new PrefabAuto<ChallengeRaidRankerComponent>("Prefab2/Component/Raid/ChallengeRaidRankerComponent");

	public UITexture HeroTex;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel NameLabel;

	public UILabel PointLabel;

	public UILabel ClearTimeLabel;

	private ChallengeRaidRankerInfo rankerInfo;

	private Listener _Listener;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetData(ChallengeRaidRankerInfo rankerInfo, Listener listener)
	{
		this.rankerInfo = rankerInfo;
		_Listener = listener;
		SetHeroImage();
		SetRank(rankerInfo.Rank + 1, rankerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		UIHelper.SetLabelText(NameLabel, rankerInfo.Nick);
		string text = rankerInfo.Score.ToString();
		string elapseTimeLabelText = UIHelper.GetElapseTimeLabelText(rankerInfo.BattleTime);
		UIHelper.SetLabelText(PointLabel, text);
		UIHelper.SetLabelText(ClearTimeLabel, elapseTimeLabelText);
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
