using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using RandomNameGen;

namespace Worker
{
  internal sealed class WorkerController
  {
    private readonly PlayerStateService _playerStateService;
    public WorkerController(PlayerStateService playerStateService)
    {
      this._playerStateService = playerStateService;
    }

    public static Worker GenerateRandomWorker()
    {
      Random rand = new Random();
      RandomName nameGen = new RandomName(rand);
      var worker = new Worker()
      {
        Name = nameGen.Generate((Sex) rand.Next(0, 2), rand.Next(0, 2)), Cost = 2000
      };
      var weights = new Dictionary<int,int>()
      {
        {1,40},
        {2,40},
        {3,15},
        {4,4},
        {5,1},
      };
      worker.Efficiency = WeightedRandomizer.From(weights).TakeOne();
      return worker;
    }

    public IEnumerable<Order> HandleOrders(IEnumerable<Order> orders)
    {
      var handleOrders = orders.ToList();
      foreach (Order order in handleOrders)
      {
        foreach (Worker worker in _playerStateService.GetWorkers().Where(worker => !worker.Occupied))
        {
          worker.Work (order);
          if (order.completed) break;
        }
      }

      return handleOrders;
    }
  }
}
