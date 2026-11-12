using NNguiExtension;
using NShared.NRaid;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChallengeRaidMultiRankerComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickChallenge(MultiPlayChallengeRaidRankerInfo rankerInfo);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/ChallengeRaidMultiRankerComponent";

	public static PrefabAuto<ChallengeRaidMultiRankerComponent> PrefabAuto = new PrefabAuto<ChallengeRaidMultiRankerComponent>("Prefab2/Component/Raid/ChallengeRaidMultiRankerComponent");

	public UISpriteSelector RankSpriteSelector;

	public UILabel NameLabel;

	public UILabel PointLabel;

	public UILabel ClearTimeLabel;

	private MultiPlayChallengeRaidRankerInfo rankerInfo;

	private Listener _listener;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void SetData(MultiPlayChallengeRaidRankerInfo rankerInfo, Listener listener)
	{
		this.rankerInfo = rankerInfo;
		_listener = listener;
		SetRank(rankerInfo.Rank + 1);
		string text = string.Empty;
		RankerInfo[] rankerInfos = rankerInfo.RankerInfos;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rankerInfos); i++)
		{
			RankerInfo rankerInfo2 = rankerInfos[i];
			text = (String_Extension_Method.IsNullOrEmpty(text) ? rankerInfo2.Nick : $"{text}\n{rankerInfo2.Nick}");
		}
		UIHelper.EnableGameObject(NameLabel, !String_Extension_Method.IsNullOrEmpty(text));
		UIHelper.SetLabelText(NameLabel, text);
		string text2 = rankerInfo.Score.ToString();
		string elapseTimeLabelText = UIHelper.GetElapseTimeLabelText(rankerInfo.BattleTime);
		UIHelper.SetLabelText(PointLabel, text2);
		UIHelper.SetLabelText(ClearTimeLabel, elapseTimeLabelText);
	}

	private void SetRank(int rank)
	{
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
		}
	}

	public void OnClickChallenge()
	{
		if (_listener != null)
		{
			_listener.OnClickChallenge(rankerInfo);
		}
	}
}
