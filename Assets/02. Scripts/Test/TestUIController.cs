using MS.Field;
using MS.Utils;
using UnityEngine;
using UnityEngine.UI;

public class TestUIController : MonoBehaviour
{
    private Button btnAttack;
    private Button btnDash;
    private Button btnJump;

    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        btnAttack = transform.FindChildComponentDeep<Button>("btnAttack");
        btnDash = transform.FindChildComponentDeep<Button>("btnDash");
        btnJump = transform.FindChildComponentDeep<Button>("btnJump");
    }

    private void Start()
    {
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();

        btnAttack.onClick.AddListener(() =>
        {
            if (playerCharacter != null)
                playerCharacter.BSC.UseSkill("TestOneHandAttack");
        });
    }
}
