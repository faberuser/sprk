using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MazeTowerComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickInfo(TowerData towerData);

		void OnClickEnter(TowerData towerData);
	}

	public static string PrefabPath = "Prefab2/Component/Tower/MazeTowerComponent";

	public static PrefabAuto<MazeTowerComponent> PrefabAuto = new PrefabAuto<MazeTowerComponent>(PrefabPath);

	public UILabel Label_Name;

	public UILabel Label_Category;

	public UIBundleTexSelector TexSelector;

	[HideInInspector]
	public TowerData _towerData;

	[HideInInspector]
	public MazeInfoData _mazeInfoData;

	private Listener _listener;

	public int DungeonIndex { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void Open(Listener listener, TowerData towerData)
	{
		_listener = listener;
		_towerData = towerData;
		_mazeInfoData = GameRoot.Instance.MazeInfoDataContainer.GetData(_towerData.TowerIndex);
		SetupTower();
	}

	private void SetupTower()
	{
		if (_towerData != null && _mazeInfoData != null)
		{
			SetCategoryTexture();
			_ = GlobalAutoBehaviour<AccountManager>.instance.UserManager.UndergroundPrisonKey;
			if (Label_Name != null)
			{
				Label_Name.applyGradient = true;
				UIHelper.SetWidgetColor(Label_Name, Color.white);
				UIHelper.SetLabelText(Label_Name, LocalizationManager.GetStringDirect(_mazeInfoData.TowerNameKey));
			}
			if (Label_Category != null)
			{
				UIHelper.SetLabelText(Label_Category, LocalizationManager.GetStringDirect("COMMON_REQ", LocalizationManager.GetString(_mazeInfoData.ClassType)));
			}
		}
	}

	private void SetCategoryTexture()
	{
		TexSelector.Select(_mazeInfoData.ClassType.ToString());
	}

	public void Close()
	{
		_listener = null;
		_towerData = null;
	}

	public void OnClickInfo()
	{
		if (_towerData != null && _listener != null)
		{
			_listener.OnClickInfo(_towerData);
		}
	}

	public void OnClickEnterTower()
	{
		if (_towerData != null && _listener != null)
		{
			_listener.OnClickEnter(_towerData);
		}
	}
}
