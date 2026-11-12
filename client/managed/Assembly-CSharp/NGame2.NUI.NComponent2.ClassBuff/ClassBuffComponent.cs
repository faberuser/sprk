using NGame2.NUI.NDescription;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NWindow.ClassBuff;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ClassBuff;

public class ClassBuffComponent : UIScrollItemBehaviour
{
	public new interface IListener
	{
		void OpenLevelUpPopup(ClassBuffData data);
	}

	public const string PrefabPath = "Prefab2/Component/ClassBuff/ClassBuffComponent";

	public static PrefabAuto<ClassBuffComponent> PrefabAuto = new PrefabAuto<ClassBuffComponent>("Prefab2/Component/ClassBuff/ClassBuffComponent");

	private const int DefaultLevelUp = 1;

	public UILabel StatDescriptionLabel;

	public UILabel NeedPointLabel;

	public UILabelAligner NeedLabelAligner;

	public UILabel SeparatorLabel;

	public UILabel NeedClassBuffLabel;

	public UILabel NeedHeroCountLabel;

	public UIButton LevelUpButton;

	public Transform IconComponentTransform;

	private ClassBuffIconComponent _iconComponent;

	private ClassBuffWrapper _wrapper;

	private IListener _iListener;

	private ClassBuffData _classBuffData;

	public int ClassBuffIndex { get; set; }

	public void Open(IListener iListener, ClassBuffData data)
	{
		Init(iListener, data);
	}

	private void Init(IListener iListener, ClassBuffData data)
	{
		_wrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		_iListener = iListener;
		_classBuffData = data;
		ClassBuffIndex = _classBuffData.ClassBuffIndex;
		SetClassBuffInfo();
		SetLevelUpButtonStatus();
	}

	private void SetClassBuffInfo()
	{
		SetStatDescription();
		SetNeedPoint();
		SetNeedClassBuff();
		SetNeedHeroCount();
		SetIconComponent();
	}

	private void SetIconComponent()
	{
		if (!(IconComponentTransform == null))
		{
			if (_iconComponent == null)
			{
				_iconComponent = ClassBuffIconComponent.PrefabAuto.Factory.Create(IconComponentTransform);
				_iconComponent.Open(_classBuffData);
			}
			else
			{
				_iconComponent.SetInfo(_classBuffData);
			}
		}
	}

	private void SetStatDescription()
	{
		if (!(StatDescriptionLabel == null) && _wrapper != null && _classBuffData != null)
		{
			if (_wrapper.GetSkillData(_classBuffData.SkillIndex) == null)
			{
				UIHelper.SetLabelText(StatDescriptionLabel, "-");
				return;
			}
			string classBuffDesc = ClassBuffMainInfo.GetClassBuffDesc(_classBuffData);
			UIHelper.SetLabelText(StatDescriptionLabel, classBuffDesc);
		}
	}

	private void SetNeedPoint()
	{
		if (NeedPointLabel == null || _wrapper == null)
		{
			return;
		}
		UIHelper.SetLabelText(NeedPointLabel, "");
		ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
		if (levelUpClassBuffData == null)
		{
			UIHelper.SetLabelText(NeedPointLabel, ChangeRedColor(_wrapper.LocalizClassBuffMaxLevel));
			return;
		}
		string text = "";
		long needPoint = levelUpClassBuffData.NeedPoint;
		if (needPoint > 0)
		{
			text = LocalizationManager.GetStringDirect(SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper.LocalizeClassBuffNeedPointLevelUp, needPoint);
			UIHelper.SetLabelText(NeedPointLabel, ChangDefaultColor(text));
			if (!_wrapper.CheckNeedPoint(levelUpClassBuffData))
			{
				UIHelper.SetLabelText(NeedPointLabel, ChangeRedColor(text));
			}
		}
	}

	private void SetNeedClassBuff()
	{
		if (NeedClassBuffLabel == null)
		{
			return;
		}
		UIHelper.SetLabelText(NeedClassBuffLabel, "");
		ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
		if (levelUpClassBuffData == null || levelUpClassBuffData.NeedClassBuffIndex <= 0)
		{
			return;
		}
		ClassBuffData needClassBuffData = GetNeedClassBuffData(levelUpClassBuffData.NeedClassBuffIndex, levelUpClassBuffData.NeedClassBuffLevel);
		if (needClassBuffData == null)
		{
			return;
		}
		SkillData skillData = _wrapper.GetSkillData(needClassBuffData.SkillIndex);
		if (skillData != null)
		{
			string skillName = SkillDescContainer.GetSkillName(skillData);
			string stringDirect = LocalizationManager.GetStringDirect(SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper.LocalizeClassBuffNeedSkillLevelUp, skillName, levelUpClassBuffData.NeedClassBuffLevel);
			UIHelper.SetLabelText(NeedClassBuffLabel, ChangDefaultColor(stringDirect));
			if (_wrapper != null && !_wrapper.CheckNeedClassBuff(levelUpClassBuffData))
			{
				UIHelper.SetLabelText(NeedClassBuffLabel, ChangeRedColor(stringDirect));
				Align();
			}
		}
	}

	private void SetSeparator(string text)
	{
		if (!(SeparatorLabel == null))
		{
			UIHelper.SetLabelText(SeparatorLabel, text);
		}
	}

	private void SetNeedHeroCount()
	{
		SetSeparator("");
		if (NeedHeroCountLabel == null)
		{
			return;
		}
		UIHelper.SetLabelText(NeedHeroCountLabel, "");
		ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
		if (levelUpClassBuffData != null && _wrapper != null && levelUpClassBuffData.NeedHeroCount > 0)
		{
			SetSeparator("/");
			string localizeClassBuffNeedHeroCount = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper.LocalizeClassBuffNeedHeroCount;
			string text = LocalizationManager.GetString(levelUpClassBuffData.TagType);
			string stringDirect = LocalizationManager.GetStringDirect(localizeClassBuffNeedHeroCount, text, levelUpClassBuffData.NeedHeroCount);
			UIHelper.SetLabelText(NeedHeroCountLabel, ChangDefaultColor(stringDirect));
			if (_wrapper != null && !_wrapper.CheckNeedHeroCount(levelUpClassBuffData.TagType, levelUpClassBuffData.NeedHeroCount))
			{
				UIHelper.SetLabelText(NeedHeroCountLabel, ChangeRedColor(stringDirect));
				Align();
			}
		}
	}

	private void Align()
	{
		if (!(NeedLabelAligner == null))
		{
			NeedLabelAligner.Align();
		}
	}

	private string ChangDefaultColor(string text)
	{
		return "[543D25]" + text + "[-]";
	}

	private string ChangeRedColor(string text)
	{
		return "[FF3B3B]" + text + "[-]";
	}

	private void SetLevelUpButtonStatus()
	{
		if (LevelUpButton == null)
		{
			return;
		}
		UIHelper.EnableButton(LevelUpButton, enable: false, grayscaleIfDisable: true);
		if (_classBuffData == null)
		{
			return;
		}
		ClassBuffData levelUpClassBuffData = GetLevelUpClassBuffData();
		if (levelUpClassBuffData != null)
		{
			bool flag = false;
			if (_wrapper != null)
			{
				flag = _wrapper.CheckPossibleLevelUp(levelUpClassBuffData);
				UIHelper.EnableButton(LevelUpButton, flag, grayscaleIfDisable: true);
			}
		}
	}

	public void PlayLevelUpEffect()
	{
		if (!(_iconComponent == null))
		{
			_iconComponent.PlayLevelUpEffect();
		}
	}

	private ClassBuffData GetLevelUpClassBuffData()
	{
		return GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(_classBuffData.TagType, _classBuffData.ClassBuffIndex, _classBuffData.ClassBuffLevel + 1);
	}

	private ClassBuffData GetNeedClassBuffData(int index, int level)
	{
		return GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(_classBuffData.TagType, index, level);
	}

	public void OnClickLevelUp()
	{
		if (_iListener != null)
		{
			_iListener.OpenLevelUpPopup(_classBuffData);
		}
	}
}
