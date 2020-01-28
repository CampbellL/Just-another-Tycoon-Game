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
        private float _money = 0;
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

        public int GetPercentageSkillUpgradeCost()
        {
            return 2 * this._playerStateService.GetMoneyPercentageSkillLevel() * 1000;
        }

        public bool CanUpgradePercentageSkill()
        {
            return _money > this.GetPercentageSkillUpgradeCost();
        }

        public int PurchasePercentageSkillUpgrade()
        {
            if (CanUpgradePercentageSkill())
            {
                this.Money -= this.GetPercentageSkillUpgradeCost();
                return this._playerStateService.UpgradePercentageSkill();
            }
            return -1;
        }

        public void Purchase(int amount)
        {
            this.Money -= amount;
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
                new Order(new Drink() {name = "Cappuccino", price = 7.50f}),
                new Order(new Drink() {name = "Flat White", price = 10.50f}),
                new Order(new Drink() {name = "Latte Macchiato", price = 5.50f}),
                new Order(new Drink() {name = "Coffee", price = 2.50f})
            };

            for (var i = 0; i < 25; i++)
            {
                orders.AddRange(orders);
            }
            return orders;
        }


        private float ApplyProfitSkill(float income)
        {
            return (float) (this._playerStateService.GetMoneyPercentageSkillLevel() * 0.05) * income + income;
        }

        private void EconomyTick()
        {
            var workerCost = _playerStateService.GetWorkers().Sum(worker => worker.CostPerTick());
            var profit = ApplyProfitSkill(
                _workerController.HandleOrders(GetOrders()).Sum(order => order.Drink.price)
            );
            this.Money += profit;
        }

        private void MoneyChange()
        {
            this.OnMoneyChanged?.Invoke();
        }
    }
}