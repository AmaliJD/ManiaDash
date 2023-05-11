using System.Collections;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;

public static class DOTweenExtensions1
{
    public static Tweener DOMove(this Transform target, Transform To, Vector3 offset, float scale, float duration, bool snapping = false)
    {
        Vector3 StartPos = target.position;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.position = Vector3.LerpUnclamped(StartPos, To.position * scale + offset, t);
        }
    }
    public static Tweener DOMoveX(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.position.x;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.position = target.position.SetX(Mathf.LerpUnclamped(StartPos, To.position.x + offset, t));
        }
    }
    public static Tweener DOMoveY(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.position.y;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.position = target.position.SetY(Mathf.LerpUnclamped(StartPos, To.position.y + offset, t));
        }
    }
    public static Tweener DOMoveZ(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.position.z;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.position = target.position.SetZ(Mathf.LerpUnclamped(StartPos, To.position.z + offset, t));
        }
    }

    public static Tweener DOLocalMove(this Transform target, Transform To, Vector3 offset, float duration, bool snapping = false)
    {
        Vector3 StartPos = target.localPosition;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.localPosition = Vector3.LerpUnclamped(StartPos, To.localPosition + offset, t);
        }
    }
    public static Tweener DOLocalMoveX(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.localPosition.x;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.localPosition = target.localPosition.SetX(Mathf.LerpUnclamped(StartPos, To.localPosition.x + offset, t));
        }
    }
    public static Tweener DOLocalMoveY(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.localPosition.y;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.localPosition = target.localPosition.SetY(Mathf.LerpUnclamped(StartPos, To.localPosition.y + offset, t));
        }
    }
    public static Tweener DOLocalMoveZ(this Transform target, Transform To, float offset, float duration, bool snapping = false)
    {
        float StartPos = target.localPosition.z;
        return DOFloat(lerp, 0, 1, duration, snapping);
        void lerp(float t)
        {
            target.localPosition = target.localPosition.SetZ(Mathf.LerpUnclamped(StartPos, To.localPosition.z + offset, t));
        }
    }


    // FLOAT, VECOTR2, VECOTR3
    public static Tweener DOFloat(Action<float> Set, float start, float end, float time, bool snapping = false)
    {
        return DOTween.To(() => start, x => Set.Invoke(x), end, time).SetOptions(snapping);
    }
    public static Tweener DOVector2(Action<Vector2> Set, Vector2 start, Vector2 end, float time, bool snapping = false)
    {
        return DOTween.To(() => start, x => Set.Invoke(x), end, time).SetOptions(snapping);
    }
    public static Tweener DOVector3(Action<Vector3> Set, Vector3 start, Vector3 end, float time, bool snapping = false)
    {
        return DOTween.To(() => start, x => Set.Invoke(x), end, time).SetOptions(snapping);
    }

    public static Tweener DOBlendableVector2By(Action<Vector2> AddDelta, Vector2 By, float Time, bool Snapping)
    {
        Vector2 to = Vector2.zero;
        return DOTween.To(() => to, x => {
            Vector2 diff = x - to;
            to = x;
            AddDelta?.Invoke(diff);
        }, By, Time)
            .Blendable().SetOptions(Snapping);
    }
    public static Tweener DOBlendableVector3By(Action<Vector3> AddDelta, Vector3 By, float Time, bool Snapping)
    {
        Vector3 to = Vector3.zero;
        return DOTween.To(() => to, x => {
            Vector3 diff = x - to;
            to = x;
            AddDelta?.Invoke(diff);
        }, By, Time)
            .Blendable().SetOptions(Snapping);
    }
    public static Tweener DOBlendableVector4By(Action<Vector4> AddDelta, Vector4 By, float Time, bool Snapping)
    {
        Vector4 to = Vector4.zero;
        return DOTween.To(() => to, x => {
            Vector4 diff = x - to;
            to = x;
            AddDelta?.Invoke(diff);
        }, By, Time)
            .Blendable().SetOptions(Snapping);
    }
    public static Tweener DOBlendableFloatBy(Action<float> AddDelta, float By, float Time, bool Snapping)
    {
        float to = 0;
        return DOTween.To(() => to, x => {
            float diff = x - to;
            to = x;
            AddDelta?.Invoke(diff);
        }, By, Time)
            .Blendable().SetOptions(Snapping);
    }
    public static Tweener DOBlendableIntBy(Action<int> AddDelta, int By, float Time)
    {
        int to = 0;
        return DOTween.To(() => to, x => {
            int diff = x - to;
            to = x;
            AddDelta?.Invoke(diff);
        }, By, Time)
            .Blendable();
    }


    // PARTICLES
    public static Tweener DOMoveParticles(this ParticleSystem target, Vector3 endValue, float duration, bool snapping = false)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[target.particleCount];
        target.GetParticles(particles);
        Vector3[] LastPositions = new Vector3[particles.Length];

        for (int i = 0; i < particles.Length; i++)
        {
            LastPositions[i] = particles[i].position;
        }


        var t = DOFloat(set =>
        {
            for (int i = 0; i < particles.Length; i++)
            {
                Vector3 NowPos = Vector3.Lerp(LastPositions[i], endValue, set);
                particles[i].position = NowPos;
            }
            target.SetParticles(particles);

        }, 0, 1, duration, snapping);

        t.SetTarget(target);

        return t;
    }
    public static Tweener DOMoveParticlesX(this ParticleSystem target, float endValue, float duration, bool snapping = false)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[target.particleCount];
        target.GetParticles(particles);
        Vector3[] LastPositions = new Vector3[particles.Length];

        for (int i = 0; i < particles.Length; i++)
        {
            LastPositions[i] = particles[i].position;
        }


        var t = DOFloat(set =>
        {
            target.GetParticles(particles);
            for (int i = 0; i < particles.Length; i++)
            {
                float NowX = Mathf.Lerp(LastPositions[i].x, endValue, set);
                var parti = particles[i];
                parti.position = new Vector3(NowX, parti.position.y, parti.position.z);
            }
            target.SetParticles(particles);

        }, 0, 1, duration, snapping);

        t.SetTarget(target);

        return t;
    }
    public static Tweener DOMoveParticlesY(this ParticleSystem target, float endValue, float duration, bool snapping = false)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[target.particleCount];
        target.GetParticles(particles);
        Vector3[] LastPositions = new Vector3[particles.Length];

        for (int i = 0; i < particles.Length; i++)
        {
            LastPositions[i] = particles[i].position;
        }


        var t = DOFloat(set =>
        {
            target.GetParticles(particles);
            for (int i = 0; i < particles.Length; i++)
            {
                float NowY = Mathf.Lerp(LastPositions[i].y, endValue, set);
                var parti = particles[i];
                parti.position = new Vector3(parti.position.x, NowY, parti.position.z);
            }
            target.SetParticles(particles);

        }, 0, 1, duration, snapping);

        t.SetTarget(target);

        return t;
    }
    public static Tweener DOBlendableMoveParticlesBy(this ParticleSystem target, Vector3 byValue, float duration, bool snapping = false)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[target.particleCount];

        Vector3 to = Vector3.zero;
        return DOTween.To(() => to, x => {
            Vector3 diff = x - to;
            target.GetParticles(particles);
            to = x;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].position += diff;
            }
            target.SetParticles(particles);
        }, byValue, duration)
            .Blendable().SetOptions(snapping).SetTarget(target);
    }
}