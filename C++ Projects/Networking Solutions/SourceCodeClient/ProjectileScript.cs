using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    // Start is called before the first frame update
    private int damage = 0;
    private float playerID = 0;
    private float targetId = 0;
    private bool isPlayer = false;
    private bool isKillingBlow = false;
    [SerializeField]private LayerMask mapLayer;
    private Vector3 Rotation;
    private bool collided = false;
    private GameObject shooter;
  
    public bool hasCollided = false;
   
    private void OnCollisionEnter(Collision collision)
    {
      
        

    //     if(collision.gameObject.tag != "Bullet" && !collided && collision.gameObject.layer != mapLayer)
    //     {
            
    //         if(collision.gameObject.tag == "Player")
    //         {
                
    //             // if(collision.gameObject.TryGetComponent(out PlayerStats player))
    //             // {
    //             //     // if(player.GetHealth() - damage <= 0)
    //             //     // {
    //             //     //     string playerTag = "Player" + playerID.ToString();
    //             //     //     GameObject playerHolder = GameObject.FindGameObjectWithTag(playerTag);
    //             //     //     playerHolder.GetComponent<PlayerStats>().IncreaseScore(50);
    //             //     //     player.TakeDamage(damage);
    //             //     //     player.SetLastPlayerDamageID(playerID);
    //             //     //     Debug.Log("killed Player");
    //             //     //     hasCollided = true;
    //             //     // //Destroy(gameObject);
    //             //     // }
    //             //     // else
    //             //     // {
    //             //         isPlayer = true;
    //             //         targetId = player.GetComponent<PlayerManager>().id;
    //             //         if(player.GetComponent<PlayerStats>().health - 25 <= 0)
    //             //         {
    //             //             isKillingBlow = true;
    //             //         }
    //             //         // player.TakeDamage(damage);
    //             //         // player.SetLastPlayerDamageID(playerID);
    //             //         Debug.Log("hit Player");
    //             //         //Destroy(gameObject);
    //             //         hasCollided = true;
    //             //     //}
                  
    //             // }
    //             collided = true;
    //             hasCollided = true;    
    //         }
    //     }
    // hasCollided = true;
    // }

    // public bool GetIsPlayer()
    // {
    //     return isPlayer;
    // }
    // public bool GetIsKillingBlow()
    // {
    //     return isKillingBlow;
    // }
    // public float GetTargetId()
    // {
    //     return targetId;
    // }
    }
}