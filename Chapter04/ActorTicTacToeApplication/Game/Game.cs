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
        private static readonly string STATE_KEY = "MyState";
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
        protected override async Task OnActivateAsync()
        {
            ConditionalValue<ActorState> state = await this.StateManager.TryGetStateAsync<ActorState>(STATE_KEY);
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
                await this.StateManager.SetStateAsync<ActorState>(STATE_KEY, actorState);
            }
        }
        private async Task<ActorState> GetActorState()
        {
            ConditionalValue<ActorState> stateValue = await this.StateManager.TryGetStateAsync<ActorState>(STATE_KEY);
            return await Task.FromResult<ActorState>(stateValue.Value);
        }
        private async Task SetActorState(ActorState state)
        {
            await this.StateManager.AddOrUpdateStateAsync<ActorState>(STATE_KEY, state, (ke, v) => state);
            return;
        }

        public async Task<bool> JoinGameAsync(long playerId, string playerName)
        {
            var state = await GetActorState();
            if (state.Players.Count >= 2 || state.Players.FirstOrDefault(p => p.Item2 == playerName) != null)
            {
                return await Task.FromResult<bool>(false);
            }

            state.Players.Add(new Tuple<long, string>(playerId, playerName));
            await SetActorState(state);
            return await Task.FromResult<bool>(true);
        }

        [ReadOnly(true)]
        public async Task<int[]> GetGameBoardAsync()
        {
            var state = await GetActorState();
            return await Task.FromResult<int[]>(state.Board);
        }
        [ReadOnly(true)]
        public async Task<string> GetWinnerAsync()
        {
            var state = await GetActorState();
            return await Task.FromResult<string>(state.Winner);
        }

        public async Task<bool> MakeMoveAsync(long playerId, int x, int y)
        {
            var state = await GetActorState();
            if (x < 0 || x > 2 || y < 0 || y > 2
                || state.Players.Count != 2
                || state.NumberOfMoves >= 9
                || state.Winner != "")
                return await Task.FromResult<bool>(false);

            int index = state.Players.FindIndex(p => p.Item1 == playerId);
            if (index == state.NextPlayerIndex)
            {
                if (state.Board[y * 3 + x] == 0)
                {
                    int piece = index * 2 - 1;
                    state.Board[y * 3 + x] = piece;
                    state.NumberOfMoves++;
                    if (await HasWonAsync(piece * 3))
                        state.Winner = state.Players[index].Item2 + " (" +
                            (piece == -1 ? "X" : "0") + ")";
                    else if (state.Winner == "" && state.NumberOfMoves >= 9)
                        state.Winner = "TIE";
                    state.NextPlayerIndex = (state.NextPlayerIndex + 1) % 2;
                    await SetActorState(state);
                    return await Task.FromResult<bool>(true);
                }
                else
                    return await Task.FromResult<bool>(false);
            }
            else
                return await Task.FromResult<bool>(false);
        }

        private async Task<bool> HasWonAsync(int sum)
        {
            var state = await GetActorState();
            bool result =  state.Board[0] + state.Board[1] + state.Board[2] == sum
                || state.Board[3] + state.Board[4] + state.Board[5] == sum
                || state.Board[6] + state.Board[7] + state.Board[8] == sum
                || state.Board[0] + state.Board[3] + state.Board[6] == sum
                || state.Board[1] + state.Board[4] + state.Board[7] == sum
                || state.Board[2] + state.Board[5] + state.Board[8] == sum
                || state.Board[0] + state.Board[4] + state.Board[8] == sum
                || state.Board[2] + state.Board[4] + state.Board[6] == sum;
            return await Task.FromResult<bool>(result);
        }
    }
}
