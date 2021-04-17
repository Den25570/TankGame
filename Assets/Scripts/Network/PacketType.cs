using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Server -> client
public enum ServerPackets
{
    SConnectionOK = 1,
    SConnectedPlayerCount = 2,
    SNewPlayerConnected = 3,
    SPlayerDisconnected = 4,
    SWorldSnapshot = 5,
    SStartMatch = 6,
    SPlayerShoot = 7,
}

//client -> server
public enum ClientPackets
{
    CConnectionOK = 1,
    CInputs = 2,
    CFire = 3,
    CReadySignal = 4,
}