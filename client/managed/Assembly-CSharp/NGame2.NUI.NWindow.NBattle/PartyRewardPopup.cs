using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NBattle;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PartyRewardPopup : BaseRewardPopup
{
	public class PartyPopupOpenData : PopupOpenData
	{
		public int ChapterIndex;

		public int DungeonIndex;

		public override int CalcRewardCount()
		{
			int num = 0;
			if (base.multiplayMemberResults == null)
			{
				return num;
			}
			for (int i = 0; i < base.multiplayMemberResults.Length; i++)
			{
				MultiplayMemberResultInfo multiplayMemberResultInfo = base.multiplayMemberResults[i];
				num += GetRewardItemResultInfoCount(multiplayMemberResultInfo.ItemResults);
				num += __Extension_Method_ICollection__.COUNT(multiplayMemberResultInfo.EquipItemInfos);
			}
			return num;
		}
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Battle/PartyRewardPopup";

	public PartyRewardPopupBinder PartyRewardPopupBinder;

	public UIGrid PartyGrid;

	public UISprite SpriteLine;

	private List<UIScrollGrid> memberRewardGridList = new List<UIScrollGrid>();

	protected override void SetupRewardItems()
	{
		if (!(_popupOpen is PartyPopupOpenData { multiplayMemberResults: not null } partyPopupOpenData))
		{
			return;
		}
		for (int i = 0; i < partyPopupOpenData.multiplayMemberResults.Length; i++)
		{
			GameObject obj = Object.Instantiate(PartyRewardPopupBinder.gameObject);
			obj.transform.parent = PartyGrid.transform;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(obj.transform);
			UIHelper.EnableGameObject(obj, enable: true);
			PartyRewardPopupBinder component = obj.GetComponent<PartyRewardPopupBinder>();
			MultiplayMemberResultInfo multiplayMemberResultInfo = partyPopupOpenData.multiplayMemberResults[i];
			long accountId = multiplayMemberResultInfo.AccountId;
			int memberIndex = multiplayMemberResultInfo.MemberIndex;
			string nick = multiplayMemberResultInfo.Nick;
			bool flag = accountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
			int num = 0;
			PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(partyPopupOpenData.ChapterIndex, partyPopupOpenData.DungeonIndex);
			if (data != null)
			{
				num = (multiplayMemberResultInfo.UseStamina ? data.IncreaseRatio : (-data.DecreaseRatio));
			}
			UIHelper.SetLabelText(component.PlayerNoLabel, nick);
			UIHelper.EnableGameObject(component.BonusRewardPercentageLabel, num != 0);
			UIHelper.SetLabelText(component.BonusRewardPercentageLabel, ((num > 0) ? "+" : "") + num + "%");
			RewardItemGrid = component.MemberRewardGrid;
			partyPopupOpenData.SetBoostEquipResultInfos(flag ? multiplayMemberResultInfo.EquipItemBoostInfos : null);
			SetupRewardItems(multiplayMemberResultInfo.ItemResults, displayInvenCount: false, displayBoosterInfo: false);
			SetupRewardItems(multiplayMemberResultInfo.EquipItemInfos, displayBoostInfo: false);
			SetupRewardItems(multiplayMemberResultInfo.CurrencyResults, isSum: true);
			memberRewardGridList.Add(RewardItemGrid);
			UIHelper.ResetGridPosition(PartyGrid);
		}
		SetLineSize(_popupOpen.multiplayMemberResults.Length);
	}

	private void SetLineSize(int count)
	{
		SpriteLine.height += (int)PartyGrid.cellHeight * (count - 1);
	}

	public override void Reset()
	{
		base.Reset();
		ClearMemberRewardGridList();
	}

	private void ClearMemberRewardGridList()
	{
		memberRewardGridList.ForEach(delegate(UIScrollGrid e)
		{
			VespaObject.DestroyGo(e);
		});
		memberRewardGridList.Clear();
	}

	protected override void SetupRewardItems(CurrencyResultInfo3[] results, bool isSum)
	{
		if (results == null || __Extension_Method_ICollection__.COUNT(results) == 0)
		{
			return;
		}
		string text = "";
		int num = 0;
		long num2 = num;
		Dictionary<CurrencyType, Tuple<long, string, int>> dictionary = new Dictionary<CurrencyType, Tuple<long, string, int>>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(results); i++)
		{
			CurrencyResultInfo3 currencyResultInfo = results[i];
			checked
			{
				if (currencyResultInfo != null && currencyResultInfo.AddValue > 0)
				{
					int num3 = 0;
					num2 = num3;
					text = "";
					GetFieldValueByCurrencyType(currencyResultInfo.CurrencyType, currencyResultInfo, out text, out num2);
					if (!isSum)
					{
						SetupRewardItemComponent(currencyResultInfo.AddValue, currencyResultInfo.CurrencyType, text, num2);
					}
					else if (!dictionary.ContainsKey(currencyResultInfo.CurrencyType))
					{
						CurrencyType currencyType = currencyResultInfo.CurrencyType;
						long addValue = currencyResultInfo.AddValue;
						string i_item = text;
						long num4 = num2;
						dictionary.Add(currencyType, new Tuple<long, string, int>(addValue, i_item, (int)num4));
					}
					else
					{
						Tuple<long, string, int> tuple = dictionary[currencyResultInfo.CurrencyType];
						tuple.Item1 += currencyResultInfo.AddValue;
						dictionary[currencyResultInfo.CurrencyType] = tuple;
					}
				}
			}
		}
		if (!isSum)
		{
			return;
		}
		foreach (KeyValuePair<CurrencyType, Tuple<long, string, int>> item3 in dictionary)
		{
			CurrencyType key = item3.Key;
			Tuple<long, string, int> value = item3.Value;
			long value2 = value.Item1;
			string item = value.Item2;
			int item2 = value.Item3;
			SetupRewardItemComponent(value2, key, item, item2);
		}
	}
}
