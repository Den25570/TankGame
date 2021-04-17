using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;          
    public Slider Slider;                        
    public Image FillImage;                      
    public Color FullHealthColor = Color.green;  
    public Color ZeroHealthColor = Color.red;    
    public GameObject m_ExplosionPrefab;
    
    private AudioSource ExplosionAudio;          
    private ParticleSystem ExplosionParticles;   
    public float CurrentHealth;
    public bool Dead;

    public TankManager Manager;


    private void Awake()
    {
        ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        ExplosionAudio = ExplosionParticles.GetComponent<AudioSource>();

        ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        CurrentHealth = m_StartingHealth;
        Dead = false;

        SetHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (Manager.GameManager.NetworkManager.isHost)    
            CurrentHealth -= amount;
    }

    private void Update()
    {
        SetHealthUI();
        if (CurrentHealth <= 0f && !Dead)
        {
            OnDeath();
        }
    }


    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        Slider.value = CurrentHealth;

        FillImage.color = Color.Lerp(ZeroHealthColor, FullHealthColor, CurrentHealth / m_StartingHealth);
    }


    public void OnDeath()
    {
        // Play the effects for the death of the tank and deactivate it.
        Dead = true;

        ExplosionParticles.transform.position = transform.position;
        ExplosionParticles.gameObject.SetActive(true);
        ExplosionParticles.Play();
        ExplosionAudio.Play();
        gameObject.SetActive(false);
    }
}