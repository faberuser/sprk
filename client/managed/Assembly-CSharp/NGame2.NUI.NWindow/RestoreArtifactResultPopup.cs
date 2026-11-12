using System;
using System.Collections;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RestoreArtifactResultPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Artifact/RestoreArtifactResultPopup";

	public Transform ArtifactIconRoot;

	public UILabel ArtifactNameLabel;

	public UILabel ArtifactDescLabel;

	public UIWidget OptionRootWidget;

	public GameObject Root;

	private ItemIconComponent _artifactIconComponent;

	private Action<EquipItemInfo> _onEquip;

	private StatListComponent _statListComponent;

	private ItemAnimationContainer _getItemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	public EquipItemInfo ArtifactItemInfo { get; private set; }

	public ItemData<EquipItemData> ArtifactItemData { get; private set; }

	public static void Open(EquipItemInfo artifactItemInfo, Action<EquipItemInfo> onEquip)
	{
		WindowPrefab<RestoreArtifactResultPopup> windowPrefab = new WindowPrefab<RestoreArtifactResultPopup>("Resources/Prefab2/Window/Artifact/RestoreArtifactResultPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(artifactItemInfo, onEquip);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(EquipItemInfo artifactItemInfo, Action<EquipItemInfo> onEquip)
	{
		if (artifactItemInfo != null)
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(artifactItemInfo.ItemIndex);
			if (data == null)
			{
				VespaDebug.LogError("EquipItemData not found : index=" + artifactItemInfo.ItemIndex);
				return;
			}
			base.Open();
			ArtifactItemInfo = artifactItemInfo;
			ArtifactItemData = data;
			_onEquip = onEquip;
			Setup();
			VespaCoroutine.Start(EnableRoot());
		}
	}

	private IEnumerator EnableRoot()
	{
		yield return new WaitForSeconds(4.5f);
		if (Root != null)
		{
			UIHelper.EnableGameObject(Root, enable: true);
		}
		yield return null;
	}

	public override void Reset()
	{
		base.Reset();
		_getItemAnimationContainer.Reset();
		FactoryBase.SafeReleaseComponent(ref _artifactIconComponent);
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
		_onEquip = null;
		if (Root != null)
		{
			UIHelper.EnableGameObject(Root, enable: false);
		}
	}

	private void Setup()
	{
		if (ArtifactItemData != null)
		{
			UIHelper.SetLabelText(ArtifactNameLabel, ArtifactItemData.Name);
			UIHelper.SetLabelText(ArtifactDescLabel, ArtifactItemData.Description);
			_artifactIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ArtifactIconRoot);
			_artifactIconComponent.OpenWithDelay(ArtifactItemData, null, isShowInvenItemCount: false, 4.5f);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_artifactIconComponent.transform);
			_getItemAnimationContainer.Reset();
			_getItemAnimationContainer.Add(_artifactIconComponent.CreateGetAnimation());
			_statListComponent = StatListComponent.Create(OptionRootWidget, 5);
			StatListComponent.AddOptionStat(_statListComponent, ArtifactItemData.SubData, ArtifactItemInfo, addSubject: false);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_getItemAnimationContainer.ChainUpdate();
	}

	public void OnClickEquipArtifact()
	{
		Vespa_System_Action_Extension_Method.fire(_onEquip, ArtifactItemInfo);
		Close();
	}
}
