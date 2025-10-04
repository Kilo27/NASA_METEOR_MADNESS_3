using UnityEngine;

public class MenuHide : MonoBehaviour
{
    public GameObject menu;

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }
}
