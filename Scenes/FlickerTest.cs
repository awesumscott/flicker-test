using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Core;
using NESSharp.Lib.VRamQueue;
using static NESSharp.Core.AL;

/*

	Flicker Test Scene

	B - skip rate decreased
	A - skip rate increased
	Select - Toggle sprite layouts (single line and rows of 20)
	D-pad - Move around player character (no-flicker) to test impact on flickering

*/

namespace FlickerTest.Scenes {
	public static class FlickerTest {
		public static Var8 CharX;
		public static Var8 CharY;
		private static Var8 _processed;
		private static Var8 _index;
		private static Var8 _pattern; //sprite layout (0 = single row, 1 = several rows)
		private static Var8 _skipOptionIndex;
		private static Array<Var8> _scatterX;
		private static Array<Var8> _scatterY;

		private static U8 _CharTile0 = 0x11;
		private static U8 _CharTile1 = 0x12;
		private static U8 _CharTile2 = 0x21;
		private static U8 _CharTile3 = 0x22;
		private static U8 _Junk = 0x10;
		private static U8 _MoveDist = 0x2;

		[Dependencies]
		public static void Declarations() {
			CharX				= Var8.New(ram, "FlickerTest_CharX");
			CharY				= Var8.New(ram, "FlickerTest_CharY");
			_processed			= Var8.New(zp, "spritesProcessed");
			_index				= Var8.New(zp, "spriteIndex");
			_pattern			= Var8.New(zp, "spritePattern");
			_skipOptionIndex	= Var8.New(zp, "skipOptionIndex");
			_scatterX			= Array<Var8>.New(16, ram, "scatterX");
			_scatterY			= Array<Var8>.New(16, ram, "scatterY");
		}

		[Subroutine]
		public static void Load() {
			NES.PPU.ClearNametable0();

			VRamQueue.Reset();

			//Draws "SKIP:"
			NES.PPU.SetAddress(0x232C);
			NES.PPU.Data.Write(0xF8, 0xF0, 0xEE, 0xF5, 0xCB, 0x00, 0xDC, 0xDD);

			CharX.Set(0x80);
			CharY.Set(0x80);
			
			Sprites[0].Tile.Set(_CharTile0);
			Sprites[0].Attr.Set(0);
			Sprites[1].Tile.Set(_CharTile1);
			Sprites[1].Attr.Set(0);
			Sprites[2].Tile.Set(_CharTile2);
			Sprites[2].Attr.Set(0);
			Sprites[3].Tile.Set(_CharTile3);
			Sprites[3].Attr.Set(0);
			
			Loop.Descend(X.Set(15), () => {
				_scatterX[X].Set(LabelFor(PatternScatter_StartX).Offset(X));
				_scatterY[X].Set(LabelFor(PatternScatter_StartY).Offset(X));
			});

			_processed.Set(0);
			_index.Set(0);
			//_pattern.Set(0);
			//_skipOptionIndex.Set(0);
			GoSub(DrawSkipNum);

			NES.PPU.LazyScrollX.Set(0);
			NES.PPU.LazyScrollY.Set(0);
			
			NES.PPU.LazyControl.Set((int)(NES.PPU.ControlFlags.NMIEnabled | NES.PPU.ControlFlags.BackgroundPT0 | NES.PPU.ControlFlags.SpritePT0)); //turn on nmi
			NES.PPU.LazyMask.Set(0b00011110);
		}

		[Subroutine]
		public static void DrawSkipNum() {
			VRamQueue.Address.Set(0x2332);
			VRamQueue.TileArray.Draw_Manual(2);
			VRamQueue.Push(() => A.Set(LabelFor(SkipOptionTile0).Offset(X.Set(_skipOptionIndex))));
			VRamQueue.Push(() => A.Set(LabelFor(SkipOptionTile1).Offset(X)));
			VRamQueue.DonePushing();
		}

		[Subroutine]
		public static void Step() {
			Y.Set(0);
			If(	Option(() => Gamepads.Player[Y].Held(NES.Button.Right), () => {
					CharX.Set(CharX.Add(_MoveDist));
				}),
				Option(() => Gamepads.Player[Y].Held(NES.Button.Left), () => {
					CharX.Set(CharX.Subtract(_MoveDist));
				})
			);
			If(	Option(() => Gamepads.Player[Y].Held(NES.Button.Down), () => {
					CharY.Set(CharY.Add(_MoveDist));
				}),
				Option(() => Gamepads.Player[Y].Held(NES.Button.Up), () => {
					CharY.Set(CharY.Subtract(_MoveDist));
				})
			);
			If(	Option(() => Gamepads.Player[Y].Pressed(NES.Button.B), () => {
					_skipOptionIndex--;
					If(() => _skipOptionIndex.Equals(255), () => {
						_skipOptionIndex.Set(0);
					});
					GoSub(DrawSkipNum);
				}),
				Option(() => Gamepads.Player[Y].Pressed(NES.Button.A), () => {
					_skipOptionIndex++;
					If(() => _skipOptionIndex.Equals(16), () => {
						_skipOptionIndex.Set(15);
					});
					GoSub(DrawSkipNum);
				})
			);
			Y.Set(0);
			If(() => Gamepads.Player[Y].Pressed(NES.Button.Select), () => {
				_pattern++;
				If(() => _pattern.Equals(3), () => {
					_pattern.Set(0);
				});
			});
			If(() => Gamepads.Player[Y].Pressed(NES.Button.Start), () => {
				Manager.QueueScene(Manager.IDs.Title);
			});

			Sprites[0].X.Set(CharX.Subtract(8));
			Sprites[0].Y.Set(CharY.Subtract(8));
			Sprites[1].X.Set(CharX);
			Sprites[1].Y.Set(CharY.Subtract(8));
			Sprites[2].X.Set(CharX.Subtract(8));
			Sprites[2].Y.Set(CharY);
			Sprites[3].X.Set(CharX);
			Sprites[3].Y.Set(CharY);

			
			var jx = Temp[1];
			var jy = Temp[2];

			U8 startOffset = 4;

			If ( Option(() => _pattern.Equals(0), () => {
					jx.Set(0x04);
					jy.Set(0x40);
			
					_processed.Set(0);
					Y.Set(startOffset);

					Loop.Do(() => {
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(jy);
						Sprites[0].Tile[Y].Set(_Junk);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(jx);

						jx.Set(jx.Add(4));
				
						Skipterate_Next();
						_processed++;
					}).While(() => A.Set(_processed).NotEquals(60));
				}),
				Option(() => _pattern.Equals(1), () => {
					U8 xStart = 0x30;
					jx.Set(xStart);
					jy.Set(0x30);

					_processed.Set(0);
					Y.Set(startOffset);

					Loop.Do(() => {
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(jy);
						Sprites[0].Tile[Y].Set(_Junk);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(jx);

						jx.Set(jx.Add(8));
						
						Skipterate_Next();
						If(Any(() => _processed.Equals(19), () => _processed.Equals(39)), () => {
							jy.Set(jy.Add(8));
							jx.Set(xStart);
						});
						_processed++;
					}).While(() => A.Set(_processed).NotEquals(60));
				}),
				Default(() => {
					Y.Set(startOffset);

					//Loop.Descend(X.Set(15), () => {
					X.Set(15);
					Loop.Do(() => {
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(_scatterY[X]);
						Sprites[0].Tile[Y].Set(0x13);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(_scatterX[X]);
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(_scatterY[X]);
						Sprites[0].Tile[Y].Set(0x14);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(_scatterX[X].Add(8));
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(_scatterY[X].Add(8));
						Sprites[0].Tile[Y].Set(0x23);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(_scatterX[X]);
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						Sprites[0].Y[Y].Set(_scatterY[X].Add(8));
						Sprites[0].Tile[Y].Set(0x24);
						Sprites[0].Attr[Y].Set(1);
						Sprites[0].X[Y].Set(_scatterX[X].Add(8));
						Skipterate_Next();

						_scatterX[X].Set(z => A.Set(z).Add(LabelFor(PatternScatter_MoveX).Offset(X)));
						_scatterY[X].Set(z => A.Set(z).Add(LabelFor(PatternScatter_MoveY).Offset(X)));

						//60 to 180
						If(	Option(() => A.Set(_scatterY[X]).GreaterThan(160), () => {
								_scatterY[X].Set(z => z.Subtract(100));
							}),
							Option(() => A.Set(_scatterY[X]).LessThan(60), () => {
								_scatterY[X].Set(z => z.Add(100));
							})
						);
						X--;
					}).While(() => X.NotEquals(0));
					//});
				})
			);
			Skipterate_Next();
		}

		private static void Skipterate_PrepareY(U8 startOffset) {
			Y.Set(_index.Add(startOffset).Multiply(4));
		}

		private static void Skipterate_Next() {
			//[1, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 49, 53, 59]
			//Stack.Preserve(X, () => {
			_index.Set(A.Set(LabelFor(SkipOption).Offset(Y.Set(_skipOptionIndex))).Add(_index)); //skipterate option for range 60
			If(() => A.Set(_index).GreaterThan(59), () => { //index > 59?
				_index.Set(z => z.Subtract(60));
			});
			//});
		}

		[DataSection]
		public static void SkipOption() {
			Raw(1, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 49, 53, 59); //indexes: 0-15
			//fav: 23 (index 6)
		}
		
		[DataSection]
		public static void SkipOptionTile0() {
			Raw(
				Tile.Zero,	//1,
				Tile.Zero,	//7,
				Tile.One,	//11,
				Tile.One,	//13,
				Tile.One,	//17,
				Tile.One,	//19,
				Tile.Two,	//23,
				Tile.Two,	//29,
				Tile.Three,	//31,
				Tile.Three,	//37,
				Tile.Four,	//41,
				Tile.Four,	//43,
				Tile.Four,	//47,
				Tile.Four,	//49,
				Tile.Five,	//53,
				Tile.Five	//59
			);
		}
		[DataSection]
		public static void SkipOptionTile1() {
			Raw(
				Tile.One,	//1,
				Tile.Seven,	//7,
				Tile.One,	//11,
				Tile.Three,	//13,
				Tile.Seven,	//17,
				Tile.Nine,	//19,
				Tile.Three,	//23,
				Tile.Nine,	//29,
				Tile.One,	//31,
				Tile.Seven,	//37,
				Tile.One,	//41,
				Tile.Three,	//43,
				Tile.Seven,	//47,
				Tile.Nine,	//49,
				Tile.Three,	//53,
				Tile.Nine	//59
			);
		}

		[DataSection]
		public static void PatternScatter_StartX() => Raw(133, 92, 50, 105, 161, 32, 9, 118, 246, 176, 143, 42, 19, 3, 240, 24);
		[DataSection]
		public static void PatternScatter_StartY() => Raw(146, 128, 95, 71, 140, 145, 119, 124, 65, 127, 108, 99, 70, 104, 118, 85);
		[DataSection]
		public static void PatternScatter_MoveX() => Raw(0xFD, 0xFE, 0xFE, 0x01, 0x02, 0x01, 0xFE, 0xFE, 0x01, 0xFE, 0xFD, 0xFD, 0x02, 0x01, 0xFE, 0xFD);
		[DataSection]
		public static void PatternScatter_MoveY() => Raw(0xFD, 0x02, 0x01, 0xFE, 0x02, 0xFE, 0x02, 0x01, 0xFD, 0xFD, 0x01, 0x01, 0x02, 0xFE, 0x01, 0xFD);

		public static class Tile {
			public static readonly U8 Zero =	0xDC;
			public static readonly U8 One =		0xDD;
			public static readonly U8 Two =		0xDE;
			public static readonly U8 Three =	0xDF;
			public static readonly U8 Four =	0xE0;
			public static readonly U8 Five =	0xE1;
			public static readonly U8 Six =		0xE2;
			public static readonly U8 Seven =	0xE3;
			public static readonly U8 Eight =	0xE4;
			public static readonly U8 Nine =	0xE5;
		}

	}
}
