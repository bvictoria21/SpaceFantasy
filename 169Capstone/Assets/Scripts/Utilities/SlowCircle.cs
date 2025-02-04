using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SlowCircle : MonoBehaviour
{
    public bool canSlow
    {
        set {
            if(_canSlow != value)
            {
                foreach(Collider target in slowTargets)
                {
                    toggleSlow(target, value);
                }
                foreach(Collider target in speedTargets)
                {
                    toggleSpeed(target, value);
                }
            }
            _canSlow = value;
        }

        get { return _canSlow; }
    }

    private Object creator;
    private HashSet<Collider> slowTargets;
    private HashSet<Collider> speedTargets;
    private int enemyLayer;
    private int friendlyLayer;
    private float speedChangePercent;
    private float damageInterval;
    private float lifetime;
    private bool _canSlow;

    void Awake()
    {
        slowTargets = new HashSet<Collider>();
        speedTargets = new HashSet<Collider>();
    }

    public void Initialize(Object creator, string enemyLayer, string friendlyLayer, float speedChangePercent, float lifetime, float radius = 1, float damageInterval = 0.5f, float fadeInDuration = 1)
    {
        Initialize(creator, LayerMask.NameToLayer(enemyLayer), LayerMask.NameToLayer(friendlyLayer), speedChangePercent, lifetime, radius, damageInterval, fadeInDuration);
    }

    public void Initialize(Object creator, int enemyLayer, int friendlyLayer, float speedChangePercent, float lifetime, float radius = 1, float damageInterval = 0.5f, float fadeInDuration = 1)
    {
        this.creator = creator;
        this.enemyLayer = enemyLayer;
        this.friendlyLayer = friendlyLayer;
        this.speedChangePercent = speedChangePercent;
        this.damageInterval = damageInterval;
        this.lifetime = lifetime;

        transform.localScale = Vector3.one * radius;

        _canSlow = false;

        StartCoroutine(fadeSprite(fadeInDuration, true));
        StartCoroutine(destroySelfCountdown(lifetime, fadeInDuration));
    }

    void OnTriggerStay(Collider other)
    {
        Movement moveScript = other.GetComponent<Movement>();
        Projectile projectileScript = other.GetComponent<Projectile>();

        if(moveScript != null)
        {
            if(other.gameObject.layer == enemyLayer)
            {
                slowTargets.Add(other);
                toggleSlow(other, true);
            }
            else if(other.gameObject.layer == friendlyLayer)
            {
                speedTargets.Add(other);
                toggleSpeed(other, true);
            }
        }
        else if(!slowTargets.Contains(other) && !speedTargets.Contains(other))
        {
            if(other.gameObject.layer == enemyLayer)
            {
                slowTargets.Add(other);
                toggleSlow(other, true);
            }
            else if(other.gameObject.layer == friendlyLayer)
            {
                speedTargets.Add(other);
                toggleSpeed(other, true);
            }
            else if(projectileScript != null)
            {
                if(projectileScript.enemyLayer == enemyLayer)
                {
                    speedTargets.Add(other);
                    toggleSpeed(other, true);
                }
                else
                {
                    slowTargets.Add(other);
                    toggleSlow(other, true);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(slowTargets.Contains(other))
        {
            toggleSlow(other, false);
            slowTargets.Remove(other);
        }
        
        if(speedTargets.Contains(other))
        {
            toggleSpeed(other, false);
            speedTargets.Remove(other);
        }
    }

    private IEnumerator fadeSprite(float duration, bool fadeIn)
    {
        float fadeProgress = 0;
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        VisualEffect vfx = GetComponentInChildren<VisualEffect>();
        if(renderer)
        {
            while(fadeProgress < 1)
            {
                fadeProgress += Time.deltaTime/duration;
                Color newColor = renderer.material.color;
                newColor.a = fadeIn ? fadeProgress : 1 - fadeProgress;
                renderer.material.color = newColor;
                yield return null;
            }
        }
        else if(vfx)
        {
            if(fadeIn)
            {
                // Set the vfx lifetime to the lifetime of the slowCircle
                vfx.SetFloat("GDLifetime", lifetime);
            }
            else
            {
                vfx.Stop();
            }
        }
        _canSlow = fadeIn;
    }

    private IEnumerator destroySelfCountdown(float lifetime, float fadeOutDuration = 0)
    {
        yield return new WaitForSeconds(lifetime - fadeOutDuration);
        StartCoroutine(fadeSprite(fadeOutDuration, false));
        yield return new WaitForSeconds(fadeOutDuration);

        foreach(Collider target in slowTargets)
        {
            toggleSlow(target, false);
        }
        slowTargets.Clear();

        foreach(Collider target in speedTargets)
        {
            toggleSpeed(target, false);
        }
        speedTargets.Clear();

        Destroy(gameObject);
    }

    private void toggleSlow(Collider target, bool enable)
    {
        if(target == null)
            return;

        Movement moveScript = target.GetComponent<Movement>();
        Pathing pathScript = target.GetComponent<Pathing>();
        Projectile projectileScript = target.GetComponent<Projectile>();

        if(moveScript != null)
        {
            if(enable)
                Player.instance.stats.SetBonusForStat(creator, StatType.MoveSpeed, EntityStats.BonusType.multiplier, -speedChangePercent);
            else
                Player.instance.stats.SetBonusForStat(creator, StatType.MoveSpeed, EntityStats.BonusType.multiplier, 0);
        }
        else if(pathScript != null)
        {
            if(enable)
                pathScript.speed *= (1 - speedChangePercent);
            else
                pathScript.speed /= (1 - speedChangePercent);
        }
        else if(projectileScript != null)
        {
            if(enable)
                projectileScript.speed *= (1 - speedChangePercent);
            else
                projectileScript.speed /= (1 - speedChangePercent);
        }
    }

    private void toggleSpeed(Collider target, bool enable)
    {
        if(target == null)
            return;
        
        Movement moveScript = target.GetComponent<Movement>();
        Pathing pathScript = target.GetComponent<Pathing>();
        Projectile projectileScript = target.GetComponent<Projectile>();

        if(moveScript != null)
        {
            if(enable)
                Player.instance.stats.SetBonusForStat(creator, StatType.MoveSpeed, EntityStats.BonusType.multiplier, speedChangePercent);
            else
                Player.instance.stats.SetBonusForStat(creator, StatType.MoveSpeed, EntityStats.BonusType.multiplier, 0);
        }
        else if(pathScript != null)
        {
            if(enable)
                pathScript.speed *= (1 + speedChangePercent);
            else
                pathScript.speed /= (1 + speedChangePercent);
        }
        else if(projectileScript != null)
        {
            if(enable)
                projectileScript.speed *= (1 + speedChangePercent);
            else
                projectileScript.speed /= (1 + speedChangePercent);
        }
    }
}
