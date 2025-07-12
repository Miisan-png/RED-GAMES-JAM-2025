using UnityEngine;

public class Parallax_Manager : MonoBehaviour
{
    public static Parallax_Manager activeParallax;

    public void Activate()
    {
        if (activeParallax != null && activeParallax != this)
            activeParallax.gameObject.SetActive(false);

        gameObject.SetActive(true);
        activeParallax = this;
    }
}
