using System.Collections.Generic;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NUI.NRender3D;

public class MotionSequence
{
	public interface IDataProvider
	{
		RenderFacade GetRenderFacade();
	}

	private IDataProvider _dataProvider;

	private int _currentSequence;

	private int _currentMotion;

	private float _elapsedEndDelay;

	private bool _initialized;

	private WeakRef<RenderFacade> _facadeRef = new WeakRef<RenderFacade>();

	private List<MotionSequenceData> _sequenceList = new List<MotionSequenceData>();

	public MotionSequence(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
	}

	public MotionSequenceData[] GetMotionSequenceDatas()
	{
		return _sequenceList.ToArray();
	}

	private List<MotionSequenceData> GetMotionSequenceList()
	{
		if (_facadeRef.Value != _dataProvider.GetRenderFacade())
		{
			_facadeRef.Value = _dataProvider.GetRenderFacade();
			_sequenceList.Clear();
			_initialized = false;
		}
		if (_initialized)
		{
			return _sequenceList;
		}
		if (_facadeRef.Value == null)
		{
			return _sequenceList;
		}
		if (!_facadeRef.Value.AssetCreatureAsync.IsDone)
		{
			return _sequenceList;
		}
		if (!_facadeRef.Value.AssetCreatureAsync.IsValid)
		{
			return _sequenceList;
		}
		_initialized = true;
		_sequenceList.Add(new MotionSequenceData(DefineCreature.EMotion.Idle, DefineCreature.EMotionEnd.Loop));
		_sequenceList.Add(new MotionSequenceData(DefineCreature.EMotion.BattleIdle, DefineCreature.EMotionEnd.Loop));
		_sequenceList.Add(new MotionSequenceData(DefineCreature.EMotion.Run, DefineCreature.EMotionEnd.Loop));
		_sequenceList.Add(new MotionSequenceData(DefineCreature.EMotion.Die, DefineCreature.EMotionEnd.Infinite));
		_sequenceList.Add(new MotionSequenceData(DefineCreature.EMotion.Stun, DefineCreature.EMotionEnd.Loop));
		MotionSequenceData motionSequenceData = new MotionSequenceData();
		motionSequenceData.MotionList.Add(new MotionSequenceData.MotionData
		{
			Key = new AnimationKeyData(DefineCreature.EMotion.Damage, string.Empty),
			MotionEnd = DefineCreature.EMotionEnd.End
		});
		motionSequenceData.MotionList.Add(new MotionSequenceData.MotionData
		{
			Key = new AnimationKeyData(DefineCreature.EMotion.BattleIdle, string.Empty),
			MotionEnd = DefineCreature.EMotionEnd.Loop,
			BlendMotion = true
		});
		_sequenceList.Add(motionSequenceData);
		MotionSequenceData motionSequenceData2 = new MotionSequenceData();
		motionSequenceData2.MotionList.Add(new MotionSequenceData.MotionData
		{
			Key = new AnimationKeyData(DefineCreature.EMotion.Down, string.Empty),
			MotionEnd = DefineCreature.EMotionEnd.End,
			EndDelay = 1f
		});
		motionSequenceData2.MotionList.Add(new MotionSequenceData.MotionData
		{
			Key = new AnimationKeyData(DefineCreature.EMotion.Getup, string.Empty),
			MotionEnd = DefineCreature.EMotionEnd.End,
			BlendMotion = true
		});
		motionSequenceData2.MotionList.Add(new MotionSequenceData.MotionData
		{
			Key = new AnimationKeyData(DefineCreature.EMotion.BattleIdle, string.Empty),
			MotionEnd = DefineCreature.EMotionEnd.Loop,
			BlendMotion = true
		});
		_sequenceList.Add(motionSequenceData2);
		ICreatureOpenData creatureOpenData = _facadeRef.Value.AssetCreature.GetCreatureOpenData();
		List<MotionSequenceData> motionSequenceList = creatureOpenData.GetMotionSequenceList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(motionSequenceList))
		{
			_sequenceList.AddRange(motionSequenceList);
		}
		for (int i = 0; i < creatureOpenData.GetAnimationCount(); i++)
		{
			AnimationKeyData key = creatureOpenData.GetAnimationData(i).GetKey();
			if (key.MotionType == DefineCreature.EMotion.EmotiMotion)
			{
				MotionSequenceData motionSequenceData3 = new MotionSequenceData();
				motionSequenceData3.MotionList.Add(new MotionSequenceData.MotionData
				{
					Key = key,
					MotionEnd = DefineCreature.EMotionEnd.End
				});
				motionSequenceData3.MotionList.Add(new MotionSequenceData.MotionData
				{
					Key = new AnimationKeyData(DefineCreature.EMotion.Idle, string.Empty),
					MotionEnd = DefineCreature.EMotionEnd.Loop,
					BlendMotion = true
				});
				_sequenceList.Add(motionSequenceData3);
			}
		}
		ValidateMotionSequence();
		_currentSequence = 0;
		_currentMotion = 0;
		return _sequenceList;
	}

	private void ValidateMotionSequence()
	{
		if (_facadeRef.Value == null || !_facadeRef.Value.AssetCreatureAsync.IsValid)
		{
			_sequenceList.Clear();
			return;
		}
		for (int i = 0; i < _sequenceList.Count; i++)
		{
			MotionSequenceData motionSequenceData = _sequenceList[i];
			bool flag = false;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(motionSequenceData.MotionList))
			{
				flag = true;
			}
			else
			{
				for (int j = 0; j < motionSequenceData.MotionList.Count; j++)
				{
					MotionSequenceData.MotionData motionData = motionSequenceData.MotionList[j];
					if (_facadeRef.Value.AssetCreature.GetAnimationData(motionData.Key) == null)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				_sequenceList.RemoveAt(i);
				i--;
			}
		}
	}

	public void Play(int sequenceIndex)
	{
		_currentSequence = sequenceIndex;
		PlayMotion();
	}

	public void PlayNext()
	{
		List<MotionSequenceData> motionSequenceList = GetMotionSequenceList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(motionSequenceList))
		{
			_currentSequence = (_currentSequence + 1) % motionSequenceList.Count;
			PlayCurrent();
		}
	}

	public void PlayPrev()
	{
		List<MotionSequenceData> motionSequenceList = GetMotionSequenceList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(motionSequenceList))
		{
			_currentSequence = (_currentSequence + motionSequenceList.Count - 1) % motionSequenceList.Count;
			PlayCurrent();
		}
	}

	public void PlayCurrent()
	{
		PlayMotion();
	}

	private void PlayMotion()
	{
		List<MotionSequenceData> motionSequenceList = GetMotionSequenceList();
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(motionSequenceList, _currentSequence))
		{
			return;
		}
		MotionSequenceData motionSequenceData = motionSequenceList[_currentSequence];
		_currentMotion = 0;
		_elapsedEndDelay = 0f;
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(motionSequenceData.MotionList, _currentMotion))
		{
			MotionSequenceData.MotionData motionData = motionSequenceData.MotionList[_currentMotion];
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = motionData.Key;
			if (motionData.MotionEnd == DefineCreature.EMotionEnd.Loop)
			{
				stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			}
			else
			{
				stockData.MotionEnd = DefineCreature.EMotionEnd.Infinite;
			}
			if (_dataProvider.GetRenderFacade().AssetCreature is CreatureInstance creatureInstance)
			{
				creatureInstance.CloseAllNonPermanentEffects();
			}
			_facadeRef.Value.AnimationData.Play(stockData, motionData.BlendMotion, MainTimer.DefaultLocal);
		}
	}

	public void Play(AnimationKeyData key)
	{
		List<MotionSequenceData> motionSequenceList = GetMotionSequenceList();
		int num = -1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(motionSequenceList); i++)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(motionSequenceList[i].MotionList); j++)
			{
				AnimationKeyData key2 = motionSequenceList[i].MotionList[j].Key;
				if (key2.MotionType == key.MotionType && key2.Name == key.Name)
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				break;
			}
		}
		if (num >= 0)
		{
			Play(num);
		}
	}

	public void Pause()
	{
		if (!(_facadeRef.Value == null))
		{
			_facadeRef.Value.Action.PauseMotion();
		}
	}

	public void Resume()
	{
		if (!(_facadeRef.Value == null))
		{
			_facadeRef.Value.AnimationData.Resume(NGame2.NRender.NRenderFacade.AnimationData.EState.Action);
		}
	}

	public bool IsPaused()
	{
		if (_facadeRef.Value == null)
		{
			return false;
		}
		return _facadeRef.Value.AnimationData.CurrentState == NGame2.NRender.NRenderFacade.AnimationData.EState.Pause;
	}

	public void ChainUpdate()
	{
		List<MotionSequenceData> motionSequenceList = GetMotionSequenceList();
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(motionSequenceList, _currentSequence) || !_facadeRef.Value.AnimationData.Runtime.IsMotionEnd)
		{
			return;
		}
		MotionSequenceData motionSequenceData = motionSequenceList[_currentSequence];
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(motionSequenceData.MotionList, _currentMotion))
		{
			return;
		}
		MotionSequenceData.MotionData motionData = motionSequenceData.MotionList[_currentMotion];
		if (motionData.MotionEnd != DefineCreature.EMotionEnd.End)
		{
			return;
		}
		_elapsedEndDelay += MainTimer.deltaTime;
		if (_elapsedEndDelay < motionData.EndDelay)
		{
			return;
		}
		_currentMotion++;
		_elapsedEndDelay = 0f;
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(motionSequenceData.MotionList, _currentMotion))
		{
			_facadeRef.Value.Action.Idle();
			return;
		}
		MotionSequenceData.MotionData motionData2 = motionSequenceData.MotionList[_currentMotion];
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.AnimationKey = motionData2.Key;
		if (motionData2.MotionEnd == DefineCreature.EMotionEnd.Loop)
		{
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
		}
		else
		{
			stockData.MotionEnd = DefineCreature.EMotionEnd.Infinite;
		}
		_facadeRef.Value.AnimationData.Play(stockData, motionData2.BlendMotion, MainTimer.DefaultLocal);
	}

	public void EnableMotionSound(bool needPlayAnimationSound)
	{
		if (_facadeRef != null && !(_facadeRef.Value == null))
		{
			_facadeRef.Value.EnableAnimationSound(needPlayAnimationSound);
		}
	}
}
