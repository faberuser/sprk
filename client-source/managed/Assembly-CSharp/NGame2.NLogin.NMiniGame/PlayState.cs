using System.Collections.Generic;
using NGameAsset;
using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class PlayState : StateBase
{
	private const float UPDATE_UI_INTERVAL = 0.1f;

	private float _obstacleCreateTimeMin = 1.3f;

	private float _obstacleCreateTimeMax = 3f;

	private const float TO_METER = 0.01f;

	private float _obstcaleCreateTime;

	private float _obstacleCreateTimer;

	private float _updatedUiTime;

	private float _scrolledX;

	private int _obstacleRatio = 5;

	private bool _isGameOver;

	public MoveEntity Sonia;

	public List<NGame2.NLogin.NMiniGame.ObstacleEntity> ObstacleContainer = new List<NGame2.NLogin.NMiniGame.ObstacleEntity>();

	public List<TerrainEntity> TerrainList = new List<TerrainEntity>();

	private int _obstacleIdIndex = 10000;

	private PlayDataProvier _dataProvier;

	public PlayState(PlayDataProvier dataProvier)
	{
		_dataProvier = dataProvier;
		_obstacleCreateTimeMin = _dataProvier.obstacleCreateTimeMin;
		_obstacleCreateTimeMax = _dataProvier.obstacleCreateTimeMax;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		Sonia = new MoveEntity(GameGlobal);
		Sonia.SetId(1);
		Sonia.AddAction(new NGame2.NLogin.NMiniGame.EntityActionJump(800f));
		Sonia.Rect = new Rect(_dataProvier.Sonia.Rect.position, World.SONIA_WIDTH_HEIGHT);
		_updatedUiTime = 0f;
		_obstacleCreateTimer = 0f;
		_scrolledX = 0f;
		_obstacleRatio = 5;
		_obstacleIdIndex = 10000;
		ObstacleContainer.Clear();
		InitTerrainList();
		GameGlobal.OnEvent(GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.StartPlay>());
		_isGameOver = false;
	}

	private void InitTerrainList()
	{
		TerrainList = _dataProvier.TerrainList;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateInput();
		CheckCollision();
		UpdateEntity();
		UpdateUI();
	}

	private void UpdateInput()
	{
		if (!Define.IsLiveBuild())
		{
			Cheat();
		}
	}

	private void Cheat()
	{
		if (GameGlobal.GeyKeyCoden() == KeyCode.F2)
		{
			if (Sonia != null)
			{
				Sonia.ClearAction();
				Sonia.AddAction(new NGame2.NLogin.NMiniGame.EntityActionAutoJump(800f, 220f));
			}
		}
		else if (GameGlobal.GeyKeyCoden() == KeyCode.F3)
		{
			_scrolledX = -15000f;
		}
	}

	private void UpdateEntity()
	{
		if (!_isGameOver)
		{
			CreateObstacles();
			MoveEntity();
		}
	}

	private void CheckCollision()
	{
		List<NGame2.NLogin.NMiniGame.ObstacleEntity>.Enumerator enumerator = ObstacleContainer.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (CheckCollision(Sonia, enumerator.Current))
			{
				Debug.LogError("onCollision");
				OnCollision(enumerator.Current.Id);
			}
		}
	}

	private bool CheckCollision(EntityBase l, EntityBase r)
	{
		return l.Rect.Overlaps(r.Rect);
	}

	private bool CheckCollisionCircle(Vector3 l, Vector3 r)
	{
		int num = 40;
		if (Vector2.Distance(l, r) < (float)(num * 2))
		{
			return true;
		}
		return false;
	}

	private void OnCollision(int obstacleId)
	{
		NGame2.NLogin.NMiniGame.OnCollision onCollision = GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.OnCollision>();
		onCollision.Set(obstacleId);
		GameGlobal.OnEvent(onCollision);
		SetStateGameOver();
	}

	private void SetStateGameOver()
	{
		_isGameOver = true;
		GameGlobal.OnEvent(GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.GameOver>());
		GameGlobal.SetNextState(State.GameOverState);
	}

	private void UpdateUI()
	{
		_scrolledX += -150f * GameGlobal.TimeDelta();
		if (!(Time.time - _updatedUiTime < 0.1f))
		{
			_updatedUiTime = Time.time;
			NGame2.NLogin.NMiniGame.UpdateUI updateUI = GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.UpdateUI>();
			updateUI.Set(GetDistance());
			GameGlobal.OnEvent(updateUI);
		}
	}

	private int GetDistance()
	{
		return (int)((float)(int)Mathf.Abs(_scrolledX) * 0.01f);
	}

	private void MoveSonia()
	{
		Sonia.Update();
	}

	private void MoveEntity()
	{
		Sonia.Update();
		for (int num = __Extension_Method_ICollection__.COUNT(ObstacleContainer) - 1; num >= 0; num--)
		{
			ObstacleContainer[num].Update();
		}
		foreach (TerrainEntity terrain in TerrainList)
		{
			terrain.Update();
		}
	}

	private void CreateObstacles()
	{
		_obstacleCreateTimer += GameGlobal.TimeDelta();
		if (_obstacleCreateTimer < _obstcaleCreateTime)
		{
			return;
		}
		_obstacleCreateTimer = 0f;
		_obstcaleCreateTime = Random.Range(_obstacleCreateTimeMin, _obstacleCreateTimeMax);
		int obstacleCount = GetObstacleCount();
		for (int i = 0; i < obstacleCount; i++)
		{
			NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType eObstacleType = GetObstacleType();
			Vector2 size = ((eObstacleType == NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Long) ? World.OBSTACLE_WIDTH_HEIGHT_LONG : World.OBSTACLE_WIDTH_HEIGHT_SHORT);
			float x = 690f + (float)i * 55f;
			float y = -248f;
			Vector2 vector = new Vector2(x, y);
			if (obstacleCount == 3)
			{
				eObstacleType = NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Orc;
				size = World.OBSTACLE_WIDTH_HEIGHT_ORC;
			}
			NGame2.NLogin.NMiniGame.CreateObstacle createObstacle = GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.CreateObstacle>();
			createObstacle.Set(++_obstacleIdIndex, vector, eObstacleType);
			GameGlobal.OnEvent(createObstacle);
			NGame2.NLogin.NMiniGame.ObstacleEntity obstacleEntity = new NGame2.NLogin.NMiniGame.ObstacleEntity(GameGlobal, _obstacleIdIndex, RemoveObstacle)
			{
				Rect = new Rect(vector, size)
			};
			obstacleEntity.VelX = -350f;
			ObstacleContainer.Add(obstacleEntity);
			if (obstacleCount == 3)
			{
				break;
			}
		}
	}

	private void RemoveObstacle(int Id)
	{
		for (int num = ObstacleContainer.Count - 1; num >= 0; num--)
		{
			if (ObstacleContainer[num].Id == Id)
			{
				ObstacleContainer.Remove(ObstacleContainer[num]);
			}
		}
	}

	private int GetObstacleCount()
	{
		int num = 1;
		int num2 = ((!IsHardMode()) ? 1 : 2);
		for (int i = 0; i < num2; i++)
		{
			if (Random.Range(0, 10) < 4)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsHardMode()
	{
		return _scrolledX < -15000f;
	}

	private NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType GetObstacleType()
	{
		if (Random.Range(0, 10) < _obstacleRatio)
		{
			_obstacleRatio--;
			return NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Long;
		}
		_obstacleRatio++;
		return NGame2.NLogin.NMiniGame.CreateObstacle.EObstacleType.Short;
	}

	private void MoveObject()
	{
		for (int num = __Extension_Method_ICollection__.COUNT(ObstacleContainer) - 1; num >= 0; num--)
		{
			ObstacleContainer[num].Update();
		}
	}

	private void MoveTerrain()
	{
		foreach (TerrainEntity terrain in TerrainList)
		{
			terrain.Update();
		}
	}
}
