
using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

[Serializable]
public class TankManager
{
    public Color m_PlayerColor;            
    public Transform SpawnPoint;         
    [HideInInspector] public int Index;
    [HideInInspector] public bool isPlayerControlled;
    [HideInInspector] public string ColoredPlayerText;
    [HideInInspector] public GameObject Instance;          
    public int Wins;
    [HideInInspector] public GameManager GameManager;

    private TankMovement movement;       
    private TankShooting shooting;
    private TankHealth health;
    private GameObject canvasgameObject;

    private bool positionUpdateRequired;
    private Vector3 updatedPosition;
    private Quaternion updatedRotation;
    private Vector3 updatedVelocity;

    public void Setup()
    {
        movement = Instance.GetComponent<TankMovement>();
        shooting = Instance.GetComponent<TankShooting>();
        health = Instance.GetComponent<TankHealth>();
        canvasgameObject = Instance.GetComponentInChildren<Canvas>().gameObject;

        movement.Manager = this;
        shooting.Manager = this;
        health.Manager = this;

        positionUpdateRequired = false;

        ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + Index + "</color>";

        MeshRenderer[] renderers = Instance.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = m_PlayerColor;
        }
    }

    public void DisableControl()
    {
        movement.enabled = false;
        shooting.enabled = false;

        canvasgameObject.SetActive(false);
    }

    public void EnableControl()
    {
        movement.enabled = true;
        shooting.enabled = true;

        canvasgameObject.SetActive(true);
    }

    public void Reset()
    {
        Instance.transform.position = SpawnPoint.position;
        Instance.transform.rotation = SpawnPoint.rotation;

        Instance.SetActive(false);
        Instance.SetActive(true);
    }

    public void UpdateTankData(TankData data)
    {
      //  Wins = data.Wins;
    
        health.CurrentHealth = data.Health;

        if (health.CurrentHealth <= 0f && !health.Dead)
        {
      //      health.OnDeath();
            return;
        }

      //  health.Dead = data.Dead;       

        updatedPosition = data.Position;
        updatedRotation = data.Rotation;
        updatedVelocity = data.Velocity;

        movement.Speed = data.Speed;
        movement.TurnSpeed = data.TurnSpeed;

        if (!isPlayerControlled)
        {
            UpdateInputs(data.Inputs);
        }

        movement.UpdateTransformValues(updatedPosition, updatedRotation, updatedVelocity);
    }

    public void UpdateInputs(Input inputs)
    {
        movement.UpdateInputs(inputs);
    }

    public void Fire(float chargeValue)
    {
        shooting.CurrentLaunchForce = chargeValue;
        shooting.needToFire = true;
    }

    public Input GetInputInfo()
    {
        var input = new Input();
        input.Index = this.Index;

        movement.GetMovementInputInfo(out input.Movement, out input.Rotating, out input.DeltaTime);

        return input;
    }

}
