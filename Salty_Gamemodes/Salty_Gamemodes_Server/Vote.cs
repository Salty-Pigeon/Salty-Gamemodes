using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class Vote : BaseScript {

        public Dictionary<string, int> VoteCounts = new Dictionary<string, int>();
        public double VoteTime = 0;
        public bool VoteStarted = false;
        public List<Player> PlayerVoted = new List<Player>();
        public Dictionary<string, string> Votes = new Dictionary<string, string>();
        Action<string, string> Winner;
        Action<string> WinnerSingle;
        public string WinnerVote;
        public bool isStopped = false;

        public Vote( Dictionary<string, string> votes, Action<string, string> winner ) {
            TriggerClientEvent( "salty::VoteMap", votes.Keys );
            Votes = votes;
            Winner = winner;
            PlayerVoted = new List<Player>();
            VoteCounts = new Dictionary<string, int>();
            VoteTime = GetGameTimer() + (15 * 1000);
            VoteStarted = true;
        }

        public Vote( List<string> votes,  Action<string> winner ) {
            foreach( var vote in votes ) {
                Votes.Add( vote, "" );
            }
            TriggerClientEvent( "salty::VoteMap", votes );
            WinnerSingle = winner;
            PlayerVoted = new List<Player>();
            VoteCounts = new Dictionary<string, int>();
            VoteTime = GetGameTimer() + (15 * 1000);
            VoteStarted = true;
        }

        public void PlayerVote( Player ply, string vote ) {
            if( PlayerVoted.Contains( ply ) )
                return;
            if( VoteCounts.ContainsKey( vote ) ) {
                VoteCounts[vote]++;
            }
            else {
                VoteCounts.Add( vote, 1 );
            }
            PlayerVoted.Add( ply );
            if( PlayerVoted.Count >= new PlayerList().ToList().Count ) {
                DecideVoteWinner();
            }

        }

        public void DecideVoteWinner() {
            VoteStarted = false;
            string winner;
            if( VoteCounts.Values.Count == 0 ) {
                winner = Votes.ElementAt( 0 ).Key;
            } else {
                winner = VoteCounts.OrderBy( x => x.Value ).ElementAt( 0 ).Key;
            }
            WinnerVote = winner;
            if( WinnerSingle == null )
                Winner( winner, Votes[winner] );
            else
                WinnerSingle( winner );

        }

        public void Update() {
            if( VoteStarted )
                if( VoteTime - GetGameTimer() < 0 )
                    DecideVoteWinner();
        }

    }
}
