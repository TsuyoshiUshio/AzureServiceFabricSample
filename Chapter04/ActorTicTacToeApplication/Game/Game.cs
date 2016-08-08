using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Game.Interfaces;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Data;
using System.ComponentModel;

namespace Game
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Game : Actor, IGame
    {
        [DataContract]
        public class ActorState
        {
            [DataMember]
            public int[] Board;
            [DataMember]
            public string Winner;
            [DataMember]
            public List<Tuple<long, string>> Players;
            [DataMember]
            public int NextPlayerIndex;
            [DataMember]
            public int NumberOfMoves;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ConditionalValue<ActorState> state = this.StateManager.TryGetStateAsync<ActorState>("MyState").GetAwaiter().GetResult();
            if (!state.HasValue)
            {
                var actorState = new ActorState()
                {
                    Board = new int[9],
                    Winner = "",
                    Players = new List<Tuple<long, string>>(),
                    NextPlayerIndex = 0,
                    NumberOfMoves = 0
                };
                this.StateManager.SetStateAsync<ActorState>("MyState", actorState);
            }
            return Task.FromResult(true);
        }
        
        private ActorState State
        {
            get
            {
                return this.StateManager.TryGetStateAsync<ActorState>("MyState").GetAwaiter().GetResult().Value;
            }

            set
            {
                this.StateManager.AddOrUpdateStateAsync<ActorState>("MyState", value, (ke, v) => value);
            }
            
        }

        public Task<bool> JoinGameAsync(long playerId, string playerName)
        {
            var state = this.State;
           if (state.Players.Count >= 2 || state.Players.FirstOrDefault(p => p.Item2 == playerName) != null)
            {
                return Task.FromResult<bool>(false);
            }

            state.Players.Add(new Tuple<long, string>(playerId, playerName));
            this.State = state;
            return Task.FromResult<bool>(true);
        }

        [ReadOnly(true)]
        public Task<int[]> GetGameBoardAsync()
        {
            return Task.FromResult<int[]>(this.State.Board);
        }
        [ReadOnly(true)]
        public Task<string> GetWinnerAsync()
        {
            return Task.FromResult<string>(this.State.Winner);
        }

        public Task<bool> MakeMoveAsync(long playerId, int x, int y)
        {
            var state = this.State;
            if (x < 0 || x > 2 || y < 0 || y > 2
                || state.Players.Count != 2
                || state.NumberOfMoves >= 9
                || state.Winner != "")
                return Task.FromResult<bool>(false);

            int index = state.Players.FindIndex(p => p.Item1 == playerId);
            if (index == state.NextPlayerIndex)
            {
                if (state.Board[y * 3 + x] == 0)
                {
                    int piece = index * 2 - 1;
                    state.Board[y * 3 + x] = piece;
                    state.NumberOfMoves++;
                    if (HasWon(piece * 3))
                        state.Winner = state.Players[index].Item2 + " (" +
                            (piece == -1 ? "X" : "0") + ")";
                    else if (state.Winner == "" && state.NumberOfMoves >= 9)
                        state.Winner = "TIE";
                    state.NextPlayerIndex = (state.NextPlayerIndex + 1) % 2;
                    this.State = state;
                    return Task.FromResult<bool>(true);
                }
                else
                    return Task.FromResult<bool>(false);
            }
            else
                return Task.FromResult<bool>(false);
        }

        private bool HasWon(int sum)
        {
            var state = this.State;
            return state.Board[0] + state.Board[1] + state.Board[2] == sum
                || state.Board[3] + state.Board[4] + state.Board[5] == sum
                || state.Board[6] + state.Board[7] + state.Board[8] == sum
                || state.Board[0] + state.Board[3] + state.Board[6] == sum
                || state.Board[1] + state.Board[4] + state.Board[7] == sum
                || state.Board[2] + state.Board[5] + state.Board[8] == sum
                || state.Board[0] + state.Board[4] + state.Board[8] == sum
                || state.Board[2] + state.Board[4] + state.Board[6] == sum;
        }
    }
}
