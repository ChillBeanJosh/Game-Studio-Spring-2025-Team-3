using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITestingController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toggleStatusText;
    [SerializeField] private TextMeshProUGUI chargeStatusText;
    [SerializeField] private TextMeshProUGUI strongChargeReadyText;
    [SerializeField] private Image reticle;
    [SerializeField] private Sprite positive;
    [SerializeField] private Sprite negative;


    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private Projectile projectile;

    private void Update()
    {
        UpdateToggleStatus();
        UpdateChargeStatus();
        UpdateStrongChargeReadyStatus();
        UpdateReticleStatus();
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

    private void UpdateReticleStatus()
    {
        if (reticle != null && playerCharacter != null)
        {
            reticle.sprite = playerCharacter.positiveCharge ? positive : negative;
        }

    }
}
