using ColossalFramework.Plugins;
using ICities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TextureReplacer
{
    public class TextureReplacerMod : LoadingExtensionBase, IUserMod
    {


        public string Name
        {
            get { return "Texture Replacer"; }
        }
        public string Description
        {
            get { return "Replaces the textures of the game assets with remastered versions"; }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            TextureReplacer.instance.SearchTexturePacks();
            /*
            TextureReplacer.instance.debug = new DebugOutput();
            TextureReplacer.instance.debugLOD = new DebugOutput();
            */
            Detour.BuildingInfoDetour.Deploy();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            /*
            DebugOutput.Serialize("TextureReplacerDebug.xml", TextureReplacer.instance.debug);
            DebugOutput.Serialize("TextureReplacerDebugLOD.xml", TextureReplacer.instance.debugLOD);
            */
            TextureReplacer.instance.UnloadUnusedTextures();
        }

        public override void OnReleased()
        {
            base.OnReleased();

            TextureReplacer.instance.Reset();

            Detour.BuildingInfoDetour.Revert();
        }
    }
}
