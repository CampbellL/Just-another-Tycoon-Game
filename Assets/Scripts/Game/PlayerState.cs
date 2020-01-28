using System.Collections.Generic;
using Economy;
using RandomNameGen;
using Worker;
using Random = System.Random;

namespace Game
{
    public sealed class PlayerState
    {
        public readonly List<Worker.Worker> Workers;
        public readonly List<Drink> UnlockedDrinks;
        public readonly List<Customer> Customers;
        public int MoneyPercentageSkillLevel;
        public int EfficiencyPercentageSkillLevel;
        public PlayerState()
        {
            this.MoneyPercentageSkillLevel = 1;
            this.EfficiencyPercentageSkillLevel = 1;
            Random rand = new Random();
            RandomName nameGen = new RandomName(rand);
            Workers = new List<Worker.Worker>
            {
                new Worker.Worker() {Efficiency = 2, Name = nameGen.Generate((Sex) rand.Next(0,2), rand.Next(0,2)), Cost = 2000},
                //new Worker.Worker() {Efficiency = 2, Name = "Hitler", Cost = 2000},
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
                Customers.Add(new Customer() { FavoriteDrink = UnlockedDrinks[0] , Name = $"Customer{i}",Satisfaction = 20});
            }
        }
    }
}