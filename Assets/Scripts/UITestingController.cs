using TMPro;
using UnityEngine;

public class UITestingController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toggleStatusText;
    [SerializeField] private TextMeshProUGUI chargeStatusText;
    [SerializeField] private PlayerCharacter playerCharacter;

    private void Update()
    {
        UpdateToggleStatus();
        UpdateChargeStatus();
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
}
