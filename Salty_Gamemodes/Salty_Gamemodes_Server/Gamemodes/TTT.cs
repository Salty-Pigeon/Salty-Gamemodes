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


        public TTT( int ID, Map map, List<Player> players ) : base( ID, map, players ) {
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

            GameMap.GameWeapons = new Dictionary<string, string>() {
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

            GameMap.WeaponSpawnAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 12  },
                { "WEAPON_COMBATPISTOL", 12 },
                { "WEAPON_MICROSMG", 32 },
                { "WEAPON_SMG", 60 },
                { "WEAPON_CARBINERIFLE", 60  },
                { "WEAPON_ASSAULTRIFLE", 60  },
                { "WEAPON_PUMPSHOTGUN", 8  },
                { "WEAPON_COMBATMG", 100 },
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
            List<Player> players = InGamePlayers.ToList();

            int traitorCount = (int)Math.Ceiling((double)players.Count / 4);
            for( var i = 0; i < traitorCount; i++ ) {
                int traitorID = rand.Next( 0, players.Count );
                Player traitor = players[traitorID];
                SpawnClient( traitor, 1, (int)Teams.Traitors );
                players.RemoveAt( traitorID );
            }

            if( players.Count >= 4 ) {
                int detectiveID = rand.Next( 0, players.Count );
                Player detective = players[detectiveID];
                SpawnClient( detective, 1, (int)Teams.Detectives );
                players.RemoveAt( detectiveID );
            }

            foreach (var ply in players) {
                SpawnClient( ply, 1, (int)Teams.Innocents );
            }

            GameMap.SpawnWeapons();

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
            TriggerClientEvent( "salty::ClearWeapons", GameMap.Name );
            base.End();
        }

        public override void Update() {

            base.Update();
        }

    }
}
