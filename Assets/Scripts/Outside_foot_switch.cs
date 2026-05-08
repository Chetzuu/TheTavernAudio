using UnityEngine;
using FMODUnity;

public class Outside_foot_switch : MonoBehaviour
{
    [SerializeField] private bool snapshotActivated = false;
    [SerializeField] private string outsideTag = "Outside"; // Upewnij się, że podłoże na zewnątrz ma ten tag!
    
    private float distToGround;
    private FMOD.Studio.EventInstance outsideSnapshotInstance;
    public EventReference outsideSnapshot;

    void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    void FixedUpdate()
    {
        CheckSurface();
    }

    private void CheckSurface()
    {
        RaycastHit hit;
        float maxDistance = distToGround + 0.5f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance))
        {
            bool isOutside = hit.collider.CompareTag(outsideTag);

            // Aktywuj tylko, gdy następuje zmiana stanu
            if (isOutside && !snapshotActivated)
            {
                ToggleSnapshot(true);
            }
            else if (!isOutside && snapshotActivated)
            {
                ToggleSnapshot(false);
            }
        }
    }

    private void ToggleSnapshot(bool activate)
    {
        if (activate)
        {
            // Tworzymy instancję tylko jeśli nie jest już odpalona
            if (!outsideSnapshotInstance.isValid())
            {
                outsideSnapshotInstance = RuntimeManager.CreateInstance(outsideSnapshot);
            }
            outsideSnapshotInstance.start();
            snapshotActivated = true;
            Debug.Log("FMOD: Snapshot Outside WŁĄCZONY");
        }
        else
        {
            if (outsideSnapshotInstance.isValid())
            {
                outsideSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                // Nie zwalniaj (release) jeśli chcesz używać tej samej instancji, 
                // albo zwalniaj i zeruj, żeby stworzyć nową.
                outsideSnapshotInstance.release(); 
            }
            snapshotActivated = false;
            Debug.Log("FMOD: Snapshot Outside WYŁĄCZONY");
        }
    }

    private void OnDestroy()
    {
        // Sprzątanie, żeby dźwięk nie grał po wyjściu z gry
        if (outsideSnapshotInstance.isValid())
        {
            outsideSnapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            outsideSnapshotInstance.release();
        }
    }
}