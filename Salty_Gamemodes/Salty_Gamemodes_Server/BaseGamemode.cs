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

        public MapManager MapManager;

        public Dictionary<Player, Dictionary<string, int>> PlayerDetails = new Dictionary<Player, Dictionary<string, int>>();

        public Map GameMap;
        public List<Player> InGamePlayers;
        public int ID;

        public double GameLength = 0;
        public double GameTime = 0;
        public bool isTimed = false;

        public string MapTag = "";

        public BaseGamemode( MapManager manager, int ID, string MapTag ) {
            MapManager = manager;
            this.ID = ID;
            this.MapTag = MapTag;

            InGamePlayers = new PlayerList().ToList();

            Random rand = new Random();
            List<Map> maps = MapManager.MapList(MapTag);
            Map map = maps[rand.Next(0, maps.Count)];

            GameMap = map;
        }

        public virtual void Start() {
            if ( isTimed )
                GameTime = GetGameTimer() + GameLength;
        }

        public virtual void End() {
            TriggerClientEvent( "salty::EndGame" );
            Init.ActiveGame = new BaseGamemode( MapManager, 0, "*" );
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

        public void SpawnClient( Player ply, int team ) {
            ply.TriggerEvent( "salty::StartGame", ID, team, GameLength, GameMap.Position, GameMap.Size, GameMap.GetNextSpawn(team), GameMap.GunSpawns );
        }

        public void CreateGameTimer( double length ) {
            GameLength = length * 1000;
            isTimed = true;
        }

        public virtual void PlayerKilled( Player player, int killerID, ExpandoObject deathData ) {
  
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
            PlayerDetails[ply]["Team"] = team;
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

        public void AddScore( Player ply, int amount ) {
            if (!PlayerDetails.ContainsKey(ply)) {
                PlayerDetails.Add(ply, new Dictionary<string, int>() { { "Team", 0 }, { "Score", 0 } });
            }
            else {
                PlayerDetails[ply]["Score"] += amount;
            }
            Debug.WriteLine(ply.Name + " has score: " + PlayerDetails[ply]["Score"]);
        }

        public void WriteChat( string str ) {
            TriggerClientEvent( "chat:addMessage", new {
                color = new[] { 255, 0, 0 },
                args = new[] { GetType().ToString(), str }
            } );
        }

    }
}
