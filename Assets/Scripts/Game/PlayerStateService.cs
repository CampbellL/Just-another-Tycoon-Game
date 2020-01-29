using System.Collections.Generic;
using Economy;

namespace Game
{
    public class PlayerStateService
    {
        private readonly PlayerState _playerState;
        
        public List<Worker.Worker> GetWorkers() => this._playerState.Workers;
        public int GetWorkerCap() => PlayerState.WorkerCap;

        public int UpgradePercentageSkill() => ++this._playerState.MoneyPercentageSkillLevel;
        
        public int GetMoneyPercentageSkillLevel() => this._playerState.MoneyPercentageSkillLevel;

        public bool HireWorker(Worker.Worker worker)
        {
            if (this._playerState.Workers.Count == PlayerState.WorkerCap) return false;
            this._playerState.Workers.Add(worker);
            return true;

        }
        public PlayerStateService(PlayerState playerState)
        {
            this._playerState = playerState;
        }

    }
}