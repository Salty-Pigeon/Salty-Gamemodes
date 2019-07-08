using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class TTT_HUD : HUD {


        TTT ActiveTTT;

        public SaltyText TeamText;
        public SaltyText DisguisedText;

        public int DetectiveTracing = -1;

        public float DNATime = 0f;
        public float DNAScanTime = 10 * 1000;
        public Vector3 DNALastPos;

        public bool isRadarActive = false;
        public List<Vector3> RadarPositions = new List<Vector3>();

        public float RadarTime = 0f;
        public float RadarScanTime = 30 * 1000;

        public TTT_HUD( BaseGamemode gamemode ) : base(gamemode) {
            ActiveTTT = gamemode as TTT;
            TeamText = new SaltyText(0.033f, 0.855f, 0, 0, 0.5f, "Spectator", 255, 255, 255, 255, false, false, 0, true);
            DisguisedText = new SaltyText(0f, 0f, 0, 0, 0.5f, "Disguise enabled", 200, 0, 0, 255, false, false, 0, true);
        }

        public override void Draw() {

            if (DetectiveTracing > -1) {
                ShowDNA();
            }

            if (isRadarActive)
                ShowRadar();

            if (ActiveTTT.Team == (int)TTT.Teams.Traitors) {
                DrawRectangle(0.025f, 0.86f, 0.07f, 0.03f, 200, 0, 0, 200);
            }
            if (ActiveTTT.Team == (int)TTT.Teams.Detectives) {
                DrawRectangle(0.025f, 0.86f, 0.07f, 0.03f, 0, 0, 200, 200);
            }
            if (ActiveTTT.Team == (int)TTT.Teams.Innocents) {
                DrawRectangle(0.025f, 0.86f, 0.07f, 0.03f, 0, 200, 0, 200);
            }

            TeamText.Draw();

            DrawHealth();
            DrawWeaponSwitch();
            ShowNames();

            HideReticle();

            if (lastLooked + 300 > GetGameTimer()) {
                HUDText.Draw();
            }

            if (ActiveTTT.Team == (int)TTT.Teams.Traitors)
                ShowTraitors();

            if (ActiveTTT.isDisguised) {
                DisguisedText.Draw();
            }

            base.Draw();
        }

        public override void DrawWeaponSwitch() {
            if (ActiveTTT.lastScroll + (2 * 1000) > GetGameTimer()) {
                int index = 0;
                foreach (var weapon in ActiveTTT.WeaponSlots.OrderBy(x => x.Value)) {
                    if (!ActiveTTT.PlayerWeapons.ContainsValue(weapon.Key))
                        continue;
                    if (WeaponTexts.Count <= weapon.Value) {
                        WeaponTexts.Add(new SaltyText(0.85f, 0.85f + (index * 0.4f), 0, 0, 0.3f, weapon.Key, 255, 255, 255, 255, false, false, 0, true));
                    }

                    if ((uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == (uint)GetHashKey(weapon.Key)) {
                        DrawRectangle(0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 230, 230, 0, 200);
                    }
                    else {
                        DrawRectangle(0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200);
                    }

                    WeaponTexts[index].Caption = ActiveTTT.GameWeapons[weapon.Key];
                    WeaponTexts[index].Position = new Vector2(0.85f, 0.85f + (index * 0.04f));
                    WeaponTexts[index].Draw();
                    index++;
                }
            }
        }

        public override void UpdateTeam( int team ) {
            switch (team) {
                case ((int)TTT.Teams.Spectators):
                    TeamText.Caption = "Spectate";
                    TeamText.Colour = System.Drawing.Color.FromArgb(150, 150, 0);
                    break;
                case ((int)TTT.Teams.Traitors):
                    TeamText.Caption = "Traitor";
                    TeamText.Colour = System.Drawing.Color.FromArgb(255, 255, 255);
                    NetworkSetVoiceChannel(1);
                    break;
                case ((int)TTT.Teams.Innocents):
                    TeamText.Caption = "Innocent";
                    TeamText.Colour = System.Drawing.Color.FromArgb(255, 255, 255);
                    TeamText.Position.X -= 0.006f;
                    NetworkSetVoiceChannel(1);
                    break;
                case ((int)TTT.Teams.Detectives):
                    TeamText.Caption = "Detective";
                    TeamText.Colour = System.Drawing.Color.FromArgb(255, 255, 255);
                    TeamText.Position.X -= 0.008f;
                    NetworkSetVoiceChannel(1);
                    break;
            }
            base.UpdateTeam( team );
        }

        public void ShowRadar() {

            if (RadarTime < GetGameTimer()) {
                RadarTime += RadarScanTime;
                UpdateRadar();
            }

            foreach (var pos in RadarPositions) {

                Vector3 camPos = GetGameplayCamCoords();
                float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, camPos.X, camPos.Y, camPos.Z, true);

                DrawText3D(pos, Math.Round(dist, 1) + "m", 0.3f, 255, 255, 255, 255, 999999);

                float x = 0, y = 0;
                var offscreen = Get_2dCoordFrom_3dCoord(pos.X, pos.Y, pos.Z - 0.08f, ref x, ref y);
                if (!offscreen)
                    DrawRect(x, y, 0.02f, 0.02f, 0, 200, 0, 255);

            }

            DrawText2D(0.025f, 0.97f, "Radar update in " + (Math.Round((RadarTime - GetGameTimer()) / 1000)).ToString(), 0.3f, 255, 255, 255, 255, false);

        }

        public void UpdateRadar() {
            RadarPositions = new List<Vector3>();
            RadarTime = GetGameTimer() + RadarScanTime;
            foreach (var ply in ActiveTTT.GetInGamePlayers()) {
                RadarPositions.Add(GetEntityCoords(GetPlayerPed(ply), true));
            }
        }

        public void SetRadarActive( bool active ) {
            if (isRadarActive)
                return;
            UpdateRadar();
            isRadarActive = active;
        }

        public void ShowTraitors() {
            foreach (var ply in ActiveTTT.OtherPlayerInfo) {
                if (ActiveTTT.GetTeam(ply.Key) == (int)TTT.Teams.Traitors) {
                    Vector3 pos = GetEntityCoords(GetPlayerPed(ply.Key), true) + new Vector3(0, 0, Game.PlayerPed.HeightAboveGround);
                    DrawSpriteOrigin(pos, "traitor", 0.02f, 0.03f, 0);
                }
            }
        }

        public override void ShowNames() {
            ShowDeadName();
            Vector3 position = Game.PlayerPed.ForwardVector;
            RaycastResult result = Raycast(Game.PlayerPed.Position, position, 75, IntersectOptions.Peds1, null);
            if (result.DitHitEntity) {
                if (result.HitEntity != Game.PlayerPed) {
                    int ent = result.HitEntity.Handle;
                    if (ActiveTTT.GetPlayerBool(NetworkGetPlayerIndexFromPed(ent), "disguised")) {
                        return;
                    }
                    if (IsPedAPlayer(ent)) {
                        HUDText.Caption = GetPlayerName(GetPlayerPed(ent)).ToString();
                        lastLooked = GetGameTimer();
                    }
                }
            }

        }

        public void ShowDNA() {

            if (DNATime < GetGameTimer()) {
                DNATime += DNAScanTime;
                UpdateDNA();
            }

            if (DNALastPos != Vector3.Zero) {
                Vector3 camPos = GetGameplayCamCoords();
                float dist = GetDistanceBetweenCoords(DNALastPos.X, DNALastPos.Y, DNALastPos.Z, camPos.X, camPos.Y, camPos.Z, true);

                DrawText3D(DNALastPos, Math.Round(dist, 1) + "m", 0.3f, 255, 255, 255, 255, 999999);

                float x = 0, y = 0;
                var offscreen = Get_2dCoordFrom_3dCoord(DNALastPos.X, DNALastPos.Y, DNALastPos.Z - 0.08f, ref x, ref y);
                if (!offscreen)
                    DrawRect(x, y, 0.02f, 0.02f, 0, 0, 230, 255);
            }

            DrawText2D(0.025f, 0.835f, "DNA update in " + (Math.Round((RadarTime - GetGameTimer()) / 1000)).ToString(), 0.3f, 255, 255, 255, 255, false);

        }

        public void UpdateDNA() {
            DNATime = GetGameTimer() + DNAScanTime;
            Vector3 newCoord = GetEntityCoords(DetectiveTracing, true);
            if (newCoord != Vector3.Zero) {
                DNALastPos = newCoord;
            }
        }

        public void ShowDeadName() {
            foreach (var body in ActiveTTT.DeadBodies) {
                Vector3 Position = GetPedBoneCoords(body.Value.ID, (int)Bone.SKEL_ROOT, 0, 0, 0);

                if (!body.Value.isDiscovered)
                    DrawText3D(Position, Game.PlayerPed.Position, body.Value.Caption, 0.3f, 255, 255, 0, 255, 2);
                else
                    DrawText3D(Position, Game.PlayerPed.Position, body.Value.Caption, 0.3f, 255, 255, 255, 255, 2);
                if (ActiveTTT.Team == (int)TTT.Teams.Spectators) {
                    DrawText3D(Position - new Vector3(0, 0, 0.2f), Game.PlayerPed.Position, "Press E to scan for DNA", 0.3f, 230, 230, 0, 255, 2);
                }
            }
        }

    }
}
