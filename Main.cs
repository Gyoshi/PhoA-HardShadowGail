using HarmonyLib;
using Mono.Cecil.Cil;
using Rewired.UI.ControlMapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;
using static UnityEngine.Random;

namespace HardShadowGail
{
#if DEBUG
    [EnableReloading]
#endif
    internal static class Main
    {
        public static Harmony harmony;
        public static UnityModManager.ModEntry.ModLogger logger;

        static void Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;

            modEntry.OnUpdate = OnUpdate;
#if DEBUG
            modEntry.OnUnload = Unload;
#endif
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }
#if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmony.UnpatchAll();

            return true;
        }
#endif
        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {

        }
    }

    // Increase max number of clones from 2/3 to 3/4
    [HarmonyPatch(typeof(ShadowLogic), "_SpawnNewShadow")]
    static class SpawnMore_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_2)
                    instruction.opcode = OpCodes.Ldc_I4_3;
                else if (instruction.opcode == OpCodes.Ldc_I4_3)
                    instruction.opcode = OpCodes.Ldc_I4_4;
                yield return instruction;
            }
        }

    }

    // Clones respawn
    [HarmonyPatch(typeof(ShadowLogic), "ReceiveAttackResult")]
    static class Respawn_Patch
    {
        static MethodInfo SpawnNewShadow = AccessTools.Method(typeof(ShadowLogic), "_SpawnNewShadow");
        static void Postfix(ShadowLogic __instance, bool __result)
        {
            if (__result)
                SpawnNewShadow.Invoke(__instance, null);

        }

    }

    //// Spawn closer to Gail
    //[HarmonyPatch(typeof(ShadowLogic), "_GoToState")]
    //static class GoToState_Patch
    //{
    //    static Enum STATE = (Enum)AccessTools.Field(typeof(ShadowLogic), "SHADOW_STATE").GetValue(null);
    //    static void Postfix(ShadowLogic __instance, STATE __new_state)
    //    {
    //        if (__new_state == 0x03)
    //        {
    //            Main.logger.Log("WARP");
    //        }
    //    }
    //}

    // Run faster
    [HarmonyPatch(typeof(ShadowLogic), "Initialize_Shadow")]
    static class Init_Patch
    {
        static FieldInfo maxSpeed = AccessTools.Field(typeof(ShadowLogic), "_max_vx_run");
        static void Postfix(ShadowLogic __instance)
        {
            maxSpeed.SetValue(__instance, 9.25f); // Run speed from 7.25 to 9.25
        }
    }

    // Infinite eating
    [HarmonyPatch(typeof(ShadowLogic), "_STATE_EatFood")]
    static class Eat_Patch // TODO: fix visual bug, stuck on idle animation :/
    {
        static FieldInfo stateCount = AccessTools.Field(typeof(ShadowLogic), "_state_count");
        static void Postfix(ShadowLogic __instance)
        {
            stateCount.SetValue(__instance, 0);
        }
    }

    // Increase APM
    // Move towards player
    [HarmonyPatch(typeof(ShadowLogic), "_STATE_AggroIdle")]
    static class Idle_Patch
    {
        static FieldInfo waitTime = AccessTools.Field(typeof(ShadowLogic), "_wait_time");
        static FieldInfo velocityField = AccessTools.Field(typeof(ShadowLogic), "_velocity");
        static FieldInfo animField = AccessTools.Field(typeof(ShadowLogic), "_anim");
        static MethodInfo ChooseAttack = AccessTools.Method(typeof(ShadowLogic), "_ChooseNextAttack");
        static MethodInfo RunUp = AccessTools.Method(typeof(ShadowLogic), "_Helper_RunUpToPlayer");
        static void Prefix(ref ShadowLogic __instance)
        {
            //if ((float)waitTime.GetValue(__instance) > -1f) // Start running
            //{
            //    RunUp.Invoke(__instance, null); // Run up to player instead of doing nothing
            //}
            if ((float)waitTime.GetValue(__instance) > 0.2f) // Reduce Idle time from 0.5s to 0.2s (servant from 5s to 4.4s)
            {
                //Vector3 velocity = (Vector3)velocityField.GetValue(__instance);
                //velocity.x = 0f;
                //velocityField.SetValue(__instance, velocity);

                //Animator anim = ((Animator)animField.GetValue(__instance));
                //anim.SetInteger(global::GL.anim, 0);
                //animField.SetValue(__instance, anim);

                ChooseAttack.Invoke(__instance, null);
            }
        }
    }

    // Choose spear more often
    // - increase roll chance
    // - eh, not really necessary with increased APM
    // - Except it is if closer spawns?

    // Spikes to prevent jumping? >:D
    // - maybe periodically to allow straight up jumps

    // Spear cheese prevention :think:
    // - Disappearing platforms on the sides

    // Spawn frogs

    // Spawn ...? if you damage the clones
    // - Respawning clones already punishing enough

    // Ninja gails (cannot be damaged if not attacking...somehow)

    // Victory fanfare!
    // Message like the "congrats on beating the game" one?
}