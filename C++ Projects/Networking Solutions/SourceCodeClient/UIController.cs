using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;
using TMPro;
using System;
public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadingText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI respawnTimeText;
    [SerializeField] private GameObject UIObjs;
    [SerializeField] private TextMeshProUGUI youWinText;
    [SerializeField] private GameObject respawnTimeTextObj;
    [SerializeField] private GameObject reloadingTextObj;
    [SerializeField] private Shoot shootScript;
    [SerializeField] private PlayerStats playerStatsScript;
    [SerializeField] private TextMeshProUGUI[] playerNames;
    [SerializeField] private TextMeshProUGUI[] playerScores;
    [SerializeField] private TextMeshProUGUI[] playerKills;
    [SerializeField] private TextMeshProUGUI[] playerDeaths;
    [SerializeField] private GameObject[] players;
    [SerializeField] private GameObject scoreBoard;
    [SerializeField] private GameObject youWinTextObj;
    [SerializeField] private TextMeshProUGUI newGameStartCountdownText;
    [SerializeField] private GameObject newGameStartCountdownObj;
    private string winnerName = "";
    public bool resetGame = false;
    private float resetGameTimer = 0.0f;
    private float resetGameTime = 7.0f;
    private bool activeSB = false;
    private float reloadTimeRemaining = 0;
    private PlayerInput playerControls;
    private InputAction openScoreBoard;
    private bool SetupEnd = false;
    private bool[] activePlayers = new bool[4] {false, false, false, false};
    private bool doOnce = true;

    private void Awake() => playerControls = new PlayerInput();

    void Start()
    {
        healthText.text = "Health: " + playerStatsScript.GetHealth().ToString();
        ammoText.text = "Ammo: " + shootScript.GetAmmoCount().ToString() + "/15";
        scoreText.text = "Score: " + playerStatsScript.GetScore().ToString();
        float GRT = (float)Math.Round((float)playerStatsScript.GetRespawningTime(),1);
        respawnTimeText.text = "Respawning in: " + GRT.ToString() + "...";
        reloadingTextObj.SetActive(false); 
        respawnTimeTextObj.SetActive(false);
        scoreBoard.SetActive(false);
        newGameStartCountdownObj.SetActive(false);

    }
    private void OnEnable()
    {
        openScoreBoard = playerControls.Player.ScoreBoard;
        openScoreBoard.Enable();
        openScoreBoard.started += DoOpenScoreBoard;
        openScoreBoard.performed += DoCloseScoreBoard;
    }
    private void OnDisable()
    {
       openScoreBoard.Disable();
    }
    private void DoCloseScoreBoard(InputAction.CallbackContext context)
    {
        scoreBoard.SetActive(false);
        activeSB = false;
    }
    private void DoOpenScoreBoard(InputAction.CallbackContext context)
    {
        scoreBoard.SetActive(true);
        activeSB = true;
    }

    void Update()
    {
        if(!SetupEnd)
        {
            healthText.text = "Health: " + playerStatsScript.GetHealth().ToString() + "/100";
            ammoText.text = "Ammo: " + shootScript.GetAmmoCount().ToString() + "/15";
            scoreText.text = "Score: " + playerStatsScript.GetScore().ToString();
            
            if(shootScript.GetReloading())
            {
                reloadingTextObj.SetActive(true);
                reloadTimeRemaining = (float)Math.Round((float)shootScript.GetReloadTimeRemaining(),1);
                string reloadTextHolder = "Reloading... " + reloadTimeRemaining.ToString() + " seconds remaining..."; 
                reloadingText.text = reloadTextHolder;
            }
            else
            {
                reloadTimeRemaining = 0;
                reloadingTextObj.SetActive(false); 
            } 
            if(playerStatsScript.isRespawning)
            {
                respawnTimeTextObj.SetActive(true);
                float GRT = (float)Math.Round((float)playerStatsScript.GetRespawningTime(),1);
                respawnTimeText.text = "Respawning in: " + GRT.ToString() + "...";
            }
            else
            {
                respawnTimeTextObj.SetActive(false);
            }
            CheckPlayersInGame();
            UpdatePlayers();
            IfPlayerWins();
        }
        
        DoGameOver();
        
        if(resetGame)
        {
           ResetGame();
        }
        
    }

    private void CheckPlayersInGame()
    {
        if(activeSB)
        {
            if(GameManager.players[1] != null)
            {
                players[0].SetActive(true);
            }
            else
            {
                players[0].SetActive(false);
            }
            if(GameManager.players.Count == 2)
            {
                if(GameManager.players[2] != null)
                {
                    players[1].SetActive(true);
                }
                else
                {
                    players[1].SetActive(false);
                }
            }
            if(GameManager.players.Count == 3)
            {
                if(GameManager.players[3] != null)
                {
                    players[2].SetActive(true);
                }
                else
                {
                    players[2].SetActive(false);
                }
            }
            if(GameManager.players.Count == 4)
            {
                if(GameManager.players[4] != null)
                {
                    players[3].SetActive(true);
                }
                else
                {
                    players[3].SetActive(false);
                }
            }
        }
    }
    private void UpdatePlayers()
    {
        for(int i = 1; i <= GameManager.players.Count; i++)
        {
            if(GameManager.players[i] != null)
            {
                string name = GameManager.players[i].username;
                int score = GameManager.players[i].score;
                int kills = GameManager.players[i].kills;
                int deaths = GameManager.players[i].deaths;

                playerNames[i-1].text = name;
                playerScores[i-1].text = score.ToString();
                playerKills[i-1].text = kills.ToString();
                playerDeaths[i-1].text = deaths.ToString();
            }
        }
    }

    private void IfPlayerWins()
    {
        for(int i = 1; i <= GameManager.players.Count; i++)
        {
            if(GameManager.players[i] != null)
            {
                if(GameManager.players[i].score >= 50)
                {
                    GameManager.players[i].GetComponent<PlayerStats>().gameOver = true;
                    winnerName = GameManager.players[i].GetComponent<PlayerManager>().username;
                    SetupEnd = true;
                }
            }
        
        }
        if(SetupEnd)
        {
            for(int i = 1; i <= GameManager.players.Count; i++)
            {
                if(GameManager.players[i] != null)
                {
                    GameManager.players[i].GetComponent<PlayerStats>().gameOver = true;
//                    GameManager.players[i].GetComponent<UIController>().SetupEnd = true;
                }
            }
        }
    }

    private void DoGameOver()
    {
        if(SetupEnd)
        {
            for(int i = 1; i <= GameManager.players.Count; i++)
            {
                //GameManager.players[i].GetComponent<PlayerStats>().priority = 1;
                Debug.Log("we zoomed every player out");
                GameManager.players[i].GetComponent<PlayerStats>().gameOver = true;
                GameManager.players[i].GetComponent<PlayerStats>().winner = winnerName;
                youWinText.text = "The Winner: " + winnerName;
                resetGame = true;
                youWinTextObj.SetActive(true);
                scoreBoard.SetActive(false);
                UIObjs.SetActive(false); 
            }
        }
    }
    private void ResetGame()
    {
        resetGameTimer += Time.deltaTime;
        
        if(doOnce)
        {
            for(int i = 1; i <= GameManager.players.Count; i++)
            {
                ClientSend.ResetGame(i);
            }
            doOnce = false;
        }
        newGameStartCountdownObj.SetActive(true);
        float remainingTime = resetGameTime - resetGameTimer;
        int convertedTime = (int)remainingTime;
        newGameStartCountdownText.text = "New game starts in: " + convertedTime.ToString();
        if(resetGameTimer >= resetGameTime)
        {
            resetGameTimer = 0;
            resetGame = false;
            SetupEnd = false;
            for(int i = 1; i <= GameManager.players.Count; i++)
            {
                if(GameManager.players[i] != null)
                {
                    Debug.Log("this worked");
                    //GameManager.players[i].GetComponent<PlayerStats>().priority = 10;
                    GameManager.players[i].GetComponent<PlayerStats>().score = 0;
                    GameManager.players[i].GetComponent<PlayerStats>().kills = 0;
                    GameManager.players[i].GetComponent<PlayerStats>().deaths = 0;
                    GameManager.players[i].GetComponent<PlayerStats>().winner = "";
                    GameManager.players[i].GetComponent<PlayerStats>().gameOver = false;
                    doOnce = true;
                    winnerName = "";
                    
                    youWinTextObj.SetActive(false);
                    UIObjs.SetActive(true); 
                    newGameStartCountdownObj.SetActive(false);
                    GameManager.players[i].gameObject.transform.position = GameManager.instance.GetRespawnLocation();
                }
            }
        }
    }
}
