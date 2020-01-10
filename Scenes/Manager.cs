using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace FlickerTest.Scenes {
	public static class Manager {
		public static Var8 Current;
		public static Var8 Attributes;

		[Flags]
		public enum SceneAttributes : byte {
			Ended	= 0b00000001,
			Paused	= 0b00000010,
			Load	= 0b00000100,	//New scene is set, Manager should call Load before draws/steps
		};

		static Manager() {
			Current = Var8.New(ram, "SceneMgr_Current");
			Attributes = Var8.New(ram, "SceneMgr_Attributes");
		}

		[Subroutine]
		public static void _load() {
			Sprites.HideAll();
			NES.PPU.OAM.Write(Addr(0x0200));

			Attributes.Set(0);
			
			Engine.bankCallTable.Call(LabelFor(LoadBankCallIndex).Offset(X.Set(Current)));
		}

		public static void QueueScene(U8 sceneId) {
			Current.Set(sceneId);
			//this will wait for a manager attr flag and do it in the main loop
			Attributes.Set(z => z.Or((byte)SceneAttributes.Load)); //Set Load flag
			NES.PPU.LazyMask.Set(0);
		}

		public static void LoadScene(U8 sceneId) {
			Current.Set(sceneId); //now A = sceneId
			GoSub(_load); 
		}

		public static void Step() {
			Engine.bankCallTable.Call(LabelFor(StepBankCallIndex).Offset(X.Set(Current)));
		}

		public static void CheckLoadNeeded() {
			If(() => Attributes.And((byte)SceneAttributes.Load).NotEquals(0), () => {
				A.Set(Current);
				GoSub(_load); //Attributes are all set to 0 in _load
			});
		}
		
		[DataSection]
		public static void LoadBankCallIndex() {
			Raw(Engine.bankCallTable.IndexOf(BankedFuncs.Title_Load));
			Raw(Engine.bankCallTable.IndexOf(BankedFuncs.Flicker_Load));
		}

		[DataSection]
		public static void StepBankCallIndex() {
			Raw(Engine.bankCallTable.IndexOf(BankedFuncs.Title_Step));
			Raw(Engine.bankCallTable.IndexOf(BankedFuncs.Flicker_Step));
		}

		public static class IDs {
			public static U8 Title = 0;
			public static U8 Flicker = 1;
		}
	}
}
