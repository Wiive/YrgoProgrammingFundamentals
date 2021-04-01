﻿using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{        
    public DiceSides dice1;
    public DiceSides dice2;
    public TMP_Text diceValue;
    private int currentValue;
    private int oldValue;

    public DicePlayerBehavoir player1;
    public DicePlayerBehavoir player2;
    public CanvasGroup player1up;
    public CanvasGroup player1down;
    public CanvasGroup player2up;
    public CanvasGroup player2down;
    public TMP_Text roundText;
    public int maxRounds = 20;
    public int round = 0;
    public bool gameEnded;
    public UnityEvent newRound;

    public TMP_Text gameStatusText;
    public enum PlayerStatus { guessing, waiting}
    public PlayerStatus currentStatus;
    public DicePlayerBehavoir currentPlayer;
    public DicePlayerBehavoir otherPlayer;

    private int scoreGiven = 500;

    GameInfo gameInfo;

    FirebaseDatabase db;

    private void Start()
    {
        if (newRound == null)
        {
            newRound = new UnityEvent();
        }

        if (round == 0)
        {
            dice1.RollTheDie();
            dice2.RollTheDie();
        }

        db = FirebaseDatabase.DefaultInstance;

        //Start with values from that activeGameInfo
        gameInfo = ActiveGame.Instance.activeGameInfo;
        round = gameInfo.round;
       
        //Load Dice Players Info
        player1.inGameName.text = gameInfo.dicePlayers[0].displayName;
        player2.inGameName.text = gameInfo.dicePlayers[1].displayName;
        player1.score = gameInfo.dicePlayers[0].score;
        player2.score = gameInfo.dicePlayers[1].score;
        player1.ChangeScoreText(player1.GetScore().ToString());
        player2.ChangeScoreText(player2.GetScore().ToString());

        UpdateValue();
        oldValue = currentValue;
        roundText.text = "Round: " + round.ToString() + "/" + maxRounds.ToString();

        UpdateDicePlayerToActivePlayer();
    }


    public void Update()
    {
        if (round < maxRounds)
        {
            if (player1.hasGuessed && player2.hasGuessed)
            {
                dice1.RollTheDie();
                dice2.RollTheDie();
                NewRound();
            }      
        }

        else if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log("Game is finnised!");
            UpdateWinner();
        }

    }

    public void UpdateDicePlayerToActivePlayer()
    {
        string playerID = ActiveUser.Instance.userID;
        if (playerID == gameInfo.dicePlayers[0].userID)
        {
            player2up.alpha = 0;
            player2down.alpha = 0;
            player2up.interactable = false;
            player2down.interactable = false;
            currentPlayer = player1;
            otherPlayer = player2;
        }
        else
        {
            player1up.alpha = 0;
            player1down.alpha = 0;
            player1up.interactable = false;
            player1down.interactable = false;
            currentPlayer = player2;
            otherPlayer = player1;
        }
    }


    public void UpdateStatusText()
    {
        if (player1.hasGuessed && player2.hasGuessed)
        {
            currentStatus = PlayerStatus.guessing;
        }
        else if (currentPlayer.hasGuessed)
        {
            currentStatus = PlayerStatus.waiting;
        }
        switch (currentStatus)
        {
            case PlayerStatus.waiting:
                gameStatusText.text = "Waiting for other player";
                break;

            case PlayerStatus.guessing:
                gameStatusText.text = "Guess higher or lower";
                break;
        }
    }


    public void UpdateGuessOnline()
    {
        if (currentPlayer == player1)
        {
            gameInfo.dicePlayers[0].guessedHigher = currentPlayer.playerGuessHiger;
            gameInfo.dicePlayers[0].hasGussed = currentPlayer.hasGuessed;


        }
        else
        {
            gameInfo.dicePlayers[1].guessedHigher = currentPlayer.playerGuessHiger;
            gameInfo.dicePlayers[1].hasGussed = currentPlayer.hasGuessed;
        }
        string path = "games/" + gameInfo.gameID;
        string jsondata = JsonUtility.ToJson(gameInfo);
        StartCoroutine(FirebaseManager.Instance.SaveData(path, jsondata));
        ActiveGame.Instance.activeGameInfo = gameInfo;
    }

    public void RefreshFromOnline()
    {
        string path = "games/" + gameInfo.gameID;
        StartCoroutine(FirebaseManager.Instance.LoadData(path, LoadUpdatedGameInfo));
    }

    public void LoadUpdatedGameInfo(string jsondata)
    {
        var gameInfo = JsonUtility.FromJson<GameInfo>(jsondata);
        ActiveGame.Instance.activeGameInfo = gameInfo;
        this.gameInfo = gameInfo;
        if (currentPlayer == player1)
        {
            player2.playerGuessHiger = gameInfo.dicePlayers[0].guessedHigher;
            player2.hasGuessed = gameInfo.dicePlayers[0].hasGussed;
        }
        else
        {
            player1.playerGuessHiger = gameInfo.dicePlayers[1].guessedHigher;
            player1.hasGuessed = gameInfo.dicePlayers[1].hasGussed;
        }
    }



    public void NewRound()
    {        
        UpdateValue();
        CalculateScores();
        oldValue = currentValue;
        player1.hasGuessed = false;
        player2.hasGuessed = false;
        round++;
        roundText.text = "Round: " + round.ToString() + "/" + maxRounds.ToString();
        newRound.Invoke();
        UpdateRoundOnline();
        
    }

    public void UpdateRoundOnline()
    {
        gameInfo.round = round;
        gameInfo.dicePlayers[0].score = player1.score;
        gameInfo.dicePlayers[1].score = player2.score;

        //Dice seeds
        //gameInfo.dice1 = randoms
        //gameInfo.dice2 = randoms

        string path = "games/" + gameInfo.gameID;
        string jsondata = JsonUtility.ToJson(gameInfo);
        StartCoroutine(FirebaseManager.Instance.SaveData(path, jsondata));
        ActiveGame.Instance.activeGameInfo = gameInfo;
    }


    public void UpdateValue()
    {
        currentValue = dice1.GetValue() + dice2.GetValue();
        diceValue.text = currentValue.ToString();
    }


    private void CalculateScores()
    {
        while(currentValue == oldValue)
        {
            ReRollDices();
        }

        var player1Guess = player1.playerGuessHiger;
        var player2Guess = player2.playerGuessHiger;

        if (player1Guess == CompereDices() && player2Guess == CompereDices())
        {
            player1.UpdateScore(scoreGiven / 2);
            player2.UpdateScore(scoreGiven / 2);
        }
        else if (player1Guess == CompereDices())
        {
            player1.UpdateScore(scoreGiven);
        }
        else if (player2Guess == CompereDices())
        {
            player2.UpdateScore(scoreGiven);
        }
        player1.ChangeScoreText(player1.GetScore().ToString());
        player2.ChangeScoreText(player2.GetScore().ToString());
    }


    private void ReRollDices()
    {
        dice1.RollTheDie();
        dice2.RollTheDie();
        UpdateValue();
    }


    public bool CompereDices()
    {
        if (currentValue > oldValue)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateWinner()
    {
        var activeUser = ActiveUser.Instance.userInfo;

        if (currentPlayer.score > otherPlayer.score)
        {
            Debug.Log("You WON!");
            activeUser.victories++;
            
        }
        else if (currentPlayer.score < otherPlayer.score)
        {
            Debug.Log("You lost");
        }
        else
        {
            Debug.Log("It's a draw");
        }

        //Remove game for user
        for ( int i = 0; i < activeUser.activeGames.Count; i++)
        {
            if (activeUser.activeGames[i].Equals(gameInfo.gameID))
            {
                activeUser.activeGames.RemoveAt(i);
            }
        }

        //Save online
        string path = "users/" + activeUser;
        string jsondata = JsonUtility.ToJson(activeUser);
        Debug.Log("Saving winner to database");
        StartCoroutine(FirebaseManager.Instance.SaveData(path, jsondata));

        GetComponent<LevelManager>().LoadScene(1);
        //Update a box that anounce the ending in the screen.
        //Exit button is back to lobby, its should destroy the game
    }
}