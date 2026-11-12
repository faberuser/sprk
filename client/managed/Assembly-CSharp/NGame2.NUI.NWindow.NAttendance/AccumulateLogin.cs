using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.GetLoginDailyReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAttendance;

public class AccumulateLogin : BaseWindow
{
	[Serializable]
	public class AccureLoginRewardUI
	{
		private string _formmat;

		public UILabel Label_Reward;

		public UISimpleGrid RewardGrid;

		private List<AttendanceRewardComponent> _componentList = new List<AttendanceRewardComponent>();

		private AccumulateLoginRewardData _info;

		public void SetInfo(string formmat, AccumulateLoginRewardData info)
		{
			_formmat = formmat;
			_info = info;
			Update();
		}

		private void Update()
		{
			Clear();
			if (_info == null)
			{
				UIHelper.SetLabelText(Label_Reward, string.Empty);
				return;
			}
			string stringDirect = LocalizationManager.GetStringDirect(_formmat, _info.Day);
			UIHelper.SetLabelText(Label_Reward, stringDirect);
			DbRewardData[] array = _info.GerRewardDataAry();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
			{
				if (array[i].Type != RewardType.None)
				{
					AttendanceRewardComponent attendanceRewardComponent = RewardGrid.AddGridItem(AttendanceRewardComponent.PrefabAccureLogin);
					attendanceRewardComponent.Init(array[i]);
					_componentList.Add(attendanceRewardComponent);
				}
			}
			RewardGrid.RepositionGrid();
		}

		public void Clear()
		{
			FactoryBase.ReleaseAndClearList(_componentList);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/AccumulateLogin/AccumulateLogin";

	private const int CURRENCT_REWARD_INDEX = 0;

	private const int NEXT_REWARD_INDEX = 1;

	public UILabel Label_Title;

	public UILabel Label_AccrueLoginTitle;

	public UIButton Button_RecvReward;

	public AccureLoginRewardUI[] RewardUIs;

	[Header("-- [닫기 버튼 (AttendanceIntegration에서 숨겨짐]")]
	public UIButton Button_Close;

	private Queue<AccumulateLoginRewardData> _queueRemainRewards = new Queue<AccumulateLoginRewardData>();

	private int _totalLoginDay;

	private AccumulateLoginRewardData _currentReward;

	public static string AccumlateLoginTypeName => typeof(AccumulateLogin).Name;

	public static string AccumlateLoginTabName => LocalizationManager.GetStringDirect("ACCUMULATE_LOGIN_TITLE");

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardUIs); i++)
		{
			RewardUIs[i].Clear();
		}
	}

	public void Open(int currentAccrueDay, bool is_in_AttendanceIntegration, Action onComplete = null)
	{
		base.Open(onComplete);
		_totalLoginDay = currentAccrueDay;
		Init();
		UpdateRewardQ();
		UpdateRewardInfo();
		UIHelper.EnableGameObject(Button_Close, !is_in_AttendanceIntegration);
	}

	private void UpdateRewardInfo()
	{
		if (__Extension_Method_ICollection__.COUNT(RewardUIs) != 2)
		{
			VespaDebug.LogError("Not Set Reward Uis / RewardUIs.COUNT() != 2");
			return;
		}
		_currentReward = DequeueRewardData();
		if (_currentReward != null && _currentReward.Day <= _totalLoginDay)
		{
			UIHelper.EnableButton(Button_RecvReward, enable: true);
		}
		else
		{
			UIHelper.EnableButton(Button_RecvReward, enable: false, grayscaleIfDisable: true);
		}
		SetRewardInfoUI("ACCUMULATE_LOGIN_ACHIEVED", RewardUIs[0], _currentReward);
		SetRewardInfoUI("ACCUMULATE_LOGIN_NEXT_DESC", RewardUIs[1], PeekRewardData());
	}

	private AccumulateLoginRewardData DequeueRewardData()
	{
		if (_queueRemainRewards.Count == 0)
		{
			return null;
		}
		return _queueRemainRewards.Dequeue();
	}

	private AccumulateLoginRewardData PeekRewardData()
	{
		if (_queueRemainRewards.Count == 0)
		{
			return null;
		}
		return _queueRemainRewards.Peek();
	}

	private void SetRewardInfoUI(string formmat, AccureLoginRewardUI ui, AccumulateLoginRewardData info)
	{
		if (ui == null)
		{
			VespaDebug.LogError("ui == null");
		}
		else
		{
			ui.SetInfo(formmat, info);
		}
	}

	private void Init()
	{
		SetTotalLoginLabel();
	}

	private void SetTotalLoginLabel()
	{
		UIHelper.SetLabelText(Label_AccrueLoginTitle, LocalizationManager.GetStringDirect("ACCUMULATE_LOGIN_TODAY", _totalLoginDay));
	}

	private bool IsRewarded(int accumulateRewardDataIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.AccumulateLoginManager.IsRewarded(accumulateRewardDataIndex);
	}

	private void UpdateRewardQ()
	{
		_queueRemainRewards.Clear();
		IList<AccumulateLoginRewardData> allData = GameRoot.Instance.AccumulateLoginRewardDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			if (!IsRewarded(allData[i].Index))
			{
				_queueRemainRewards.Enqueue(allData[i]);
			}
		}
		_queueRemainRewards.OrderBy((AccumulateLoginRewardData data) => data.Day);
	}

	public void OnClick_Close()
	{
		Close();
	}

	public void OnClickReward()
	{
		ReqRecvReward(delegate
		{
			UpdateRewardQ();
			UpdateRewardInfo();
		});
	}

	private void ReqRecvReward(Action OnSuccess)
	{
		NShared.GetLoginDailyReward.Request request = new NShared.GetLoginDailyReward.Request
		{
			LoginDailyIndex = _currentReward.Index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetLoginDailyReward.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.AccumulateLoginManager.Apply(response.LoginDailyInfo);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.LoginDailyRewardResult.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.LoginDailyRewardResult.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.LoginDailyRewardResult.EquipItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.LoginDailyRewardResult.HeroInfos, response.LoginDailyRewardResult.ExpResultInfos);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.LoginDailyRewardResult.StaminaResultInfos);
					GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.LoginDailyRewardResult.PetInfos);
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetCurrencyResultInfos3(response.LoginDailyRewardResult.CurrencyResults).SetItemResultInfos(response.LoginDailyRewardResult.ItemResults).SetEquipResultInfos(response.LoginDailyRewardResult.EquipItemResults)
						.SetStaminaResultInfos(response.LoginDailyRewardResult.StaminaResultInfos)
						.AddPetInfos(response.LoginDailyRewardResult.PetInfos), isGlobal: false);
					Vespa_System_Action_Extension_Method.fire(OnSuccess);
				}
			}
		});
	}

	public static bool RewardAble()
	{
		int loginDailyCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.LoginDailyCount;
		IList<AccumulateLoginRewardData> allData = GameRoot.Instance.AccumulateLoginRewardDataContainer.GetAllData();
		return GlobalAutoBehaviour<AccountManager>.instance.AccumulateLoginManager.ExistLoginReward(loginDailyCount, allData);
	}
}
