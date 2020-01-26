using System.Collections.Generic;
using System.Linq;
using Economy;
using Worker;

namespace Game
{
    public sealed class PlayerState
    {
        public readonly List<Worker.Worker> Workers;
        public readonly List<Drink> UnlockedDrinks;
        public readonly List<Customer> Customers;
        public PlayerState()
        {
            Workers = new List<Worker.Worker>
            {
                new Worker.Worker() {Efficiency = 2, Name = "Carlo", Cost = 2000},
            };

            Customers = new List<Customer>();
            
            UnlockedDrinks = new List<Drink>
            {
                new Drink() {name = "Cappuccino", price = 7},
                new Drink() {name = "coffee", price = 3},
                new Drink() {name = "flat white", price = 2}
            };
            
            for (var i = 0; i < 100; i++)
            {
                Customers.Add(new Customer() { FavoriteDrink = UnlockedDrinks[0] , Name = $"Cutomer{i}",Satisfaction = 20});
            }
        }
    }
}