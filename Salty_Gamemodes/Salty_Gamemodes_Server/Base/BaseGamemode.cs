﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;

namespace Salty_Gamemodes_Server {
    public class BaseGamemode : BaseScript {

        public Dictionary<Player, Dictionary<string, int>> PlayerDetails = new Dictionary<Player, Dictionary<string, int>>();
        public Dictionary<int, List<Player>> PlayerTeams = new Dictionary<int, List<Player>>();

        public Map GameMap;
        public List<Player> InGamePlayers;
        public int ID;

        public double GameLength = 0;
        public double GameTime = 0;
        public bool isTimed = false;

        public string MapTag = "";

        public Dictionary<uint, string> HashToModel = new Dictionary<uint, string>();

        public Dictionary<string, int> WeaponSlots = new Dictionary<string, int>() {
            { "WEAPON_UNARMED", 0 },
        };


        public BaseGamemode( int ID, Map map, List<Player> players ) {

            this.ID = ID;
            this.MapTag = map.Name.Split('_')[0];

            InGamePlayers = players;

            GameMap = map;
        }

        public virtual void Start() {
            if ( isTimed )
                GameTime = GetGameTimer() + GameLength;
        }

        public virtual void End() {
            GameMap.ResetSpawns();
            Init.Salty.NextGame(ID);
        }

        public virtual void Update() {
            if( isTimed && GameTime != 0 ) {
                if( GameTime - GetGameTimer() < 0 ) {
                    isTimed = false;
                    OnTimerEnd();
                    End();
                }
            }
        }

        public virtual void PlayerJoined( Player ply ) {

        }

        public bool GetPlayerBoolean( Player ply, string key ) {
            if(PlayerDetails.ContainsKey(ply)) {
                if (PlayerDetails[ply].ContainsKey(key))
                    return PlayerDetails[ply][key] != 0;
            }
            return false;
        }


        public void UpdatePlayerBoolean( Player ply, string key ) {
            if (PlayerDetails.ContainsKey(ply)) {
                if(PlayerDetails[ply].ContainsKey(key)) {
                    if( PlayerDetails[ply][key] == 1)
                        PlayerDetails[ply][key] = 0;
                    else
                        PlayerDetails[ply][key] = 1;
                } else {
                    PlayerDetails[ply].Add(key, 1);
                }
            } else {
                PlayerDetails.Add(ply, new Dictionary<string, int> { { key, 1 } });
            }
            TriggerClientEvent("salty::GMPlayerUpdate", Convert.ToInt32(ply.Handle), key, PlayerDetails[ply][key]);
            
        }

        public void SpawnClient( Player ply, int teamSpawn, int team ) {
            ply.TriggerEvent( "salty::StartGame", ID, team, GameLength, GameMap.Position, GameMap.Size, GameMap.Name, GameMap.GetNextSpawn( teamSpawn ), GameMap.GunSpawns );
            SetTeam( ply, team );
        }

        public virtual bool OnChatMessage( Player ply, string message ) {
            if( GetTeam( ply ) == 0 ) {
                WriteChat( "[DEAD] " + ply.Name, message, 230, 0, 0 );
            }
            else {
                WriteChat( ply.Name, message, 0, 230, 0 );
            }
            return true;
        }

        public void CreateGameTimer( double length ) {
            GameLength = length * 1000;
            isTimed = true;
        }

        public virtual void PlayerKilled( Player player, int killerID, Vector3 deathcords ) {
  
        }

        public virtual void PlayerDied( Player player, int killerType, Vector3 deathcords ) {

        }

        public virtual void OnTimerEnd() {

        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

        public void SetTeam( Player ply, int team ) {
            if (!PlayerDetails.ContainsKey(ply))
                PlayerDetails.Add(ply, new Dictionary<string, int>() { { "Team", 0 }, { "Score", 0 } });
            int plyOldTeam = PlayerDetails[ply]["Team"];
            if( PlayerTeams.ContainsKey(plyOldTeam)) {
                if (PlayerTeams[plyOldTeam].Contains(ply))
                    PlayerTeams[plyOldTeam].Remove(ply);
            }
            PlayerDetails[ply]["Team"] = team;
            if( !PlayerTeams.ContainsKey(team)) {
                PlayerTeams.Add(team, new List<Player>() { ply });
            } else {
                PlayerTeams[team].Add(ply);
            }
            TriggerClientEvent( "salty::GMPlayerUpdate", Convert.ToInt32( ply.Handle ), "Team", team );
        }

        public int TeamCount( int team ) {
            return PlayerTeams.ContainsKey( team ) ? PlayerTeams[team].Count : 0;
        }

        public int GetTeam( Player ply ) {
            if (PlayerDetails.ContainsKey(ply)) {
                return PlayerDetails[ply]["Team"];
            }
            return 0;
        }

        public Dictionary<Player, int> GetScores() {
            Dictionary<Player, int> PlayerScores = new Dictionary<Player, int>();
            foreach( var ply in PlayerDetails) {
                PlayerScores.Add(ply.Key, ply.Value["Score"]);
            }
            return PlayerScores;
        } 

        public int GetScore( Player ply ) {
            if( PlayerDetails.ContainsKey(ply)) {
                return PlayerDetails[ply]["Score"];
            }
            return 0;
        }

        public void PlayerDropped( [FromSource] Player ply, string reason ) {
            int team = GetTeam( ply );
            if( PlayerTeams.ContainsKey( team ) ) {
                PlayerTeams[GetTeam( ply )].Remove( ply );
                PlayerDetails.Remove( ply );
            }
        }

        public void AddScore( Player ply, int amount ) {
            if (!PlayerDetails.ContainsKey(ply)) {
                PlayerDetails.Add(ply, new Dictionary<string, int>() { { "Team", 0 }, { "Score", 0 } });
            }
            else {
                PlayerDetails[ply]["Score"] += amount;
            }
            ply.TriggerEvent( "salty::UpdateScore", GetScore( ply ) );
        }

        public void WriteChat( string prefix, string str, int r, int g, int b ) {
            TriggerClientEvent( "chat:addMessage", new {
                color = new[] { r, g, b },
                args = new[] { prefix, str }
            } );
        }

        public void WriteChat( Player ply, string prefix, string str, int r, int g, int b ) {
            ply.TriggerEvent( "chat:addMessage", new {
                color = new[] { r, g, b },
                args = new[] { prefix, str }
            } );
        }

    }
}
