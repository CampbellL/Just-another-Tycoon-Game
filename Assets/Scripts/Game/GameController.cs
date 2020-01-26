using System;
using Economy;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        public static IObservable<long> TickObserver;
        
        private void Awake()
        {
            TickObserver = Observable.Interval(TimeSpan.FromSeconds(1));
        }
    }
}