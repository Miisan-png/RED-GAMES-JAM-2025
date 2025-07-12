using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AnimationClip
{
    public string name;
    public Sprite[] frames;
    public float frameRate = 10f;
    public bool loop = true;
}

public class Player_Animation_System : MonoBehaviour
{
    public AnimationClip[] animationClips;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    private AnimationClip currentAnimation;
    private Dictionary<string, AnimationClip> animationDict;
    private bool isPlaying = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        animationDict = new Dictionary<string, AnimationClip>();
        foreach (AnimationClip clip in animationClips)
        {
            animationDict[clip.name] = clip;
        }
        
        if (animationClips.Length > 0)
        {
            currentAnimation = animationClips[0];
        }
    }

    void Update()
    {
        if (currentAnimation == null || currentAnimation.frames.Length == 0 || !isPlaying) return;

        timer += Time.deltaTime;
        if (timer >= 1f / currentAnimation.frameRate)
        {
            currentFrame++;
            
            if (currentFrame >= currentAnimation.frames.Length)
            {
                if (currentAnimation.loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.frames.Length - 1;
                    isPlaying = false;
                }
            }
            
            spriteRenderer.sprite = currentAnimation.frames[currentFrame];
            timer = 0f;
        }
    }

    public void PlayAnimation(string animationName)
    {
        if (animationDict.ContainsKey(animationName))
        {
            currentAnimation = animationDict[animationName];
            currentFrame = 0;
            timer = 0f;
            isPlaying = true;
            
            if (currentAnimation.frames.Length > 0)
            {
                spriteRenderer.sprite = currentAnimation.frames[0];
            }
        }
    }

    public void StopAnimation()
    {
        isPlaying = false;
    }

    public void ResumeAnimation()
    {
        isPlaying = true;
    }

    public bool IsAnimationPlaying()
    {
        return isPlaying;
    }

    public string GetCurrentAnimationName()
    {
        return currentAnimation != null ? currentAnimation.name : "";
    }
}