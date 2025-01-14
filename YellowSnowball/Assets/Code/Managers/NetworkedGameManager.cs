﻿using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkedGameManager : SingletonBehaviour<NetworkedGameManager>
{
    public bool IsProxyManager;

    [HideInInspector]
    public NetworkedWorldManager WorldManager;

    [HideInInspector]
    public RPCManager RPCManager;

    [HideInInspector]
    public ShopManager ShopManager;

    [HideInInspector]
    public UIManager UIManager;

    public PlayerData[] PlayerData = new PlayerData[2];

    [SerializeField]
    private GameData m_gameData;
    public GameData GameData => m_gameData;

    public int StartGameTimerInSec = 100;
    public int GameTimer = 0;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene((int)SceneNameEnum.MainMenu);
        SceneManager.LoadScene((int)SceneNameEnum.StartGame, LoadSceneMode.Additive);
        SceneManager.LoadScene((int)SceneNameEnum.LeaderBoard, LoadSceneMode.Additive);
        SceneManager.LoadScene((int)SceneNameEnum.Tutorial, LoadSceneMode.Additive);
        SceneManager.LoadScene((int)SceneNameEnum.Credits, LoadSceneMode.Additive);
    }

    public void GoToGame()
    {
        SceneManager.LoadScene((int)SceneNameEnum.World_1);
    }

    // When player exits game over screen
    public void GameOverTeardown()
    {
    }

    public PlayerData GetLocalPlayerData()
    {
        return PlayerData[WorldManager.PlayerIndex];
    }

    private string GetWinner()
    {
        // If only one player, return the local player
        if (WorldManager.Players.Length == 1)
            return $"Player {(WorldManager.PlayerIndex + 1).ToString()}";

        // If two players determine who won
        if (WorldManager.SnowTerrain[0].RemainingSnow < WorldManager.SnowTerrain[1].RemainingSnow)
            return $"Left Player";

        return $"Right Player";
    }

    private IEnumerator GameTimerRoutine()
    {
        yield return new WaitForSeconds(1f);

        var countDownTimer = 3;
        // Start countdown:
        while (countDownTimer >= 0)
        {
            yield return new WaitForSeconds(1f);
            if (UIManager != null)
            {
                UIManager.TimerText.SetText(countDownTimer == 0 ? "GO!" : countDownTimer.ToString());
            }
            countDownTimer--;
        }

        RPCManager.Instance.CanUpdateSnow = true;
        WorldManager.GetLocalPlayer().CanMove = true;

        while (true)
        {
            GameTimer--;
            if (UIManager != null)
            {
                UIManager.TimerText.SetText(GameTimer.ToString());
            }
            yield return new WaitForSeconds(1f);

            if (GameTimer <= 0)
            {
                // Determine winner
                string winnerName = GetWinner();
                UIManager.ShowGameOver(winnerName);
                EventManager.Fire(new OnWinnerDeclared());
                yield break;
            }
        }
    }

    public void StartGame()
    {
        StartCoroutine(GameTimerRoutine());
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (!IsProxyManager)
            GoToMainMenu();

        foreach(var playerData in PlayerData)
        {
            playerData.Init();
        }

        GameTimer = StartGameTimerInSec;
    }
}
