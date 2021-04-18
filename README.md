# TankGame
Game based on [Unity Tanks Tutorial](https://learn.unity.com/project/tanks-tutorial) with multiplayer.
Basic movement, shoot and scenes scripts taken from the tutorial.

Multiplayer realises authoritative server-client architecture created with basic C# TCP sockets in [network section](Assets/Scripts/Network/).
Authoritative server has strong control over world as it sends [world snapshots](Assets/Scripts/Network/SnaphotData/) to all connected clients, 
clients interpolate snapshots depending on ping and packet loss to ensure smooth gameplay and prevent lags. 
From the other side every client only sends its own actions to server. Such architecture protects server from client-side hacking.

After server is started client can connect to session by entering servers ip address. As two or more clients connected game starts.
Game only accepts two players at one game session. You can add more players by adding new spawn points in unity game manager. 

### Connection menu:

![Connection menu](https://i.ibb.co/5FPSqth/Tank-Main-Menu.png)
![Connection menu](https://i.ibb.co/xHFMJJM/Tank-Main-Menu-2.png)

### Network code

Network code constists of 4 main scripts:
- [ServerTCP](Assets/Scripts/Network/ServerTCP.cs) provides network functionality to server, declares packet sender unctions anf packet handlers for server
- [ClientTCP](Assets/Scripts/Network/ClientTCP.cs) provides network functionality to client, declares packet sender unctions anf packet handlers for client
- [Network manager](Assets/Scripts/Managers/NetworkManager.cs) controls server snapshot sending and client action sending, 
manages network state with help of  **UIManger** and **GameManager** classes
- [PacketBuffer class](Assets/Scripts/Network/PacketBuffer.cs) converts data types to array of bytes to send it over network to client or server. 
Before writing any data sending side must write [PacketType](Assets/Scripts/Network/PacketType.cs) to buffer as header.
Receiving side reads header and calls handler according to PacketBuffer code

Client sending actions to server:
``` C#
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
```
Server receiving client action:
``` C#
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
```
Initialization of packet handlers:
``` C#
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
```
