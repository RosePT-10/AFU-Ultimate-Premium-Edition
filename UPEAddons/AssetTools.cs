using System;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace UPEAddons;

public class AssetTools : Core
{
    public string AnimTextureName;
    public int timer;
    private UnityEngine.Texture AnimTexture;
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
}

