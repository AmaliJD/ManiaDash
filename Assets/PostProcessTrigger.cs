using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[ExecuteAlways]
public class PostProcessTrigger : MonoBehaviour
{
    static Dictionary<VolumeProfile, PPData> ActivePPTrigger = new Dictionary<VolumeProfile, PPData>();

    public class PPData
    {
        public PostProcessTrigger trigger;
        public Bloom bloom;
        public Vignette vig;
        public ShadowsMidtonesHighlights smh;
        public ChromaticAberration chrabr;
        public ColorAdjustments colorAdj;
        public FilmGrain filmgrain;
    }

    public VolumeProfile volumeProfile;

    public bool setBloom;
    public bool setThreshold;
    [Min(0)] [SerializeField] private float threshold;
    public bool setIntensity;
    [Min(0)] [SerializeField] private float intensity;
    public bool setScatter;
    [Range(0, 1)] [SerializeField] private float scatter;
    public bool setTint;
    [SerializeField] private Color tint;

    public bool setVignette;
    public bool setVigColor;
    [SerializeField] private Color vigColor;
    public bool setVigCenter;
    [Min(0)] [SerializeField] private Vector2 vigCenter;
    public bool setVigIntensity;
    [Range(0, 1)] [SerializeField] private float vigIntensity;
    public bool setVigSmoothness;
    [Range(0.01f, 1)] [SerializeField] private float vigSmoothness;

    public bool setChromaticAberration;
    public bool setChrAbrIntensity;
    [Range(0, 1)] [SerializeField] private float chrAbrIntensity;

    public bool setColorAdjustments;
    public bool setPostExposure;
    [SerializeField] private float postExposure;
    public bool setContrast;
    [Range(-100, 100)] [SerializeField] private float contrast;
    public bool setColorFilter;
    [ColorUsage(true, true)] [SerializeField] private Color colorFilter;
    public bool setHueShift;
    [Range(-180, 180)] [SerializeField] private float hueShift;
    public HueShiftDirection hueShiftDirection;
    public bool setSaturation;
    [Range(-100, 100)] [SerializeField] private float saturation;

    public enum HueShiftDirection { Nearest, Increase, Decrease }

    public bool setSMH; //shadow midtones highlights
    public bool setShadows;
    [ColorUsage(false)]
    [SerializeField] private Color shadows;
    [Range(-1, 1)] public float shadowsSlider;

    public bool setMidtones;
    [ColorUsage(false)]
    [SerializeField] private Color midtones;
    [Range(-1, 1)] public float midtonesSlider;

    public bool setHighlights;
    [ColorUsage(false)]
    [SerializeField] private Color highlights;
    [Range(-1, 1)] public float highlightsSlider;

    public bool setLimits;
    public float shadowLimitStart, shadowLimitEnd, highlightsLimitStart, highlightsLimitEnd;

    public bool setFilmGrain;
    public bool setFilmIntensity;
    [Range(0, 1)] [SerializeField] private float filmIntensity;
    public bool setFilmResponse;
    [Range(0, 1)] [SerializeField] private float filmResponse;

    [Min(0)] [SerializeField] private float duration;

    [SerializeField] private bool hideTexture;
    [SerializeField] private bool activateOnStart;
    [SerializeField] private bool activateOnDeath;
    [SerializeField] private bool activateOnRespawn;

    private void Awake()
    {
        if (hideTexture) { gameObject.transform.GetChild(0).gameObject.SetActive(false); }
    }

    private void Start()
    {
        if (!ActivePPTrigger.ContainsKey(volumeProfile))
        {
            //Debug.Log("adding pp data");
            AddPPData();
        }

        if (activateOnStart)
        {
            //Debug.Log("start");
            Activate();
        }
    }

    void AddPPData()
    {
        PPData data = new PPData();
        volumeProfile.TryGet(out data.bloom);
        volumeProfile.TryGet(out data.vig);
        volumeProfile.TryGet(out data.smh);
        volumeProfile.TryGet(out data.chrabr);
        volumeProfile.TryGet(out data.colorAdj);
        volumeProfile.TryGet(out data.filmgrain);
        ActivePPTrigger.Add(volumeProfile, data);
    }

    IEnumerator ModifyProfile()
    {
        float t = 0;
        float time = 0;
        bool overTime = false;

        // Bloom
        bool useBloom = setBloom && ActivePPTrigger[volumeProfile].bloom != null;
        float startThreshold = 0;
        float startIntensity = 0;
        float startScatter = 0;
        Color startTint = Color.white;
        if (useBloom)
        {
            startThreshold = (float)ActivePPTrigger[volumeProfile].bloom.threshold;
            startIntensity = (float)ActivePPTrigger[volumeProfile].bloom.intensity;
            startScatter = (float)ActivePPTrigger[volumeProfile].bloom.scatter;
            startTint = (Color)ActivePPTrigger[volumeProfile].bloom.tint;
        }

        // Vignette
        bool useVignette = setBloom && ActivePPTrigger[volumeProfile].vig != null;
        Color startVigColor = Color.white;
        Vector2 startVigCenter = Vector2.one * 0.5f;
        float startVigIntensity = 0;
        float startSmoothness = 0.01f;
        if (useVignette)
        {
            startVigColor = (Color)ActivePPTrigger[volumeProfile].vig.color;
            startVigCenter = (Vector2)ActivePPTrigger[volumeProfile].vig.center;
            startVigIntensity = (float)ActivePPTrigger[volumeProfile].vig.intensity;
            startSmoothness = (float)ActivePPTrigger[volumeProfile].vig.smoothness;
        }

        // Chromatic Aberration
        bool useChrAbr = setChromaticAberration && ActivePPTrigger[volumeProfile].chrabr != null;
        float startChrAbrIntensity = 0;
        if (useChrAbr)
        {
            startChrAbrIntensity = (float)ActivePPTrigger[volumeProfile].chrabr.intensity;
        }

        // Film Grain
        bool useFilmGrain = setFilmGrain && ActivePPTrigger[volumeProfile].filmgrain != null;
        float startFilmIntensity = 0;
        float startFilmResponse = 0;
        if (useFilmGrain)
        {
            startFilmIntensity = (float)ActivePPTrigger[volumeProfile].filmgrain.intensity;
            startFilmResponse = (float)ActivePPTrigger[volumeProfile].filmgrain.response;
        }

        // Color Adjustments
        bool useColorAdj = setColorAdjustments && ActivePPTrigger[volumeProfile].colorAdj != null;
        float startPostExposure = 0;
        float startContrast = 0;
        Color startColorFilter = Color.white;
        float startHueShift = 0;
        float startSaturation = 0;
        float totalHueShiftIncrease = (180f - startHueShift) + (hueShift - -180f);
        float percentageFirstPartHueShiftIncrease = (180f - startHueShift) / totalHueShiftIncrease;
        float totalHueShiftDecrease = (180f - hueShift) + (startHueShift - -180f);
        float percentageFirstPartHueShiftDecrease = (startHueShift - -180f) / totalHueShiftDecrease;
        if (useColorAdj)
        {
            startPostExposure = (float)ActivePPTrigger[volumeProfile].colorAdj.postExposure;
            startContrast = (float)ActivePPTrigger[volumeProfile].colorAdj.contrast;
            startColorFilter = (Color)ActivePPTrigger[volumeProfile].colorAdj.colorFilter;
            startHueShift = (float)ActivePPTrigger[volumeProfile].colorAdj.hueShift;
            startSaturation = (float)ActivePPTrigger[volumeProfile].colorAdj.saturation;
        }

        // Shadows Midtones Highlights
        bool useSMH = setSMH && ActivePPTrigger[volumeProfile].smh != null;
        Vector4 startShadows = Vector4.zero;
        Vector4 startMidtones = Vector4.zero;
        Vector4 startHighlights = Vector4.zero;
        Vector4 startLimits = Vector4.zero;
        if (useSMH)
        {
            startShadows = (Vector4)ActivePPTrigger[volumeProfile].smh.shadows;
            startMidtones = (Vector4)ActivePPTrigger[volumeProfile].smh.midtones;
            startHighlights = (Vector4)ActivePPTrigger[volumeProfile].smh.highlights;
            startLimits = new Vector4((float)ActivePPTrigger[volumeProfile].smh.shadowsStart,
                                      (float)ActivePPTrigger[volumeProfile].smh.shadowsEnd,
                                      (float)ActivePPTrigger[volumeProfile].smh.highlightsStart,
                                      (float)ActivePPTrigger[volumeProfile].smh.highlightsEnd);
        }

        Vector4 shadowsVector4 = new Vector4(shadows.r, shadows.g, shadows.b, shadowsSlider);
        Vector4 midtonesVector4 = new Vector4(midtones.r, midtones.g, midtones.b, midtonesSlider);
        Vector4 highlightsVector4 = new Vector4(highlights.r, highlights.g, highlights.b, highlightsSlider);

        while (time <= duration)
        {
            t = time / duration;

            if(useBloom)
            {
                if (setThreshold) { ActivePPTrigger[volumeProfile].bloom.threshold.Interp(startThreshold, threshold, t); }
                if (setIntensity) { ActivePPTrigger[volumeProfile].bloom.intensity.Interp(startIntensity, intensity, t); }
                if (setScatter) { ActivePPTrigger[volumeProfile].bloom.scatter.Interp(startScatter, scatter, t); }
                if (setTint) { ActivePPTrigger[volumeProfile].bloom.tint.Interp(startTint, tint, t); }
            }

            if (useVignette)
            {
                if (setVigColor) { ActivePPTrigger[volumeProfile].vig.color.Interp(startVigColor, vigColor, t); }
                if (setVigCenter) { ActivePPTrigger[volumeProfile].vig.center.Interp(startVigCenter, vigCenter, t); }
                if (setVigIntensity) { ActivePPTrigger[volumeProfile].vig.intensity.Interp(startVigIntensity, vigIntensity, t); }
                if (setVigSmoothness) { ActivePPTrigger[volumeProfile].vig.smoothness.Interp(startSmoothness, vigSmoothness, t); }
            }

            if (useChrAbr)
            {
                if (setChrAbrIntensity) { ActivePPTrigger[volumeProfile].chrabr.intensity.Interp(startChrAbrIntensity, chrAbrIntensity, t); }
            }

            if (useFilmGrain)
            {
                if (setFilmIntensity) { ActivePPTrigger[volumeProfile].filmgrain.intensity.Interp(startFilmIntensity, filmIntensity, t); }
                if (setFilmResponse) { ActivePPTrigger[volumeProfile].filmgrain.response.Interp(startFilmResponse, filmResponse, t); }
            }

            if (useColorAdj)
            {
                if (setPostExposure) { ActivePPTrigger[volumeProfile].colorAdj.postExposure.Interp(startPostExposure, postExposure, t); }
                if (setContrast) { ActivePPTrigger[volumeProfile].colorAdj.contrast.Interp(startContrast, contrast, t); }
                if (setColorFilter) { ActivePPTrigger[volumeProfile].colorAdj.colorFilter.Interp(startColorFilter, colorFilter, t); }
                if (setHueShift)
                {
                    if (hueShiftDirection == HueShiftDirection.Nearest ||
                       (hueShiftDirection == HueShiftDirection.Increase && hueShift > startHueShift) ||
                       (hueShiftDirection == HueShiftDirection.Decrease && hueShift < startHueShift))
                    {
                        ActivePPTrigger[volumeProfile].colorAdj.hueShift.Interp(startHueShift, hueShift, t);
                    }
                    else if(hueShiftDirection == HueShiftDirection.Increase && hueShift <= startHueShift)
                    {
                        float t1 = time / (duration * percentageFirstPartHueShiftIncrease);
                        float t2 = ((time - (duration * percentageFirstPartHueShiftIncrease)) / (duration * (1- percentageFirstPartHueShiftIncrease)));
                        if (t <= percentageFirstPartHueShiftIncrease)
                        {
                            ActivePPTrigger[volumeProfile].colorAdj.hueShift.Interp(startHueShift, 180, t1);
                        }
                        else
                        {
                            ActivePPTrigger[volumeProfile].colorAdj.hueShift.Interp(-180, hueShift, t2);
                        }
                    }
                    else if (hueShiftDirection == HueShiftDirection.Decrease && hueShift >= startHueShift)
                    {
                        float t1 = time / (duration * percentageFirstPartHueShiftDecrease);
                        float t2 = ((time - (duration * percentageFirstPartHueShiftDecrease)) / (duration * (1 - percentageFirstPartHueShiftDecrease)));
                        if (t <= percentageFirstPartHueShiftDecrease)
                        {
                            ActivePPTrigger[volumeProfile].colorAdj.hueShift.Interp(startHueShift, -180, t1);
                        }
                        else
                        {
                            ActivePPTrigger[volumeProfile].colorAdj.hueShift.Interp(180, hueShift, t2);
                        }
                    }
                }
                if (setSaturation) { ActivePPTrigger[volumeProfile].colorAdj.saturation.Interp(startSaturation, saturation, t); }
            }

            if (useSMH)
            {
                if (setShadows) { ActivePPTrigger[volumeProfile].smh.shadows.Interp(startShadows, shadowsVector4, t); }
                if (setMidtones) { ActivePPTrigger[volumeProfile].smh.midtones.Interp(startMidtones, midtonesVector4, t); }
                if (setHighlights) { ActivePPTrigger[volumeProfile].smh.highlights.Interp(startHighlights, highlightsVector4, t); }
                if (setLimits)
                {
                    ActivePPTrigger[volumeProfile].smh.shadowsStart.Interp(startLimits.x, shadowLimitStart, t);
                    ActivePPTrigger[volumeProfile].smh.shadowsEnd.Interp(startLimits.y, shadowLimitEnd, t);
                    ActivePPTrigger[volumeProfile].smh.highlightsStart.Interp(startLimits.z, highlightsLimitStart, t);
                    ActivePPTrigger[volumeProfile].smh.highlightsEnd.Interp(startLimits.w, highlightsLimitEnd, t);
                }
            }

            yield return null;
            time += Time.deltaTime;

            if(time > duration && !overTime)
            {
                time = duration;
                overTime = true;
            }
        }
    }

    public void SetProfile()
    {
        if (!ActivePPTrigger.ContainsKey(volumeProfile))
            AddPPData();

        //Debug.Log("set profile");

        bool useBloom = setBloom && ActivePPTrigger[volumeProfile].bloom != null;
        if (useBloom)
        {
            if (setThreshold) { ActivePPTrigger[volumeProfile].bloom.threshold.value = threshold; }
            if (setIntensity) { ActivePPTrigger[volumeProfile].bloom.intensity.value = intensity; }
            if (setScatter) { ActivePPTrigger[volumeProfile].bloom.scatter.value = scatter; }
            if (setTint) { ActivePPTrigger[volumeProfile].bloom.tint.value = tint; }
        }

        bool useVignette = setBloom && ActivePPTrigger[volumeProfile].vig != null;
        if (useVignette)
        {
            if (setVigColor) { ActivePPTrigger[volumeProfile].vig.color.value = vigColor; }
            if (setVigCenter) { ActivePPTrigger[volumeProfile].vig.center.value = vigCenter; }
            if (setVigIntensity) { ActivePPTrigger[volumeProfile].vig.intensity.value = vigIntensity; }
            if (setVigSmoothness) { ActivePPTrigger[volumeProfile].vig.smoothness.value = vigSmoothness; }
        }

        bool useChrAbr = setChromaticAberration && ActivePPTrigger[volumeProfile].chrabr != null;
        if (useChrAbr)
        {
            if (setChrAbrIntensity) { ActivePPTrigger[volumeProfile].chrabr.intensity.value = chrAbrIntensity; }
        }

        bool useFilmGrain = setFilmGrain && ActivePPTrigger[volumeProfile].filmgrain != null;
        if (useFilmGrain)
        {
            if (setFilmIntensity) { ActivePPTrigger[volumeProfile].filmgrain.intensity.value = filmIntensity; }
            if (setFilmResponse) { ActivePPTrigger[volumeProfile].filmgrain.response.value = filmResponse; }
        }

        bool useColorAdj = setColorAdjustments && ActivePPTrigger[volumeProfile].colorAdj != null;
        if (useColorAdj)
        {
            if (setPostExposure) { ActivePPTrigger[volumeProfile].colorAdj.postExposure.value = postExposure; }
            if (setContrast) { ActivePPTrigger[volumeProfile].colorAdj.contrast.value = contrast; }
            if (setColorFilter) { ActivePPTrigger[volumeProfile].colorAdj.colorFilter.value = colorFilter; }
            if (setHueShift) { ActivePPTrigger[volumeProfile].colorAdj.hueShift.value = hueShift; }
            if (setSaturation) { ActivePPTrigger[volumeProfile].colorAdj.saturation.value = saturation; }
        }

        Vector4 shadowsVector4 = new Vector4(shadows.r, shadows.g, shadows.b, shadowsSlider);
        Vector4 midtonesVector4 = new Vector4(midtones.r, midtones.g, midtones.b, midtonesSlider);
        Vector4 highlightsVector4 = new Vector4(highlights.r, highlights.g, highlights.b, highlightsSlider);
        bool useSMH = setSMH && ActivePPTrigger[volumeProfile].smh != null;
        if (useSMH)
        {
            if (setShadows) { ActivePPTrigger[volumeProfile].smh.shadows.value = shadowsVector4; }
            if (setMidtones) { ActivePPTrigger[volumeProfile].smh.midtones.value = midtonesVector4; }
            if (setHighlights) { ActivePPTrigger[volumeProfile].smh.highlights.value = highlightsVector4; }
            if (setLimits)
            {
                ActivePPTrigger[volumeProfile].smh.shadowsStart.value = shadowLimitStart;
                ActivePPTrigger[volumeProfile].smh.shadowsEnd.value = shadowLimitEnd;
                ActivePPTrigger[volumeProfile].smh.highlightsStart.value = highlightsLimitStart;
                ActivePPTrigger[volumeProfile].smh.highlightsEnd.value = highlightsLimitEnd;
            }
        }
    }

    public void GrabProfile()
    {
        if (!ActivePPTrigger.ContainsKey(volumeProfile))
            AddPPData();

        bool useBloom = setBloom && ActivePPTrigger[volumeProfile].bloom != null;
        if (useBloom)
        {
            if (setThreshold) { threshold = ActivePPTrigger[volumeProfile].bloom.threshold.value; }
            if (setIntensity) { intensity = ActivePPTrigger[volumeProfile].bloom.intensity.value; }
            if (setScatter) { scatter = ActivePPTrigger[volumeProfile].bloom.scatter.value; }
            if (setTint) { tint = ActivePPTrigger[volumeProfile].bloom.tint.value; }
        }

        bool useVignette = setBloom && ActivePPTrigger[volumeProfile].vig != null;
        if (useVignette)
        {
            if (setVigColor) { vigColor = ActivePPTrigger[volumeProfile].vig.color.value; }
            if (setVigCenter) { vigCenter = ActivePPTrigger[volumeProfile].vig.center.value; }
            if (setVigIntensity) { vigIntensity = ActivePPTrigger[volumeProfile].vig.intensity.value; }
            if (setVigSmoothness) { vigSmoothness = ActivePPTrigger[volumeProfile].vig.smoothness.value; }
        }

        bool useChrAbr = setChromaticAberration && ActivePPTrigger[volumeProfile].chrabr != null;
        if (useChrAbr)
        {
            if (setChrAbrIntensity) { chrAbrIntensity = ActivePPTrigger[volumeProfile].chrabr.intensity.value; }
        }

        bool useFilmGrain = setFilmGrain && ActivePPTrigger[volumeProfile].filmgrain != null;
        if (useFilmGrain)
        {
            if (setFilmIntensity) { filmIntensity = ActivePPTrigger[volumeProfile].filmgrain.intensity.value; }
            if (setFilmResponse) { filmResponse = ActivePPTrigger[volumeProfile].filmgrain.response.value; }
        }

        bool useColorAdj = setColorAdjustments && ActivePPTrigger[volumeProfile].colorAdj != null;
        if (useColorAdj)
        {
            if (setPostExposure) { postExposure = ActivePPTrigger[volumeProfile].colorAdj.postExposure.value; }
            if (setContrast) { contrast = ActivePPTrigger[volumeProfile].colorAdj.contrast.value; }
            if (setColorFilter) { colorFilter = ActivePPTrigger[volumeProfile].colorAdj.colorFilter.value; }
            if (setHueShift) { hueShift = ActivePPTrigger[volumeProfile].colorAdj.hueShift.value; }
            if (setSaturation) { saturation = ActivePPTrigger[volumeProfile].colorAdj.saturation.value; }
        }

        //Vector4 shadowsVector4 = new Vector4(shadows.r, shadows.g, shadows.b, shadowsSlider);
        //Vector4 midtonesVector4 = new Vector4(midtones.r, midtones.g, midtones.b, midtonesSlider);
        //Vector4 highlightsVector4 = new Vector4(highlights.r, highlights.g, highlights.b, highlightsSlider);
        bool useSMH = setSMH && ActivePPTrigger[volumeProfile].smh != null;
        if (useSMH)
        {
            if (setShadows)
            {
                shadows = ActivePPTrigger[volumeProfile].smh.shadows.value;
                shadowsSlider = ActivePPTrigger[volumeProfile].smh.shadows.value.w;
            }
            if (setMidtones)
            {
                midtones = ActivePPTrigger[volumeProfile].smh.midtones.value;
                midtonesSlider = ActivePPTrigger[volumeProfile].smh.midtones.value.w;
            }
            if (setHighlights)
            {
                highlights = ActivePPTrigger[volumeProfile].smh.highlights.value;
                highlightsSlider = ActivePPTrigger[volumeProfile].smh.highlights.value.w;
            }
            if (setLimits)
            {
                shadowLimitStart = ActivePPTrigger[volumeProfile].smh.shadowsStart.value;
                shadowLimitEnd = ActivePPTrigger[volumeProfile].smh.shadowsEnd.value;
                highlightsLimitStart = ActivePPTrigger[volumeProfile].smh.highlightsStart.value;
                highlightsLimitEnd = ActivePPTrigger[volumeProfile].smh.highlightsEnd.value;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player" || collision.isTrigger)
            return;

        if (!ActivePPTrigger.ContainsKey(volumeProfile))
            AddPPData();

        if (ActivePPTrigger[volumeProfile].trigger != null)
            ActivePPTrigger[volumeProfile].trigger.StopAllCoroutines();

        ActivePPTrigger[volumeProfile].trigger = this;

        Activate();
    }

    private void Activate()
    {
        if (volumeProfile == null)
            return;

        //Debug.Log("activate: " + this.GetHashCode());
        if (duration == 0) { SetProfile(); }
        else { StartCoroutine(ModifyProfile()); }
    }

    private void ActivateOnDeath()
    {
        if (activateOnDeath) { Activate(); }
    }
    private void ActivateOnRespawn()
    {
        if (activateOnRespawn) { Activate(); }
    }

    public void Log()
    {
        if (volumeProfile == null)
            return;

        if (!ActivePPTrigger.ContainsKey(volumeProfile))
            AddPPData();

        Debug.Log("Shadows: " + ActivePPTrigger[volumeProfile].smh.shadows);
        Debug.Log("Midtones: " + ActivePPTrigger[volumeProfile].smh.midtones);
        Debug.Log("Highlights: " + ActivePPTrigger[volumeProfile].smh.highlights);
    }

    void OnEnable()
    {
        PlayerControllerV2.OnDeath += ActivateOnDeath;
        PlayerControllerV2.OnRespawn += ActivateOnRespawn;
    }


    void OnDisable()
    {
        PlayerControllerV2.OnDeath -= ActivateOnDeath;
        PlayerControllerV2.OnRespawn -= ActivateOnRespawn;
    }
}
