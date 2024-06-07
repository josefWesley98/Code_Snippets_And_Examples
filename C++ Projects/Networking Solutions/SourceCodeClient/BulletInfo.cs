using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInfo : MonoBehaviour
{
    [SerializeField]private LayerMask mapLayer;
    public int bulletId = 0;
    public int playerId = 0;
    public Vector3 position = new Vector3(0, 0, 0);
    public Quaternion rotation = Quaternion.identity;
    private ProjectileScript script;
    public bool ifCopy = false;
    public bool hasCollided = false;
    public int targetId = 0;
    public bool isPlayer = false;
    public bool isKillingBlow = false;
    public void Start()
    {
        script = gameObject.GetComponent<ProjectileScript>();
    }
    private void FixedUpdate()
    {
        if(hasCollided)
        {
            hasCollided = true;
        }
        if(ifCopy && !hasCollided)
        {
            // Debug.Log("bullet receiving pos");
            // position = BulletManager.bullets[bulletId].position;
            // rotation = BulletManager.bullets[bulletId].rotation;

            // transform.position = position;
            // transform.rotation = rotation;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == mapLayer || collision.gameObject.tag == "Map")
        {
            
            hasCollided = true;
            Destroy(gameObject);
        }

        if(collision.gameObject.TryGetComponent(out PlayerStats player))
        {
            if(collision.gameObject.tag == "Player")
            {
                hasCollided = true;
                isPlayer = true;
                targetId = player.GetComponent<PlayerManager>().id;
                GameManager.players[targetId].GetComponent<PlayerStats>().enemyId = playerId;
                bool isRespawning =  GameManager.players[targetId].GetComponent<PlayerStats>().isRespawning;
                if(player.GetComponent<PlayerStats>().health - 25 <= 0)
                {
                    isKillingBlow = true;
                }
                
                BulletManager.bullets[bulletId].bulletId = 0;
                BulletManager.bullets[bulletId].playerId = 0;
                BulletManager.bullets[bulletId].position = Vector3.zero;
                BulletManager.bullets[bulletId].rotation = Quaternion.identity;

                ClientSend.BulletCollided(bulletId, targetId, playerId, isPlayer, isKillingBlow, isRespawning);
                Destroy(gameObject);
            }
        }
    }
}
