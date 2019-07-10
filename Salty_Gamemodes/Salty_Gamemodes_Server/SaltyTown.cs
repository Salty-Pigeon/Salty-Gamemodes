using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class SaltyTown : BaseScript {

        Salty MainGame;
        MapManager Maps;
        GamemodeManager GamemodeManager;
        List<Player> ActivePlayers = new List<Player>();
        public Dictionary<int, BaseGamemode> ActiveGamemodes = new Dictionary<int, BaseGamemode>();
        Dictionary<int, List<Player>> ActiveRooms = new Dictionary<int, List<Player>>();
        

        Dictionary<int, float> NextMapTimer = new Dictionary<int, float>();

        public SaltyTown( MapManager maps ) {
            Maps = maps;
            MainGame = new Salty();
            GamemodeManager = new GamemodeManager();
            ActivePlayers = new PlayerList().ToList();
        }

        public void JoinRoom( Player ply, int gamemode ) {
            
            Debug.WriteLine( "Joining room " + (GamemodeManager.Gamemodes)gamemode );

            if( !ActiveRooms.ContainsKey(gamemode) ) {
                Debug.WriteLine( "No gamemode created, creating." );
                StartRoom( gamemode );
            } else if( ActiveRooms[gamemode].Contains( ply ) ) {
                WriteChat( ply, "SaltyTown", "Already in this room.", 255, 255, 255 );
                return;
            }

            if( !ActivePlayers.Contains( ply ) ) {
                LeaveRoom( ply );
            }

            if( ActiveGamemodes.ContainsKey(gamemode) ) {
                Debug.WriteLine( " players: " + ActiveGamemodes[gamemode].InGamePlayers.Count );
                WriteChat( ply, "SaltyTown", "You will join the next game when it starts." , 255, 255, 255 );
            }

            ActivePlayers.Remove( ply );
            ActiveRooms[gamemode].Add( ply );

        }

        public void JoinRoom( Player ply, string gamemodeName ) {
            int gamemode = GamemodeManager.Names[gamemodeName];
            JoinRoom( ply, gamemode );
        }


        public void SendRoomsToClient( Player ply ) {
            if( isPlayerInRoom(ply) ) {
                ply.TriggerEvent( "salty::ShowRooms", GamemodeManager.GetNames() );
            } else {
                ply.TriggerEvent( "salty::ShowRooms", GamemodeManager.GetNames() );
            }
        }

        public void StartRoom( int gamemode ) {
            if( ActiveRooms.ContainsKey(gamemode) ) {
                Debug.WriteLine( "Room already created for: " + (GamemodeManager.Gamemodes)gamemode );
                return;
            }
            ActiveRooms.Add( gamemode, new List<Player>() );
        }


        public BaseGamemode GetGame( Player ply ) {
            int room = GetRoom( ply );
            if( !ActiveGamemodes.ContainsKey(room) ) {
                Debug.WriteLine( room + " game has not started" );
                return null;
            }
            int game = GamemodeManager.PlayerInGame( ply );
            if ( game > 0 ) {
                Debug.WriteLine( ply.Name + " is in game " + room );
                return ActiveGamemodes[room];
            }
            Debug.WriteLine( "Player not active in a gamemode" );

            return null;
        }

        public int GetRoom( Player ply ) {
            foreach( var room in ActiveRooms ) {
                if( room.Value.Contains( ply ) ) {
                    return room.Key;
                }
            }
            return 0;
        }

        public Map RandomMap( string mapTag ) {
            Random rand = new Random();
            List<Map> maps = Maps.MapList( mapTag );
            return maps[rand.Next( 0, maps.Count )];
        }

        public void StartGame( int gamemode, Map map ) {
            if( !ActiveRooms.ContainsKey(gamemode ) ) {
                Debug.WriteLine( "No room created" );
                return;
            }
            if( ActiveGamemodes.ContainsKey(gamemode) ) {
                Debug.WriteLine( "Game in progress." );
            } else {
                ActiveGamemodes.Add( gamemode, GamemodeManager.StartGame( gamemode, map, ActiveRooms[gamemode] ) );
            }
        }

        public void StartGame( int gamemode ) {
            if( !GamemodeManager.Names.ContainsValue(gamemode) ) {
                Debug.WriteLine( "No gamemode found" );
                return;
            }
            if( !ActiveRooms.ContainsKey( gamemode ) ) {
                Debug.WriteLine( "No room created" );
                return;
            }
            if( ActiveGamemodes.ContainsKey( gamemode ) ) {
                Debug.WriteLine( "Game in progress." );
                return;
            }
            else {
                Map map = RandomMap( GamemodeManager.Maps[(GamemodeManager.Gamemodes)gamemode] );
                ActiveGamemodes.Add( gamemode, GamemodeManager.StartGame( gamemode, map, ActiveRooms[gamemode] ) );
                map.SendToClients( ActivePlayers );
            }
        }

        public bool isPlayerInRoom( Player ply ) {
            return ActivePlayers.Contains( ply );
        }

        public GamemodeManager.Gamemodes GetPlayerRoom( Player ply ) {
            if( isPlayerInRoom(ply) ) {
                foreach( var room in ActiveRooms ) {
                    if( room.Value.Contains( ply ) )
                        return (GamemodeManager.Gamemodes)room.Key;
                }
            }
            return GamemodeManager.Gamemodes.None;
        }

        public void NextGame( int gamemode ) {
            foreach( var ply in ActiveRooms[gamemode] ) {
                Debug.WriteLine( ply.Name );
                ply.TriggerEvent( "salty::EndGame" );
            }
            WriteChat( gamemode, ((GamemodeManager.Gamemodes)gamemode).ToString(), "Next game starting in 5 seconds", 230, 0, 0 );
            NextMapTimer[gamemode] = GetGameTimer() + 5 * 1000;
            ActiveGamemodes.Remove( gamemode );
        }

        public void Update() {
            foreach( var mapTimer in NextMapTimer.ToDictionary( x => x.Key, x => x.Value ) ) {
                if( mapTimer.Value - GetGameTimer() < 0 ) {
                    StartGame( mapTimer.Key );
                    NextMapTimer.Remove( mapTimer.Key );
                }
            }
        }

        public void EndRoom( int gamemode ) {
            ActiveGamemodes[gamemode].End();
            ActiveGamemodes.Remove( gamemode );
            foreach( var ply in ActiveRooms[gamemode] ) {
                ActivePlayers.Add( ply );
            }
            ActiveRooms.Remove( gamemode );
        }

        public void LeaveRoom( Player ply ) {
            var room = GetRoom( ply );
            Debug.WriteLine( ply.Name + " leaving room " + (GamemodeManager.Gamemodes)room );
            ActiveRooms[room].Remove( ply );
            ActivePlayers.Add( ply );
            GamemodeManager.RemovePlayer( ply, room );
        }

        public void WriteChat( Player ply, string prefix, string str, int r, int g, int b ) {
            ply.TriggerEvent( "chat:addMessage", new {
                color = new[] { r, g, b },
                args = new[] { prefix, str }
            } );
        }

        public void WriteChat( int gamemode, string prefix, string str, int r, int g, int b ) {
            foreach( Player ply in ActiveRooms[gamemode] ) {
                ply.TriggerEvent( "chat:addMessage", new {
                    color = new[] { r, g, b },
                    args = new[] { prefix, str }
                } );
            }
            
        }


    }
}
