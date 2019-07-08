﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;

namespace Salty_Gamemodes_Client {
    public class HUD : BaseScript {

        public BaseGamemode ActiveGame;

        public List<SaltyText> WeaponTexts = new List<SaltyText>();

        public SaltyText HUDText;
        public SaltyText AddScoreText;
        public SaltyText ScoreText;
        public SaltyText HealthText;
        public SaltyText AmmoText;
        public SaltyText GameTimeText;
        public SaltyText BoundText;
        public SaltyText GoalText;

        public float lastLooked = 0;
        private float showScoreTimer = 0;
        private float showScoreLength = 500;
        public float GoalTextTime = 0;



        public HUD( BaseGamemode gamemode ) {
            ActiveGame = gamemode;
            HUDText = new SaltyText(0.5f, 0.5f, 0, 0, 0.5f, "", 255, 255, 255, 255, false, true, 0, true);
            HealthText = new SaltyText(0.085f, 0.895f, 0, 0, 0.5f, "Health: ", 255, 255, 255, 255, false, true, 0, true);
            AmmoText = new SaltyText(0.085f, 0.935f, 0, 0, 0.5f, "Ammo: ", 255, 255, 255, 255, false, true, 0, true);

            GameTimeText = new SaltyText(0.121f, 0.855f, 0, 0, 0.5f, "", 255, 255, 255, 255, false, true, 0, true);
            ScoreText = new SaltyText(0.5f, 0.01f, 0, 0, 0.7f, "Score: 0", 255, 255, 255, 255, false, true, 0, true);
            AddScoreText = new SaltyText(0.5f + (ScoreText.Caption.Length), 0.025f, 0, 0, 0.3f, "", 255, 255, 255, 255, false, true, 0, true);
            BoundText = new SaltyText(0.5f, 0.1f, 0, 0, 1, "", 255, 255, 255, 255, true, true, 0, true);

            GoalText = new SaltyText(0.5f, 0.1f, 0, 0, 1f, "", 255, 255, 255, 255, false, true, 0, true);
        }

        public virtual void Draw() {

            ShowTalking();

            if (ActiveGame.isTimed)
                DrawGameTimer();

        }

        public void SetGoal( string caption, int r, int g, int b, int a, int duration ) {
            GoalText.Caption = caption;
            GoalText.Colour = System.Drawing.Color.FromArgb(a, r, g, b);
            SetGoalTimer(duration);
        }

        public void SetGameTimePosition( int x, int y, bool centre ) {
            GameTimeText.Position = new Vector2(x, y);
            GameTimeText.Centre = centre;
        }

        public void DrawText2D( float x, float y, string text, float scale, int r, int g, int b, int a, bool centre ) {
            SetTextScale(scale, scale);
            SetTextFont(0);
            SetTextProportional(true);
            SetTextColour(r, g, b, a);
            SetTextDropshadow(0, 0, 0, 0, 55);
            SetTextEdge(2, 0, 0, 0, 150);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            SetTextCentre(centre);
            AddTextComponentString(text);
            DrawText(x, y);
        }
        public void DrawScore() {
            ScoreText.Draw();
            float time = showScoreTimer - GetGameTimer();
            if (time >= 0) {
                if (time <= (showScoreLength / 2)) {
                    float percent = time / (showScoreLength / 2);
                    AddScoreText.Scale = percent * 0.3f;
                }
                else {
                    float percent = (time - (showScoreLength / 2)) / (showScoreLength / 2);
                    AddScoreText.Scale = 0.3f - (percent * 0.3f);
                }
                AddScoreText.Draw();
            }
        }

        public virtual void UpdateTeam( int team ) {

        }

        public virtual void DrawHealth() {

            HideHudAndRadarThisFrame();

            if (ActiveGame.Team == 0)
                return;

            HealthText.Caption = Game.Player.Character.Health.ToString();
            AmmoText.Caption = Game.PlayerPed.Weapons.Current.AmmoInClip + " / " + Game.PlayerPed.Weapons.Current.Ammo;

            DrawRectangle(0.025f, 0.9f, 0.12f, 0.03f, 0, 0, 0, 200);
            float healthPercent = (float)Game.Player.Character.Health / Game.Player.Character.MaxHealth;
            if (healthPercent < 0)
                healthPercent = 0;
            if (healthPercent > 1)
                healthPercent = 1;
            DrawRectangle(0.025f, 0.9f, healthPercent * 0.12f, 0.03f, 200, 0, 0, 200);

            float ammoPercent = (float)Game.PlayerPed.Weapons.Current.AmmoInClip / Game.PlayerPed.Weapons.Current.MaxAmmoInClip;

            DrawRectangle(0.025f, 0.94f, 0.12f, 0.03f, 0, 0, 0, 200);

            DrawRectangle(0.025f, 0.94f, ammoPercent * 0.12f, 0.03f, 230, 230, 0, 200);

            HealthText.Draw();
            AmmoText.Draw();

        }


        public void DrawText3D( Vector3 pos, string text, float scale, int r, int g, int b, int a, float minDistance ) {
            float x = 0, y = 0;
            bool offScreen = Get_2dCoordFrom_3dCoord(pos.X, pos.Y, pos.Z, ref x, ref y);

            if (offScreen)
                return;

            Vector3 camPos = GetGameplayCamCoords();
            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, camPos.X, camPos.Y, camPos.Z, true);
            if (dist > minDistance)
                return;

            SetTextScale(scale, scale);
            SetTextFont(0);
            SetTextProportional(true);
            SetTextColour(r, g, b, a);
            SetTextDropshadow(0, 0, 0, 0, 55);
            SetTextEdge(2, 0, 0, 0, 150);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            SetTextCentre(true);
            AddTextComponentString(text);
            DrawText(x, y);
        }

        public void DrawText3D( Vector3 pos, Vector3 camPos, string text, float scale, int r, int g, int b, int a, float minDistance ) {
            float x = 0, y = 0;
            bool offScreen = Get_2dCoordFrom_3dCoord(pos.X, pos.Y, pos.Z, ref x, ref y);

            if (offScreen)
                return;

            float dist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, camPos.X, camPos.Y, camPos.Z, true);
            if (dist > minDistance)
                return;

            SetTextScale(scale, scale);
            SetTextFont(0);
            SetTextProportional(true);
            SetTextColour(r, g, b, a);
            SetTextDropshadow(0, 0, 0, 0, 55);
            SetTextEdge(2, 0, 0, 0, 150);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            SetTextCentre(true);
            AddTextComponentString(text);
            DrawText(x, y);
        }

        public virtual void ShowNames() {
            Vector3 position = Game.PlayerPed.ForwardVector;

            RaycastResult result = Raycast(Game.PlayerPed.Position, position, 75, IntersectOptions.Peds1, null);
            if (result.DitHitEntity) {
                if (result.HitEntity != Game.PlayerPed) {
                    int ent = NetworkGetEntityFromNetworkId(result.HitEntity.NetworkId);
                    if (IsPedAPlayer(ent)) {
                        HUDText.Caption = GetPlayerName(GetPlayerPed(ent)).ToString();
                        lastLooked = GetGameTimer();
                    }

                }

            }
        }

        public static RaycastResult Raycast( Vector3 source, Vector3 direction, float maxDistance, IntersectOptions options, Entity ignoreEntity = null ) {
            Vector3 target = source + direction * maxDistance;

            return new RaycastResult(Function.Call<int>(Hash._CAST_RAY_POINT_TO_POINT, source.X, source.Y, source.Z, target.X, target.Y, target.Z, options, ignoreEntity == null ? 0 : ignoreEntity.Handle, 7));
        }

        public virtual void ShowTalking() {
            if (IsControlPressed(0, 249)) {
                int offset = 0;
                if (ActiveGame.Team == 0) {
                    foreach (var ply in new PlayerList()) {
                        if (!NetworkIsPlayerTalking(ply.Handle)) { continue; }
                        if (ActiveGame.GetTeam(ply.Handle) == 0) {
                            DrawRectangle(0.85f, 0.05f + (offset * 0.052f), 0.13f, 0.05f, 230, 230, 0, 255);
                            DrawText2D(0.915f, 0.061f + (offset * 0.052f), ply.Name, 0.5f, 255, 255, 255, 255, true);
                            offset++;
                        }
                    }
                }
                else {
                    foreach (var ply in new PlayerList()) {
                        if (!NetworkIsPlayerTalking(ply.Handle)) { continue; }
                        if (ActiveGame.GetTeam(ply.Handle) != 0) {
                            DrawRectangle(0.85f, 0.05f + (offset * 0.052f), 0.13f, 0.05f, 0, 230, 0, 255);
                            DrawText2D(0.915f, 0.061f + (offset * 0.052f), ply.Name, 0.5f, 255, 255, 255, 255, true);
                            offset++;
                        }
                    }
                }

            }
        }

        public void DrawGameTimer() {
            TimeSpan time = TimeSpan.FromMilliseconds(ActiveGame.GameTime - GetGameTimer());

            GameTimeText.Caption = string.Format("{0:00}:{1:00}", Math.Ceiling(time.TotalMinutes - 1), time.Seconds);

            GameTimeText.Draw();
        }

        public virtual void DrawWeaponSwitch() {

            if (ActiveGame.Team == 0)
                return;

            if (ActiveGame.lastScroll + (2 * 1000) > GetGameTimer()) {
                int index = 0;
                foreach (var weapon in ActiveGame.PlayerWeapons) {
                    if (WeaponTexts.Count <= index) {
                        WeaponTexts.Add(new SaltyText(0.85f, 0.85f + (index * 0.4f), 0, 0, 0.3f, weapon.Value, 255, 255, 255, 255, false, false, 0, true));
                    }

                    if ((uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == (uint)GetHashKey(weapon.Value)) {
                        DrawRectangle(0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 230, 230, 0, 200);
                    }
                    else {
                        DrawRectangle(0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200);
                    }

                    WeaponTexts[index].Caption = ActiveGame.GameWeapons[weapon.Value];
                    WeaponTexts[index].Position = new Vector2(0.85f, 0.85f + (index * 0.04f));
                    WeaponTexts[index].Draw();

                    index++;
                }
            }

            if (IsControlJustPressed(2, 15)) {
                ActiveGame.ChangeSelectedWeapon(+1);
            }

            if (IsControlJustPressed(2, 14)) {
                ActiveGame.ChangeSelectedWeapon(-1);
            }

        }

        public void UpdateScore( int score, int difference ) {
            AddScoreText.Caption = "+" + difference;
            showScoreTimer = GetGameTimer() + showScoreLength;
            ScoreText.Caption = "Score: " + score;
            AddScoreText.Position = new Vector2(0.5f + (ScoreText.Size.X), 0.025f);
        }

        public void HideReticle() {
            Weapon w = Game.PlayerPed?.Weapons?.Current;

            if (w != null) {
                WeaponHash wHash = w.Hash;
                if (wHash.ToString() != "SniperRifle") {
                    HideHudComponentThisFrame(14);
                }
            }
        }

        public void SetGoalTimer( float time ) {
            GoalTextTime = GetGameTimer() + (time * 1000);
        }

        public void DrawGoal() {
            if (GoalTextTime > GetGameTimer()) {
                GoalText.Draw();
            }
        }

        public void DrawSpriteOrigin( Vector3 pos, string texture, float width, float height, float rotation ) {
            SetDrawOrigin(pos.X, pos.Y, pos.Z, 0);
            DrawSprite("saltyTextures", texture, 0, 0, width, height, rotation, 255, 255, 255, 255);
            ClearDrawOrigin();
        }

        public void DrawRectangle( float x, float y, float width, float height, int r, int g, int b, int alpha ) {
            DrawRect(x + (width / 2), y + (height / 2), width, height, r, g, b, alpha);
        }

    }
}
