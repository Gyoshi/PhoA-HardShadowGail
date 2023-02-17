using HarmonyLib;
using Mono.Cecil.Cil;
using Rewired.UI.ControlMapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

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
    static class Spawn_Patch
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

    // Infinite eating
    [HarmonyPatch(typeof(ShadowLogic), "_STATE_EatFood")]
    static class Eat_Patch
    {
        static FieldInfo stateCount = AccessTools.Field(typeof(ShadowLogic), "_state_count");
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.StoresField(stateCount)) // Don't update _state_count
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                }
                else
                    yield return instruction;
            }
        }
    }

    // Choose spear more often

    // Spikes to prevent jumping? >:D
    // - maybe periodically to allow straight up jumps

    // Spear cheese prevention :think:
    // - Disappearing platforms on the sides

    // Spawn frogs

    // Spawn ...? if you damage the clones

    // Ninja gails (cannot be damaged if not attacking...somehow)
}