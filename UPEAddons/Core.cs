using MelonLoader;
using Unity.Collections.LowLevel.Unsafe;
using System;
using UnityEngine;
using Il2CppView_Audio;
using Il2CppMS.Internal.Xml.XPath;
using Il2CppSystem.Net.Cache;
using UnityEngine.ResourceManagement.ResourceProviders;
using Il2CppSystem.Linq;
using UnityEngine.UI;
using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using Il2CppQuantum;
using Unity.Mathematics;
using HarmonyLib;
using HarmonyLib.Tools;
using Il2Cpp;
using Unity.Collections;
using System.Net.NetworkInformation;
using UnityEngine.InputSystem;
using System.Reflection;

[assembly: MelonInfo(typeof(UPEAddons.Core), "UPEAddons", "1.0.0", "RosePT-10", null)]
[assembly: MelonGame("Videocult", "Airframe")]

namespace UPEAddons
{
    public class Core : MelonMod
    {
        public UnityEngine.GameObject jumpscare_game_object;
        public UnityEngine.Texture jump_scare_texture;
        public UnityEngine.AudioClip jump_scare_sound_file;
        public UnityEngine.Texture image;
        UnityEngine.AssetBundle bundle;

        public static Core core;

        private MelonPreferences_Category JumpScareCat;
        private MelonPreferences_Entry<int> Chance;
        private MelonPreferences_Entry<bool> IsSilly;

        int timer; // check once every second
        bool is_animation_playing;
        bool y_or_n; // rng check
        string path;


        [HarmonyPatch(typeof(SplashScreenHandler), "OnAnyButtonPress")]
        private static class Patch
        {
            //private static void TestMethod()
            //{
                //core = new Core();
                //core.LoggerInstance.Msg("Ran TestMethod");
            //}
            //public static void HarmonyLib.Preix()
            //{
                //Melon<Core>.Logger.Msg();
            //}
            public static void Postfix()
            { 
                Melon<Core>.Logger.Msg("Attempting to display an image...");
                //Melon<Core>.Instance.DrawImage();
                MelonEvents.OnGUI.Subscribe(Melon<Core>.Instance.DrawImage, 0);
                Melon<Core>.Logger.Msg("Worked!");
            }
        }
        

        private void DrawAnimation()
        {   
            //LoggerInstance.Msg(jump_scare_texture);
            //determine what frame to display
            decimal framecounter = (timer * 1.2M) / 4;
            framecounter = Decimal.Truncate(framecounter);
            framecounter = Math.Clamp(framecounter, 0, 12);
            LoggerInstance.Msg(framecounter.ToString());
            

            jump_scare_texture = bundle.LoadAsset<Texture>("jump" + framecounter);
            Texture.Instantiate(jump_scare_texture);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), jump_scare_texture);
            
        }

        private void DrawImage()
        {
            image = bundle.LoadAsset<Texture>("jump1");
            Texture.Instantiate(image);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), image);
            LoggerInstance.Msg("Ran DrawImage");
        }
        private bool CheckRng()
        {
            int rng = System.Random.Shared.Next(0, Chance.Value);
            //LoggerInstance.Msg(rng);
            if (rng == 1)
            {
                y_or_n = true;
            }
            else
            {
                y_or_n = false;
            }

            return y_or_n;
        }
        public override void OnInitializeMelon()
        {
            // initialize config file
            JumpScareCat = MelonPreferences.CreateCategory("FoxyJumpScareRNG");
            Chance = JumpScareCat.CreateEntry<int>("JumpScareRarity", 10000);
            IsSilly = JumpScareCat.CreateEntry<bool>("GoofySoundEffect", false);
            
            
            // initialize asset bundle
            bool got_asset;
            path = Path.GetFullPath("jump_scare.assets").Replace('\\', '/');
            LoggerInstance.Msg(path);
            bundle = AssetBundle.LoadFromFile("./UserData/jump_scare.assets");
            if (bundle == null)
            {
                LoggerInstance.Msg("Failed to load custom asset bundle :[");
                got_asset = false;
            }
            else
            {
                LoggerInstance.Msg("Loaded custom asset bundle");
                got_asset = true;
            }

            
            // set timer
            timer = 0;

            // log outcome
            if (got_asset == true)
            {
                LoggerInstance.Msg("Successfully Initialized! Yipee!");
            }
            else
            {
                LoggerInstance.Msg("Oh no! The asset bundle failed to load and most of this mod will not work. Did you put the .assets in UserData?");
            }
            
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            
            if (sceneName == "MainMenu")
            {
                
                
                // jumpscare testing
                // play noise
                //jumpscare_game_object = bundle.LoadAsset<GameObject>("JumpScareAudio");
                //GameObject.Instantiate(jumpscare_game_object);
                //jumpscare_game_object.GetComponent<AudioSource>().Play();
                
                // play video
                //MelonEvents.OnGUI.Subscribe(DrawAnimation, 0);
                //is_animation_playing = true;
            }
        }
        

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            // only check once every second
            if (timer >= 50)
            {
                timer = 0;

                // stop animation after a full second of playing
                if (is_animation_playing == true)
                {
                    MelonEvents.OnGUI.Unsubscribe(DrawAnimation);
                    is_animation_playing = false;
                }

                // play jumpscare this frame depending on rng
                //LoggerInstance.Msg("Checking rng...");
                CheckRng();
                if (y_or_n == true && is_animation_playing == false)
                {
                    // play noise
                    jumpscare_game_object = bundle.LoadAsset<GameObject>("JumpScareAudio");
                    
                    if (IsSilly.Value == true)
                    {
                        jump_scare_sound_file = bundle.LoadAsset<AudioClip>("Poke");
                        jumpscare_game_object.GetComponent<AudioSource>().clip = jump_scare_sound_file;
                        jumpscare_game_object.GetComponent<AudioSource>().volume = 1;
                    }
                    GameObject.Instantiate(jumpscare_game_object);
                    jumpscare_game_object.GetComponent<AudioSource>().Play();
                    LoggerInstance.Msg("Played jump scare sound this frame.");

                    // play video
                    MelonEvents.OnGUI.Subscribe(DrawAnimation, 0);
                    is_animation_playing = true;
                }
                else
                {
                    //LoggerInstance.Msg("\"You missed that one, try another!\"");
                }
            }
            else
            {
                timer ++;
                //LoggerInstance.Msg(timer);
            }
            
        }

        

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            bundle.Unload(true);
            LoggerInstance.Msg("Unloaded custom asset bundle");
        }
        
    }

    
}