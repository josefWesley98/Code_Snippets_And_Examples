using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public int score = 0;
    public int kills = 0;
    public int deaths = 0;
    private PlayerManager playerManager;
    public bool isRespawning = false;
    private float respawnTime = 5.0f;
    private float respawnTimer = 0.0f;
    public bool isCopy = false;
    public int enemyId = 0;
    public int priority = 10;
    public bool whileDead = false;
    public bool handleDeath = false;
    public bool gameOver = false;
    public string winner = "none";
    [SerializeField] UIController UI;
    [SerializeField] GameObject body;
    [SerializeField] Camera cam;
    [SerializeField] private Cinemachine.CinemachineFreeLook playerCam;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        health = 100;
    }
    void FixedUpdate()
    { 
        if(health <= 0 && !isRespawning)
        {
            body.SetActive(false);
            isRespawning = true;
            health = 1;
            ClientSend.SendHealth(playerManager.id ,health);
            
        }
        if(isCopy && health == 1 && !isRespawning)
        {
            body.SetActive(false);
            isRespawning = true;
        }
       
    }
    private void Update()
    {  
        if(isRespawning)
        {
            if(!isCopy)
            {
                priority = 1;
               playerCam.Priority = priority;
            }
            respawnTimer += Time.deltaTime;
            if(respawnTimer >= respawnTime)
            {
                respawnTimer = 0;
                Reset();
                isRespawning = false;
            }
        }
        if(!isCopy)
        {
            if(UI.resetGame)
            {
                priority = 1;
                playerCam.Priority = priority;
            }
            else if(!UI.resetGame && !isRespawning && priority != 10)
            {
                priority = 10;
                playerCam.Priority = priority;
            }
        }
        updateMyPlayerManager();

    }
    public void updateMyPlayerManager()
    {
        GameManager.players[playerManager.id].score = score;
        GameManager.players[playerManager.id].kills = kills;
        GameManager.players[playerManager.id].deaths = deaths;
    }
    public float GetHealth()
    {
        return health;
    }
    public float GetScore()
    {
        return score;
    }
    public float GetRespawningTime()
    {
        return respawnTime - respawnTimer;
    }
    public void SendHealth()
    {
        ClientSend.SendHealth(playerManager.id ,health);
    }
    public void DoDamage(int damage, int _enemyId, bool _killingBlow)
    {
        if(!isCopy)
        {
            health -= damage;
            SendHealth();   
        }
    }
    public void IncreaseScore(int _score, int _id)
    {
        if(!isCopy)
        {
            ClientSend.IncreasePlayerScore(_id, _score);
        }
    }
    
    public void Reset()
    {
        health = 100;
        ClientSend.SendHealth(playerManager.id ,health);
        if(!isCopy)
        {
            priority = 10;
            playerCam.Priority = priority;
        }
        transform.position = GameManager.instance.GetRespawnLocation();
        transform.rotation = Quaternion.identity;
        body.SetActive(true);
    }
}
