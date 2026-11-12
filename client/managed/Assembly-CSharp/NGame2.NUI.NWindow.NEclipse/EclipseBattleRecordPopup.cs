using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NEclipse;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEclipse;

public class EclipseBattleRecordPopup : BaseWindow, EclipseBattleRecordComponent.Listener
{
	public interface IListener
	{
		void ShowEclipseClearWaveReward(int deckIndex, int waveIndex);
	}

	public const string PrefabPath = "Prefab2/Window/Eclipse/Popup/EclipseBattleRecordPopup";

	public UIScrollGrid EclipseBattleRecordScrollGrid;

	private IListener _iListener;

	private List<EclipseBattleRecordComponent> _eclipseBattleRecordComponentList = new List<EclipseBattleRecordComponent>();

	public static void Open(IListener iListener)
	{
		EclipseBattleRecordPopup eclipseBattleRecordPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<EclipseBattleRecordPopup>();
		if (eclipseBattleRecordPopup != null)
		{
			eclipseBattleRecordPopup.CompleteAnimationWithoutCallback();
			eclipseBattleRecordPopup.OpenInternal(iListener);
			eclipseBattleRecordPopup.PlayWindowOpenAnimation();
		}
		else
		{
			WindowPrefab<EclipseBattleRecordPopup> windowPrefab = new WindowPrefab<EclipseBattleRecordPopup>("Prefab2/Window/Eclipse/Popup/EclipseBattleRecordPopup", GetCurrentLayerType());
			windowPrefab.instance.OpenInternal(iListener);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	private void OpenInternal(IListener iListener)
	{
		base.Open(delegate
		{
			Init(iListener);
		});
	}

	private static GameUIRoot.ELayerType GetCurrentLayerType()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			return GameUIRoot.ELayerType.UI3DUI;
		}
		return GameUIRoot.ELayerType.UI;
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		SetBattleRecordList();
	}

	public override void Close()
	{
		base.Close();
		FactoryBase.ReleaseAndClearList(_eclipseBattleRecordComponentList);
	}

	private void SetBattleRecordList()
	{
		FactoryBase.ReleaseAndClearList(_eclipseBattleRecordComponentList);
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(savedDeckInfos))
		{
			return;
		}
		EclipseBattleRecordComponent eclipseBattleRecordComponent = null;
		EclipseDeckResult eclipseDeckResult = null;
		for (int i = 0; i < savedDeckInfos.Length; i++)
		{
			eclipseDeckResult = savedDeckInfos[i];
			if (eclipseDeckResult != null)
			{
				eclipseBattleRecordComponent = UIHelper.AddGridItem(EclipseBattleRecordScrollGrid, EclipseBattleRecordComponent.PrefabAuto);
				if (!(eclipseBattleRecordComponent == null))
				{
					eclipseBattleRecordComponent.Open(this, eclipseDeckResult);
					_eclipseBattleRecordComponentList.Add(eclipseBattleRecordComponent);
				}
			}
		}
		UIHelper.RepositionGrid(EclipseBattleRecordScrollGrid);
	}

	void EclipseBattleRecordComponent.Listener.ShowReward(int deckIndex, int waveIndex)
	{
		if (_iListener != null)
		{
			_iListener.ShowEclipseClearWaveReward(deckIndex, waveIndex);
		}
	}
}
