using ModernUO.Serialization;
using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    /// <summary>
    /// Example vendor NPC using ProducerVendorBehavior with custom workbench detection.
    /// Demonstrates how to reuse the producer behavior for woodworking professions.
    /// </summary>
    [SerializationGenerator(0, false)]
    public partial class AdvancedCarpenter : Carpenter
    {
        private static readonly string[] _workingPhrases =
                [
                    "The wood grain must be respected, worked with, not against.",
                    "Another careful cut and this piece will be flawless.",
                    "The best craftsmen listen to what the wood tells them.",
                    "Measure twice, cut once—that is the carpenter's way.",
                    "This wood will become something both beautiful and strong."
                ];

        private static readonly string[] _restingPhrases =
                [
                    "Even the strongest arms tire from the saw and chisel.",
                    "A moment to sharpen my tools and clear my mind.",
                    "The wood can wait while I catch my breath.",
                    "Fine woodwork demands rest between sessions.",
                    "I've earned a brief respite from the bench."
                ];

        private static readonly string[] _tradingPhrases =
                [
                    "A customer! Step forward and see what I have wrought.",
                    "Just putting away my tools for a moment.",
                    "What manner of craftwork brings you to my workshop?",
                    "A patron of fine woodwork—how delightful.",
                    "Your interest in quality goods is most welcome."
                ];

        private ProducerVendorBehavior _producerBehavior;

        [Constructible]
        public AdvancedCarpenter() : base()
        {
            Title = "the advanced carpenter";
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsWorkbench,
                _workingPhrases,
                _restingPhrases,
                _tradingPhrases,
                searchRange: 2
            );
        }

        public AdvancedCarpenter(Serial serial) : base(serial)
        {
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsWorkbench,
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

        private static bool IsWorkbench(Item item)
        {
            if (item == null)
            {
                return false;
            }

            // If you later want attribute-based detection, define WorkbenchAttribute in DefCarpentry or a shared attribute file.
            // For now, rely on item IDs that represent carpentry saws.
            return item.ItemID is 4148;
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
