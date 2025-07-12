using UnityEngine;

public class Player_Animation_System : MonoBehaviour
{
    public Sprite[] animationFrames;
    public float frameRate = 10f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (animationFrames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[currentFrame];
            timer = 0f;
        }
    }
}
