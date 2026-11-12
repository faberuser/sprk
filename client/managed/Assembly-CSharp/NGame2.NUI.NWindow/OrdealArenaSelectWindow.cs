using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NEventTrigger;
using NGame2.NOrdeal;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2.NOrdeal;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class OrdealArenaSelectWindow : BaseWindow
{
	public interface IListener : OrdealArenaTierSelectComponent.IListener
	{
		void OnClickRewardInfo();
	}

	private class TierDataCompare : IComparer<OrdealArenaTierData>
	{
		int IComparer<OrdealArenaTierData>.Compare(OrdealArenaTierData x, OrdealArenaTierData y)
		{
			return x.SortingOrder.CompareTo(y.SortingOrder);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaSelectWindow";

	public UISimpleGrid SimpleGrid;

	private OrdealArenaTierData[] cachedTierDatas;

	private List<OrdealArenaTierSelectComponent> _listComponent = new List<OrdealArenaTierSelectComponent>();

	private IListener _listener;

	private OrdealArenaTierSelectComponent _curSelectComponent;

	private bool _isSessionExperiod;

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(delegate
		{
			Init();
		}, delegate
		{
			SceneAutoBehaviour<OrdealArenaManagement>.instance.AddUpdateWindow(this, UpdateCheckSeasonTime);
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_listener = null;
		_curSelectComponent = null;
		_isSessionExperiod = false;
		ReleaseComp();
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listComponent);
	}

	private bool IsValidSeasonData()
	{
		if (!GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance)
		{
			return false;
		}
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.MatchSeasonData == null)
		{
			return false;
		}
		return true;
	}

	public void UpdateCheckSeasonTime()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return;
		}
		TimeSpan seasonRemainTime = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSeasonRemainTime();
		if (seasonRemainTime.TotalSeconds < 0.0)
		{
			if (!_isSessionExperiod)
			{
				_isSessionExperiod = true;
				string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_SETTLEUP_TITLE");
				string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_SETTLEUP_DESC");
				if (SceneAutoBehaviour<OrdealArenaManagement>.instance.IsOpened<OrdealArenaRewardInfoPopup>())
				{
					SceneAutoBehaviour<OrdealArenaManagement>.instance.GetInstance<OrdealArenaRewardInfoPopup>().Close();
				}
				GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
				MessagePopup.Open(stringDirect, stringDirect2, delegate
				{
					Close();
				});
			}
		}
		else if (_curSelectComponent != null)
		{
			_curSelectComponent.SetLabelDesc(seasonRemainTime);
		}
	}

	private void SetComp()
	{
		if (cachedTierDatas == null)
		{
			cachedTierDatas = GameRoot.Instance.OrdealArenaTierDataContainer.GetAllData().ToArray();
			Array.Sort(cachedTierDatas, new TierDataCompare());
		}
		MatchTierType maxMatchTierType = OrdealHelper.GetMaxMatchTierType();
		bool flag = GlobalAutoBehaviour<OrdealArenaManager>.instance.IsCompleteOrdeal();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(cachedTierDatas); i++)
		{
			OrdealArenaTierData ordealArenaTierData = cachedTierDatas[i];
			OrdealArenaTierSelectComponent ordealArenaTierSelectComponent = UIHelper.AddGridItem(SimpleGrid, OrdealArenaTierSelectComponent.PrefabAuto);
			if (ordealArenaTierSelectComponent == null)
			{
				break;
			}
			bool num = GlobalAutoBehaviour<OrdealArenaManager>.instance.IsSelctedTier();
			bool flag2 = false;
			if (num)
			{
				flag2 = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier() == ordealArenaTierData.MatchTierType;
				if (flag2)
				{
					_curSelectComponent = ordealArenaTierSelectComponent;
				}
			}
			else
			{
				flag2 = ordealArenaTierData.MatchTierType <= maxMatchTierType;
			}
			ordealArenaTierSelectComponent.Open(ordealArenaTierData.MatchTierType, _listener);
			ordealArenaTierSelectComponent.SetEnable(flag2);
			ordealArenaTierSelectComponent.SetDescLabel(ordealArenaTierData.DescKey, ordealArenaTierData.MatchTierType);
			ordealArenaTierSelectComponent.SetComplete(ordealArenaTierSelectComponent == _curSelectComponent && flag);
			_listComponent.Add(ordealArenaTierSelectComponent);
		}
		UIHelper.RepositionGrid(SimpleGrid);
	}

	private void Init()
	{
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenOrdealArenaUI()) && SceneAutoBehaviour<ContentsNoticePopupManagement>.isValidInstance)
		{
			ContentsNoticePopup instance = SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.GetInstance<ContentsNoticePopup>(openedOnly: true);
			if (instance != null && instance.IsOpen())
			{
				instance.Close();
			}
		}
		SetComp();
		UpdateCheckSeasonTime();
	}

	public void OnClickRewardPopup()
	{
		if (_listener != null)
		{
			_listener.OnClickRewardInfo();
		}
	}
}
