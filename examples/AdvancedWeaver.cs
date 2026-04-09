using ModernUO.Serialization;
using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    /// <summary>
    /// Example vendor NPC using ProducerVendorBehavior with custom loom detection.
    /// Demonstrates how to reuse the producer behavior for different crafting professions.
    /// </summary>
    [SerializationGenerator(0, false)]
    public partial class AdvancedWeaver : Weaver
    {
        private static readonly string[] _workingPhrases =
                [
                    "The threads must be perfectly aligned or the cloth will be worthless.",
                    "One more pass through the loom and this weave will be complete.",
                    "A steady hand and a patient mind make the finest fabric.",
                    "The threads sing when the loom is working properly.",
                    "Focus now; silk requires far more care than wool."
                ];

        private static readonly string[] _restingPhrases =
                [
                    "Even a weaver's hands need a moment of rest.",
                    "The threads will wait, but not for long.",
                    "A brief pause to steady the nerves.",
                    "Good weaving demands both speed and precision.",
                    "The loom is hungry for more work."
                ];

        private static readonly string[] _tradingPhrases =
                [
                    "Ah, a customer with discerning taste in fabric.",
                    "Just a moment while I leave the loom.",
                    "Tell me what cloth you seek.",
                    "The finest fabrics are worth your attention.",
                    "Your custom is more precious than my work right now."
                ];

        private ProducerVendorBehavior _producerBehavior;

        [Constructible]
        public AdvancedWeaver() : base()
        {
            Title = "the advanced weaver";
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsLoom,
                _workingPhrases,
                _restingPhrases,
                _tradingPhrases,
                searchRange: 2
            );
        }

        public AdvancedWeaver(Serial serial) : base(serial)
        {
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsLoom,
                _workingPhrases,
                _restingPhrases,
                _tradingPhrases,
                searchRange: 2
            );
        }

        public override void OnAfterSpawn()
        {
            base.OnAfterSpawn();
            _producerBehavior?.Start();
        }

        public override void OnDelete()
        {
            _producerBehavior?.Stop();
            base.OnDelete();
        }

        [AfterDeserialization]
        private void AfterDeserialization()
        {
            _producerBehavior?.Start();
        }

        private static bool IsLoom(Item item)
        {
            if (item == null)
            {
                return false;
            }

            if (item is ILoom)
            {
                return true;
            }

            // Check for loom item IDs (adjust these based on your shard's definitions)
            return item.ItemID is 10998 or 10999 or 10994 or 10995 or 11000 or 11001 or 11002 or 11003;
        }

        public override void VendorBuy(Mobile from)
        {
            _producerBehavior?.OnTradeStarted();
            base.VendorBuy(from);
        }

        public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
        {
            var result = base.OnBuyItems(buyer, list);
            _producerBehavior?.OnTradeFinished();
            return result;
        }

        public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
        {
            var result = base.OnSellItems(seller, list);
            _producerBehavior?.OnTradeFinished();
            return result;
        }
    }
}
