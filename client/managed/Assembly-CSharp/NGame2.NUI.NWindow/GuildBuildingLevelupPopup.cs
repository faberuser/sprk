using System.Collections;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildBuildingLevelupPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildBuildingLevelupPopup";

	public const string closeAnimState = "Anim_GuildConstruction_LevelUp_End";

	public Animator Animator;

	public UILabel Label_PrevLevel;

	public UILabel Label_Level;

	public UILabel Label_Name;

	public UITexture Texture_BuildingPortrait;

	public float OpenDuration;

	private float _openedTime;

	private GuildBuildingData _buildingData;

	private int _prevLevel;

	private int _level;

	private Coroutine _closeCo;

	public static void Open(int buildingDataIndex, int prevLevel, int level)
	{
		GuildBuildingData data = GameRoot.Instance.GuildBuildingDataContainer.GetData(buildingDataIndex, level);
		if (data == null)
		{
			VespaDebug.LogError("Index[" + buildingDataIndex + "],Level[" + level + "] data == null");
		}
		else
		{
			WindowPrefab<GuildBuildingLevelupPopup> windowPrefab = new WindowPrefab<GuildBuildingLevelupPopup>("Resources/Prefab2/Window/Guild/GuildBuildingLevelupPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(data, prevLevel, level);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public static void Open(GuildBuildingData buildingData)
	{
		WindowPrefab<GuildBuildingLevelupPopup> windowPrefab = new WindowPrefab<GuildBuildingLevelupPopup>("Resources/Prefab2/Window/Guild/GuildBuildingLevelupPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(buildingData, buildingData.Level - 1, buildingData.Level);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(GuildBuildingData buildingData, int prevLevel, int level)
	{
		if (buildingData != null)
		{
			Open(null, delegate
			{
				Init(buildingData, prevLevel, level);
			});
		}
	}

	private void Init(GuildBuildingData buildingData, int prevLevel, int level)
	{
		_closeCo = null;
		_openedTime = Time.time;
		_buildingData = buildingData;
		_prevLevel = prevLevel;
		_level = level;
		if (_buildingData == null)
		{
			VespaDebug.LogError("buildingData is null");
			return;
		}
		InitPortrait();
		SetLabel();
	}

	private void InitPortrait()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_buildingData.PortraitPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(_buildingData.PortraitPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_BuildingPortrait, plainTexture);
		}
	}

	private void SetLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_buildingData.NameKey);
		UIHelper.SetLabelText(Label_Name, stringDirect);
		UIHelper.SetLabelText(Label_PrevLevel, "Lv." + _prevLevel);
		UIHelper.SetLabelText(Label_Level, "Lv." + _level);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen() && Time.time - _openedTime > OpenDuration && _closeCo == null)
		{
			_closeCo = ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coClosePopup());
		}
	}

	public void OnClickWindow()
	{
		if (_closeCo == null)
		{
			_closeCo = ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coClosePopup());
		}
	}

	private IEnumerator coClosePopup()
	{
		Animator.Play("Anim_GuildConstruction_LevelUp_End");
		yield return null;
		while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
		{
			yield return null;
		}
		Close();
	}
}
