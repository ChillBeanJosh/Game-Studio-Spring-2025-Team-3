using TMPro;
using UnityEngine;

public class UITestingController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toggleStatusText;
    [SerializeField] private TextMeshProUGUI chargeStatusText;
    [SerializeField] private TextMeshProUGUI strongChargeReadyText; 

    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private Projectile projectile;

    private void Update()
    {
        UpdateToggleStatus();
        UpdateChargeStatus();
        UpdateStrongChargeReadyStatus();
    }

    private void UpdateToggleStatus()
    {
        if (toggleStatusText != null && playerCharacter != null)
        {
            toggleStatusText.color = playerCharacter.chargeToggle ? Color.green : Color.white;
        }
    }

    private void UpdateChargeStatus()
    {
        if (chargeStatusText != null && playerCharacter != null)
        {
            chargeStatusText.color = playerCharacter.positiveCharge ? Color.red : Color.blue;
        }
    }

    private void UpdateStrongChargeReadyStatus()
    {
        if (strongChargeReadyText != null && projectile != null)
        {
            strongChargeReadyText.color = projectile.currentChargeTime >= projectile.chargeTimeRequired ? Color.green : Color.red;
        }
    }
}
