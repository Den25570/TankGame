
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankData
{
    //Network
    public int index;

    //transform
    public Vector3 Position;
    public Vector3 Forward;
    public Quaternion Rotation;
    public Vector3 Velocity;

    //Player info
    public float Health;
    public bool Dead;
    public float Speed;
    public float TurnSpeed;
    public int Wins;

    public Input Inputs;

    public TankData() { }

    public TankData(TankManager TankManager)
    {
        var TankInstance = TankManager.Instance;
        var movement = TankInstance.GetComponent<TankMovement>();
        var health = TankInstance.GetComponent<TankHealth>();
        var shooting = TankInstance.GetComponent<TankShooting>();

        Inputs = TankManager.GetInputInfo();

        Position = TankInstance.transform.position;
        Forward = TankInstance.transform.forward;
        Rotation = TankInstance.transform.rotation;
        Velocity = TankInstance.GetComponent<Rigidbody>().velocity;

        Health = health.CurrentHealth;
        Dead = health.Dead;
        Speed = movement.Speed;
        TurnSpeed = movement.TurnSpeed;
        Wins = TankManager.Wins;

        index = TankManager.Index;
    }
}


