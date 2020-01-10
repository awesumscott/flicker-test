using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Common.Mapper30;
using NESSharp.Core;
using NESSharp.Core.Mappers;
using NESSharp.Lib.VRamQueue;
using System.Reflection;
using static NESSharp.Core.AL;

namespace FlickerTest {
	public class Rom {
		public const int Bank_Title = 0;
		public const int Bank_Flicker = 0;
		[PrgBankDef(0)]
		public static void FixedBank() {
			Include.Module(
				typeof(BankSwitching),
				typeof(BankSwitchTable),
				typeof(VRamQueue),
				typeof(Gamepads),
				typeof(Scrolling),
				typeof(ShadowAttributeTable_old),
				//typeof(Scenes.Manager),
				typeof(Scenes.Title),
				typeof(Scenes.FlickerTest),
				typeof(Engine)
			);
		}

		[ChrBankDef(0)]
		public static void ChrBank1() {
			Include.File(@"chr/sp-plain.chr");
		}
	}
	class Program {
		static void Main(string[] args) {
			ROMManager.SetMapper(new Mapper0());
			VRamQueue.Init(0x0600, 64,
				VRamQueue.Option.Addr,
				VRamQueue.Option.Increment,
				VRamQueue.Option.TileArray
				//VRamQueue.Option.TilesRLE,
				//VRamQueue.Option.FromAddress,
				//VRamQueue.Option.Tile,
				//VRamQueue.Option.Pause
			);
			ROMManager.CompileRom(typeof(Rom));
			ROMManager.SetInterrupts(Engine.NMI, Engine.Reset, Engine.IRQ);
			ROMManager.WriteToFile(@"flicker-test");
		}
	}
}
