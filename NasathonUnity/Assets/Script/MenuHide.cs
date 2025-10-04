using UnityEngine;

public class MenuHide : MonoBehaviour
{
    public GameObject menu;
    public GameObject scroller;

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
        scroller.SetActive(!scroller.activeSelf);
    }
}
