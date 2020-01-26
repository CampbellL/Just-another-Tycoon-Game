using System.Collections.Generic;
using System.Linq;
using Game;
using UniRx;
using UnityEngine;
using Worker;

namespace Economy
{
    internal class EconomyController
    {
        public delegate void MoneyChanged();
        public event MoneyChanged OnMoneyChanged;
        private float _money;
        private readonly WorkerController _workerController;
        private readonly PlayerStateService _playerStateService;

        public float Money
        {
            get => _money;

            private set
            {
                this.MoneyChange();
                _money = value;
            }
        }

        public EconomyController(WorkerController workerController, PlayerStateService playerStateService)
        {
            this._workerController = workerController;
            this._playerStateService = playerStateService;
            GameController.TickObserver.Subscribe(tick => this.EconomyTick());
        } 

        private static List<Order> GetOrders()
        {
            var orders = new List<Order>
            {

            };
            return orders;
        }

        private void EconomyTick()
        {
            var workerCost = _playerStateService.GetWorkers().Sum(worker => worker.CostPerTick());
            var profit = _workerController.HandleOrders(GetOrders()).Sum(order => order.Drink.price);
            //profit -= workerCost;
            this.Money += profit;
        }
        
        private void MoneyChange()
        {
            this.OnMoneyChanged?.Invoke();
        }
    }
}