using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Core;
using NESSharp.Lib.SceneManager;
using NESSharp.Lib.VRamQueue;
using static NESSharp.Core.AL;

namespace FlickerTest {
	public enum BankedFuncs {
		Flicker_Load,
		Flicker_Step,
		Title_Load,
		Title_Step
	}
	public class Engine : Module {
		public VByte nmiCount;

		[Dependencies]
		public void Variables() {
			//Variables
			nmiCount = VByte.New(Zp, "NMI_Count");
		}

		[CodeSection]
		public void Reset() {
			NES.IRQ.Disable();
			CPU6502.CLD();
			NES.APU.FrameCounter.Set(0x40);
			Stack.Reset();
			NES.PPU.Control.Set(0);
			NES.PPU.Mask.Set(0);
			NES.APU.DMC.Disable();
			//NES.APU.SetChannelsEnabled(NES.APU.Channels.DMC | NES.APU.Channels.Pulse1);

			Hardware.WaitForVBlank();
			Hardware.ClearRAM();
			Hardware.WaitForVBlank();

			////TODO: move this to title, along with datasection
			//Comment("Load palettes");
			//Hardware.LoadPalettes(Palettes);

			Module<SceneManager>().Load(Module<Scenes.Title>());
			Hardware.WaitForVBlank();
			NES.PPU.Control.Set(NES.PPU.LazyControl);
			NES.PPU.Mask.Set(NES.PPU.LazyMask);
			ScrollUtil.Update();
			Loop.Infinite(_ => {
				Comment("Main Loop");

				Module<Gamepads>().Update();

				Module<SceneManager>().Step();

				A.Set(nmiCount);
				Loop.Do_old().While(() => nmiCount.Equals(A) );
				
				Module<SceneManager>().CheckLoadNeeded();

				//Set shadow OAM address
				NES.PPU.OAM.Address.Set(0x00);	//low byte of RAM address
				NES.PPU.OAM.DMA.Set(0x02);		//high byte of RAM address
			});
		}
		[Interrupt]
		public void NMI() {
			Stack.Backup();

			nmiCount.Inc();

			Module<VRamQueue>().Execute();
			
			NES.PPU.Control.Set(NES.PPU.LazyControl);
			NES.PPU.Mask.Set(NES.PPU.LazyMask);
			ScrollUtil.Update();

			Stack.Restore();
		}
		[Interrupt]
		public void IRQ() {} //Just jump back into regular execution
		//[DataSection]
		//public void Palettes() {
		//	U8 black = 0x0F;
		//	Raw(black, 0x27, 0x07, 0x02,		black, 0x36, 0x17, 0x0F,		black, 0x30, 0x21, 0x0F,		black, 0x27, 0x17, 0x0F);
		//	Raw(black, 0x06, 0x16, 0x26,		black, 0x02, 0x38, 0x3C,		black, 0x1C, 0x15, 0x14,		black, 0x02, 0x38, 0x3C);
		//}
	}
}
