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
using System.Collections;
using Il2CppView_Music;
using Il2CppPhoton.Client.StructWrapping;
using Il2CppMenus;
using Il2CppCustomCharacters;

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
        public UnityEngine.GameObject audio_disclaimer_ytp;
        public UnityEngine.GameObject audio_disclaimer_ytp_2_Citrus;
        public UnityEngine.GameObject main_menu_remix_game_object;
        public UnityEngine.AssetBundle bundle;

        public static Core core;

        private MelonPreferences_Category JumpScareCat;
        private MelonPreferences_Entry<int> Chance;
        private MelonPreferences_Entry<bool> IsSilly;
        private MelonPreferences_Entry<bool> IsEnabled;

        bool is_animation_playing;
        bool y_or_n; // rng check
        public bool is_second_button_press;
        public bool is_third_button_press = false;
        public int timer = 0;
        public int double_click_protection_timer = 0;
        public string current_scene;

        
        // basically args for DrawAnimation()
        string AnimTextureName;
        UnityEngine.Texture AnimTexture;
        public void DrawAnimation()
        {   
            //LoggerInstance.Msg(jump_scare_texture);
            //determine what frame to display
            decimal framecounter = (timer * 1.2M) / 4;
            framecounter = Decimal.Truncate(framecounter);
            framecounter = Math.Clamp(framecounter, 0, 12);
            LoggerInstance.Msg(framecounter.ToString());
            

            AnimTexture = Melon<Core>.Instance.bundle.LoadAsset<Texture>(AnimTextureName + framecounter);
            Texture.Instantiate(AnimTexture);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), AnimTexture);
            
        }

        public void TrackTime()
        {
            if (timer == 50)
            {
                timer = 0;
            }
            else
            {
                timer ++;
            }
        }

        public string ImageTextureName;
        private void DrawImage()
        {
            image = bundle.LoadAsset<Texture>(ImageTextureName);
            Texture.Instantiate(image);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), image);
            
            //LoggerInstance.Msg("Ran DrawImage");
        }

        private UnityEngine.GameObject ManageAudio
            (
                int mode, 
                UnityEngine.GameObject prefab_instance, 
                string prefab_name = null,
                string audio_clip_name = null
            )
        {
            UnityEngine.GameObject prefab;
            UnityEngine.AudioClip audio_clip;
            
            switch (mode)
            {
                case 0: // instantiate the GameObject containing the AudioSource
                    if (prefab_name != null)
                    {
                        prefab = bundle.LoadAsset<GameObject>(prefab_name);
                        prefab_instance = GameObject.Instantiate(prefab);
                        return prefab_instance;
                    }
                    else {return null;}
                case 1: // using the instance of the previously instantiated GameObject, change the assigned AudioClip
                    if (audio_clip_name != null)
                    {
                        audio_clip = bundle.LoadAsset<AudioClip>(audio_clip_name);
                        prefab_instance.GetComponent<AudioSource>().clip = audio_clip;
                        return prefab_instance;
                    }
                    else {return null;}
                case 2: // make the AudioSource play it's AudioClip
                    prefab_instance.GetComponent<AudioSource>().Play();
                    return prefab_instance;
                case 3: // make the AudioSource stop playing it's AudioClip
                    prefab_instance.GetComponent<AudioSource>().Stop();
                    return prefab_instance;
                default:
                    return null;
            }
        }
        private bool OneInXChance(int chance)
        {
            int rng = System.Random.Shared.Next(1, chance);
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

        private int GetRandomNumberWithinRange(int chance)
        {
            int rng = System.Random.Shared.Next(0, chance);
            return rng;
        }

        public string image_name;

        [HarmonyPatch(typeof(SplashScreenHandler), "OnAnyButtonPress")]
        private static class CustomSplashScreen
        {
            public static void SuperDuperHardCodedImageDisplayMethod()
            {
                UnityEngine.Texture name = Melon<Core>.Instance.bundle.LoadAsset<Texture>("citrus_name");
                Texture.Instantiate(name);
                GUI.DrawTexture(new Rect(0, 0, 1920, 1080), name);
            }

            public static UnityEngine.AudioClip AudioLoader(string audioclip_to_load)
            {
                UnityEngine.AudioClip buh = Melon<Core>.Instance.bundle.LoadAsset<AudioClip>(audioclip_to_load);
                Melon<Core>.Logger.Msg(buh.name);
                return buh;
            }

            
            public static void NotHardCodedImageDisplayMethod()
            {
                UnityEngine.Texture texture = Melon<Core>.Instance.bundle.LoadAsset<Texture>(Melon<Core>.Instance.image_name);
                Texture.Instantiate(texture);
                GUI.DrawTexture(new Rect(0, 0, 1920, 1080), texture);
                
            }
            
            public static void DoubleClickProtection()
            {
                Melon<Core>.Instance.double_click_protection_timer ++;
            }
            public static void Postfix()
            { 
                //Melon<Core>.Logger.Msg("Detected method: OnAnyButtonPress");
                
                if (Melon<Core>.Instance.is_second_button_press == false)
                {
                    // start counting a timer to keep track of how long it has been since the first button press
                    MelonEvents.OnUpdate.Subscribe(DoubleClickProtection, 0);

                    // if the ytp edit of the custom disclaimer screen is still playing, stop it
                    Melon<Core>.Instance.ManageAudio(3, Melon<Core>.Instance.audio_disclaimer_ytp);

                    // display the custom controller splash screen
                    Melon<Core>.Logger.Msg("Attempting to display an image...");
                    // draw image arg
                    Melon<Core>.Instance.ImageTextureName = "CustomControllerWarning";
                    MelonEvents.OnGUI.Subscribe(Melon<Core>.Instance.DrawImage, 0);
                    Melon<Core>.Logger.Msg("Worked!");


                    // new, not-hardcoded way of playing the ytp
                    // collect every currently exisitng ytp audio into an array
                    UnityEngine.AudioClip[] ytp_array = {AudioLoader("AFU_UPE_YTP_2"), AudioLoader("AFU_UPE_YTP_2_melli")};
                    // randomly select one of the clips by using the total amount of clips to clamp the rng method
                    int selected_ytp_id = Melon<Core>.Instance.GetRandomNumberWithinRange(ytp_array.Length);
                    // pin down the ytp that was selected and put it in a variable
                    UnityEngine.AudioClip selected_ytp_auidoclip = ytp_array[selected_ytp_id];
                    // instantiate the gameobject which'll play the ytp audioclip
                    UnityEngine.GameObject ytp_object = Melon<Core>.Instance.bundle.LoadAsset<GameObject>("BetaDisclSoundSource1");
                    // swap the default audioclip for the selected on instead
                    ytp_object.GetComponent<AudioSource>().clip = selected_ytp_auidoclip;
                    // instantiate the object now that it has been modified before hand
                    UnityEngine.GameObject.Instantiate(ytp_object);
                    // play the selected audio clip
                    Melon<Core>.Instance.ManageAudio(2, ytp_object);


                    // no longer hardcoded as well: display the name of who is talking
                    var dict = new Dictionary<int, string>()
                    {
                        {0, "citrus_name"},
                        {1, "melli_name"}
                    };
                    Melon<Core>.Instance.image_name = dict[selected_ytp_id];
                    MelonEvents.OnGUI.Subscribe(NotHardCodedImageDisplayMethod, 1);


                    Melon<Core>.Instance.is_second_button_press = true;

                }
                else if (Melon<Core>.Instance.double_click_protection_timer > 20)
                {
                    MelonEvents.OnGUI.Unsubscribe(Melon<Core>.Instance.DrawImage);
                    MelonEvents.OnGUI.Unsubscribe(NotHardCodedImageDisplayMethod);
                    MelonEvents.OnUpdate.Unsubscribe(DoubleClickProtection);
                }
            }
        }

        //[HarmonyPatch(typeof(Accessory), "DrawTick", new Type[] {typeof(float)})]
        //public static class ClothingReplacement
        //{
        //    public static void Postfix(Accessory __instance)
        //    {
        //        __instance.clothingItem = new ClothingItem();
        //    }
        //}
    
        public override void OnInitializeMelon()
        {
            // initialize config file
            JumpScareCat = MelonPreferences.CreateCategory("FoxyJumpScareRNG");
            Chance = JumpScareCat.CreateEntry<int>("JumpScareRarity", 10000);
            IsSilly = JumpScareCat.CreateEntry<bool>("GoofySoundEffect", false);
            IsEnabled = JumpScareCat.CreateEntry<bool>("Enabled?", false);
            
            
            // initialize asset bundle
            bool got_asset;
            bundle = AssetBundle.LoadFromFile("./UserData/upeaddons.assets");
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

            // fuck with music
            new SongsLibrary().menu = null;
            new SongsLibrary().warehouse = null;            

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
            LoggerInstance.Msg(sceneName);

            current_scene = sceneName;
            
            if (sceneName == "Splashes")
            {
                audio_disclaimer_ytp = ManageAudio(0, null, "BetaDisclSoundSource");
                ManageAudio(2, prefab_instance: audio_disclaimer_ytp);
                ImageTextureName = "rose_name";
                MelonEvents.OnGUI.Subscribe(DrawImage, 0);
            }
            if (sceneName == "MainMenu")
            {
                // get rid of all custom splash screen gui elements
                MelonEvents.OnGUI.Unsubscribe(DrawImage);

                // play menu music overlay (thanks melli!)
                main_menu_remix_game_object = ManageAudio(0, null, "MusicRemixAudio");
                main_menu_remix_game_object.GetComponent<AudioSource>().volume = 0.7F;
                main_menu_remix_game_object.GetComponent<AudioSource>().loop = true;
                ManageAudio(1, main_menu_remix_game_object, audio_clip_name: "MenuMusicFull");
                main_menu_remix_game_object.GetComponent<AudioSource>().PlayDelayed(0.00097F);
                // the PlayDelayed() was because this used to require
                // putting an overlay of the music edit on top of the base song.
                // i figured out how to stop the main menu music
                // hours later.
                // the delay remains in rememberance of the hours spend adjusting it
                // to sync up correctly.
                //
                // rest in peace MenuMusicOverlay.
                
                // model testing
                UnityEngine.Vector3 bigV3;
                bigV3.x = 10;
                bigV3.y = 10;
                bigV3.z = 10;
                UnityEngine.Vector3 locationV3;
                locationV3.x = 0;
                locationV3.y = 1.4F;
                locationV3.z = 9.7F;
                UnityEngine.Vector3 rotationV3;
                rotationV3.x = 1;
                rotationV3.y = 70;
                rotationV3.z = 0;

                UnityEngine.GameObject cirno = bundle.LoadAsset<GameObject>("CirnoPrefab");
                
                cirno.active = true;
                cirno.GetComponent<Transform>().position = locationV3;
                cirno.GetComponent<Transform>().localScale = bigV3;
                cirno.GetComponent<Transform>().Rotate(rotationV3);
                GameObject.Instantiate(cirno);


                // jumpscare testing
                // play noise
                //jumpscare_game_object = bundle.LoadAsset<GameObject>("JumpScareAudio");
                //GameObject.Instantiate(jumpscare_game_object);
                //jumpscare_game_object.GetComponent<AudioSource>().Play();
                
                // play video
                //AnimTextureName = "jump";
                //MelonEvents.OnGUI.Subscribe(DrawAnimation, 0);
                //is_animation_playing = true;

                
            }
            if (sceneName == "Warehouse")
            {
                // model testing
                /*
                UnityEngine.Vector3 bigV3;
                bigV3.x = 10;
                bigV3.y = 10;
                bigV3.z = 10;
                UnityEngine.Vector3 locationV3;
                locationV3.x = 0;
                locationV3.y = 1.4F;
                locationV3.z = 0;

                UnityEngine.GameObject cirno = bundle.LoadAsset<GameObject>("CirnoPrefab");
                
                cirno.active = true;
                cirno.GetComponent<Transform>().position = locationV3;
                cirno.GetComponent<Transform>().localScale = bigV3;
                //GameObject.Instantiate(cirno);

                UnityEngine.GameObject helm = bundle.LoadAsset<GameObject>("HelmetKappaPrefab");
                locationV3.x = 0;
                locationV3.y = 1.4F;
                locationV3.z = 0;
                bigV3.x = 100;
                bigV3.y = 100;
                bigV3.z = 100;
                helm.active = true;
                helm.GetComponent<Transform>().position = locationV3;
                helm.GetComponent<Transform>().localScale = bigV3;
                GameObject.Instantiate(helm);
                */
            }
        }
        

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            TrackTime();

            // only check once every second
            if (timer >= 50)
            {   
                // stop animation after a full second of playing
                if (is_animation_playing == true)
                {
                    MelonEvents.OnGUI.Unsubscribe(DrawAnimation);
                    is_animation_playing = false;
                }

                // play jumpscare this frame depending on rng
                //LoggerInstance.Msg("Checking rng...");
                if (IsEnabled.Value == true)
                {
                    OneInXChance(Chance.Value);
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

                        // effectively the arg for DrawAnimation()
                        AnimTextureName = "jump";
                        // play video
                        MelonEvents.OnGUI.Subscribe(DrawAnimation, 0);
                        is_animation_playing = true;
                    }
                    else
                    {
                        //LoggerInstance.Msg("\"You missed that one, try another!\"");
                    }
                }
            }
        }


        public override void OnUpdate()
        {
            base.OnUpdate();

            // audio testing
            if (current_scene == "MainMenu" | current_scene == "Splashes")
            {
                UnityEngine.Object[] audio_sources = GameObject.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
                //LoggerInstance.Msg($"{Environment.NewLine}{Environment.NewLine}CURRENTLY PLAYING SOUND SOURCES:{Environment.NewLine}");
                foreach (AudioSource a in audio_sources)
                {
                    if (a.isPlaying == true)
                    {
                        //LoggerInstance.Msg(a.name);
                    }
                    if (a.name == "Music")
                    {
                        a.Stop();
                    }
                }
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