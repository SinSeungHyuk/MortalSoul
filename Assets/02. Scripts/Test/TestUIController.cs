using Cysharp.Threading.Tasks;
using MS.Field;
using MS.UI.HUD;
using MS.Utils;
using UnityEngine;
using UnityEngine.UI;

public class TestUIController : MonoBehaviour
{
    private Button btnAttack;
    private Button btnDash;
    private Button btnJump;
    private HUDInteractButton btnInteract;

    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        btnAttack = transform.FindChildComponentDeep<Button>("btnAttack");
        btnDash = transform.FindChildComponentDeep<Button>("btnDash");
        btnJump = transform.FindChildComponentDeep<Button>("btnJump");
        btnInteract = transform.FindChildComponentDeep<HUDInteractButton>("btnInteract");
    }

    public void InitTest()
    {
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();

        btnAttack.onClick.AddListener(() =>
        {
            if (playerCharacter != null)
                playerCharacter.BSC.UseSkill("TestOneHandAttack").Forget();
        });

        btnInteract.InitTest();
    }
}
