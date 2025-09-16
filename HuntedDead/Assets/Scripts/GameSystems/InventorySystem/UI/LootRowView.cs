using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootRowView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameTxt, qtyTxt;
    public Button takeBtn, takeAllBtn;
    [HideInInspector] public int index;
}
