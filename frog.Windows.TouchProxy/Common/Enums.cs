using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace frog.Windows.TouchProxy.Common
{
	public enum ScreenTarget 
	{
		[DescriptionAttribute("Primary (main display)")]
		Primary,
		[DescriptionAttribute("Virtual (all displays)")]
		Virtual 
	}

	public enum ProtocolTraceCategory
	{
		[DescriptionAttribute("OSC")]
		OSC,
		[DescriptionAttribute("TUIO")]
		TUIO
	}
}