using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    // basic client handler.
   public static void Welcome(Packet _packet)
   {
      //Note: Make sure you do these in the same order on both sides so it reads correctly.
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        //Note: test message sending.
        Debug.Log("Message From Server: " + _msg);
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
   }
   public static void SpawnPlayer(Packet _packet)
   {
     int _id = _packet.ReadInt();
     string _username = _packet.ReadString();
     Vector3 _position = _packet.ReadVector3();
     Quaternion _rotation = _packet.ReadQuaternion();

     GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
   }
   public static void PlayerPosition(Packet _packet)
   {
      // player pos update.
      int _id = _packet.ReadInt();
      Vector3 _position = _packet.ReadVector3();
      GameManager.players[_id].transform.position = _position;
   }
   public static void PlayerRotation(Packet _packet)
   {
      //player rotation update.
      int _id = _packet.ReadInt();
      Quaternion _rotation = _packet.ReadQuaternion();
      GameManager.players[_id].transform.rotation = _rotation;
   }
   public static void SpawnBulletForEnemies(Packet _packet)
   {
      // spawning bullets for every player at position and rotation and the target they are moving towards.
      int _bulletId = _packet.ReadInt();
      int _id = _packet.ReadInt();
      Vector3 _position = _packet.ReadVector3();
      Quaternion _rotation = _packet.ReadQuaternion();
      Vector3 _target = _packet.ReadVector3();

      BulletManager.instance.SpawnBullet( _id, _bulletId, _position, _rotation, _target);
   }
   public static void PlayerHealth(Packet _packet)
   {
      // player health update.
      int _id = _packet.ReadInt();
      int _health = _packet.ReadInt();
      GameManager.players[_id].GetComponent<PlayerStats>().health = _health;
   }
   public static void DamagePlayer(Packet _packet)
   {
      // checks if player is a copy in someone elses version and if they are currently respawning before using the targetId passed from the bullet when i collides to deal damage.
      int _id = _packet.ReadInt();
      int _targetId = _packet.ReadInt();
      int _playerDamage = _packet.ReadInt();
      bool _isKillingBlow = _packet.ReadBool();

      if(!GameManager.players[_targetId].GetComponent<PlayerStats>().isCopy && !GameManager.players[_targetId].GetComponent<PlayerStats>().isRespawning)
      {
         GameManager.players[_targetId].GetComponent<PlayerStats>().DoDamage(_playerDamage, _targetId, _isKillingBlow); 
      }     
   }
   public static void scoreUpdate(Packet _packet)
   {
      // updates players stats.
      int _id = _packet.ReadInt();
      int _score = _packet.ReadInt();
      int _kills = _packet.ReadInt();
      int _deaths = _packet.ReadInt();

      GameManager.players[_id].GetComponent<PlayerStats>().score = _score;
      GameManager.players[_id].GetComponent<PlayerStats>().kills = _kills;
      GameManager.players[_id].GetComponent<PlayerStats>().deaths = _deaths;
   }
        
}
