using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ClearMissionTabComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickTab(ClearMissionProductData productData);
	}

	public const string PrefabPath = "Prefab2/Component/ClearMission/ClearMissionTabComponent";

	public static PrefabAuto<ClearMissionTabComponent> PrefabAuto = new PrefabAuto<ClearMissionTabComponent>("Prefab2/Component/ClearMission/ClearMissionTabComponent");

	public UIToggle ToggleTabType;

	public UILabel LabelTabName;

	public UISprite SpriteCanReceiveReward;

	public UISprite SpriteNew;

	private IListener _iListener;

	public ClearMissionProductData ProductData { get; private set; }

	public void Init(IListener listener, ClearMissionProductData productData)
	{
		_iListener = listener;
		ProductData = productData;
		string stringDirect = LocalizationManager.GetStringDirect(productData.MissionName);
		UIHelper.SetLabelText(LabelTabName, stringDirect);
		ShowNew(show: false);
		ShowCanReceiveRewardExist(show: false);
		SelectToggle(0);
	}

	public void ShowNew(bool show)
	{
		UIHelper.EnableGameObject(SpriteNew, show);
	}

	public void ShowCanReceiveRewardExist(bool show)
	{
		UIHelper.EnableGameObject(SpriteCanReceiveReward, show);
	}

	void InitOnCreate.OnCreate()
	{
		UIHelper.SetLabelText(LabelTabName, string.Empty);
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetLabelText(LabelTabName, string.Empty);
	}

	public void OnClickToggle()
	{
		if (!(ToggleTabType == null) && ToggleTabType.value && ProductData != null && _iListener != null)
		{
			_iListener.OnClickTab(ProductData);
		}
	}

	public void SelectToggle(int productIndex)
	{
		bool state = ProductData.Index == productIndex;
		ToggleTabType.Set(state);
	}

	public bool IsSelectedTab()
	{
		return ToggleTabType.value;
	}
}
