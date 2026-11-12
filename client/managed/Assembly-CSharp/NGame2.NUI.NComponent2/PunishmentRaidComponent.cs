using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentRaidComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickEnterButton(PunishmentRaidComponent Selected);

		void OnClickInfoButton(PunishmentRaidComponent Selected);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/PunishmentRaidComponent";

	public static PrefabAuto<PunishmentRaidComponent> PrefabAuto = new PrefabAuto<PunishmentRaidComponent>("Prefab2/Component/Raid/PunishmentRaidComponent");

	public Transform ComponentRoot;

	public GameObject AffixBoardRoot;

	public UIButton Button_Info;

	public UITexture Texture_Entry;

	public UILabel Label_EntryName;

	public Transform OpenRoot;

	public Transform ClosedRoot;

	private Listener _listener;

	private PunishmentRaidStatePanel _boardComponent;

	private int _groupIndex;

	public PunishmentRaidData PunishmentRaidData { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		PunishmentRaidData = null;
		FactoryBase.SafeReleaseComponent(ref _boardComponent);
	}

	public void Open(PunishmentRaidData punishmentRaidData, Listener listener, int groupIndex, bool isOpen = true)
	{
		_listener = listener;
		PunishmentRaidData = punishmentRaidData;
		_groupIndex = groupIndex;
		SetPos();
		SetComp();
		SetAffixBoard();
		RefreshComp();
	}

	private void SetPos()
	{
		Vector3 localPosition = base.gameObject.transform.localPosition;
		localPosition.x = PunishmentRaidData.EntryPositionX;
		localPosition.y = PunishmentRaidData.EntryPositionY;
		localPosition.z = 0f;
		base.gameObject.transform.localPosition = localPosition;
	}

	private void SetComp()
	{
		if (PunishmentRaidData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(PunishmentRaidData.EntryLocalKey);
			UIHelper.SetLabelText(Label_EntryName, stringDirect);
		}
	}

	public void SetAffixBoard()
	{
		bool isBoss = PunishmentRaidData.IsBoss;
		UIHelper.EnableGameObject(AffixBoardRoot, isBoss);
		if (isBoss && _boardComponent == null)
		{
			_boardComponent = PunishmentRaidStatePanel.PrefabAuto.Factory.Create(AffixBoardRoot.transform);
			_boardComponent.Open(PunishmentRaidData.RaidIndex, PunishmentRaidData.RaidLevel, AffixBoardRoot);
		}
	}

	public void RefreshComp()
	{
		bool num = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.IsOpenPunishmentRaid(_groupIndex, EPunishmentDungeonType.Boss);
		PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(_groupIndex, PunishmentRaidData.RaidIndex, PunishmentRaidData.RaidLevel);
		bool flag = punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime);
		bool flag2 = num && !flag;
		UIHelper.EnableGameObject(OpenRoot, flag2);
		UIHelper.EnableGameObject(ClosedRoot, !flag2);
		if (!(_boardComponent == null))
		{
			_boardComponent.Refresh();
		}
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnterButton(this);
		}
	}

	public void OnClickInfoButton()
	{
		if (_listener != null)
		{
			_listener.OnClickInfoButton(this);
		}
	}
}
