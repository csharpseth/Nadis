using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour, IDisableIfRemotePlayer
{
    public Transform healthBar;
    private Image healthBarFill;
    private TextMeshProUGUI healthBarText;

    public Transform powerBar;
    private Image powerBarFill;
    private TextMeshProUGUI powerBarText;

    private void Awake()
    {
        if(healthBar != null)
        {
            healthBarFill = healthBar.GetChild(0).GetComponent<Image>();
            healthBarText = healthBar.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if(powerBar != null)
        {
            powerBarFill = powerBar.GetChild(0).GetComponent<Image>();
            powerBarText = powerBar.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        Events.PlayerStats.OnAlterHealth += OnAlterHealth;
        Events.PlayerStats.OnAlterPower += OnAlterPower;
    }

    private void OnAlterHealth(float percent)
    {
        if(healthBar)
            healthBarFill.fillAmount = percent;
        if(healthBarText)
            healthBarText.text = "Health: " + Mathf.Round(100f * percent) + "%";
    }
    private void OnAlterPower(float percent)
    {
        if(powerBar)
            powerBarFill.fillAmount = percent;
        if(powerBarText)
            powerBarText.text = "Power: " + Mathf.Round(100f * percent) + "%";
    }

    private void OnDestroy()
    {
        Events.PlayerStats.OnAlterHealth -= OnAlterHealth;
        Events.PlayerStats.OnAlterPower -= OnAlterPower;
    }

    public void Disable(bool disabled)
    {
        this.enabled = !disabled;
    }
}
