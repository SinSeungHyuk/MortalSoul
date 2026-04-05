using UnityEngine;
using UnityEngine.UI;
using MS.Utils;

public class TestUIController : MonoBehaviour
{
    private Button btnAttack;
    private Button btnDash;
    private Button btnJump;

    private void Awake()
    {
        btnAttack = transform.FindChildComponentDeep<Button>("btnAttack");
        btnDash = transform.FindChildComponentDeep<Button>("btnDash");
        btnJump = transform.FindChildComponentDeep<Button>("btnJump");
    }
}
