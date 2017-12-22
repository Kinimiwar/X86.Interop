﻿using System;

namespace X86.Interop
{
    /// <summary>
    /// Patches a location in memory with a JMP instruction to intercept execution.
    /// </summary>
    public class JumpPatch : Patch
    {
        private readonly Action<X86Writer> _writeIntercept;
        private ManagedX86Asm _asm;

        /// <summary>
        /// Patches a location in memory with a JMP instruction to intercept execution.
        /// </summary>
        /// <param name="address">The address to write the JMP asm</param>
        /// <param name="writeIntercept">A delegate that writes the intercept asm, which will be JMPed to</param>
        public JumpPatch(IntPtr address, Action<X86Writer> writeIntercept)
            : base(address)
        {
            Guard.ArgumentNotNull(nameof(writeIntercept), writeIntercept);
            _writeIntercept = writeIntercept;
            _writeAsm = writer => writer.Jmp(Target.Address);
        }

        /// <summary>
        /// Patches a location in memory with a JMP instruction to intercept execution.
        /// </summary>
        /// <param name="dllName">The name of the native dll loaded in memory</param>
        /// <param name="offset">The offset from the base address of the dll</param>
        /// <param name="writeIntercept">A delegate that writes the intercept asm, which will be JMPed to</param>
        public JumpPatch(string dllName, UInt32 offset, Action<X86Writer> writeIntercept)
            : this(DllHelper.GetOffset(dllName, offset), writeIntercept)
        {
        }

        /// <summary>
        /// Asm is lazy-loaded, only written to memory once requested.
        /// </summary>
        public IX86Asm Target
        {
            get { return _asm ?? (_asm = X86Asm.Create(_writeIntercept)); }
        }

        protected override void Dispose(bool disposing)
        {
            _asm?.Dispose();
            _asm = null;
            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return string.Format("{0} => Jump to {1} @{2}", Address.ToHexString(), Target.GetType().Name, Target.Address.ToHexString());
        }
    }
}
