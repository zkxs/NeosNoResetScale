using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NeosNoResetScale
{
    public class NeosNoResetScale : NeosMod
    {
        public override string Name => "NeosNoResetScale";
        public override string Author => "runtime";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/zkxs/NeosNoResetScale";

        private static MethodInfo _startTask;
        private static MethodInfo _isAtScale;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.michaelripley.NeosNoResetScale");

            // a function call we'll be searching for later
            _startTask = AccessTools.DeclaredMethod(typeof(Worker), nameof(Worker.StartTask), new Type[] { typeof(Func<Task>) });
            if (_startTask == null)
            {
                Error("Could not find method Worker.StartTask(Func<Task>)");
                return;
            }

            // another function call we'll be searching for later
            _isAtScale = AccessTools.DeclaredMethod(typeof(UserRoot), nameof(UserRoot.IsAtScale), new Type[] { typeof(float) });
            if (_isAtScale == null)
            {
                Error("Could not find method UserRoot.IsAtScale(float)");
                return;
            }

            // where we start reading IL from
            MethodInfo openContextMenu = AccessTools.DeclaredMethod(typeof(CommonTool), "OpenContextMenu");
            if (openContextMenu == null)
            {
                Error("Could not find method CommonTool.OpenContextMenu(*)");
                return;
            }

            // constructor for our target async method body
            MethodInfo asyncMethodConstructor = FindAsyncMethod(PatchProcessor.GetOriginalInstructions(openContextMenu));
            if (asyncMethodConstructor == null)
            {
                Error("Could not find target async block constructor in CommonTool.OpenContextMenu(*)");
                return;
            }
            Debug($"Found async method constructor: \"{asyncMethodConstructor.FullDescription()}\"");

            // helpful attribute that points us to the async method body
            AsyncStateMachineAttribute asyncAttribute = (AsyncStateMachineAttribute)asyncMethodConstructor.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
            if (asyncAttribute == null)
            {
                Error($"Could not find AsyncStateMachine for \"{asyncMethodConstructor.FullDescription()}\"");
                return;
            }

            // our actual async method body we want to patch
            MethodInfo asyncMethodBody = AccessTools.DeclaredMethod(asyncAttribute.StateMachineType, "MoveNext", new Type[] { });
            if (asyncMethodBody == null)
            {
                Error("Could not find target async block in CommonTool.OpenContextMenu(*)");
                return;
            }
            // should be FrooxEngine.CommonTool.'<>c__DisplayClass333_0'.'<<OpenContextMenu>b__0>d'.MoveNext()
            Debug($"Found async method: \"{asyncMethodBody.FullDescription()}\"");

            harmony.Patch(asyncMethodBody, transpiler: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(NeosNoResetScale), nameof(Transpiler))));
        }

        // search for StartTask((Func<Task>) (async () => {...})) and grab the async method
        private static MethodInfo FindAsyncMethod(List<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int callIdx = 0; callIdx < codes.Count; callIdx++)
            {
                if (codes[callIdx].Calls(_startTask))
                {
                    // idx-5 should be enough instructions to search to fiund the ldftn we're looking for
                    for (int ldftnIdx = callIdx - 1; ldftnIdx >= Math.Max(0, callIdx - 5); ldftnIdx--)
                    {
                        if (codes[ldftnIdx].opcode.Equals(OpCodes.Ldftn))
                        {
                            return (MethodInfo)codes[ldftnIdx].operand;
                        }
                    }
                }
            }

            return null;
        }

        // search for IL matching if(FrooxEngine.UserRoot::IsAtScale) and make it unconditionally take the true branch
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug($"There are {instructions.Count()} instructions");
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int idx = 1; idx < codes.Count; idx++)
            {
                CodeInstruction call = codes[idx - 1];
                CodeInstruction branch = codes[idx];

                if (call.Calls(_isAtScale) && branch.opcode.Equals(OpCodes.Brfalse_S))
                {
                    // replace the brfalse.s with a pop, which makes it always take the true branch.
                    codes[idx] = new CodeInstruction(OpCodes.Pop);

                    Msg("Transpiler succeeded");
                    return codes.AsEnumerable();
                }
            }

            throw new TranspilerException("Failed to find IL matching if(FrooxEngine.UserRoot::IsAtScale)");
        }

        private class TranspilerException : Exception
        {
            public TranspilerException(string message) : base(message) { }
        }
    }
}
