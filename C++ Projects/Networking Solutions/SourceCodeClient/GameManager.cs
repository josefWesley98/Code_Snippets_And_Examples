using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private Transform[] respawnLocations;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Debug.Log("Instance already exists, destroying this object");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        // initialise's the player object as either a copy of an enemy player or your own player.
        if(_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.GetComponent<PlayerStats>().isCopy = true;
        }
        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        _player.GetComponent<PlayerManager>().score = 0;
        _player.GetComponent<PlayerManager>().kills = 0;
        _player.GetComponent<PlayerManager>().deaths = 0;
        
        _player.transform.position = GetRespawnLocation();
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
    public Vector3 GetRespawnLocation()
    {
       //gives a random respawn point for the player.
        int rand = Random.Range(0, respawnLocations.Length);
        return respawnLocations[rand].position;
    }
    
}
