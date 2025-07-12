using UnityEngine;
using System.Collections.Generic;

public class Player_Animation_System : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [System.Serializable]
    public class AnimationClip
    {
        public string name;
        public List<Sprite> frames;
        public float frameRate = 10f;
        public bool loop = true;
    }

    public List<AnimationClip> animations;

    private Dictionary<string, AnimationClip> animationDict;
    private AnimationClip currentClip;
    private int currentFrame;
    private float timer;

    void Awake()
    {
        animationDict = new Dictionary<string, AnimationClip>();
        foreach (var clip in animations)
        {
            animationDict[clip.name] = clip;
        }
    }

    void Update()
    {
        if (currentClip == null || currentClip.frames.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / currentClip.frameRate)
        {
            timer = 0f;
            currentFrame++;
            if (currentFrame >= currentClip.frames.Count)
            {
                if (currentClip.loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentClip.frames.Count - 1;
                }
            }
            spriteRenderer.sprite = currentClip.frames[currentFrame];
        }
    }

    public void Play(string animationName)
    {
        if (currentClip != null && currentClip.name == animationName) return;
        if (!animationDict.ContainsKey(animationName)) return;
        currentClip = animationDict[animationName];
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = currentClip.frames[0];
    }
}
