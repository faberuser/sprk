using System.Collections.Generic;
using NGame2.NUI;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class BaseBattleChattingEmoticon : LinkFactoryComponent
{
	public enum FrameDirectionType
	{
		BottomLeft,
		BottomRight,
		BottomMiddle,
		SideRight
	}

	public const string PrefabPath = "";

	public const int DefualtPanelDepth = 0;

	public static List<int> PanelDepthList = new List<int>();

	[SerializeField]
	protected UIPanel _panel;

	public UISpriteSelector UISpriteSelectorFrame;

	public TweenScale TweenScaleEmoticon;

	public UIBundleLoader EmoticonLoader;

	public GameObject RootEmoticon;

	protected float _elapsedTime;

	protected float _restoreTime = 3f;

	protected int _startDepth;

	public virtual bool IsChangeDepth => true;

	public static void ReleaseInstance(BaseBattleChattingEmoticon instance)
	{
		if (!(instance == null))
		{
			FactoryBase.ReleaseComponent(instance);
		}
	}

	public void Show(EmoticonData emoticonData, FrameDirectionType frameDirectionType)
	{
		Show(emoticonData);
		SetFrameDirection(frameDirectionType);
	}

	public void Show(EmoticonData emoticonData)
	{
		if (emoticonData != null)
		{
			SetPanelDepth();
			string assetPath = emoticonData.AssetPath;
			UIHelper.EnableGameObject(RootEmoticon, enable: true);
			UIHelper.SetBundleTexture(EmoticonLoader, assetPath);
			StartTween();
			_elapsedTime = 0f;
		}
	}

	public void SetFrameDirection(FrameDirectionType frameDirectionType)
	{
		if (!(UISpriteSelectorFrame == null))
		{
			UISpriteSelectorFrame.Select(frameDirectionType.ToString());
		}
	}

	public void StartTween()
	{
		if (!(TweenScaleEmoticon == null))
		{
			TweenScaleEmoticon.onFinished.Clear();
			TweenScaleEmoticon.ResetToBeginning();
			TweenScaleEmoticon.PlayForward();
		}
	}

	public void EndTween()
	{
		if (!(TweenScaleEmoticon == null))
		{
			TweenScaleEmoticon.PlayReverse();
			TweenScaleEmoticon.AddOnFinished(delegate
			{
				Close();
			});
		}
	}

	public int GetMaxPanelDepth()
	{
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PanelDepthList); i++)
		{
			int num2 = PanelDepthList[i];
			if (num < num2)
			{
				num = num2;
			}
		}
		if (num < 0)
		{
			return 0;
		}
		return num;
	}

	public void SetPanelDepth()
	{
		if (IsChangeDepth)
		{
			ResetPanelDepth();
			int num = GetMaxPanelDepth() + 10;
			PanelDepthList.Add(num);
			_panel.depth = num;
			_startDepth = num;
		}
	}

	public void ResetPanelDepth()
	{
		if (IsChangeDepth)
		{
			PanelDepthList.Remove(_startDepth);
			_startDepth = 0;
		}
	}

	public void UpdateTexture()
	{
		if (CheckUpdateElasedTime())
		{
			EndTween();
		}
	}

	public void Close()
	{
		ResetPanelDepth();
		UIHelper.EnableGameObject(RootEmoticon, enable: false);
	}

	public virtual void ChainUpdate()
	{
		UpdateTexture();
	}

	public virtual bool CheckUpdateElasedTime()
	{
		if (!(EmoticonLoader != null))
		{
			if (RootEmoticon != null)
			{
				return RootEmoticon.activeSelf;
			}
			return false;
		}
		return true;
	}
}
