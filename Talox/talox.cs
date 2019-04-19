using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace be.vanderlei.arne.Talox
{
    [BepInDependency("com.bepis.r2api")] 
    [BepInPlugin("be.vanderlei.arne.Talox", "Talox", "1.0")]
    public class ExamplePlugin : BaseUnityPlugin
    {

        private GameObject _TaloxBody;

        public void Awake()
        {
            string pluginfolder = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);

            //asset bundle to load
            string assetBundle = "talox";

            //prefab location inside asset bundle
            string prefab = "assets/prefabs/taloxbody.prefab";
            
            //load assetbundle then load the prefab
            _TaloxBody = AssetBundle.LoadFromFile($"{pluginfolder}/{assetBundle}").LoadAsset<GameObject>(prefab);

            R2API.SurvivorAPI.SurvivorCatalogReady += (s, e) =>
            {
                var survivor = new SurvivorDef
                {
                    bodyPrefab = BodyCatalog.FindBodyPrefab("AssassinBody"),
                    descriptionToken = "TALOX_DESCRIPTION",
                    displayPrefab = _TaloxBody,
                    primaryColor = new Color(0.8039216f, 0.482352942f, 0.843137264f),
                    unlockableName = "" 
                };

                R2API.SurvivorAPI.SurvivorDefinitions.Add(survivor);
            };
        }
    }
}