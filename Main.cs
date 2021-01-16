using NESSharp.Common.Input;
using NESSharp.Core;
using NESSharp.Core.Mappers;
using NESSharp.Lib.SceneManager;
using NESSharp.Lib.VRamQueue;
using static NESSharp.Core.AL;

namespace FlickerTest {
	class Program {
		static void Main(string[] args) {
			ROMManager.SetMapper(new Mapper000());

			ROMManager.AddPrgBank(0, (id, bank) => {
				EngineStuff(id, bank);
				Scenes(id, bank);
			});
			
			ROMManager.AddChrBank(0, (id, bank) => {
				Include.File(@"chr/sp-plain.chr");
			});

			var engine = Module<Engine>();
			ROMManager.SetInterrupts(engine.NMI, engine.Reset, engine.IRQ);
			ROMManager.WriteToFile(@"flicker-test");
		}

		private static void EngineStuff(U8 id, Bank bank) {
			Module<VRamQueue>().SetupAligned(0x0600, 64,
				VRamQueue.Option.Addr,
				VRamQueue.Option.Increment,
				VRamQueue.Option.TileArray
			);
			Include.Module(Module<VRamQueue>());
			Include.Module(Module<Gamepads>().Setup(1));
			Include.Module(Module<SceneManager>().AddScene(
				Module<Scenes.Title>(),
				Module<Scenes.FlickerTest>()
			));
			Include.Module(Module<Engine>());
		}
		private static void Scenes(U8 id, Bank bank) {
			Include.Module(Module<Scenes.Title>(NES.Mem.Remainder()));
			Include.Module(Module<Scenes.FlickerTest>(NES.Mem.Remainder()));
		}
	}
}
