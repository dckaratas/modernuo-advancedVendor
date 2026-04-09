using ModernUO.Serialization;
using System;
using System.Collections.Generic;
using Server;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Mobiles
{
    [SerializationGenerator(0, false)]
    public partial class AdvancedBlacksmith : Blacksmith
    {
        private static readonly string[] _workingPhrases =
                [
                    "I must keep the fire hot while the metal is still malleable.",
                    "One more hammer strike and this will be perfect.",
                    "The bellows sing, now comes the true work.",
                    "I can almost taste the heat of the forge.",
                    "Stay steady, iron. I have no time for sloppiness."
                ];

        private static readonly string[] _restingPhrases =
                [
                    "A brief pause, then back to the hammer.",
                    "Even the strongest arm needs a moment to breathe.",
                    "The iron will wait, but not for long.",
                    "I should not tire now; a short break and then more work.",
                    "The fire is still hot, but the hands are not."
                ];

        private static readonly string[] _tradingPhrases =
                [
                    "Ah, a customer. I will attend to you at once.",
                    "Just a moment while I put the hammer down.",
                    "Step closer and tell me what you need.",
                    "A good smith must serve his patrons before his forge.",
                    "My hands are busy, but your coin is more important now."
                ];

        private ProducerVendorBehavior _producerBehavior;

        [Constructible]
        public AdvancedBlacksmith() : base()
        {
            Title = "the advanced blacksmith";
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsForgeOrAnvil,
                _workingPhrases,
                _restingPhrases,
                _tradingPhrases,
                searchRange: 2
            );
        }

        public AdvancedBlacksmith(Serial serial) : base(serial)
        {
            _producerBehavior = new ProducerVendorBehavior(
                this,
                IsForgeOrAnvil,
                _workingPhrases,
                _restingPhrases,
                _tradingPhrases,
                searchRange: 2
            );
        }

        public override void InitOutfit()
        {
            base.InitOutfit();
            AddItem(new FullApron(Utility.RandomBrightHue()));
            AddItem(new LeatherGloves());
            if (FindItemOnLayer(Layer.Helm) != null)
            {
                RemoveItem(FindItemOnLayer(Layer.Helm));    
            }
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

        private static bool IsForgeOrAnvil(Item item)
        {
            if (item == null)
            {
                return false;
            }

            var type = item.GetType();
            return type.IsDefined(typeof(AnvilAttribute), false)
                || type.IsDefined(typeof(ForgeAttribute), false)
                || item.ItemID is 4015 or 4016 or 11733 or 11734 or 4017 or >= 6522 and <= 6569 or 11736;
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
