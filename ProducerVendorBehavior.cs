using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    /// <summary>
    /// Reusable behavior system for vendor NPCs that perform production tasks and navigate to tools.
    /// Supports custom item types and configurable behavior phrases.
    /// </summary>
    public class ProducerVendorBehavior
    {
        private enum ProductionPhase
        {
            Idle,
            Working,
            Resting,
            Trading
        }

        private readonly Mobile _mobile;
        private readonly Func<Item, bool> _isToolPredicate;
        private readonly string[] _workingPhrases;
        private readonly string[] _restingPhrases;
        private readonly string[] _tradingPhrases;
        private readonly int _searchRange;

        private ProductionPhase _productionPhase;
        private DateTime _cycleStart = Core.Now;
        private bool _isTrading;
        private DateTime _tradeStartedAt = DateTime.MinValue;
        private TimerExecutionToken _productionTimer;
        private PathFollower _toolPath;

        /// <summary>
        /// Creates a new producer vendor behavior.
        /// </summary>
        /// <param name="mobile">The NPC mobile this behavior is attached to.</param>
        /// <param name="isToolPredicate">Function to identify valid tools (forges, anvils, looms, etc).</param>
        /// <param name="workingPhrases">Phrases to say while working.</param>
        /// <param name="restingPhrases">Phrases to say while resting.</param>
        /// <param name="tradingPhrases">Phrases to say while trading with customers.</param>
        /// <param name="searchRange">Range to search for tools (default 2 tiles).</param>
        public ProducerVendorBehavior(
            Mobile mobile,
            Func<Item, bool> isToolPredicate,
            string[] workingPhrases,
            string[] restingPhrases,
            string[] tradingPhrases,
            int searchRange = 2)
        {
            _mobile = mobile;
            _isToolPredicate = isToolPredicate ?? throw new ArgumentNullException(nameof(isToolPredicate));
            _workingPhrases = workingPhrases ?? Array.Empty<string>();
            _restingPhrases = restingPhrases ?? Array.Empty<string>();
            _tradingPhrases = tradingPhrases ?? Array.Empty<string>();
            _searchRange = searchRange;
            _cycleStart = Core.Now;
        }

        /// <summary>
        /// Starts the production timer. Call this in OnAfterSpawn or during initialization.
        /// </summary>
        public void Start()
        {
            if (_productionTimer.Running)
            {
                return;
            }

            Timer.StartTimer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), UpdateProduction, out _productionTimer);
        }

        /// <summary>
        /// Stops the production timer and cleans up. Call this in OnDelete.
        /// </summary>
        public void Stop()
        {
            _productionTimer.Cancel();
            _toolPath = null;
        }

        /// <summary>
        /// Notifies the behavior that the NPC is trading. Call from VendorBuy override.
        /// </summary>
        public void OnTradeStarted()
        {
            _isTrading = true;
            _tradeStartedAt = Core.Now;
        }

        /// <summary>
        /// Notifies the behavior that the NPC finished trading. Call from OnBuyItems/OnSellItems.
        /// </summary>
        public void OnTradeFinished()
        {
            _isTrading = false;
        }

        private void UpdateProduction()
        {
            if (_mobile.Deleted || _mobile.Map == null)
            {
                return;
            }

            // Auto-reset trading flag after 3 minutes
            if (_isTrading && Core.Now - _tradeStartedAt > TimeSpan.FromMinutes(3))
            {
                _isTrading = false;
            }

            var previousPhase = _productionPhase;
            var newPhase = ProductionPhase.Idle;

            if (_isTrading)
            {
                newPhase = ProductionPhase.Trading;
            }
            else if (HasToolNearby())
            {
                var elapsed = (Core.Now - _cycleStart).TotalSeconds;
                newPhase = elapsed % 120.0 < 60.0 ? ProductionPhase.Working : ProductionPhase.Resting;
            }

            // Only update if phase changed
            if (newPhase != previousPhase)
            {
                _productionPhase = newPhase;

                switch (newPhase)
                {
                    case ProductionPhase.Working:
                        SayRandom(_workingPhrases);
                        StartPathToTool();
                        break;
                    case ProductionPhase.Resting:
                        SayRandom(_restingPhrases);
                        _toolPath = null;
                        break;
                    case ProductionPhase.Trading:
                        SayRandom(_tradingPhrases);
                        _toolPath = null;
                        break;
                }
            }

            // Animate and follow path during working phase
            if (newPhase == ProductionPhase.Working)
            {
                if (_mobile is BaseCreature bc)
                {
                    bc.CheckedAnimate(11, 5, Utility.Random(2, 5), true, false, 1);
                }
                FollowToolPath();
            }
        }

        private bool HasToolNearby()
        {
            if (_mobile.Map == null)
            {
                return false;
            }

            foreach (var item in _mobile.Map.GetItemsInRange(_mobile.Location, _searchRange))
            {
                if (_isToolPredicate(item))
                {
                    return true;
                }
            }

            return false;
        }

        private void StartPathToTool()
        {
            _toolPath = null;

            if (_mobile.Deleted || _mobile.Map == null)
            {
                return;
            }

            var item = FindNearestTool();
            if (item == null)
            {
                return;
            }
            if (_mobile is BaseCreature bc && bc.AIObject != null){
                _toolPath = new PathFollower(_mobile, item) { Mover = bc.AIObject.DoMoveImpl };
            }
        }

        private void FollowToolPath()
        {
            if (_toolPath == null)
            {
                return;
            }

            if (_toolPath.Follow(true, 1))
            {
                _toolPath = null;
            }
        }

        private Item FindNearestTool()
        {
            if (_mobile.Map == null)
            {
                return null;
            }

            Item best = null;
            int bestDistance = int.MaxValue;

            foreach (var item in _mobile.Map.GetItemsInRange(_mobile.Location, _searchRange))
            {
                if (!_isToolPredicate(item))
                {
                    continue;
                }

                double dist = Utility.GetDistanceToSqrt(_mobile, item);
                if (dist < bestDistance)
                {
                    bestDistance = (int)dist;
                    best = item;
                }
            }

            return best;
        }

        private void SayRandom(string[] phrases)
        {
            if (phrases.Length == 0)
            {
                return;
            }

            _mobile.Say(phrases[Utility.Random(phrases.Length)]);
        }
    }
}
