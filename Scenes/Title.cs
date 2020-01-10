﻿using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Core;
using NESSharp.Lib.VRamQueue;
using static NESSharp.Core.AL;

namespace FlickerTest.Scenes {
	public class Title {
		[Subroutine]
		public static void Load() {
			NES.PPU.ClearNametable0();
			
			VRamQueue.Reset();

			Hardware.LoadPalettes(Palettes);

			DrawBG();

			NES.PPU.LazyScrollX.Set(0);
			NES.PPU.LazyScrollY.Set(0);
			
			NES.PPU.LazyControl.Set((int)(NES.PPU.ControlFlags.NMIEnabled | NES.PPU.ControlFlags.BackgroundPT0)); //turn on nmi
			NES.PPU.LazyMask.Set(0b00011110);
		}

		[Subroutine]
		public static void Step() {
			Y.Set(0);
			If(	Option(() => Gamepads.Player[Y].Pressed(NES.Button.Up), () => {

				}),
				Option(() => Gamepads.Player[Y].Pressed(NES.Button.Down), () => {

				}),
				Option(() => Gamepads.Player[Y].Pressed(NES.Button.Start), () => {
					Manager.QueueScene(Manager.IDs.Flicker);
				})
			);
		}

		private static void DrawBG() {
			NES.PPU.SetAddress(0x2060);
			NES.PPU.Data.Write(Title_Full_0);
			
			NES.PPU.SetAddress(0x2160);
			NES.PPU.Data.Write(Title_Full_1);
			
			NES.PPU.SetAddress(0x2220);
			NES.PPU.Data.Write(Title_Full_2);
			
			NES.PPU.SetAddress(0x2320);
			NES.PPU.Data.Write(Title_Full_3);
		}

		[DataSection]
		public static void Title_Full_0() {	//224
			Raw(0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD1,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD2,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0xF8,0xF0,0xEE,0xF5,0xF9,0xEA,0xF7,0xE6,0xF9,0xEA,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0xEB,0xF1,0xEE,0xE8,0xF0,0xEA,0xF7,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0xE9,0xEA,0xF2,0xF4,0xF3,0xF8,0xF9,0xF7,0xE6,0xF9,0xEE,0xF4,0xF3,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD6,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xD3,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD5,0xD4,0x00,0x00,0x00,0x00,0x00,0x00,0x00);
		}

		[DataSection]
		public static void Title_Full_1() {	//96
			Raw(0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF8,0xE8,0xF4,0xF9,0xF9,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xCC,0xF9,0xED,0xEA,0x00,0xE9,0xF4,0xE8,0xCD,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF1,0xF4,0xFC,0xEA,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00);
		}

		[DataSection]
		public static void Title_Full_2() {	//160
			Raw(0x00,0x00,0xE9,0xC8,0xF5,0xE6,0xE9,0xCB,0x00,0xF2,0xF4,0xFB,0xEA,0x00,0xED,0xEE,0x00,0xF5,0xF7,0xEE,0xF4,0xF7,0xEE,0xF9,0xFE,0x00,0xE8,0xED,0xE6,0xF7,0x00,0x00,
				0x00,0x00,0xF8,0xEA,0xF1,0xEA,0xE8,0xF9,0xCB,0xE8,0xFE,0xE8,0xF1,0xEA,0x00,0xF8,0xF5,0xF7,0xEE,0xF9,0xEA,0x00,0xF2,0xF4,0xE9,0xEA,0xF8,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0xF8,0xF9,0xE6,0xF7,0xF9,0xCB,0x00,0xF9,0xF4,0xEC,0xEC,0xF1,0xEA,0x00,0xEE,0xF3,0xF8,0xF9,0xF7,0xFA,0xE8,0xF9,0xEE,0xF4,0xF3,0xF8,0x00,0x00,0x00,0x00,
				0x00,0x00,0xE6,0xCB,0x00,0x00,0x00,0x00,0x00,0xEE,0xF3,0xE8,0xF7,0xEA,0xE6,0xF8,0xEA,0x00,0xF8,0xF0,0xEE,0xF5,0x00,0xFB,0xE6,0xF1,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x00,0xE7,0xCB,0x00,0x00,0x00,0x00,0x00,0xE9,0xEA,0xE8,0xF7,0xEA,0xE6,0xF8,0xEA,0x00,0xF8,0xF0,0xEE,0xF5,0x00,0xFB,0xE6,0xF1,0x00,0x00,0x00,0x00,0x00,0x00);
		}

		[DataSection]
		public static void Title_Full_3() {	//32
			Raw(0x00,0x00,0x00,0x00,0xDE,0xDC,0xDD,0xE5,0x00,0xC8,0x00,0xF8,0xE8,0xF4,0xF9,0xF9,0xC7,0xF8,0xE8,0xF4,0xF9,0xF9,0xF1,0xF4,0xFC,0xEA,0xCA,0xFA,0xF8,0x00,0x00,0x00);
		}

		[DataSection]
		public static void Palettes() {
			U8 black = 0x0F;
			Raw(black, 0x03, 0x23, 0x33,		black, 0x01, 0x21, 0x31,		black, 0x30, 0x21, 0x0F,		black, 0x27, 0x17, 0x0F);
			Raw(black, 0x06, 0x16, 0x26,		black, 0x04, 0x24, 0x34,		black, 0x1C, 0x15, 0x14,		black, 0x02, 0x38, 0x3C);
		}
	}
}