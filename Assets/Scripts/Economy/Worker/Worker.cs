using System.Collections.Generic;
using UnityEngine;

namespace Worker
{
    public sealed class Worker
    {
        public string Name;
        public int Efficiency;
        private int _actions;
        public int Cost;
        public List<ITrait> Traits;
        public bool Occupied;
        public readonly byte HairType;
        public readonly byte BodyType;
        public readonly byte KitType;
        public readonly byte FaceType;

        public float CostPerTick() => (float) this.Cost / 43800;

        public Worker()
        {
            this.HairType = (byte) Random.Range(1, 15);
            this.BodyType = (byte) Random.Range(1, 4);
            this.FaceType = (byte) Random.Range(1, 4);
            this.KitType = (byte) Random.Range(1, 18);
        }

        public void Work(Order order)
        {
            if (_actions == Efficiency)
            {
                Occupied = true;
            }
            order.completed = true;
            _actions++;
        }
    }
}