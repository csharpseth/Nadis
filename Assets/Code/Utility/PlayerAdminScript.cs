using UnityEngine;

public class PlayerAdminScript : MonoBehaviour
{
    private void Awake()
    {
        Events.PlayerStats.OnAlterHealth += AlterHealth;
    }

    private void AlterHealth(int playerID, float perc, bool send)
    {
        //Debug.LogFormat("Player:{0}  Health Alterd To:{1}  Sent:{2}", playerID, perc, send);
    }

    private void Update()
    {
        

        if(Input.GetKeyDown(KeyCode.Y))
        {
            Events.PlayerStats.Heal(NetworkManager.LocalPlayer.ID, 5, true);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Events.PlayerStats.Damage(NetworkManager.LocalPlayer.ID, 25, true);
        }
    }
}
