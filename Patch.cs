using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Terraria;
using static HarmonyLib.Code;
using Microsoft.Xna.Framework;

namespace Tweaks
{
	[HarmonyPatch]
    public class Patch
    {
        public static void Execute()
        {
            var harmony = new Harmony("com.pezeee.no_biome_torches_from_pots");
			
			harmony.PatchAll();
        }

        public static int NewItemTorch(Terraria.DataStructures.IEntitySource source, int X, int Y, int Width, int Height, int type, int stack = 1, bool noBroadcast = false, int prefix = 0, NewItemOwnership ownership = NewItemOwnership.None, Vector2? velocity = null, Terraria.Item.NewItemModifier modifier = null)
        {
            type = 8;
            #pragma warning disable CS0618 // Type or member is obsolete
            return Item.NewItem(source, X,  Y, Width, Height, type, stack, noBroadcast, prefix, ownership, velocity, modifier);
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        [HarmonyPatch(typeof(WorldGen), "SpawnThingsFromPot")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher
                .MatchStartForward(
                    Ldc_I4[5293] //mushroom torch
                )
                .MatchStartForward(
                    Ret //return after drop glowstick
                )
                .Advance() //so it doesn't match current ret
                .MatchStartForward(
                    Ret //return after drop torch
                )
                .Advance(-2) //skipping pop
                .SetInstruction(
                    Call[AccessTools.Method(typeof(Patch), "NewItemTorch")]
                );


            return codeMatcher.Instructions();
        }
    }
}