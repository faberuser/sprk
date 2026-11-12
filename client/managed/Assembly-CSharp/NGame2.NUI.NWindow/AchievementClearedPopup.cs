using NGame2.NAchievement;
using NGame2.NQuest;
using NGame2.NUI.NManager;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AchievementClearedPopup : BaseWindow
{
	public interface ITextProvider
	{
		string GetAchievementName(AchievementData achievementData);

		string GetAchievementDesc(AchievementData achievementData);
	}

	public interface Listener
	{
		void OnClickAchievementClearedPopup(int QuestIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Achievement/AchievementClearedPopup";

	public UIPanel Panel_Root;

	public UILabel Label_AchievementName;

	public UILabel Label_AchievementDesc;

	public UITexture Texture_AchievementType;

	public UISpriteSelector LogoSpriteSelector;

	public float OpenDuration;

	private float _openedTime;

	private Listener _listener;

	private AchievementType achievementType;

	private int _achievementIndex;

	public void Open(AchievementData achievementData, ITextProvider textProvider, Listener listener)
	{
	}

	public void Init(AchievementData achievementData, ITextProvider textProvider, Listener listener)
	{
		UpdateChangeLayer();
		_listener = listener;
		_openedTime = Time.time;
		achievementType = achievementData.Type;
		_achievementIndex = achievementData.Index;
		if (achievementType == AchievementType.Hero)
		{
			UIHelper.SetLabelText(Label_AchievementName, textProvider.GetAchievementName(achievementData));
			UIHelper.SetLabelText(Label_AchievementDesc, textProvider.GetAchievementDesc(achievementData));
		}
		else
		{
			string localizeMissionText = SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(achievementData.Title, achievementData.Variable1);
			UIHelper.SetLabelText(Label_AchievementName, localizeMissionText);
			string localizeMissionText2 = SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(achievementData.Goal, achievementData.Variable3);
			UIHelper.SetLabelText(Label_AchievementDesc, localizeMissionText2);
		}
		EnableLogoSpriteSelector();
		EnableAchievementTypeIcon(achievementData.SubType);
		if (Panel_Root != null)
		{
			Panel_Root.alpha = 1f;
		}
	}

	private void EnableLogoSpriteSelector()
	{
		if (!(LogoSpriteSelector == null))
		{
			bool enable = achievementType == AchievementType.Daily || achievementType == AchievementType.Weekly || achievementType == AchievementType.Limited;
			UIHelper.EnableGameObject(LogoSpriteSelector, enable);
		}
	}

	private void EnableAchievementTypeIcon(AchievementSubType subType)
	{
		if (!(Texture_AchievementType == null) && !(Texture_AchievementType.transform.parent == null))
		{
			bool flag = achievementType == AchievementType.General || achievementType == AchievementType.Hero;
			UIHelper.EnableGameObject(Texture_AchievementType.transform.parent, flag);
			if (flag)
			{
				AssetTexture achievementTypeIcon = UIHelper.GetAchievementTypeIcon(EnumTools.GetName(subType));
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AchievementType, achievementTypeIcon);
			}
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateChangeLayer();
		if (IsOpen() && Time.time - _openedTime > OpenDuration)
		{
			Close();
		}
	}

	private void UpdateChangeLayer()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
			if (base.gameObject.layer != layerInfo.Layer)
			{
				ChangeLayerRecursive(layerInfo);
			}
		}
		else
		{
			LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
			if (base.gameObject.layer != layerInfo2.Layer)
			{
				ChangeLayerRecursive(layerInfo2);
			}
		}
	}

	public void OnClickWindow()
	{
		SingletonAuto<AchievementNotiChecker>.instance.Clear();
		if (SceneAutoBehaviour<AchievementClearedPopupQueue>.isValidInstance)
		{
			SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.RemoveAllClearedPopup("Achievement");
		}
		if (_listener != null)
		{
			_listener.OnClickAchievementClearedPopup(_achievementIndex);
		}
		Close();
	}
}
