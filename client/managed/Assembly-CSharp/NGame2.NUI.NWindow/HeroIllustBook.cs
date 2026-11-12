using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroIllustBook : BaseWindow
{
	public interface Listener
	{
	}

	public enum IllustBookType
	{
		Normal,
		SeasonReward
	}

	private struct DragInfo
	{
		public int TouchId;

		public Vector2 CurrentPos;

		public Vector2 Delta;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/HeroIllustBook";

	public UILabel LabelHeroName;

	public UITexture HeroIllustTexture;

	public Transform IllustRoot;

	public UIWidget RootPanel;

	public UIWidget WidgetHideUI;

	public UIWidget WidgetHeroName;

	public UIWidget WidgetPrev;

	public UIWidget WidgetNext;

	public UIButton ButtonPrev;

	public UIButton ButtonNext;

	public TweenScale TweenScale;

	public TweenPosition TweenPosition;

	private Listener _listener;

	private CachedHeroInfo _heroInfo;

	private bool _dragging;

	private IncreaseDic<int, DragInfo> _dragDic = new IncreaseDic<int, DragInfo>();

	private CostumeData _costumeData;

	private int _currentCostumeArrayIndex;

	private List<int> _costumeIndexList = new List<int>();

	private List<CostumeData> _seasonRewardDataList = new List<CostumeData>();

	private int _scrollFactor;

	private int _scaleLimitMax;

	private int _scaleLimitMin;

	private IllustBookType _illustBookType;

	public CachedHeroInfo HeroInfo => _heroInfo;

	public void Init(Listener listener, CachedHeroInfo heroInfo, int costumeIndex, List<int> costumIndexList)
	{
		_listener = listener;
		_heroInfo = heroInfo;
		_costumeData = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		_currentCostumeArrayIndex = costumIndexList.IndexOf(costumeIndex);
		_costumeIndexList = costumIndexList;
		_illustBookType = IllustBookType.Normal;
		_scrollFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScrollFactor", 1);
		GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScaleLimitMax", 1);
		_scaleLimitMax = 8;
		_scaleLimitMin = GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScaleLimitMin", 1);
		SetupIllust();
		SetupHeroName();
		UpdateControlButtonState();
	}

	public void InitSeasonReward(Listener listener, int currentCostumeArrayIndex, List<CostumeData> _seasonRewardDatas)
	{
		_listener = listener;
		_heroInfo = null;
		_seasonRewardDataList = _seasonRewardDatas;
		_currentCostumeArrayIndex = currentCostumeArrayIndex;
		_costumeData = _seasonRewardDataList[_currentCostumeArrayIndex];
		_illustBookType = IllustBookType.SeasonReward;
		_scrollFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScrollFactor", 1);
		GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScaleLimitMax", 1);
		_scaleLimitMax = 8;
		_scaleLimitMin = GameRoot.Instance.ConstantDataContainer.GetValueInt("ArchiveHeroIllustViewerScaleLimitMin", 1);
		SetupIllust(_currentCostumeArrayIndex);
		SetupHeroName();
		UpdateControlButtonState();
	}

	public override void Close()
	{
		SprkIllustrationUpscale.Closing(this);
		UIHelper.ReleaseTextureInfo(HeroIllustTexture);
		base.Close();
	}

	public void SetupIllust(int arrayIndex = 0)
	{
		SprkIllustrationUpscale.BeforeChange(this);
		if (HeroIllustTexture != null)
		{
			UIHelper.ReleaseTextureInfo(HeroIllustTexture);
		}
		switch (_illustBookType)
		{
		case IllustBookType.Normal:
		{
			AssetTexture plainTexture2 = UIHelper.GetPlainTexture(_costumeData.IllustPath);
			SprkIllustrationUpscale.Assign(this, HeroIllustTexture, plainTexture2, UIHelper.EAssetTextureAssign.None, null);
			break;
		}
		case IllustBookType.SeasonReward:
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(_costumeData.IllustPath);
			SprkIllustrationUpscale.Assign(this, HeroIllustTexture, plainTexture, UIHelper.EAssetTextureAssign.None, null);
			break;
		}
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(IllustRoot);
	}

	public void SetupHeroName()
	{
		UIHelper.SetLabelText(LabelHeroName, _costumeData.CostumeName);
	}

	private void UpdateControlButtonState()
	{
		switch (_illustBookType)
		{
		case IllustBookType.Normal:
			UIHelper.EnableGameObject(ButtonPrev, _currentCostumeArrayIndex > 0);
			UIHelper.EnableGameObject(ButtonNext, _currentCostumeArrayIndex < __Extension_Method_ICollection__.COUNT(_costumeIndexList) - 1);
			break;
		case IllustBookType.SeasonReward:
			UIHelper.EnableGameObject(ButtonPrev, _currentCostumeArrayIndex > 0);
			UIHelper.EnableGameObject(ButtonNext, _currentCostumeArrayIndex < __Extension_Method_ICollection__.COUNT(_seasonRewardDataList) - 1);
			break;
		}
	}

	private CostumeData GetOwnedCostumeData(bool isNext)
	{
		int num = 0;
		bool flag = false;
		CostumeData costumeData = null;
		while (!flag)
		{
			if (isNext)
			{
				if (_currentCostumeArrayIndex >= _costumeIndexList.Count - 1)
				{
					return null;
				}
			}
			else if (_currentCostumeArrayIndex <= 0)
			{
				return null;
			}
			_currentCostumeArrayIndex += (isNext ? 1 : (-1));
			num = _costumeIndexList[_currentCostumeArrayIndex];
			costumeData = GameRoot.Instance.CostumeDataContainer.GetData(num);
			if (_costumeData.EquipCondition == EEquipCondition.NeedAwaken && _heroInfo.Star == 5)
			{
				flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) != null)
			{
				flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
			}
		}
		if (!flag)
		{
			return null;
		}
		return costumeData;
	}

	public void OnClickPrev()
	{
		switch (_illustBookType)
		{
		case IllustBookType.Normal:
			if (_currentCostumeArrayIndex > 0)
			{
				_currentCostumeArrayIndex--;
				int key = _costumeIndexList[_currentCostumeArrayIndex];
				_costumeData = GameRoot.Instance.CostumeDataContainer.GetData(key);
				SetupIllust();
				SetupHeroName();
				UpdateControlButtonState();
			}
			break;
		case IllustBookType.SeasonReward:
			Debug.Log("### IllustBookType.SeasonReard Button Prev");
			_currentCostumeArrayIndex--;
			if (_currentCostumeArrayIndex < 0)
			{
				_currentCostumeArrayIndex = 0;
			}
			_costumeData = _seasonRewardDataList[_currentCostumeArrayIndex];
			SetupIllust();
			SetupHeroName();
			UpdateControlButtonState();
			break;
		}
	}

	public void OnClickNext()
	{
		switch (_illustBookType)
		{
		case IllustBookType.Normal:
			if (_currentCostumeArrayIndex < _costumeIndexList.Count - 1)
			{
				_currentCostumeArrayIndex++;
				int key = _costumeIndexList[_currentCostumeArrayIndex];
				_costumeData = GameRoot.Instance.CostumeDataContainer.GetData(key);
				SetupIllust();
				SetupHeroName();
				UpdateControlButtonState();
			}
			break;
		case IllustBookType.SeasonReward:
			_currentCostumeArrayIndex++;
			if (_currentCostumeArrayIndex >= __Extension_Method_ICollection__.COUNT(_seasonRewardDataList))
			{
				_currentCostumeArrayIndex = __Extension_Method_ICollection__.COUNT(_seasonRewardDataList) - 1;
			}
			_costumeData = _seasonRewardDataList[_currentCostumeArrayIndex];
			SetupIllust();
			SetupHeroName();
			UpdateControlButtonState();
			break;
		}
	}

	public void OnClickHideUI()
	{
		VespaCoroutine.Start(coHideUI());
	}

	private IEnumerator coHideUI()
	{
		float elapsedTime = 0f;
		float animTime = 0.3f;
		while (true)
		{
			float num = Mathf.Lerp(1f, 0f, elapsedTime / animTime);
			WidgetHideUI.alpha = num;
			WidgetHeroName.alpha = num;
			WidgetPrev.alpha = num;
			WidgetNext.alpha = num;
			elapsedTime += Time.deltaTime;
			if (num <= 0f)
			{
				break;
			}
			yield return null;
		}
		UIHelper.EnableGameObject(WidgetHideUI, enable: false);
		UIHelper.EnableGameObject(WidgetHeroName, enable: false);
	}

	private IEnumerator coShowUI()
	{
		UIHelper.EnableGameObject(WidgetHideUI, enable: true);
		UIHelper.EnableGameObject(WidgetHeroName, enable: true);
		float elapsedTime = 0f;
		float animTime = 0.3f;
		while (true)
		{
			float num = Mathf.Lerp(0f, 1f, elapsedTime / animTime);
			WidgetHideUI.alpha = num;
			WidgetHeroName.alpha = num;
			WidgetPrev.alpha = num;
			WidgetNext.alpha = num;
			elapsedTime += Time.deltaTime;
			if (num >= 1f)
			{
				break;
			}
			yield return null;
		}
	}

	public void OnClickClose()
	{
		if (!UIHelper.IsEnabledGameObjet(WidgetHideUI))
		{
			VespaCoroutine.Start(coShowUI());
		}
		else
		{
			Close();
		}
	}

	public void StartDrag()
	{
		_dragging = true;
		DragInfo value = new DragInfo
		{
			TouchId = UICamera.currentTouchID,
			CurrentPos = UICamera.currentTouch.pos,
			Delta = UICamera.currentTouch.delta
		};
		_dragDic.AddAndOverride(value.TouchId, value);
	}

	public void OnDrag()
	{
		if (_dragging && _dragDic.ContainsKey(UICamera.currentTouchID))
		{
			DragInfo value = new DragInfo
			{
				TouchId = UICamera.currentTouchID,
				CurrentPos = UICamera.currentTouch.pos,
				Delta = UICamera.currentTouch.delta
			};
			_dragDic.AddAndOverride(value.TouchId, value);
		}
	}

	public void EndDrag()
	{
		_dragging = false;
		_dragDic.Clear();
	}

	public void OnDoubleClick()
	{
		TweenScale.enabled = true;
		TweenScale.from = IllustRoot.localScale;
		TweenScale.to = Vector3.one;
		TweenScale.PlayForwardReset();
		TweenPosition.enabled = true;
		TweenPosition.from = IllustRoot.localPosition;
		TweenPosition.to = Vector3.zero;
		TweenPosition.PlayForwardReset();
	}

	private void GetTranslateLimit(out int transX, out int transY)
	{
		Vector2 localSize = RootPanel.localSize;
		transX = (int)Mathf.Abs((IllustRoot.localScale.x * (float)HeroIllustTexture.width - localSize.x) / 2f);
		transY = (int)Mathf.Abs((IllustRoot.localScale.y * (float)HeroIllustTexture.height - localSize.y) / 2f);
	}

	private void SetIllustPosition(Vector3 position)
	{
		GetTranslateLimit(out var transX, out var transY);
		position.x = Mathf.Clamp(position.x, -transX, transX);
		position.y = Mathf.Clamp(position.y, -transY, transY);
		IllustRoot.localPosition = position;
	}

	private void DragIllust(Vector2 delta)
	{
		float num = RootPanel.localSize.x / (float)Screen.width;
		Vector3 localPosition = IllustRoot.localPosition;
		localPosition.x += delta.x * num * (float)_scrollFactor;
		localPosition.y += delta.y * num * (float)_scrollFactor;
		SetIllustPosition(localPosition);
	}

	private void SetZoomRate(float zoomRate)
	{
		Vector3 localScale = IllustRoot.localScale;
		zoomRate = Mathf.Clamp(zoomRate, _scaleLimitMin, _scaleLimitMax);
		localScale.x = (localScale.y = zoomRate);
		IllustRoot.localScale = localScale;
		Vector3 localPosition = IllustRoot.localPosition;
		SetIllustPosition(localPosition);
	}

	private void ZoomIllust(float zoomInValue)
	{
		Vector3 localScale = IllustRoot.localScale;
		float p = (0f - zoomInValue) / (float)Screen.height;
		p = localScale.x * Mathf.Pow(2f, p);
		SetZoomRate(p);
	}

	private float CalcZoomInValue(Vector2 delta, Vector2 position, Vector2 pivot)
	{
		if (delta.magnitude < Mathf.Epsilon)
		{
			return 0f;
		}
		if (Vector2.Dot(pivot - position, delta) < 0f)
		{
			return 0f - delta.magnitude;
		}
		return delta.magnitude;
	}

	public override void UpdateDerived()
	{
		SprkIllustrationZoom.Update(this);
		base.UpdateDerived();
		if (_dragging && _dragDic.Count != 0)
		{
			if (_dragDic.Count == 1)
			{
				DragIllust(_dragDic.GetValueByIndex(0).Delta);
			}
			else if (_dragDic.Count == 2)
			{
				DragInfo valueByIndex = _dragDic.GetValueByIndex(0);
				DragInfo valueByIndex2 = _dragDic.GetValueByIndex(1);
				float num = CalcZoomInValue(valueByIndex.Delta, valueByIndex.CurrentPos, valueByIndex2.CurrentPos);
				float num2 = CalcZoomInValue(valueByIndex2.Delta, valueByIndex2.CurrentPos, valueByIndex.CurrentPos);
				ZoomIllust(num + num2);
			}
			for (int i = 0; i < _dragDic.Count; i++)
			{
				DragInfo valueByIndex3 = _dragDic.GetValueByIndex(i);
				valueByIndex3.Delta = Vector2.zero;
				_dragDic.AddAndOverride(valueByIndex3.TouchId, valueByIndex3);
			}
		}
	}
}
