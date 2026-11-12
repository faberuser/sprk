using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NPet;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetJukebox : RenewResourceWindow, PetJukeboxBGMListComponent.IListener
{
	public interface IListener
	{
		void OnClickBackButton();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetJukebox";

	public static LocalAccountString PetHouseBGM = new LocalAccountString("EntryMenu.PetHouseBGM", "PetHouseBGM");

	public const string NewBGMStrFormat = "PetHouse_NewBGM_{0}";

	public UIBundleLoader PlayingBGMTex;

	public UIScrollGrid BGMListGrid;

	public UILabel PlayingBGMTitle;

	private IListener _listener;

	private List<PetJukeboxBGMListComponent> _petJukeboxBGMListComponentList = new List<PetJukeboxBGMListComponent>();

	private JukeboxData _currentPlayingJukeboxData;

	public void Open(IListener listener)
	{
		_listener = listener;
		base.Open();
		Init();
		InitPlayingJukeboxData();
		InitBGMList();
		SetPlayingTitle();
		SetPlayingBgmTexture();
	}

	private void InitPlayingJukeboxData()
	{
		JukeboxData data = GameRoot.Instance.JukeboxDataContainer.GetData(PetHouseBGM.Value);
		_currentPlayingJukeboxData = data;
	}

	private void InitBGMList()
	{
		Vector3 targetChild = Vector3.zero;
		List<JukeboxData> jukeboxDatas = GameRoot.Instance.JukeboxDataContainer.GetJukeboxDatas();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(jukeboxDatas); i++)
		{
			JukeboxData jukeboxData = jukeboxDatas[i];
			if (jukeboxData != null)
			{
				PetJukeboxBGMListComponent petJukeboxBGMListComponent = UIHelper.AddGridItem(BGMListGrid, PetJukeboxBGMListComponent.PrefabAuto);
				petJukeboxBGMListComponent.Open(this, jukeboxData, isNewBGM(jukeboxData.Code));
				petJukeboxBGMListComponent.FixPanelsDepth(100);
				_petJukeboxBGMListComponentList.Add(petJukeboxBGMListComponent);
			}
		}
		UIHelper.RepositionGrid(BGMListGrid);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_petJukeboxBGMListComponentList); j++)
		{
			PetJukeboxBGMListComponent petJukeboxBGMListComponent2 = _petJukeboxBGMListComponentList[j];
			if (PetHouseBGM.Value == petJukeboxBGMListComponent2.JukeboxCode)
			{
				petJukeboxBGMListComponent2.SelectToggle(isSelect: true, noEvent: true);
				targetChild = petJukeboxBGMListComponent2.transform.localPosition;
				break;
			}
		}
		UIHelper.MoveGridPositionToSelectedItem(BGMListGrid.Grid, targetChild);
		BGMListGrid.ScrollView.RestrictWithinBounds(instant: true);
	}

	private bool isNewBGM(string bgmCode)
	{
		if (bgmCode == PetHouseBGM.Value)
		{
			return false;
		}
		return new LocalAccountBool(i_default: true, $"PetHouse_NewBGM_{bgmCode}").Value;
	}

	private void SetPlayingTitle()
	{
		if (_currentPlayingJukeboxData != null)
		{
			UIHelper.SetLabelText(PlayingBGMTitle, LocalizationManager.GetStringDirect(_currentPlayingJukeboxData.Title));
		}
	}

	private void SetPlayingBgmTexture()
	{
		if (_currentPlayingJukeboxData != null)
		{
			UIHelper.SetBundleTexture(PlayingBGMTex, GetJukeboxPortraitPath(_currentPlayingJukeboxData.PortraitPath));
		}
	}

	public static string GetJukeboxPortraitPath(string path)
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return null;
		}
		return $"JukeBox/{path}";
	}

	public override void Reset()
	{
		base.Reset();
		ClearJukeboxBGMCompList();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		PayShopResourceList.ClearAll();
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		if (_listener != null)
		{
			_listener.OnClickBackButton();
		}
	}

	private void ClearJukeboxBGMCompList()
	{
		_currentPlayingJukeboxData = null;
		FactoryBase.ReleaseAndClearList(_petJukeboxBGMListComponentList);
	}

	void PetJukeboxBGMListComponent.IListener.OnClick(JukeboxData jukeboxData)
	{
		if (jukeboxData != null || !(jukeboxData.Code == PetHouseBGM.Value))
		{
			_currentPlayingJukeboxData = jukeboxData;
			PetHouseBGM.Value = _currentPlayingJukeboxData.Code;
			SetPlayingBgmTexture();
			SingletonAuto<BgmManager2>.instance.StopEvent(PetHouseBGM.Value);
			SingletonAuto<BgmManager2>.instance.Request(_currentPlayingJukeboxData.Code, isEvent: true);
			SetPlayingTitle();
			SceneAutoBehaviour<PetManagement>.instance.PlayPetHouseNodeTree(_currentPlayingJukeboxData.EffectCode, null);
		}
	}
}
