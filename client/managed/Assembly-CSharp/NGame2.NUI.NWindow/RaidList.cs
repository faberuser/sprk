using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidList : BaseWindow, RaidListComponent.Listener, IUserObserver<NShared.RaidInfo>, IUserObserver, LobbyTopmostMenu.IUserStatObserver, LobbyTopmostMenu.IMiniGuideButtonObserver, LobbyTopmostMenu.ICustomStaminaObserver
{
	public interface Listener
	{
		void OnClickEnter(RaidList caller, RaidListComponent component);

		void OnClickViewInfo(RaidList caller, RaidListComponent component);

		void OnClickRaidRoomList(RaidList caller, RaidListComponent component);

		void OnClickDispatch(RaidList caller, RaidListComponent component);
	}

	[Serializable]
	public class LocalizeLabel
	{
		[Serializable]
		public class RaidLocalizeInfo
		{
			public RaidType RaidType;

			public LocalizeLinkDataInner Localize;
		}

		public UILabel Label;

		public List<RaidLocalizeInfo> LocalizeList = new List<RaidLocalizeInfo>();

		public void Localize(RaidType type)
		{
			UIHelper.SetLabelText(Label, string.Empty);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(LocalizeList))
			{
				RaidLocalizeInfo raidLocalizeInfo = LocalizeList.Find((RaidLocalizeInfo info) => info.RaidType == type);
				if (raidLocalizeInfo != null && raidLocalizeInfo.Localize != null)
				{
					UIHelper.SetLabelText(Label, raidLocalizeInfo.Localize.GetLocalize());
				}
			}
		}
	}

	[Serializable]
	public class WindowGuideInfo
	{
		public RaidType RaidType;

		public int WindowGuideIndex;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidList";

	public List<WindowGuideInfo> WindowGuideInfoList = new List<WindowGuideInfo>();

	public LocalizeLabel TitleLabel = new LocalizeLabel();

	public LocalizeLabel DescLabel = new LocalizeLabel();

	public UIButton HardModeButton;

	public UIButton NormalModeButton;

	public UIScrollGrid RaidGrid;

	public Transform RaidDeco;

	public Transform TechnoMagicRaidDeco;

	private Listener _listener;

	private List<RaidListComponent> _componentList = new List<RaidListComponent>();

	public new int WindowGuideIndex
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(WindowGuideInfoList))
			{
				return 0;
			}
			return WindowGuideInfoList.Find((WindowGuideInfo info) => info.RaidType == RaidType)?.WindowGuideIndex ?? 0;
		}
	}

	public UserManager.UserStatType StatType => UserManager.UserStatType.RaidPoint;

	private RaidType RaidType { get; set; }

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => StaminaType;

	private StaminaType StaminaType
	{
		get
		{
			if (RaidType == RaidType.TechnoEnchant || RaidType == RaidType.TechnoEnchantSingle)
			{
				return StaminaType.TechnoEnchantKey;
			}
			return Stamina;
		}
	}

	public void Open(Listener listener, RaidType mode = RaidType.DragonNormal)
	{
		base.Open();
		_listener = listener;
		Init(mode);
	}

	private void Init(RaidType mode)
	{
		SwitchMode(mode);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void InitList(RaidType raidType)
	{
		ClearList();
		RaidType = raidType;
		ICollection<int> raidIndices = GameRoot.Instance.RaidDataContainer.GetRaidIndices(raidType);
		if (raidIndices == null)
		{
			return;
		}
		foreach (int item in raidIndices)
		{
			int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(item);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(item, raidMinLevel);
			if (data != null && (!Define.IsLiveBuild() || data.IsOpen))
			{
				RaidListComponent raidListComponent = UIHelper.AddGridItem(RaidGrid, RaidListComponent.PrefabAuto);
				raidListComponent.Open(item, this);
				_componentList.Add(raidListComponent);
			}
		}
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
			{
				UIHelper.RefreshComponent(_componentList[i]);
			}
		});
		UIHelper.RepositionGrid(RaidGrid);
	}

	public void SwitchHardMode()
	{
		SwitchMode(RaidType.DragonHard);
	}

	public void SwitchNormalMode()
	{
		SwitchMode(RaidType.DragonNormal);
	}

	private void SwitchMode(RaidType raidType)
	{
		InitList(raidType);
		TitleLabel.Localize(raidType);
		DescLabel.Localize(raidType);
		UIHelper.EnableGameObject(HardModeButton, raidType == RaidType.DragonNormal);
		UIHelper.EnableGameObject(NormalModeButton, raidType == RaidType.DragonHard);
		bool flag = raidType == RaidType.TechnoMagic || raidType == RaidType.TechnoEnchant;
		UIHelper.EnableGameObject(RaidDeco, !flag);
		UIHelper.EnableGameObject(TechnoMagicRaidDeco, flag);
	}

	void IUserObserver<NShared.RaidInfo>.OnChanged(NShared.RaidInfo changedInfo)
	{
		if (changedInfo != null)
		{
			RaidListComponent raidListComponent = _componentList.FirstOrDefault((RaidListComponent e) => e.RaidIndex == changedInfo.RaidIndex);
			if ((bool)raidListComponent)
			{
				raidListComponent.Init(changedInfo.RaidIndex);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void RaidListComponent.Listener.OnClickEnter(RaidListComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this, caller);
		}
	}

	void RaidListComponent.Listener.OnClickViewInfo(RaidListComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickViewInfo(this, caller);
		}
	}

	void RaidListComponent.Listener.OnClickRaidRoomList(RaidListComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickRaidRoomList(this, caller);
		}
	}

	void RaidListComponent.Listener.OnClickDispatch(RaidListComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickDispatch(this, caller);
		}
	}
}
