using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Server
{
    public class Init : BaseScript {

        enum Gamemodes {
            None,
            TTT,
            DriveOrDie,
            Murder,
            IceCreamMan
        }


        public int GamemodeVoteCount = 0;

        public bool inGame = false;
        public static BaseGamemode ActiveGame;

        Database SQLConnection;
        MapManager MapManager;
        Vote ActiveVote;
        public static SaltyTown Salty;

        public Init() {

            SQLConnection = new Database();
            MapManager = new MapManager( SQLConnection.Load() );
            Salty = new SaltyTown( MapManager );

            ActiveGame = new BaseGamemode( (int)Gamemodes.None, Map.None(), new PlayerList().ToList() );

            EventHandlers["playerDropped"] += new Action<Player, string>(ActiveGame.PlayerDropped);

            EventHandlers["baseevents:onPlayerDied"] += new Action<Player, int, List<dynamic>>(PlayerDied);
            EventHandlers["baseevents:onPlayerKilled"] += new Action<Player, int, ExpandoObject>(PlayerKilled);

            EventHandlers[ "salty::netStartGame" ] += new Action( ActiveGame.Start );
            EventHandlers[ "salty::netEndGame" ] += new Action( EndGame );
            EventHandlers[ "salty::netSpawnPointGUI" ] += new Action<Player>( SpawnPointGUI );
            EventHandlers[ "salty::netModifyMapPos" ] += new Action<Player, string, string, int, Vector3>( ModifyMapPosition );
            EventHandlers[ "salty::netModifyWeaponPos" ] += new Action<Player, string, string, string, Vector3>( ModifyWeaponPosition );
            EventHandlers[ "salty::netModifyMap" ] += new Action<Player, string, string, int, Vector3, Vector3>( ModifyMap );
            EventHandlers[ "salty::netAddScore" ] += new Action<Player, int>( AddScoreToPlayer );
            EventHandlers[ "salty::netVoteMap" ] += new Action<Player, string>( PlayerVote );
            EventHandlers[ "salty::netJoined" ] += new Action<Player>( PlayerJoined );
            EventHandlers[ "salty::netUpdatePlayerBool" ] += new Action<Player, string>( UpdatePlayerBool );
            EventHandlers[ "salty::netBodyDiscovered" ] += new Action<Player, int>( BodyDiscovered );
            EventHandlers["salty::netJoinRoom"] += new Action<Player, string>( JoinRoom );

            EventHandlers["chatMessage"] += new Action<int, string, string>( ChatMessage );
            

            RegisterCommand( "endGame", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                EndGame();
            } ), false );

            RegisterCommand( "createroom", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                Salty.StartRoom( Convert.ToInt32( args[0] ) );
            } ), false );

            RegisterCommand( "endroom", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                Salty.EndRoom( Convert.ToInt32( args[0] ) );
            } ), false );

            RegisterCommand("rooms", new Action<int, List<object>, string>(( source, args, raw ) => {
                // Show rooms GUI to client
                Salty.SendRoomsToClient( SourceToPlayer( source ) );
            }), false);

            RegisterCommand( "joinroom", new Action<int, List<object>, string>( ( source, args, raw ) => {
                Salty.JoinRoom( SourceToPlayer(source), Convert.ToInt32( args[0] ) );
            } ), false );

            RegisterCommand( "startroom", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                Salty.StartGame( Convert.ToInt32( args[0] ) );
            } ), false );

            RegisterCommand( "vote", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source == 0 ) {
                    ActiveVote = new Vote( new Dictionary<string, string> { { "Trouble in Terrorist Town", "ttt" }, { "Murder", "mmm" }, { "Drive Or Die", "dod" }, { "Ice Cream Man", "icm" } }, GamemodeVoteOver );
                    return;
                }
                GamemodeVoteCount++;
                int needed = (int)Math.Round( new PlayerList().ToList().Count * 0.7 );
                if( GamemodeVoteCount >= needed) {
                    ActiveGame.WriteChat( "Salty Gamemodes", "Gamemode vote started", 230, 230, 0 );
                    ActiveVote = new Vote( new Dictionary<string, string> { { "Trouble in Terrorist Town", "ttt" }, { "Murder", "mmm" }, { "Drive Or Die", "dod" }, { "Ice Cream Man", "icm" } }, GamemodeVoteOver );
                    GamemodeVoteCount = 0;
                } else {
                    ActiveGame.WriteChat( "Salty Gamemodes", needed + " more vote(s) needed to vote new gamemode.", 230, 230, 0 );
                }

            } ), false );

            RegisterCommand( "loadSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                SQLConnection.Load();
            } ), false );

            RegisterCommand( "saveSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( source != 0 ) { return; }
                SQLConnection.SaveAll(MapManager.Maps);
            } ), false );

            Tick += Init_Tick;
        }


        public void JoinRoom([FromSource] Player ply, string gamemode ) {
            Debug.WriteLine( gamemode );
            Salty.JoinRoom( ply, gamemode );
        }


        private string MapTagWinner;

        public void MapVoteOver( string winner ) {
            switch( MapTagWinner ) {
                case ("ttt"):
                    //StartTTT( MapManager.Maps[winner] );
                    break;
                case ("mmm"):
                    //StartMurder( MapManager.Maps[winner] );
                    break;
                case ("icm"):
                    //StartIceCreamMan( MapManager.Maps[winner] );
                    break;
                case ("dod"):
                    //StartDriveOrDie( MapManager.Maps[winner] );
                    break;
            }
            ActiveVote = null;
        }
        public void GamemodeVoteOver( string winner, string mapTag ) {
            MapTagWinner = mapTag;
            ActiveVote = new Vote(  MapManager.Maps.Keys.Where( x => x.Contains(mapTag)).ToList(), MapVoteOver );
        }

        public void PlayerJoined( [FromSource]Player ply ) {
            foreach( var game in Salty.ActiveGamemodes.Values ) {
                game.PlayerJoined( ply );
            }
        }

        public Player SourceToPlayer( int source ) {
            return new PlayerList().ToList().Where( x => Convert.ToInt32( x.Handle ) == source ).First();
        }

        public void ChatMessage( int author, string message, string lol ) {
            foreach( var ply in new PlayerList() ) {
                if( Convert.ToInt32(ply.Handle) == author ) {
                    if (ActiveGame.OnChatMessage( ply, lol ) ) {
                        CancelEvent();
                    }
                }
            }
            
        }

        public void BodyDiscovered( [FromSource] Player ply, int body ) {
            if( Salty.ActiveGamemodes.ContainsKey((int)GamemodeManager.Gamemodes.TTT) ){
                (Salty.ActiveGamemodes[(int)GamemodeManager.Gamemodes.TTT] as TTT).DeadBodies[body] = true;
                TriggerClientEvent( "salty::UpdateDeadBody", body );
            }

        }

        public void UpdatePlayerBool( [FromSource]Player ply, string key ) {
            BaseGamemode activeGame = Salty.GetGame( ply );
            if( activeGame != null )
                activeGame.UpdatePlayerBoolean( ply, key );

        }

        private async Task Init_Tick() {
            if( ActiveGame != null )
                ActiveGame.Update();
            if( ActiveVote != null )
                ActiveVote.Update();
            MapManager.Update();
            Salty.Update();
        }

        public void EndGame() {
            ActiveGame.End();
        }


        private void PlayerKilled( [FromSource] Player ply, int killerID, ExpandoObject deathData ) {
            int killerType = 0;
            List<dynamic> deathCoords = new List<dynamic>();
            foreach( var data in deathData ) {
                if( data.Key == "killertype" ) {
                    killerType = (int)data.Value;
                }
                if( data.Key == "killerpos" ) {
                    deathCoords = data.Value as List<dynamic>;
                }
            }
            PlayerDied( ply, killerType, deathCoords );
            BaseGamemode activeGame = Salty.GetGame( ply );
            if( activeGame != null )
                activeGame.PlayerKilled( ply, killerID, new Vector3(deathCoords[0], deathCoords[1], deathCoords[2]) );
        }

        private void PlayerDied( [FromSource] Player ply, int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[0], (float)deathcords[1], (float)deathcords[2] );
            BaseGamemode activeGame = Salty.GetGame( ply );
            if( activeGame != null )
                activeGame.PlayerDied( ply, killerType, coords );
        }

        private void AddScoreToPlayer( [FromSource] Player ply, int amount ) {
            ActiveGame.AddScore( ply, amount );
        }

        private void ModifyMap([FromSource] Player ply, string setting, string mapName, int team, Vector3 position, Vector3 size ) {
            if( setting == "delete" ) {
                MapManager.Maps.Remove( mapName );
                SQLConnection.Remove( mapName );
            }
            if( setting == "add" ) {
                if( MapManager.Maps.ContainsKey( mapName ) )
                    return;
                Map map = new Map( position, size, mapName );
                MapManager.Maps.Add( mapName, map );
                MapManager.Maps[mapName].AddSpawnPoint( team, position );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
            if( setting == "edit" ) {
                if( !MapManager.Maps.ContainsKey( mapName ) )
                    return;
                MapManager.Maps[mapName].Position = position;
                MapManager.Maps[mapName].Size = size;
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
        }

        private void ModifyWeaponPosition( [FromSource] Player ply, string setting, string mapName, string weapon, Vector3 position ) {
            if( setting == "delete" ) {
                MapManager.Maps[mapName].DeleteWeaponSpawn( weapon, position );
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
            if( setting == "add" ) {
                if( mapName == "AUTO" ) {
                    mapName = MapManager.FindInsideMap( position ).Name;
                    MapManager.Maps[mapName].AddWeaponSpawn( weapon, position );
                    SQLConnection.SaveMap( MapManager.Maps[mapName] );
                }
            }
        }

        private void ModifyMapPosition([FromSource] Player ply, string setting, string mapName, int team, Vector3 pos ) {
            if( setting == "delete" ) {
                MapManager.Maps[ mapName ].DeleteSpawnPoint( team, pos );
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
            if( setting == "add" ) {
                if( mapName == "AUTO" ) {
                    mapName = MapManager.FindInsideMap( pos ).Name;
                }
                MapManager.Maps[mapName].AddSpawnPoint( team, pos );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
        }

        private void SpawnPointGUI([FromSource] Player ply) {
             ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsBounds(), MapManager.AllMapsSpawns() );
        }


        public void PlayerVote( [FromSource] Player ply, string vote ) {
            if( ActiveVote != null ) {
                ActiveVote.PlayerVote( ply, vote );
            }
        }

    }
}
