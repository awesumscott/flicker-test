using NESSharp.Common;
using NESSharp.Common.Input;
using NESSharp.Core;
using NESSharp.Lib.SceneManager;
using NESSharp.Lib.VRamQueue;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

/*

	Flicker Test Scene

	B - skip rate decreased
	A - skip rate increased
	Select - Toggle sprite layouts (single line and rows of 20)
	D-pad - Move around player character (no-flicker) to test impact on flickering

*/

namespace FlickerTest.Scenes {
	public class FlickerTest : Scene {
		private VByte _charX;
		private VByte _charY;
		private VByte _processed;
		private VByte _index;
		private VByte _pattern; //sprite layout (0 = single row, 1 = several rows, 2 = scattered)
		private VByte _skipOptionIndex;
		private StructOfArrays<ScatterSprite> _scatterSprites;
		
		private static readonly U8 _PlayerTile0	= 0x11;
		private static readonly U8 _PlayerTile1	= 0x12;
		private static readonly U8 _PlayerTile2	= 0x21;
		private static readonly U8 _PlayerTile3	= 0x22;
		private static readonly U8 _NPCTile0	= 0x13;
		private static readonly U8 _NPCTile1	= 0x14;
		private static readonly U8 _NPCTile2	= 0x23;
		private static readonly U8 _NPCTile3	= 0x24;
		private static readonly U8 _JunkTile	= 0x10;
		private static readonly U8 _MoveDist	= 0x2;
		private VRamQueue VRamQueue;
		private Gamepad _gamepad;

		[Dependencies]
		public void Declarations() {
			VRamQueue			= Module<VRamQueue>();
			_gamepad			= Module<Gamepads>()[0];
			_charX				= VByte.New(Ram, $"{nameof(FlickerTest)}{nameof(_charX)}");
			_charY				= VByte.New(Ram, $"{nameof(FlickerTest)}{nameof(_charY)}");
			_processed			= VByte.New(Zp, $"{nameof(FlickerTest)}{nameof(_processed)}");
			_index				= VByte.New(Zp, $"{nameof(FlickerTest)}{nameof(_index)}");
			_pattern			= VByte.New(Zp, $"{nameof(FlickerTest)}{nameof(_pattern)}");
			_skipOptionIndex	= VByte.New(Zp, $"{nameof(FlickerTest)}{nameof(_skipOptionIndex)}");
			_scatterSprites			= StructOfArrays<ScatterSprite>.New($"{nameof(FlickerTest)}{nameof(_scatterSprites)}", 16).Dim(Ram);
			Include.Module(_scatterSprites[0]);
		}

		[Subroutine]
		public override void Load() {
			NES.PPU.ClearNametable0(0);
			
			VRamQueue.Reset();

			NES.PPU.SetAddress(0x232C);
			NES.PPU.Data.Write(0xF8, 0xF0, 0xEE, 0xF5, 0xCB, 0x00, 0xDC, 0xDD); //Draws "SKIP:"

			_charX.Set(0x80);
			_charY.Set(0x80);
			
			NES.PPU.OAM.Object[0].Tile.Set(_PlayerTile0);
			NES.PPU.OAM.Object[0].Attr.Set(0);
			NES.PPU.OAM.Object[1].Tile.Set(_PlayerTile1);
			NES.PPU.OAM.Object[1].Attr.Set(0);
			NES.PPU.OAM.Object[2].Tile.Set(_PlayerTile2);
			NES.PPU.OAM.Object[2].Attr.Set(0);
			NES.PPU.OAM.Object[3].Tile.Set(_PlayerTile3);
			NES.PPU.OAM.Object[3].Attr.Set(0);
			
			Loop.ForEach(X, _scatterSprites, (face, _) => face.Init());

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
		public override void Step() {
			If.Block(c => c
				.True(_gamepad.IsHeld(NES.Button.Right),	() => _charX.Set(z => z.Add(_MoveDist)))
				.True(_gamepad.IsHeld(NES.Button.Left),		() => _charX.Set(z => z.Subtract(_MoveDist)))
			);
			If.Block(c => c
				.True(_gamepad.IsHeld(NES.Button.Down),		() => _charY.Set(z => z.Add(_MoveDist)))
				.True(_gamepad.IsHeld(NES.Button.Up),		() => _charY.Set(z => z.Subtract(_MoveDist)))
			);
			If.Block(c => c
				.True(_gamepad.WasPressed(NES.Button.B), () => {
					_skipOptionIndex.Dec();
					If.True(() => _skipOptionIndex.Equals(255), () => _skipOptionIndex.Set(0));	//low limit
					GoSub(DrawSkipNum);
				})
				.True(_gamepad.WasPressed(NES.Button.A), () => {
					_skipOptionIndex.Inc();
					If.True(() => _skipOptionIndex.Equals(_skipOptions.Length), () => _skipOptionIndex.Set(_skipOptions.Length - 1));	//high limit
					GoSub(DrawSkipNum);
				})
			);
			Y.Set(0);
			If.True(_gamepad.WasPressed(NES.Button.Select), () => {
				_pattern.Inc();
				If.True(() => _pattern.Equals(3), () => _pattern.Set(0));
			});
			If.True(() => _gamepad.Pressed(NES.Button.Start), () => Module<SceneManager>().Queue(Module<Title>()));

			var jx = Temp[1];
			var jy = Temp[2];

			jx.Set(_charX.Subtract(8));
			jy.Set(_charY.Subtract(8));

			NES.PPU.OAM.Object[0].SetPosition(jx,		jy);
			NES.PPU.OAM.Object[1].SetPosition(_charX,	jy);
			NES.PPU.OAM.Object[2].SetPosition(jx,		_charY);
			NES.PPU.OAM.Object[3].SetPosition(_charX,	_charY);

			U8 startOffset = 4;

			If.Block(c => c
				 .True(() => _pattern.Equals(0), () => {
					Comment("Single row pattern");
					jx.Set(0x04);
					jy.Set(0x40);
			
					_processed.Set(0);
					Y.Set(startOffset);

					Loop.Do_old(_ => {
						var spriteObj = NES.PPU.OAM.Object[Y];
						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_JunkTile);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(jx, jy);
						Skipterate_Next();

						jx.Set(z => z.Add(4));
						_processed.Inc();
					}).While(() => A.Set(_processed).NotEquals(60));
				})
				.True(() => _pattern.Equals(1), () => {
					Comment("Four rows pattern");
					U8 xStart = 0x30;
					jx.Set(xStart);
					jy.Set(0x30);

					_processed.Set(0);
					Y.Set(startOffset);

					Loop.Do_old(_ => {
						var spriteObj = NES.PPU.OAM.Object[Y];
						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_JunkTile);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(jx, jy);
						Skipterate_Next();

						jx.Set(z => z.Add(8));
						
						If.Any(() => _processed.Equals(19), () => _processed.Equals(39)).Then(() => {
							jy.Set(jy.Add(8));
							jx.Set(xStart);
						});
						_processed.Inc();
					}).While(() => A.Set(_processed).NotEquals(60));
				})
				.Else(() => {
					Comment("Moving scattered objects pattern");
					Y.Set(startOffset);

					Loop.ForEach(X, _scatterSprites, (face, _) => {
						var spriteObj = NES.PPU.OAM.Object[Y];
						jx.Set(face.PosX.Add(8));
						jy.Set(face.PosY.Add(8));

						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_NPCTile0);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(face.PosX, face.PosY);
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_NPCTile1);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(jx, face.PosY);
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_NPCTile2);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(face.PosX, jy);
						Skipterate_Next();
						
						Skipterate_PrepareY(startOffset);
						spriteObj.Tile.Set(_NPCTile3);
						spriteObj.Attr.Set(1);
						spriteObj.SetPosition(jx, jy);
						Skipterate_Next();

						face.Move();
					});
				})
			);
			Skipterate_Next();
		}

		private void Skipterate_PrepareY(U8 startOffset) => Y.Set(_index.Add(startOffset).Multiply(4));

		private void Skipterate_Next() {
			_index.Set(z => A.Set(LabelFor(SkipOption)[Y.Set(_skipOptionIndex)]).Add(z));
			If.True(() => A.Set(_index).GreaterThan(59),
				() => _index.Set(z => z.Subtract(60)));
		}

		[Subroutine]
		public void DrawSkipNum() {
			VRamQueue.Address.SetU16(0x2332);
			VRamQueue.TileArray.Draw_Manual(2);
			VRamQueue.Push(() => A.Set(LabelFor(SkipOptionTile10s)[X.Set(_skipOptionIndex)]));
			VRamQueue.Push(() => A.Set(LabelFor(SkipOptionTile1s)[X]));
			VRamQueue.DonePushing();
		}

		private static readonly byte[] _skipOptions = { 1, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 49, 53, 59 }; //skipterate options for range 60 //fav: 23 (index 6)
		private static readonly Dictionary<int, byte> _digitTiles = new() {
			{0,	0xDC},
			{1,	0xDD},
			{2,	0xDE},
			{3,	0xDF},
			{4,	0xE0},
			{5,	0xE1},
			{6,	0xE2},
			{7,	0xE3},
			{8,	0xE4},
			{9,	0xE5}
		};

		[DataSection]
		public static void SkipOption() => Raw(_skipOptions);
		[DataSection]
		public static void SkipOptionTile10s() => Raw(_skipOptions.Select(x => _digitTiles[x / 10]).ToArray());
		[DataSection]
		public static void SkipOptionTile1s() => Raw(_skipOptions.Select(x => _digitTiles[x % 10]).ToArray());
		
		public class ScatterSprite : Struct {
			public VByte PosX { get; set; }
			public VByte PosY { get; set; }
		
			public void Init() {
				PosX.Set(LabelFor(StartX)[Index]);
				PosY.Set(LabelFor(StartY)[Index]);
			}

			public void Move() {
				PosX.Set(z => A.Set(z).Add(LabelFor(MoveX)[Index]));
				PosY.Set(z => A.Set(z).Add(LabelFor(MoveY)[Index]));

				//60 to 180
				If.Block(c => c
					.True(() => A.Set(PosY).GreaterThan(160),
						() => PosY.Set(z => z.Subtract(100)))
					.True(() => A.Set(PosY).LessThan(60),
						() => PosY.Set(z => z.Add(100)))
				);
			}

			[DataSection]
			public static void StartX() =>	Raw(133, 92, 50, 105, 161, 32, 9, 118, 246, 176, 143, 42, 19, 3, 240, 24);
			[DataSection]
			public static void StartY() =>	Raw(146, 128, 95, 71, 140, 145, 119, 124, 65, 127, 108, 99, 70, 104, 118, 85);
			[DataSection]
			public static void MoveX() =>	Raw(0xFD, 0xFE, 0xFE, 0x01, 0x02, 0x01, 0xFE, 0xFE, 0x01, 0xFE, 0xFD, 0xFD, 0x02, 0x01, 0xFE, 0xFD);
			[DataSection]
			public static void MoveY() =>	Raw(0xFD, 0x02, 0x01, 0xFE, 0x02, 0xFE, 0x02, 0x01, 0xFD, 0xFD, 0x01, 0x01, 0x02, 0xFE, 0x01, 0xFD);
		}
	}
}
