using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;

using var dt = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, false);
_ = dt;
