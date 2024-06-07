using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{

    //just commenting this class up here as its largely self explanatory. its where i send all the player data to the server so its shared to the other players.
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }
    public static void WelcomeReceived()
    {
        using(Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }
    public static void PlayerRotation(int _id, Quaternion _rotation)
    {
        using(Packet _packet = new Packet((int)ClientPackets.playerRotation))
        {
            _packet.Write(_id);
            _packet.Write(_rotation);
            SendUDPData(_packet);
        }
    }
    public static void PlayerPosition(int _id, Vector3 _position, float _time)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerPosition))
        {
            _packet.Write(_id);
            _packet.Write(_time);
            _packet.Write(_position);

            SendUDPData(_packet);
        }
    }

    public static void BulletCollided(int _bulletId, int _targetId,int _playerId, bool _isPlayer, bool _isKillingBlow, bool _isRespawning)
    {
        using (Packet _packet = new Packet((int)ClientPackets.bulletCollided))
        {
            _packet.Write(_bulletId);
            _packet.Write(_targetId);
            _packet.Write(_playerId);
            _packet.Write(_isPlayer);
            _packet.Write(_isKillingBlow);
            _packet.Write(_isRespawning);
            
            SendTCPData(_packet);
        }
    }
 
    public static void bulletSpawned(int _id, int _bulletId, Vector3 _position, Quaternion _rotation, Vector3 _target)
    {
        using (Packet _packet = new Packet((int)ClientPackets.bulletSpawned))
        {
            _packet.Write(_bulletId);
            _packet.Write(_id);
            _packet.Write(_position);
            _packet.Write(_rotation);
            _packet.Write(_target);

            SendTCPData(_packet);
        }
    }
    public static void SendScore(int _id, int _score)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendScore))
        {
            _packet.Write(_id);
            _packet.Write(_score);

            SendTCPData(_packet);
        }
    }
    public static void ResetGame(int _id)
    {
        using (Packet _packet = new Packet((int)ClientPackets.resetGame))
        {
            _packet.Write(_id);
            SendTCPData(_packet);
        }
    }
    public static void IncreasePlayerScore(int _id, int _score)
    {
        using (Packet _packet = new Packet((int)ClientPackets.increasePlayerScore))
        {
            _packet.Write(_id);
            _packet.Write(_score);

            SendTCPData(_packet);
        }
    }
 
    public static void SendHealth(int _id, int _health)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendHealth))
        {
            _packet.Write(_id);
            _packet.Write(_health);

            SendTCPData(_packet);
        }
    }
}
