using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePointIncrease : MonoBehaviour
{
    private bool canIncreaseScore = true;
    private float timer = 0.0f;
    private float waitTime = 1.0f;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("on point");
    }
    void OnTriggerStay(Collider collision)
    {
        Debug.Log("Staying on Point");
            if(canIncreaseScore)
            {
                GameObject other = collision.gameObject;
                if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Player3") || other.CompareTag("Player4") || other.CompareTag("Player"))
                {
                    other.GetComponent<PlayerStats>().IncreaseScore(10, other.GetComponent<PlayerManager>().id);
                    canIncreaseScore = false;
                }
                
            }
           

    }
    private void Update()
    {
        if(!canIncreaseScore)
        {
            timer += Time.deltaTime;
            if(timer >= waitTime)
            {
                canIncreaseScore = true;
                timer = 0.0f;
            }
        }
    }
}
