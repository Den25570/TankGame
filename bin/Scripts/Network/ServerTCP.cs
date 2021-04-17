using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class ServerTCP : MonoBehaviour
{
    private delegate void Packet_(int index, byte[] data);
    private static Dictionary<int, Packet_> Packets;

    private Socket mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] buffer = new byte[1024];

    private Client[] _clients = new Client[Constants.MAX_PLAYERS];

    public NetworkManager NetworkManager;

    // private static void Notify
    private void Awake()
    {
        InitializeNetworkPackages();
    }

    void Start()
    {
        SetupServer();
    } 

    public void CloseSockets()
    {
        mainSocket.Close();
        Debug.Log("Main socket closed");
        foreach(var client in _clients)
        {
            client.Socket.Close();
        }
        Debug.Log("All client sockets closed");
    }

    public void SetupServer()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            _clients[i] = new Client(this);
        }
        mainSocket.Bind(new IPEndPoint(IPAddress.Any, 5555));
        mainSocket.Listen(10);
        mainSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

    }

    private void AcceptCallback(IAsyncResult ar)
    {
        Socket socket = mainSocket.EndAccept(ar);
        mainSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (_clients[i].Socket == null)
            {
                _clients[i].Socket = socket;
                _clients[i].Index = i;
                _clients[i].Ip = socket.RemoteEndPoint.ToString();
                _clients[i].isReady = false;
                _clients[i].StartClient();
                Debug.Log("Connection from '"+ _clients[i].Ip + "' received on index[" + i + "]. Total is "+ CountActiveClients());
                SendConnectionOK(i);
                return;
            }
        }
    }

    public int CountActiveClients()
    {
        int clientCount = 0;
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (_clients[i].Socket != null)
            {
                clientCount++;
            }
        }
        return clientCount;
    }

    private bool AllClientsReady()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (_clients[i].Socket != null)
            {
                if (!_clients[i].isReady)
                {
                    Debug.Log("[" + i + "] not ready");
                    return false;
                }
            }
        }
        return true;
    }

    #region SendersRegion

    public void SendDataTo(int index, byte[] data)
    {
        byte[] sizeinfo = new byte[4];
        sizeinfo[0] = (byte)data.Length;
        sizeinfo[1] = (byte)(data.Length >> 8);
        sizeinfo[1] = (byte)(data.Length >> 16);
        sizeinfo[1] = (byte)(data.Length >> 24);

        _clients[index].Socket.Send(sizeinfo);
        _clients[index].Socket.Send(data);
    }

    public void SendConnectionOK(int index)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ServerPackets.SConnectionOK);
        buffer.WriteInt32(index);
        buffer.WriteString("You are succesfully connected to the server. index[" + index + "]");
        SendDataTo(index, buffer.ToArray());
        buffer.Dispose();
    }

    public void SendPlayerCount()
    {
        int ActiveClients = CountActiveClients();

        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ServerPackets.SConnectedPlayerCount);
        buffer.WriteInt32(ActiveClients);

        for (int i = 0; i < _clients.Length; i++)
        {
            if (_clients[i].Socket != null)
            {
                SendDataTo(i, buffer.ToArray());
                Debug.Log("Player count sended to Client["+i+"]");
            }
        }

        buffer.Dispose();
    }

    public void SendClientDisconnect()
    {
        
    }

    public void SendWorldSnapshot(WorldSnapshot WorldSnapshot)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ServerPackets.SWorldSnapshot);

        buffer.WriteInt64(WorldSnapshot.createTime);
        buffer.WriteInt32(WorldSnapshot.RoundNumber);
        buffer.WriteInt32(WorldSnapshot.TankData.Count);
        foreach(var Tank in WorldSnapshot.TankData)
        {
            buffer.WriteVector3(Tank.Position);
            buffer.WriteVector3(Tank.Forward);
            buffer.WriteQuaternion(Tank.Rotation);
            buffer.WriteVector3(Tank.Velocity);

            buffer.WriteFloat(Tank.Health);
            buffer.WriteBoolean(Tank.Dead);
            buffer.WriteFloat(Tank.Speed);
            buffer.WriteFloat(Tank.TurnSpeed);
            buffer.WriteInt32(Tank.Wins);

            buffer.WriteFloat(Tank.Inputs.Movement);
            buffer.WriteFloat(Tank.Inputs.Rotating);

            buffer.WriteInt32(Tank.index);          
        }

        for (int i = 0; i < _clients.Length; i++)
        {
            if (_clients[i].Socket != null)
            {
                SendDataTo(i, buffer.ToArray());
            }
        }

        buffer.Dispose();
    }

    public void SendFireData(int index, float ChargeValue)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ServerPackets.SPlayerShoot);

        buffer.WriteInt32(index);
        buffer.WriteFloat(ChargeValue);

        for (int i = 0; i < _clients.Length; i++)
        {
            if (_clients[i].Socket != null && i != index-1)
            {
                SendDataTo(i, buffer.ToArray());
            }
        }

        buffer.Dispose();
    }

    private void SendStartMatchSignal()
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ServerPackets.SStartMatch);
        buffer.WriteString("Match is started!");

        for (int i = 0; i < _clients.Length; i++)
        {
            if (_clients[i].Socket != null)
            {
                SendDataTo(i, buffer.ToArray());
            }
        }

        buffer.Dispose();

        NetworkManager.GameManager.StartGame = true;
    }

    #endregion

    #region HandlersRegion

    public void InitializeNetworkPackages()
    {
        Debug.Log("Initializing Server Network Packages...");
        Packets = new Dictionary<int, Packet_>
            {
                { (int)ClientPackets.CConnectionOK, HandleConnectionOK },
                { (int)ClientPackets.CInputs, HandleClientInputs },
                { (int)ClientPackets.CFire, HandleClientFire },
                { (int)ClientPackets.CReadySignal, HandleReadySignal },
            };
    }

    public void HandleNetworkInformation(int index, byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        int packetNum = buffer.ReadInt32();
        buffer.Dispose();
        if (Packets.TryGetValue(packetNum, out Packet_ Packet))
        {
            Packet.Invoke(index, data);
        }
    }

    private void HandleConnectionOK(int index, byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();
        string message = buffer.ReadString();
        buffer.Dispose();

        //code
        Debug.Log("["+index+"]: " + message);

        SendPlayerCount();
    }

    private void HandleClientInputs(int index, byte[] data)
    {
        if (NetworkManager.GameManager.GameStatus == GameManager.MatchStatus.OnGoing)
        {
            var Inputs = new Input();
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInt32();

            Inputs = new Input();
            Inputs.Movement = buffer.ReadFloat();
            Inputs.Rotating = buffer.ReadFloat();
            Inputs.Index = buffer.ReadInt32();

            buffer.Dispose();

            NetworkManager.GameManager.ApplyInputs(Inputs);
        }
    }

    private void HandleClientFire(int index, byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();

        float chargeValue = buffer.ReadFloat();

        buffer.Dispose();

        NetworkManager.GameManager.ApplyShot(index+1, chargeValue);
        SendFireData(index+1, chargeValue);
    }

    private void HandleReadySignal(int index, byte[] data)
    {
        Debug.Log("[" + index + "] is ready");
        _clients[index].isReady = true;
        int ActivePlayerCount = CountActiveClients();
        Debug.Log("Players: " + ActivePlayerCount + " - " + NetworkManager.GameManager.PlayersRequired);

        if ((ActivePlayerCount == NetworkManager.GameManager.PlayersRequired) && AllClientsReady())
        {
            SendStartMatchSignal();
        }
    }

    #endregion Handlers

}

public class Client
{
    private ServerTCP hostServer;

    public int Index;
    public string Ip;
    public Socket Socket;
    public bool closing = false;
    private byte[] _buffer = new byte[1024];

    public bool isReady = false;

    public Client(ServerTCP hostServer)
    {
        this.hostServer = hostServer;
    }

    public void StartClient()
    {
        Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), Socket);
        closing = false;
        isReady = false;
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        Socket Socket = (Socket)ar.AsyncState;

        try
        {
            int received = Socket.EndReceive(ar);
            if (received <= 0)
            {
                CloseClient(Index);
            }
            else
            {
                byte[] databuffer = new byte[received];
                Array.Copy(_buffer, databuffer, received);
                hostServer.HandleNetworkInformation(Index, databuffer);
                Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), Socket);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            CloseClient(Index);
        }
    }

    private void CloseClient(int Index)
    {
        closing = true;
        Debug.Log("Connection from " + Ip + " has been terminated");
        //PlayerLeft
        hostServer.SendClientDisconnect();
        Socket.Close();

        Socket = null;
    }
}
