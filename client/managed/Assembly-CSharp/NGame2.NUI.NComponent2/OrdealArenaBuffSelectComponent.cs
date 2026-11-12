using System;
using System.Collections.Generic;
using NGame2.NUI.NDescription;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class OrdealArenaBuffSelectComponent : LinkFactoryComponent, InitOnCreate
{
	[Serializable]
	public class GradeInfo
	{
		public int GradeIndex;

		public string GradeLocalKey;

		public Color GradeColor;
	}

	public interface Listener
	{
		void OnClickOrdealBuffComponent(OrdealArenaBuffSelectComponent data);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/OrdealArena/OrdealArenaBuffSelectComponent";

	public static PrefabAuto<OrdealArenaBuffSelectComponent> PrefabAuto = new PrefabAuto<OrdealArenaBuffSelectComponent>("Resources/Prefab2/Component/OrdealArena/OrdealArenaBuffSelectComponent");

	[Header("[Grade Infos]")]
	public List<GradeInfo> GradeInfoList = new List<GradeInfo>();

	[Header("[Basic UI]")]
	public UILabel GradeNameLabel;

	public UILabel BuffDescLabel;

	public Transform BuffIconPos;

	public UISprite SelectedSprite;

	public TweenAlpha TweenSelect;

	public TweenRotation TweenRot;

	public TweenAlpha TweenAlpha;

	public UIWidget CompAlphaWidget;

	private Listener _listener;

	private OrdealArenaBuffData _ordealArenaBuffdata;

	private OrdealArenaBuffIconComponent _buffIconComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _buffIconComponent);
	}

	public void Open(Listener listener, OrdealArenaBuffData data, float rotTweenDelay = 0f)
	{
		GradeInfo gradeInfo = GradeInfoList.Find((GradeInfo x) => x.GradeIndex == data.Grade);
		if (gradeInfo != null)
		{
			_listener = listener;
			_ordealArenaBuffdata = data;
			InitBuffGrade(gradeInfo);
			InitBuffDesc();
			InitBuffIcon();
			InitTweens(rotTweenDelay);
			SetSelected(isSelect: false);
		}
	}

	private void InitBuffGrade(GradeInfo gradeInfo)
	{
		string stringDirect = LocalizationManager.GetStringDirect(gradeInfo.GradeLocalKey);
		UIHelper.SetLabelText(GradeNameLabel, stringDirect);
		UIHelper.SetLabelColor(GradeNameLabel, gradeInfo.GradeColor);
	}

	private void InitBuffDesc()
	{
		OrdealArenaBuffData ordealArenaBuffdata = _ordealArenaBuffdata;
		string empty = string.Empty;
		if (ordealArenaBuffdata.OptionType != OperationType.UseSkill)
		{
			OperationType optionType = ordealArenaBuffdata.OptionType;
			int optionValue = ordealArenaBuffdata.OptionValue;
			empty = StatData.GetOptionDisplay(optionType, optionValue, opened: true, 0, appendColor: false);
		}
		else
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(ordealArenaBuffdata.OptionValue);
			if (data == null)
			{
				VespaDebug.LogError("OrdealArenaBuffSelectComponent Skill Data Not Found!!!");
				return;
			}
			empty = SkillDescContainer.GetSkillName(data);
		}
		UIHelper.SetLabelText(BuffDescLabel, empty);
	}

	private void InitBuffIcon()
	{
		if (_ordealArenaBuffdata != null)
		{
			if (_buffIconComponent == null)
			{
				_buffIconComponent = OrdealArenaBuffIconComponent.PrefabAuto.Factory.Create(BuffIconPos);
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_buffIconComponent.transform);
			_buffIconComponent.SetInfo(_ordealArenaBuffdata);
		}
	}

	private void InitTweens(float delay)
	{
		if (TweenRot != null)
		{
			base.transform.localEulerAngles = TweenRot.from;
			TweenRot.enabled = false;
			TweenRot.delay = delay;
			TweenRot.PlayForwardReset();
		}
		if (TweenSelect != null)
		{
			TweenSelect.enabled = false;
		}
		if (TweenAlpha != null)
		{
			TweenAlpha.enabled = false;
		}
		CompAlphaWidget.alpha = 1f;
	}

	public void StartFadeOut()
	{
		if (!(TweenAlpha == null))
		{
			TweenAlpha.PlayForwardReset();
		}
	}

	public OrdealArenaBuffData GetBuffData()
	{
		if (_ordealArenaBuffdata == null)
		{
			return null;
		}
		return _ordealArenaBuffdata;
	}

	public void SetSelected(bool isSelect)
	{
		UIHelper.EnableGameObject(SelectedSprite, isSelect);
		if (isSelect)
		{
			TweenSelect.PlayForwardReset();
		}
	}

	public void OnClickBuffComponent()
	{
		if (_listener != null && _ordealArenaBuffdata != null)
		{
			_listener.OnClickOrdealBuffComponent(this);
		}
	}
}
