using System.Collections.Generic;
using System.Linq;
using Game;

namespace Worker
{
  internal sealed class WorkerController
  {
    private readonly PlayerStateService _playerStateService;
    public WorkerController(PlayerStateService playerStateService)
    {
      this._playerStateService = playerStateService;
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
