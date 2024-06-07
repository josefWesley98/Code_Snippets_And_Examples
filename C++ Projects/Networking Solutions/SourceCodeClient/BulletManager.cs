using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;
    public GameObject bulletPrefab;
    public static Dictionary<int, BulletInfo> bullets = new Dictionary<int, BulletInfo>();
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

        for (int i = 1; i <= 100; i++)
        {
            bullets.Add(i,new BulletInfo());
        }

    }
    public void SpawnBullet(int _id, int _bulletId, Vector3 _position, Quaternion _rotation, Vector3 _target)
    {
        bullets[_bulletId].playerId = _id;
        bullets[_bulletId].bulletId = _bulletId;
        bullets[_bulletId].playerId = _id;
        bullets[_bulletId].ifCopy = true;
    
        GameObject _bullet;
        _bullet = Instantiate(bulletPrefab, _position , _rotation);
        _bullet.GetComponent<BulletInfo>().playerId = _id;
        _bullet.GetComponent<BulletInfo>().bulletId = _bulletId;
        _bullet.GetComponent<BulletInfo>().ifCopy = true;
        _bullet.GetComponent<Rigidbody>().velocity = _target;
        
    }
}
