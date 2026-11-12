using System;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class SkillUpgradeMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SkillUpgradeMessagePopup";

	private Action<int> _onConfirm;

	private int _targetLevel;

	public UIButton ConfirmButton;

	public UILabel skillLevelLabel1;

	public UILabel skillLevelLabel2;

	public UILabel skillNameLabel1;

	public UILabel skillNameLabel2;

	public UILabel priceLabel;

	public UITexture skillIconTexture1;

	public UITexture skillIconTexture2;

	public static void Open(HeroInfo heroInfo, int skillSlotIndex, Action<int> onConfirm)
	{
		WindowPrefab<SkillUpgradeMessagePopup> windowPrefab = new WindowPrefab<SkillUpgradeMessagePopup>("Resources/Prefab2/Window/MessagePopup/SkillUpgradeMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, skillSlotIndex, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(HeroInfo heroInfo, int skillSlotIndex, Action<int> onConfirm)
	{
		base.Open();
		_onConfirm = onConfirm;
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(heroInfo, skillSlotIndex);
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(heroInfo, skillSlotIndex);
		if (skillDetailInfo.BasicInfo.TableData == null)
		{
			return;
		}
		AssetTexture skillIcon = UIHelper.GetSkillIcon2(skillDetailInfo.BasicInfo.TableIndex, heroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, skillIconTexture1, skillIcon);
		AssetTexture skillIcon2 = UIHelper.GetSkillIcon2(skillDetailInfo.BasicInfo.TableIndex, heroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, skillIconTexture2, skillIcon2);
		int num = 0;
		_targetLevel = skillBasicInfo.Level + 1;
		while (true)
		{
			if (_targetLevel > heroInfo.Level)
			{
				_targetLevel--;
				break;
			}
			SkillPriceData data = GameRoot.Instance.SkillPriceDataContainer.GetData(new SkillPriceKey(skillSlotIndex, _targetLevel - 1));
			if (data == null)
			{
				_targetLevel--;
				break;
			}
			if (num + data.ReqGold > GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold)
			{
				_targetLevel--;
				break;
			}
			num += data.ReqGold;
			_targetLevel++;
		}
		string skillName = SkillDescContainer.GetSkillName(heroInfo, skillBasicInfo.TableData);
		UIHelper.EnableButton(ConfirmButton, _targetLevel > skillBasicInfo.Level);
		UIHelper.SetLabelText(priceLabel, num);
		UIHelper.SetLabelText(skillLevelLabel1, "Lv " + skillBasicInfo.Level);
		UIHelper.SetLabelText(skillLevelLabel2, "Lv " + _targetLevel);
		UIHelper.SetLabelText(skillNameLabel1, skillName);
		UIHelper.SetLabelText(skillNameLabel2, skillName);
	}

	public void ClickCancel()
	{
		Close();
	}

	public void ClickConfirm()
	{
		Action<int> onConfirm = _onConfirm;
		Close();
		Vespa_System_Action_Extension_Method.fire(onConfirm, _targetLevel);
	}
}
