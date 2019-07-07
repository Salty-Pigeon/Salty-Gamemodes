using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TTT : BaseGamemode {

        public Dictionary<int, bool> DeadBodies = new Dictionary<int, bool>();

        public enum Teams {
            Spectators,
            Traitors,
            Innocents,
            Detectives
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public GameState CurrentState = GameState.None;


        public TTT( MapManager manager, int ID, string MapTag ) : base ( manager, ID, MapTag ) {
            Init();
        }

        public TTT( MapManager manager, int ID, Map map ) : base( manager, ID, map ) {
            Init();
        }

        public void Init() {
            GameMap.WeaponWeights = new Dictionary<string, int>() {
                { "WEAPON_PISTOL", 8 },
                { "WEAPON_COMBATPISTOL", 7  },
                { "WEAPON_SMG", 6  },
                { "WEAPON_CARBINERIFLE", 5  },
                { "WEAPON_ASSAULTRIFLE", 4  },
                { "WEAPON_PUMPSHOTGUN", 4  },
                { "WEAPON_MICROSMG", 6 },
                { "WEAPON_COMBATMG", 2 }
            };

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Fists" },
                { "WEAPON_PISTOL", "Pistol"  },
                { "WEAPON_COMBATPISTOL", "Combat Pistol" },
                { "WEAPON_SMG", "SMG" },
                { "WEAPON_CARBINERIFLE", "Carbine"  },
                { "WEAPON_ASSAULTRIFLE", "AK47"  },
                { "WEAPON_PUMPSHOTGUN", "Pump Shotgun"  },
                { "WEAPON_MICROSMG", "Micro-SMG" },
                { "WEAPON_COMBATMG", "Light Machine Gun" },
                { "WEAPON_KNIFE", "Knife" }
            };

            WeaponSlots = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 1  },
                { "WEAPON_COMBATPISTOL", 1 },
                { "WEAPON_MICROSMG", 1 },
                { "WEAPON_SMG", 2 },
                { "WEAPON_CARBINERIFLE", 2  },
                { "WEAPON_ASSAULTRIFLE", 2  },
                { "WEAPON_PUMPSHOTGUN", 2  },
                { "WEAPON_COMBATMG", 2 },
                { "WEAPON_KNIFE", 3 },
            };

            WeaponMaxAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 38  },
                { "WEAPON_COMBATPISTOL", 38 },
                { "WEAPON_MICROSMG", 96 },
                { "WEAPON_SMG", 180 },
                { "WEAPON_CARBINERIFLE", 180  },
                { "WEAPON_ASSAULTRIFLE", 180  },
                { "WEAPON_PUMPSHOTGUN", 24  },
                { "WEAPON_COMBATMG", 300 },
                { "WEAPON_KNIFE", 1 },
            };
        }

        public override void PlayerJoined( Player ply ) {
            base.PlayerJoined( ply );
        }

        public override bool OnChatMessage( Player ply, string message ) {
            if( GetTeam(ply) == (int)Teams.Spectators ) {
                foreach( var player in PlayerDetails ) {
                    if( GetTeam(player.Key) == (int)Teams.Spectators ) {
                        WriteChat( player.Key, "[DEAD] " + ply.Name, message, 200, 200, 0 );
                    }
                }
            } else {
                foreach( var player in PlayerDetails ) {
                    if( GetTeam( ply ) == (int)Teams.Detectives ) {
                        WriteChat( player.Key, "[DETECTIVE] " + ply.Name, message, 0, 0, 230 );
                    } else {
                        WriteChat( player.Key, ply.Name, message, 0, 230, 0 );
                    }
                }
            }
            return true;
        }

        public override void Start() {
            Random rand = new Random();
            List<Player> players = Players.ToList();

            int traitorCount = (int)Math.Ceiling((double)players.Count / 4);
            for( var i = 0; i < traitorCount; i++ ) {
                int traitorID = rand.Next( 0, players.Count );
                Player traitor = players[traitorID];
                SetTeam( traitor, (int)Teams.Traitors );
                SpawnClient( traitor, 1 );
                players.RemoveAt( traitorID );
            }

            if( players.Count >= 4 ) {
                int detectiveID = rand.Next( 0, players.Count );
                Player detective = players[detectiveID];
                SetTeam( detective, (int)Teams.Detectives );
                SpawnClient( detective, 1 );
                players.RemoveAt( detectiveID );
            }

            foreach (var ply in players) {
                SetTeam( ply, (int)Teams.Innocents );
                SpawnClient( ply, 1 );
            }

            base.Start();
        }

        public override void PlayerKilled( Player player, int killerID, Vector3 deathcords ) {
            SetTeam( player, (int)Teams.Spectators );
            TriggerClientEvent( "salty::SpawnDeadBody", deathcords, Convert.ToInt32( player.Handle ), killerID );
            if( GetTeam( player ) == (int)Teams.Traitors ) {
                GameTime += 30 * 1000;
            }
            base.PlayerKilled( player, killerID, deathcords );
        }

        public override void PlayerDied( [FromSource] Player player, int killerType, Vector3 deathcords ) {
            SetTeam(player, (int)Teams.Spectators);
            TriggerClientEvent( "salty::SpawnDeadBody", deathcords, Convert.ToInt32( player.Handle ), Convert.ToInt32( player.Handle ) );
            if( TeamCount((int)Teams.Traitors) <= 0 ) {
                WriteChat( "TTT", "Innocents Win!", 0, 230, 0 );
                End();
            } else if( TeamCount((int)Teams.Innocents) + TeamCount((int)Teams.Detectives) <= 0 ) {
                WriteChat( "TTT", "Traitors Win!", 230, 0, 0 );
                End();
            }
            base.PlayerDied(player, killerType, deathcords);
        }

        public override void End() {
            WriteChat( "TTT", "Game ending", 255, 0, 0 );
            base.End();
        }

        public override void Update() {

            base.Update();
        }

    }
}
