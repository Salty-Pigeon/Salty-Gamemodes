﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TTT : BaseGamemode {

        List<Player> traitors = new List<Player>();
        List<Player> innocents = new List<Player>();


        public enum Teams {
            Spectators,
            Traitors,
            Innocents
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public GameState CurrentState = GameState.None;


        public TTT( Map gameMap, PlayerList players, int ID ) : base ( ID ) {
            GameMap = gameMap;
            this.players = players;
        }

        public override void Start() {
            Debug.WriteLine( "TTT starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            // Set traitor
            int traitorID = rand.Next( 0, players.Count );
            Player traitor = players[ traitorID ];
            traitors.Add( traitor );
            traitor.TriggerEvent( "salty::StartGame", ID, (int)Teams.Traitors, GameMap.Position, GameMap.Size );
            players.RemoveAt( traitorID );

            // Set innocents
            foreach( var ply in players ) {
                innocents.Add( ply );
                ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Innocents, GameMap.Position, GameMap.Size );
            }

            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
        }

        public override void End() {
            Debug.WriteLine( "Game ending" );
        }

        public override void Update() {

        }

    }
}
