using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientTCP : MonoBehaviour
{
    public string RemoteServerIP;
    public int RemoteServerPort;
    public NetworkManager NetworkManager;

    private Socket socket;
    private byte[] asyncbuffer;
    private delegate void Packet_(byte[] data);
    private static Dictionary<int, Packet_> Packets;
    private long lastSnaphotReceivedTime;

    public int PlayerCount = 0;

    public int MyIndex;
    

    public enum ClientStatus
    {
        NotConnected = 0,
        NotReady = 1,
        Ready = 2
    }

    public ClientStatus Status;

    private void Awake()
    {
        InitializeNetworkPackages();
    }

    // Start is called before the first frame update
    void Start()
    {
        Status = ClientStatus.NotConnected;
    }

    public void CloseSocket()
    {
        socket.Close();
    }

    public bool ConnectToServer(string ip, int port)
    {
        Debug.Log("Connecting to server...");
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), socket);

        return true;

    }

    private void ConnectCallback(IAsyncResult ar)
    {
        socket.EndConnect(ar);
        while (true)
        {
            if (OnReceive() == 0) break;
        }

        socket.Close();
        NetworkManager.GameManager.Endmatch();
    }

    private int OnReceive()
    {
        byte[] sizeInfo = new byte[4];
        byte[] receivedBuffer = new byte[1024];

        int totalRead = 0, currentRead = 0;

        try
        {
            //Receiving package info
            currentRead = totalRead = socket.Receive(sizeInfo);
            if (totalRead <= 0)
            {
                Debug.Log("You are not connected to the server.");
                Status = ClientStatus.NotConnected;
                return 0;
            }
            else
            {

                while (totalRead < sizeInfo.Length && currentRead > 0)
                {
                    currentRead = socket.Receive(sizeInfo, totalRead, sizeInfo.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                int messagesSize = 0;
                messagesSize |= sizeInfo[0];
                messagesSize |= (sizeInfo[1] << 8);
                messagesSize |= (sizeInfo[2] << 8 * 2);
                messagesSize |= (sizeInfo[3] << 8 * 3);

                //Receiving package data
                byte[] data = new byte[messagesSize];

                totalRead = 0;
                currentRead = totalRead = socket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);
                while (totalRead < messagesSize && currentRead > 0)
                {
                    currentRead = totalRead = socket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                HandleNetworkInformation(data);
            }
        }
        catch
        {
            Debug.Log("You are not connected to the server!");
            Status = ClientStatus.NotConnected;
            return 0;
        }
        return 1;
    }

    public void SendData(byte[] data)
    {
        socket.Send(data);
    }

    private void SendClientConnectionOK()
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ClientPackets.CConnectionOK);
        buffer.WriteString("Client connnected succesfully");
        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public void SendClientReady()
    {
        Debug.Log(Status);
        if (Status == ClientStatus.NotReady)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInt32((int)ClientPackets.CReadySignal);
            SendData(buffer.ToArray());
            buffer.Dispose();

            Status = ClientStatus.Ready;
        }
    }

    public void SendInputdata(Input input)
    {
        var Inputs = NetworkManager.GameManager.PlayerControlledTank.GetInputInfo();
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ClientPackets.CInputs);

        buffer.WriteFloat(Inputs.Movement);
        buffer.WriteFloat(Inputs.Rotating);

        buffer.WriteInt32(Inputs.Index);

        SendData(buffer.ToArray());

        buffer.Dispose();
    }

    public void SendFireData(float ChargeValue)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInt32((int)ClientPackets.CFire);

        buffer.WriteFloat(ChargeValue);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public void InitializeNetworkPackages()
    {
        Debug.Log("Initializing Network Packages...");
        Packets = new Dictionary<int, Packet_>
            {
                { (int)ServerPackets.SConnectionOK, HandleConnectionOK },
                { (int)ServerPackets.SConnectedPlayerCount, HandePlayerCount },
                { (int)ServerPackets.SWorldSnapshot, HandleWorldSnapShot },
                { (int)ServerPackets.SStartMatch, HandleMatchStart },
                { (int)ServerPackets.SPlayerShoot, HandleClientFire},
            };
    }

    public void HandleNetworkInformation(byte[] data)
    {
        try
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetNum = buffer.ReadInt32();
            buffer.Dispose();

            if (Packets.TryGetValue(packetNum, out Packet_ Packet))
            {
                Packet.Invoke(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        
    }

    private void HandePlayerCount(byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();
        PlayerCount = buffer.ReadInt32();
        buffer.Dispose();

        //code
        Debug.Log("Players: " + PlayerCount);
    }

    private void HandleConnectionOK(byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();

        MyIndex = buffer.ReadInt32();
        string message = buffer.ReadString();

        buffer.Dispose();

        //code
        Debug.Log(message);
        Status = ClientStatus.NotReady; 
        SendClientConnectionOK();
    }

    private void HandleMatchStart(byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();

        string message = buffer.ReadString();

        buffer.Dispose();

        //code
        Debug.Log(message);
        NetworkManager.GameManager.StartGame = true;
    }

    private void HandleWorldSnapShot(byte[] data)
    {

        var WorldSnapshot = new WorldSnapshot();

        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();

        WorldSnapshot.createTime = buffer.ReadInt64();
        if (WorldSnapshot.createTime < lastSnaphotReceivedTime)
        {
            buffer.Dispose();
            return;
        }
        lastSnaphotReceivedTime = WorldSnapshot.createTime;

        WorldSnapshot.RoundNumber = buffer.ReadInt32();
        int TanksCount = buffer.ReadInt32();
        for (int i = 0; i < TanksCount; i++)
        {
            var Tank = new TankData();
            Tank.Position = buffer.ReadVector3();
            Tank.Forward = buffer.ReadVector3();
            Tank.Rotation = buffer.ReadQuaternion();
            Tank.Velocity = buffer.ReadVector3();

            Tank.Health = buffer.ReadFloat();
            Tank.Dead = buffer.ReadBoolean();
            Tank.Speed = buffer.ReadFloat();
            Tank.TurnSpeed = buffer.ReadFloat();
            Tank.Wins = buffer.ReadInt32();

            Tank.Inputs = new Input();
            Tank.Inputs.Movement = buffer.ReadFloat();
            Tank.Inputs.Rotating = buffer.ReadFloat();

            Tank.index = buffer.ReadInt32();

            WorldSnapshot.TankData.Add(Tank);
        }


        NetworkManager.GameManager.ApplyWorldSnapshot(WorldSnapshot);

        Debug.Log("Snapshot received");
        buffer.Dispose();
    }

    private void HandleClientFire(byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInt32();

        int index = buffer.ReadInt32();
        float chargeValue = buffer.ReadFloat();

        buffer.Dispose();

        NetworkManager.GameManager.ApplyShot(index, chargeValue);
    }

    public void CloseClient()
    {
        Debug.Log("Closing Connection to the server");
        Status = ClientStatus.NotConnected;
        socket.Close();
    }
}
