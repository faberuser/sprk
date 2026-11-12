using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NGameSetting;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUtil;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NodeBackground
{
	public RenderFacade NpcRenderFacade;

	public RenderFacade NpcRenderFacade2;

	private Mediator _mediator;

	private EntryMenuBackground _menuBackground = new EntryMenuBackground();

	private EntryMenuSettingData _menuAppearance;

	private OpenData _npcOpenData;

	private OpenData _npcOpenData2;

	public bool IsAssetLoadDone
	{
		get
		{
			if (_menuBackground.NguiPrefab.ISDONE() && _npcOpenData.ISDONE())
			{
				return _npcOpenData2.ISDONE();
			}
			return false;
		}
	}

	public bool IsShow { get; private set; }

	public void Load(EntryMenuSettingData menuAppearance)
	{
		if (_menuAppearance == menuAppearance)
		{
			return;
		}
		ReleaseGameAsset();
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI);
		_menuAppearance = menuAppearance;
		_menuBackground.Open(menuAppearance, layerInfo.Camera, layerInfo.Camera.transform);
		if (!String_Extension_Method.IsNullOrEmpty(_menuAppearance.NpcCode))
		{
			Builder builder = SingletonAuto<BuilderContainer>.instance.Get(_menuAppearance.NpcCode);
			if (builder != null)
			{
				CreatureStock creatureStock = GetCreatureStock(_menuAppearance.NpcCode, _menuAppearance.NpcStar, _menuAppearance.NpcPrefix);
				_npcOpenData = builder.CreateOpenData(creatureStock);
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(_menuAppearance.NpcCode2))
		{
			Builder builder2 = SingletonAuto<BuilderContainer>.instance.Get(_menuAppearance.NpcCode2);
			if (builder2 != null)
			{
				CreatureStock creatureStock2 = GetCreatureStock(_menuAppearance.NpcCode2, _menuAppearance.NpcStar2, _menuAppearance.NpcPrefix2);
				_npcOpenData2 = builder2.CreateOpenData(creatureStock2);
			}
		}
	}

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
	}

	public void Close()
	{
		ReleaseGameAsset();
		if (_mediator != null)
		{
			_mediator = null;
		}
	}

	public void ReleaseGameAsset()
	{
		IsShow = false;
		if (NpcRenderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(NpcRenderFacade);
			NpcRenderFacade = null;
		}
		if (_npcOpenData != null)
		{
			_npcOpenData.Close();
			_npcOpenData = null;
		}
		if (NpcRenderFacade2 != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(NpcRenderFacade2);
			NpcRenderFacade2 = null;
		}
		if (_npcOpenData2 != null)
		{
			_npcOpenData2.Close();
			_npcOpenData2 = null;
		}
		_menuBackground.Close();
		_menuAppearance = null;
	}

	private CreatureStock GetCreatureStock(string code, int star, string prefix)
	{
		return new CreatureStock
		{
			Code = code,
			Star = star,
			Prefix = prefix,
			LoadLevel = CreatureStock.ELoadLevel.IncludeFacial
		};
	}

	private RenderFacade GetNpcRenderFacade(int npcIndex = 1)
	{
		return npcIndex switch
		{
			1 => NpcRenderFacade, 
			2 => NpcRenderFacade2, 
			_ => null, 
		};
	}

	private void MotionNpcNormalStand(ref RenderFacade npcRenderFacade, bool blendMotion)
	{
		if (!(npcRenderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Normal");
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			npcRenderFacade.Action.Motion(stockData, blendMotion, null);
		}
	}

	public void MotionNpc(string motionName, bool isLoop = false)
	{
		if (!(NpcRenderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, motionName);
			stockData.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			NpcRenderFacade.Action.Motion(stockData, null);
		}
	}

	public void MotionNpcBuy()
	{
		if (!(NpcRenderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Buy");
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			NpcRenderFacade.Action.Motion(stockData, null);
		}
	}

	public void ChainUpdate()
	{
		_menuBackground.ChainUpdate();
		if (NpcRenderFacade != null && NpcRenderFacade.AssetCreature != null)
		{
			if (NpcRenderFacade.Action.IsActionEnd())
			{
				MotionNpcNormalStand(ref NpcRenderFacade, blendMotion: true);
			}
			NpcRenderFacade.transform.localPosition = new Vector3(_menuAppearance.NpcPositionX, _menuAppearance.NpcPositionY, _menuAppearance.NpcPositionZ);
			NpcRenderFacade.transform.localRotation = Quaternion.LookRotation(new Vector3(_menuAppearance.NpcDirectionX, _menuAppearance.NpcDirectionY, _menuAppearance.NpcDirectionZ));
			NpcRenderFacade.transform.localScale = Vector3.one * _menuAppearance.NpcScale;
			NpcRenderFacade.ChainUpdate();
		}
		if (NpcRenderFacade2 != null && NpcRenderFacade2.AssetCreature != null)
		{
			if (NpcRenderFacade2.Action.IsActionEnd())
			{
				MotionNpcNormalStand(ref NpcRenderFacade2, blendMotion: true);
			}
			NpcRenderFacade2.transform.localPosition = new Vector3(_menuAppearance.NpcPosition2X, _menuAppearance.NpcPosition2Y, _menuAppearance.NpcPosition2Z);
			NpcRenderFacade2.transform.localRotation = Quaternion.LookRotation(new Vector3(_menuAppearance.NpcDirection2X, _menuAppearance.NpcDirection2Y, _menuAppearance.NpcDirection2Z));
			NpcRenderFacade2.transform.localScale = Vector3.one * _menuAppearance.NpcScale2;
			NpcRenderFacade2.ChainUpdate();
		}
	}

	public ITask CreateShowTask(bool skipAnim)
	{
		SequenceTask sequenceTask = new SequenceTask();
		if (NpcRenderFacade != null)
		{
			sequenceTask.AddTask(delegate
			{
				NpcRenderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(0f);
			});
		}
		if (NpcRenderFacade2 != null)
		{
			sequenceTask.AddTask(delegate
			{
				NpcRenderFacade2.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(0f);
			});
		}
		sequenceTask.AddTask(delegate
		{
			IsShow = true;
		});
		sequenceTask.AddTask(_menuBackground.coShowNgui(skipAnim, reverse: false));
		sequenceTask.AddTask(delegate
		{
			CreateNpc(1, ref NpcRenderFacade);
			CreateNpc(2, ref NpcRenderFacade2);
		});
		sequenceTask.AddTask(delegate
		{
			VespaCoroutine.Start(coShowNpc(1, _npcOpenData, skipAnim));
			VespaCoroutine.Start(coShowNpc(2, _npcOpenData2, skipAnim));
		});
		return sequenceTask;
	}

	private void CreateNpc(int npcIndex, ref RenderFacade npcRenderFacade)
	{
		if (npcRenderFacade == null)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Camera.transform, TeamType.Left, _menuAppearance.GetNpcCode(npcIndex), _menuAppearance.GetNpcStar(npcIndex));
			renderFacadeBuilder.SetModelPrefix(_menuAppearance.GetNpcPrefix(npcIndex));
			renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
			npcRenderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
			MotionNpcNormalStand(ref npcRenderFacade, blendMotion: false);
		}
	}

	private Vector3 MakeVector3(float[] arr)
	{
		if (arr.Length >= 3)
		{
			return new Vector3(arr[0], arr[1], arr[2]);
		}
		return new Vector3(0f, 0f, 0f);
	}

	private IEnumerator coShowNpc(int npcIndex, OpenData npcOpenData, bool skipAnim)
	{
		if (npcOpenData == null)
		{
			yield break;
		}
		RenderFacade npcRenderFacade = GetNpcRenderFacade(npcIndex);
		if (npcRenderFacade == null)
		{
			yield break;
		}
		npcRenderFacade.transform.localPosition = MakeVector3(_menuAppearance.GetNpcPosition(npcIndex));
		npcRenderFacade.transform.localRotation = Quaternion.LookRotation(MakeVector3(_menuAppearance.GetNpcDirection(npcIndex)));
		npcRenderFacade.transform.localScale = Vector3.one * _menuAppearance.GetNpcScale(npcIndex);
		Vector3 from = MakeVector3(_menuAppearance.GetNpcPosition(npcIndex));
		from.x += _menuAppearance.GetNpcSlideOffset(npcIndex);
		Vector3 to = MakeVector3(_menuAppearance.GetNpcPosition(npcIndex));
		npcRenderFacade.transform.localPosition = from;
		npcRenderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(0f);
		npcRenderFacade.ChainUpdate();
		EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
		float elapsedTime = 0f;
		do
		{
			if (skipAnim)
			{
				elapsedTime += _menuAppearance.GetNpcSlideTimeLength(npcIndex);
			}
			else
			{
				yield return null;
				elapsedTime += Time.deltaTime;
			}
			float num = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(_menuAppearance.GetNpcSlideTimeLength(npcIndex)));
			float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(entryMenuSetting.NpcSlideCurve, num, num);
			npcRenderFacade.transform.localPosition = Vector3.Lerp(from, to, num2);
			npcRenderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(num2);
		}
		while (!(elapsedTime >= _menuAppearance.GetNpcSlideTimeLength(npcIndex)));
	}

	public ITask CreateHideTask()
	{
		if (!IsShow)
		{
			return null;
		}
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(delegate
		{
			IsShow = false;
		});
		ParallelTask parallelTask = new ParallelTask();
		parallelTask.AddTask(_menuBackground.coHideNgui());
		parallelTask.AddTask(coHideNpc(1));
		parallelTask.AddTask(coHideNpc(2));
		sequenceTask.AddTask(parallelTask);
		return sequenceTask;
	}

	private IEnumerator coHideNpc(int npcIndex, System.Action onComplete = null)
	{
		RenderFacade npcRenderFacade = GetNpcRenderFacade(npcIndex);
		if (npcRenderFacade == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / entryMenuSetting.BlendTimeLength);
			npcRenderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(1f - num);
		}
		while (!(elapsedTime >= entryMenuSetting.BlendTimeLength));
		RenderFacadeBuilder.ReleaseRenderFacade(npcRenderFacade);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}
}
