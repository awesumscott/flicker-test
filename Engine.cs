using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Common.Mapper30;
using NESSharp.Core;
using NESSharp.Lib.VRamQueue;
using static NESSharp.Core.AL;

namespace FlickerTest {
	public enum BankedFuncs {
		Flicker_Load,
		Flicker_Step,
		Title_Load,
		Title_Step
	}
	public static class Engine {
		public static Var8 nmiCount;
		public static BankSwitchTable bankCallTable = new BankSwitchTable();

		static Engine() {
			bankCallTable.Add(new BankedSubroutine(BankedFuncs.Flicker_Load,		Rom.Bank_Flicker,		Scenes.FlickerTest.Load));
			bankCallTable.Add(new BankedSubroutine(BankedFuncs.Flicker_Step,		Rom.Bank_Flicker,		Scenes.FlickerTest.Step));
			bankCallTable.Add(new BankedSubroutine(BankedFuncs.Title_Load,			Rom.Bank_Title,			Scenes.Title.Load));
			bankCallTable.Add(new BankedSubroutine(BankedFuncs.Title_Step,			Rom.Bank_Title,			Scenes.Title.Step));
		}

		[Dependencies]
		public static void Variables() {
			//Modules
			Include.Module(typeof(Scenes.Manager));
			//Variables
			nmiCount = Var8.New(zp, "NMI_Count");
			
		}

		[DataSection]
		public static void BankCallTable() => bankCallTable.Write();

		[CodeSection]
		public static void Reset() {
			NES.IRQ.Disable();
			Use(Asm.CLD);
			NES.APU.FrameCounter.Set(0x40);
			Stack.Reset();
			NES.PPU.Control.Set(0);
			NES.PPU.Mask.Set(0);
			NES.APU.DMC.Disable();
			//NES.APU.SetChannelsEnabled(NES.APU.Channels.DMC | NES.APU.Channels.Pulse1);

			Hardware.WaitForVBlank();
			Hardware.ClearRAM();
			Hardware.WaitForVBlank();

			//TODO: move this to title, along with datasection
			Comment("Load palettes");
			Hardware.LoadPalettes(Palettes);

			//GoSub(Scenes.FlickerTest.Load);
			Scenes.Manager.LoadScene(Scenes.Manager.IDs.Title);
			Hardware.WaitForVBlank();
			NES.PPU.Control.Set(NES.PPU.LazyControl);
			NES.PPU.Mask.Set(NES.PPU.LazyMask);
			Scrolling.Update();
			Loop.Infinite(() => {
				Comment("Main Loop");

				GoSub(Gamepads.Read);

				//GoSub(Scenes.FlickerTest.Step);
				Scenes.Manager.Step();

				A.Set(nmiCount);
				Loop.Do().While(() => nmiCount.Equals(A) );

				Scenes.Manager.CheckLoadNeeded();

				//Set shadow OAM address
				NES.PPU.OAM.Address.Set(0x00);	//low byte of RAM address
				NES.PPU.OAM.DMA.Set(0x02);		//high byte of RAM address
			});
		}
		[Interrupt]
		public static void NMI() {
			Stack.Backup();

			nmiCount++;

			VRamQueue.Execute();
			
			NES.PPU.Control.Set(NES.PPU.LazyControl);
			NES.PPU.Mask.Set(NES.PPU.LazyMask);
			Scrolling.Update();

			Stack.Restore();
		}
		[Interrupt]
		public static void IRQ() {
			//Just jump back into regular execution
		}
		[DataSection]
		public static void Palettes() {
			U8 black = 0x0F;
			Raw(black, 0x27, 0x07, 0x02,		black, 0x36, 0x17, 0x0F,		black, 0x30, 0x21, 0x0F,		black, 0x27, 0x17, 0x0F);
			Raw(black, 0x06, 0x16, 0x26,		black, 0x02, 0x38, 0x3C,		black, 0x1C, 0x15, 0x14,		black, 0x02, 0x38, 0x3C);
		}
	}
}
