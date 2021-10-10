﻿using ItemChanger.Extensions;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using HutongGames.PlayMaker.Actions;
using UObject = UnityEngine.Object;

namespace ItemChanger.Internal.Preloaders
{
    public class CorePreloader : Preloader
    {
        public override IEnumerable<(string, string)> GetPreloadNames()
        {
            yield return (SceneNames.Tutorial_01, "_Props/Chest");
            yield return (SceneNames.Tutorial_01, "_Enemies/Crawler 1");
            yield return (SceneNames.Tutorial_01, "_Props/Cave Spikes (1)");
            yield return (SceneNames.Tutorial_01, "_Scenery/plat_float_17");
            yield return (SceneNames.Tutorial_01, "_Props/Tut_tablet_top (1)");
            yield return (SceneNames.Deepnest_36, "d_break_0047_deep_lamp2/lamp_bug_escape (7)");
        }

        public override void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            _chest = objectsByScene[SceneNames.Tutorial_01]["_Props/Chest"];
            _shinyItem = _chest.transform.Find("Item").Find("Shiny Item (1)").gameObject;
            _shinyItem.transform.parent = null;
            _shinyItem.name = "Shiny Item Mod";
            UObject.DontDestroyOnLoad(_chest);
            UObject.DontDestroyOnLoad(_shinyItem);
            PlayMakerFSM shinyFSM = _shinyItem.LocateFSM("Shiny Control");
            _relicGetMsg = UObject.Instantiate(shinyFSM.GetState("Trink Flash").GetActionsOfType<SpawnObjectFromGlobalPool>()[1].gameObject.Value);
            _relicGetMsg.SetActive(false);
            UObject.DontDestroyOnLoad(_relicGetMsg);

            HealthManager health = objectsByScene[SceneNames.Tutorial_01]["_Enemies/Crawler 1"].GetComponent<HealthManager>();
            _smallGeo = UObject.Instantiate(
                ReflectionHelper.GetField<HealthManager, GameObject>(health, "smallGeoPrefab"));
            _mediumGeo =
                UObject.Instantiate(ReflectionHelper.GetField<HealthManager, GameObject>(health, "mediumGeoPrefab"));
            _largeGeo = UObject.Instantiate(
                ReflectionHelper.GetField<HealthManager, GameObject>(health, "largeGeoPrefab"));

            _smallGeo.SetActive(false);
            _mediumGeo.SetActive(false);
            _largeGeo.SetActive(false);
            UObject.DontDestroyOnLoad(_smallGeo);
            UObject.DontDestroyOnLoad(_mediumGeo);
            UObject.DontDestroyOnLoad(_largeGeo);

            UObject.Destroy(objectsByScene[SceneNames.Tutorial_01]["_Enemies/Crawler 1"]);

            _smallPlatform = objectsByScene[SceneNames.Tutorial_01]["_Scenery/plat_float_17"];
            UObject.DontDestroyOnLoad(_smallPlatform);

            _lumaflyEscape = objectsByScene[SceneNames.Deepnest_36]["d_break_0047_deep_lamp2/lamp_bug_escape (7)"];
            FixLumaflyEscape(_lumaflyEscape);
            UObject.DontDestroyOnLoad(_lumaflyEscape);

            _loreTablet = objectsByScene[SceneNames.Tutorial_01]["_Props/Tut_tablet_top (1)"];
            _loreTablet.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            UObject.DontDestroyOnLoad(_loreTablet);
        }

        public GameObject Chest => UObject.Instantiate(_chest);
        public GameObject ShinyItem => UObject.Instantiate(_shinyItem);
        public GameObject SmallGeo => UObject.Instantiate(_smallGeo);
        public GameObject MediumGeo => UObject.Instantiate(_mediumGeo);
        public GameObject LargeGeo => UObject.Instantiate(_largeGeo);
        public GameObject SmallPlatform => UObject.Instantiate(_smallPlatform);
        public GameObject RelicGetMsg => UObject.Instantiate(_relicGetMsg);
        public GameObject LoreTablet => UObject.Instantiate(_loreTablet);
        public GameObject LumaflyEscape => UObject.Instantiate(_lumaflyEscape);

        private GameObject _chest;
        private GameObject _shinyItem;
        private GameObject _relicGetMsg;
        private GameObject _smallGeo;
        private GameObject _mediumGeo;
        private GameObject _largeGeo;
        private GameObject _smallPlatform;
        private GameObject _loreTablet;
        private GameObject _lumaflyEscape;


        private static void FixLumaflyEscape(GameObject lumaflyEscape)
        {
            ParticleSystem.MainModule psm = lumaflyEscape.GetComponent<ParticleSystem>().main;
            ParticleSystem.EmissionModule pse = lumaflyEscape.GetComponent<ParticleSystem>().emission;
            ParticleSystem.ShapeModule pss = lumaflyEscape.GetComponent<ParticleSystem>().shape;
            ParticleSystem.TextureSheetAnimationModule pst = lumaflyEscape.GetComponent<ParticleSystem>().textureSheetAnimation;
            ParticleSystem.ForceOverLifetimeModule psf = lumaflyEscape.GetComponent<ParticleSystem>().forceOverLifetime;

            psm.duration = 1f;
            psm.startLifetimeMultiplier = 4f;
            psm.startSizeMultiplier = 2f;
            psm.startSizeXMultiplier = 2f;
            psm.gravityModifier = -0.2f;
            psm.maxParticles = 99;              // In practice it only spawns 9 lumaflies
            pse.rateOverTimeMultiplier = 10f;
            pss.radius = 0.5868902f;
            pst.cycleCount = 15;
            psf.xMultiplier = 3;
            psf.yMultiplier = 8;

            // I have no idea what this is supposed to be lmao
            AnimationCurve yMax = new AnimationCurve(new Keyframe(0, 0.0810811371f), new Keyframe(0.230769232f, 0.108108163f),
                new Keyframe(0.416873455f, -0.135135055f), new Keyframe(0.610421836f, -0.054053992f), new Keyframe(0.799007416f, -0.29729721f));
            AnimationCurve yMin = new AnimationCurve(new Keyframe(0, 0.486486584f), new Keyframe(0.220843673f, 0.567567647f),
                new Keyframe(0.411910683f, 0.270270377f), new Keyframe(0.605459034f, 0.405405462f), new Keyframe(0.801488876f, 0.108108193f));
            psf.y = new ParticleSystem.MinMaxCurve(8, yMin, yMax);

            psf.x.curveMax.keys[0].value = -0.324324369f;
            psf.x.curveMax.keys[1].value = -0.432432413f;

            psf.x.curveMin.keys[0].value = 0.162162244f;
            psf.x.curveMin.keys[1].time = 0.159520522f;
            psf.x.curveMin.keys[1].value = 0.35135144f;

            Transform t = lumaflyEscape.GetComponent<Transform>();
            Vector3 loc = t.localScale;
            loc.x = 1f;
            t.localScale = loc;
        }
    }
}
