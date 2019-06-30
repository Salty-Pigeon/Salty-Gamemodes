using CitizenFX.Core;
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

        public Dictionary<Player, int> PlayerScores = new Dictionary<Player, int>();


        public Map GameMap;
        public PlayerList players;
        public int ID;

        public double GameLength = 0;
        public double GameTime = 0;
        public bool isTimed = false;

        public string MapTag = "";

        public BaseGamemode( MapManager manager, int ID, string MapTag ) {
            MapManager = manager;
            this.ID = ID;
            this.MapTag = MapTag;
        }

        public virtual void Start() {

            Random rand = new Random();
            List<Map> maps = MapManager.MapList( MapTag );
            Map map = maps[rand.Next( 0, maps.Count )];

            GameMap = map;

            if( isTimed )
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
                    End();
                }
            }
        }

        public virtual void PlayerJoined( Player ply ) {

        }

        public void SpawnClient( Player ply, int team, Vector3 spawn ) {
            ply.TriggerEvent( "salty::StartGame", ID, team, GameTime - GetGameTimer(), GameMap.Position, GameMap.Size, spawn, GameMap.GunSpawns );

        }

        public void CreateGameTimer( double length ) {
            GameLength = length * 1000;
            isTimed = true;
        }

        public virtual void PlayerKilled( Player player, int killerID, ExpandoObject deathData ) {
  
        }

        public virtual void PlayerDied( Player player, int killerType, Vector3 deathcords ) {

        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

        public void AddScore( Player ply, int amount ) {
            if( PlayerScores.ContainsKey(ply) ) {
                PlayerScores[ply] += amount;
            }
            else {
                PlayerScores.Add( ply, amount );
            }
            Debug.WriteLine( ply.Name + " has score: " + PlayerScores[ply] );
        }

        public void WriteChat( string str ) {
            TriggerClientEvent( "chat:addMessage", new {
                color = new[] { 255, 0, 0 },
                args = new[] { GetType().ToString(), str }
            } );
        }

    }
}
