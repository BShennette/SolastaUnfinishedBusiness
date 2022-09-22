﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

internal static class CursorLocationGeometricShapePatcher
{
    //PATCH: UseHeightOneCylinderEffect
    [HarmonyPatch(typeof(CursorLocationGeometricShape), "UpdateGeometricShape")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UpdateGeometricShape_Patch
    {
        public static void MyUpdateCubePosition_Regular(
            [NotNull] GeometricShape __instance,
            Vector3 origin,
            float edgeSize,
            bool adaptToGroundLevel,
            bool isValid,
            int height)
        {
            __instance.UpdateCubePosition_Regular(origin, edgeSize, adaptToGroundLevel, isValid);

            if (!Main.Settings.UseHeightOneCylinderEffect)
            {
                return;
            }

            if (height == 0)
            {
                return;
            }

            var vector3 = new Vector3();

            if (!adaptToGroundLevel)
            {
                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 = new Vector3(0.5f, 0.0f, 0.5f);
                }

                if (height % 2.0 == 0.0)
                {
                    vector3.y = 0.5f;
                }
            }
            else
            {
                vector3.y = (float)((0.5 * height) - 0.5);

                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 += new Vector3(0.5f, 0.0f, 0.5f);
                }
            }

            var transform = __instance.cubeRenderer.transform;
            transform.SetPositionAndRotation(origin + vector3, Quaternion.identity);
            transform.localScale = new Vector3(edgeSize, height, edgeSize);
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetParameter2Field =
                typeof(CursorLocationGeometricShape).GetField("targetParameter2",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var updateCubePositionRegularMethod = typeof(GeometricShape).GetMethod("UpdateCubePosition_Regular");
            var myUpdateCubePositionRegularMethod =
                typeof(UpdateGeometricShape_Patch).GetMethod("MyUpdateCubePosition_Regular");

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(updateCubePositionRegularMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Ldfld, targetParameter2Field);
                    yield return new CodeInstruction(OpCodes.Call, myUpdateCubePositionRegularMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    //PATCH: UseHeightOneCylinderEffect
    [HarmonyPatch(typeof(GameLocationTargetingManager), "BuildAABB")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class BuildAABB_Patch
    {
        public static void Postfix(GameLocationTargetingManager __instance)
        {
            if (!Main.Settings.UseHeightOneCylinderEffect)
            {
                return;
            }

            if (__instance.shapeType != MetricsDefinitions.GeometricShapeType.Cube)
            {
                return;
            }

            if (__instance.geometricParameter2 <= 0)
            {
                return;
            }

            var edgeSize = __instance.geometricParameter;
            var height = __instance.geometricParameter2;

            Vector3 vector = new();

            if (__instance.hasMagneticTargeting || __instance.rangeType == RuleDefinitions.RangeType.Self)
            {
                if (edgeSize % 2.0 == 0.0)
                {
                    vector = new Vector3(0.5f, 0f, 0.5f);
                }

                if (height % 2.0 == 0.0)
                {
                    vector.y = 0.5f;
                }
            }
            else
            {
                vector = new Vector3(0.0f, (float)((0.5 * height) - 0.5), 0.0f);

                if (edgeSize % 2.0 == 0.0)
                {
                    vector += new Vector3(0.5f, 0.0f, 0.5f);
                }
            }

            __instance.bounds = new Bounds(__instance.origin + vector, new Vector3(edgeSize, height, edgeSize));
        }
    }

    //PATCH: UseHeightOneCylinderEffect
    [HarmonyPatch(typeof(GameLocationTargetingManager), "DoesShapeContainPoint")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class DoesShapeContainPoint_Patch
    {
        public static bool MyCubeContainsPoint_Regular(
            Vector3 cubeOrigin,
            float edgeSize,
            bool hasMagneticTargeting,
            Vector3 point,
            float height)
        {
            var result = GeometryUtils.CubeContainsPoint_Regular(cubeOrigin, edgeSize, hasMagneticTargeting, point);

            if (!Main.Settings.UseHeightOneCylinderEffect)
            {
                return result;
            }

            if (height == 0)
            {
                return result;
            }

            // Code from CubeContainsPoint_Regular modified with height
            var vector3 = new Vector3();

            if (hasMagneticTargeting)
            {
                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 = new Vector3(0.5f, 0f, 0.5f);
                }

                if (height % 2.0 == 0.0)
                {
                    vector3.y = 0.5f;
                }
            }
            else
            {
                vector3.y = (float)((0.5 * height) - 0.5);

                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 += new Vector3(0.5f, 0.0f, 0.5f);
                }
            }

            var vector32 = point - cubeOrigin - vector3;

            result =
                Mathf.Abs(vector32.x) <= (double)0.5f * edgeSize
                && Mathf.Abs(vector32.y) <= (double)0.5f * height
                && Mathf.Abs(vector32.z) <= (double)0.5f * edgeSize;

            return result;
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var geometricParameter2Field =
                typeof(GameLocationTargetingManager).GetField("geometricParameter2",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var cubeContainsPointRegularMethod = typeof(GeometryUtils).GetMethod("CubeContainsPoint_Regular");
            var myCubeContainsPointRegularMethod =
                typeof(DoesShapeContainPoint_Patch).GetMethod("MyCubeContainsPoint_Regular");

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(cubeContainsPointRegularMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Ldfld, geometricParameter2Field);
                    yield return new CodeInstruction(OpCodes.Call, myCubeContainsPointRegularMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
