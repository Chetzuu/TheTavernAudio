using UnityEngine;
using FMODUnity;

/// <summary>
/// Zarządza odtwarzaniem dźwięków kroków, skoków i lądowania w zależności od powierzchni.
/// </summary>
public class Footsteps : MonoBehaviour
{
    // FMOD - Instancje zdarzeń.
    private FMOD.Studio.EventInstance footstepsSoundInstance;
    private FMOD.Studio.EventInstance jumpSoundInstance;
    private FMOD.Studio.EventInstance landSoundInstance;

    // Publiczne referencje do zdarzeń FMOD.
    public EventReference footstepsEvent;
    public EventReference jumpEvent;
    public EventReference landEvent;

    private float lastFootstepTime = 0f;
    private float distToGround;

    [SerializeField]
    private bool isGrounded = true;
    [SerializeField]
    private bool isJumping = false;

    void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    void Update()
    {
        // Sprawdza, czy gracz skacze, używając spacji.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayJump();
        }
    }

    void FixedUpdate()
    {
        HandleFootsteps();
    }

    /// <summary>
    /// Obsługuje logikę odtwarzania dźwięków kroków.
    /// </summary>
    private void HandleFootsteps()
    {
        // Sprawdza, czy gracz się porusza.
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);
        
        // Sprawdza, czy gracz biegnie.
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isMoving && IsGrounded())
        {
            // Ustawia interwał na podstawie tego, czy gracz biegnie.
            float footstepInterval = isRunning ? 0.25f : 0.5f;

            if (Time.time - lastFootstepTime > footstepInterval)
            {
                lastFootstepTime = Time.time;
                
                // NOWE: Przekazujemy informację o bieganiu do funkcji!
                PlayFootsteps(isRunning); 
            }
        }
    }

    /// <summary>
    /// Odtwarza dźwięk kroków w zależności od powierzchni.
    /// </summary>
    // NOWE: Funkcja teraz przyjmuje zmienną isRunning
    private void PlayFootsteps(bool isRunning) 
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
        {
            string surfaceTag = hit.collider.tag;
            
            // NOWE: Przekazujemy isRunning dalej do odtwarzacza
            PlaySurfaceSound(footstepsSoundInstance, footstepsEvent, surfaceTag, isRunning);
        }
    }

    /// <summary>
    /// Odtwarza dźwięk skoku.
    /// </summary>
    private void PlayJump()
    {
        if (IsGrounded())
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
            {
                string surfaceTag = hit.collider.tag;
                PlaySurfaceSound(jumpSoundInstance, jumpEvent, surfaceTag); // Skok nie używa biegu
            }
            isGrounded = false;
            isJumping = true;
        }
    }

    /// <summary>
    /// Obsługuje dźwięk lądowania po skoku.
    /// </summary>
    private void OnCollisionEnter(Collision col)
    {
        if (!isGrounded && isJumping)
        {
            PlayLanding();
        }
    }

    /// <summary>
    /// Odtwarza dźwięk lądowania.
    /// </summary>
    private void PlayLanding()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
        {
            string surfaceTag = hit.collider.tag;
            PlaySurfaceSound(landSoundInstance, landEvent, surfaceTag); // Lądowanie nie używa biegu
        }
        isGrounded = true;
        isJumping = false;
    }

    /// <summary>
    /// Ogólna metoda do odtwarzania dźwięku na podstawie tagu powierzchni.
    /// </summary>
    // NOWE: Funkcja opcjonalnie przyjmuje zmienną isRunning (domyślnie false dla skoków i lądowań)
    private void PlaySurfaceSound(FMOD.Studio.EventInstance soundInstance, EventReference eventRef, string surfaceTag, bool isRunning = false)
    {
        // LOG 1: Sprawdza w co dokładnie uderzył Raycast
        Debug.Log("<color=cyan>Raycast trafił w obiekt z tagiem: </color><b>" + surfaceTag + "</b>");

        string surfaceParameter = null; 

        // Instrukcja SWITCH do mapowania Tagu na Parametr FMOD.
        switch (surfaceTag)
        {
            case "Stone":
            case "Inside_stone":
            case "Outside":
                surfaceParameter = "Stone";
                break;
            
            case "Wood":
            case "Inside_wood":
                surfaceParameter = "Wood";
                break;

            case "Stairs":
                surfaceParameter = "Stairs";
                break;
                
            default:
                Debug.Log("<color=orange>Brak tagu na liście! Ustawiam awaryjnie: Wood</color>");
                surfaceParameter = "Wood";
                break;
        }

        if (surfaceParameter != null)
        {
            soundInstance = RuntimeManager.CreateInstance(eventRef);
            soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
            
            // LOG 2: Sprawdza, co skrypt próbuje przekazać do FMODa
            Debug.Log("<color=green>Wysyłam do FMOD powierzchnię: </color>" + surfaceParameter);

            // Wysyła parametr podłoża
            FMOD.RESULT result = soundInstance.setParameterByNameWithLabel("Manager_Footsteps", surfaceParameter); 
            
            // NOWE: Wysyłanie trybu chód/bieg
            string modeLabel = isRunning ? "Run" : "Walk";
            Debug.Log("<color=yellow>Wysyłam do FMOD tryb: </color>" + modeLabel);
            
            // UWAGA: Jeśli zmieniłeś nazwę "Parameter 2" w FMODzie, musisz ją podmienić również tutaj!
            soundInstance.setParameterByNameWithLabel("Parameter 2", modeLabel);

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogError("<color=red>FMOD BŁĄD: </color>" + result);
            }

            soundInstance.start();
            soundInstance.release();
        }
    }

    /// <summary>
    /// Sprawdza, czy gracz znajduje się na podłożu.
    /// </summary>
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, distToGround + 0.5f);
    }  
}