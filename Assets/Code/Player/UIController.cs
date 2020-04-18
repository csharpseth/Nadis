using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image powerBar;
    
    private void Awake()
    {
        Events.PlayerStats.OnAlterPower += PowerChange;
    }

    private void PowerChange(int id, float amt, bool send)
    {
        if (id != Events.Player.GetLocalID()) return;
        
        powerBar.fillAmount = amt;
    }
}
