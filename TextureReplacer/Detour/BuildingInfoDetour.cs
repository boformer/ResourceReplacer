using System;
using System.Reflection;
using UnityEngine;

namespace TextureReplacer.Detour
{
    public class BuildingInfoDetour : BuildingInfo
    {
        private static bool deployed = false;

        private static RedirectCallsState _InitializePrefab_state;
        private static MethodInfo _InitializePrefab_original;
        private static MethodInfo _InitializePrefab_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _InitializePrefab_original = typeof(BuildingInfo).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                _InitializePrefab_detour = typeof(BuildingInfoDetour).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                _InitializePrefab_state = RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);

                deployed = true;

                Debug.Log("Texture Replacer: BuildingInfo Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
                _InitializePrefab_original = null;
                _InitializePrefab_detour = null;

                deployed = false;

                Debug.Log("Texture Replacer: BuildingInfo Methods restored!");
            }
        }

        public new virtual void InitializePrefab()
        {
            try
            {
                TextureReplacer.instance.ProcessBuilding(base.GetComponent<BuildingInfo>());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
            base.InitializePrefab();
            RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);
        }
    }
}
