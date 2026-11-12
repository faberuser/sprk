using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class SkillIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Icon/SkillIconComponent";

	public static PrefabAuto<SkillIconComponent> PrefabAuto = new PrefabAuto<SkillIconComponent>("Resources/Prefab2/Component/Icon/SkillIconComponent");

	public UIWidget WidgetThis;

	public UIBundleLoader uiBundleLoader;

	private SkillData _skillData;

	private SkillTooltipTextInfo _TooltipTextInfo;

	private string iconPath = string.Empty;

	public void Open(int skillIndex)
	{
		_skillData = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (_skillData != null)
		{
			string skillIconPath = UIHelper.GetSkillIconPath(_skillData.Index, null);
			iconPath = skillIconPath;
			SetIcon(uiBundleLoader, iconPath);
		}
	}

	public void SetIcon(UIBundleLoader loader, string path)
	{
		if (loader != null && !String_Extension_Method.IsNullOrEmpty(path))
		{
			UIHelper.SetBundleTexture(loader, path);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void OnShowTooltip()
	{
		StateIconTooltipTextInfo stateIconTooltipTextInfo = new StateIconTooltipTextInfo();
		stateIconTooltipTextInfo.Name.Value = LocalizationManager.GetStringDirect(SkillDescContainer.GetSkillName(_skillData));
		stateIconTooltipTextInfo.Desc.Value = LocalizationManager.GetStringDirect(_skillData.Description);
		stateIconTooltipTextInfo.Desc1.Value = LocalizationManager.GetStringDirect(_skillData.Description);
		StateIconTooltip.Open(WidgetThis, stateIconTooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		StateIconTooltip.Hide(WidgetThis);
	}
}
