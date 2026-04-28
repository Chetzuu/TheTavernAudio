using UnityEngine;
using FMODUnity;

/// <summary>
/// Zarządza aktywacją snapshotu 'Outside' na podstawie tagu powierzchni, na której znajduje się gracz.
/// </summary>
public class Outside_foot_switch : MonoBehaviour
{
    // Prywatna zmienna do edycji w Inspektorze.
    [SerializeField]
    private bool snapshotActivated = false;

    // Odległość do podłoża od środka kolidera.
    private float distToGround;

    // FMOD - Instancja snapshotu.
    private FMOD.Studio.EventInstance outsideSnapshotInstance;
    public EventReference outsideSnapshot;

    void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    void FixedUpdate()
    {
        ToggleSnapshotLogic();
    }

    /// <summary>
    /// Sprawdza, czy należy włączyć lub wyłączyć snapshot.
    /// </summary>
private void ToggleSnapshotLogic()
    {
        RaycastHit hit;
        float maxDistance = distToGround + 0.5f;

        // Rysuje czerwoną linię w oknie Scene, żebyś widział jak długi jest promień
        Debug.DrawRay(transform.position, Vector3.down * maxDistance, Color.red);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance))
        {
            string tag = hit.collider.tag;
            
            // Wypisuje w konsoli, w co dokładnie uderzył promień!
        }
    }

    /// <summary>
    /// Włącza lub wyłącza instancję snapshotu FMOD.
    /// </summary>
    /// <param name="activate">True, aby włączyć, false, aby wyłączyć.</param>
    private void ToggleSnapshot(bool activate)
    {
        if (activate)
        {
            // Tworzy i startuje instancję snapshotu.
            outsideSnapshotInstance = FMODUnity.RuntimeManager.CreateInstance(outsideSnapshot);
            outsideSnapshotInstance.start();
        }
        else
        {
            // Zatrzymuje i zwalnia instancję snapshotu, jeśli jest prawidłowa.
            if (outsideSnapshotInstance.isValid())
            {
                outsideSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                outsideSnapshotInstance.release();
            }
        }
        // Przełącza stan.
        snapshotActivated = activate;
    }
}