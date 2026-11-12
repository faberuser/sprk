using System.Text;
using NGame2.NAccount;
using NGame2.NOrdeal;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.EndOrdealArena;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaPvpResult : BaseWindow
{
	public interface IListener
	{
		void OnClickStatics();

		void OnClickLobby();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaPvpResult";

	public GameObject GoWin;

	public GameObject GoLose;

	public UILabel LabelMatchScore;

	public UILabel LabelAddMatchScore;

	public UILabel LabelLiveBonus;

	public UILabel LabelTimeBonus;

	public UILabel LabelHpBonus;

	public UILabel LabelPoint;

	public UIWidget IconProxyRoot;

	public UILabel LabelPointBonus;

	private IconProxy _iconProxy;

	private IListener _listener;

	private const string AddScoreFormat = "({0}{1})";

	public void Open(IListener listener, NShared.EndOrdealArena.Response res)
	{
		_listener = listener;
		Open(delegate
		{
			Init(res);
		}, null);
	}

	private void Init(NShared.EndOrdealArena.Response res)
	{
		bool win = res.ClearNodeIndices != null;
		bool reservedBuffSelectPopup = res.AreaRewardInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(res.SelectableBuffIndices);
		if (res.AreaRewardInfo != null)
		{
			OrdealHelper.OpenAreaRewardPopup(res.AreaRewardInfo, delegate
			{
				if (reservedBuffSelectPopup)
				{
					SceneAutoBehaviour<OrdealArenaManagement>.instance.OpenBuffSelectPopup();
				}
			});
		}
		if (!reservedBuffSelectPopup)
		{
			SceneAutoBehaviour<OrdealArenaManagement>.instance.OpenBuffSelectPopup();
		}
		int matchScore = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetMatchScore();
		SetScore(matchScore, res.AddMatchScore);
		SetPoint(res.CurrencyResult, res.HpPoint, res.SurvivePoint, res.TimePoint);
		SetWin(win);
		PlaySound(win);
	}

	private void SetScore(int score, int addScore)
	{
		UIHelper.SetLabelText(LabelMatchScore, score.ToString());
		StringBuilder stringBuilder = new StringBuilder();
		string arg = ((addScore >= 0) ? "+" : "");
		stringBuilder.AppendFormat("({0}{1})", arg, addScore);
		UIHelper.SetLabelText(LabelAddMatchScore, stringBuilder.ToString());
	}

	private void PlaySound(bool win)
	{
		if (win)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Campaign.Victory",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.VictoryBGM", isEvent: true);
		}
		else
		{
			SoundStock stock2 = new SoundStock
			{
				Code = "UI.Campaign.Lose",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock2, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.LoseBGM", isEvent: true);
		}
	}

	private void SetWin(bool isWin)
	{
		UIHelper.EnableGameObject(GoWin, isWin);
		UIHelper.EnableGameObject(GoLose, !isWin);
	}

	private IconProxy SetIconProxy()
	{
		if (IconProxyRoot == null)
		{
			return null;
		}
		if (_iconProxy == null)
		{
			_iconProxy = IconProxy.Create(IconProxyRoot);
		}
		if (_iconProxy != null)
		{
			_iconProxy.SetupCurrencyNoBg(CurrencyType.OrdealArenaPoint);
		}
		return _iconProxy;
	}

	private void SetPoint(CurrencyResultInfo3 currencyResult, int hpBonus, int liveBonus, int timeBouns)
	{
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.OrdealArenaPoint);
		UIHelper.SetLabelText(LabelPoint, Vespa_Long_Extension_Method.ToCommaString(userStat));
		SetIconProxy();
		long num = ((currencyResult != null && currencyResult.CurrencyType == CurrencyType.OrdealArenaPoint) ? currencyResult.AddValue : 0);
		UIHelper.SetLabelText(LabelPointBonus, Vespa_Long_Extension_Method.ToCommaString(num));
		UIHelper.SetLabelText(LabelHpBonus, $" + {Vespa_Int_Extension_Method.ToCommaString(hpBonus)}");
		UIHelper.SetLabelText(LabelLiveBonus, $" + {Vespa_Int_Extension_Method.ToCommaString(liveBonus)}");
		UIHelper.SetLabelText(LabelTimeBonus, $" + {Vespa_Int_Extension_Method.ToCommaString(timeBouns)}");
	}

	public void OnClickStatics()
	{
		if (_listener != null)
		{
			_listener.OnClickStatics();
		}
	}

	public void OnClickReturnLobby()
	{
		if (_listener != null)
		{
			_listener.OnClickLobby();
		}
	}
}
