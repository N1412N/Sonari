using UnityEngine;

public class NPCArrowGuide : MonoBehaviour
{
    public Transform[] npcTargets;   // Drag your NPCs here
    public Transform player;         // Drag your Player here
    public Transform arrow3D;        // Drag the 3D arrow object here

    private int currentTargetIndex = 0;

    void Update()
    {
        if (npcTargets.Length == 0 || arrow3D == null || player == null) return;

        // Direction from player to current NPC
        Vector3 dir = npcTargets[currentTargetIndex].position - player.position;
        dir.y = 0; // ignore vertical difference

        // Rotate arrow to face NPC
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            arrow3D.rotation = targetRotation;
        }
    }

    // Call this when player finishes interacting with current NPC
    public void NextNPC()
    {
        currentTargetIndex++;
        if (currentTargetIndex >= npcTargets.Length)
        {
            arrow3D.gameObject.SetActive(false); // hide arrow when done
        }
    }
}
