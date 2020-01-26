using System.Collections.Generic;
using Economy;

namespace Game
{
    public class PlayerStateService
    {
        private readonly PlayerState _playerState;
        
        public List<Worker.Worker> GetWorkers() => this._playerState.Workers;
        public List<Customer> GetCustomers() => this._playerState.Customers;
        
        public PlayerStateService(PlayerState playerState)
        {
            this._playerState = playerState;
        }

    }
}