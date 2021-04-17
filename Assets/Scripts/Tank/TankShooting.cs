using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    private int CurrentShellIndex = 0;

    public Rigidbody m_Shell;            
    public Transform m_FireTransform;    
    public Slider m_AimSlider;           
    public AudioSource m_ShootingAudio;  
    public AudioClip m_ChargingClip;     
    public AudioClip m_FireClip;         
    public float m_MinLaunchForce = 15f; 
    public float MaxLaunchForce = 30f; 
    public float m_MaxChargeTime = 0.75f;

    private string m_FireButton = "Fire1";
    public float CurrentLaunchForce;
    public float ChargeSpeed;
    public bool Fired;

    public bool isFireButtonDown = false;
    public bool isFireButtonUp = false;
    public bool isFireButton = false;
    public bool needToFire = false;

    public TankManager Manager;


    private void OnEnable()
    {
        CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        ChargeSpeed = (MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }

    private void Update()
    {
        if (needToFire)
            Fire();
        // Track the current state of the fire button and make decisions based on the current launch force.
        m_AimSlider.value = m_MinLaunchForce;
      
        if (Manager.isPlayerControlled)
        {
            isFireButtonDown = UnityEngine.Input.GetButtonDown(m_FireButton);
            isFireButton = UnityEngine.Input.GetButton(m_FireButton);
            isFireButtonUp = UnityEngine.Input.GetButtonUp(m_FireButton);
        }
        
        if (CurrentLaunchForce >= MaxLaunchForce && !Fired)
        {
            CurrentLaunchForce = MaxLaunchForce;
            Manager.GameManager.NetworkManager.Client.SendFireData(CurrentLaunchForce);
            Fire();
        }
        else if (isFireButtonDown)
        {
            Fired = false;
            CurrentLaunchForce = m_MinLaunchForce;

            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        else if (isFireButton && !Fired) 
        { 
            CurrentLaunchForce += ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = CurrentLaunchForce;
        }
        else if (isFireButtonUp && !Fired)
        {
            Manager.GameManager.NetworkManager.Client.SendFireData(CurrentLaunchForce);
            Fire();
        }
    }


    public void Fire()
    {
        Fired = true;
        needToFire = false;

        Rigidbody shellinstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        var shellExplosion = shellinstance.GetComponent<ShellExplosion>();
        shellExplosion.Index = CurrentShellIndex++;

        shellinstance.velocity = CurrentLaunchForce * m_FireTransform.forward;

        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        CurrentLaunchForce = m_MinLaunchForce;
    }
}