namespace Worker
{
    public class Order
    {
        public bool completed;
        public Drink Drink;

        public Order(Drink drink)
        {
            Drink = drink;
        }
    }
}