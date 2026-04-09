/*
 * PRODUCER VENDOR BEHAVIOR - IMPLEMENTATION GUIDE
 * 
 * This file is intentionally a comment-only template to avoid compilation errors.
 * Copy the examples from AdvancedBlacksmith.cs, AdvancedWeaver.cs, or AdvancedCarpenter.cs
 * and adapt them to your own vendor type.
 * 
 * Use the following valid structure in your own vendor class:
 * 
 * public partial class AdvancedYourVendor : YourBaseVendor
 * {
 *     private ProducerVendorBehavior _producerBehavior;
 *     
 *     [Constructible]
 *     public AdvancedYourVendor() : base()
 *     {
 *         Title = "the advanced [profession]";
 *         _producerBehavior = new ProducerVendorBehavior(
 *             this,
 *             IsMyTool,
 *             _workingPhrases,
 *             _restingPhrases,
 *             _tradingPhrases,
 *             searchRange: 2
 *         );
 *     }
 *
 *     public AdvancedYourVendor(Serial serial) : base(serial)
 *     {
 *         _producerBehavior = new ProducerVendorBehavior(
 *             this,
 *             IsMyTool,
 *             _workingPhrases,
 *             _restingPhrases,
 *             _tradingPhrases,
 *             searchRange: 2
 *         );
 *     }
 *
 *     public override void OnAfterSpawn()
 *     {
 *         base.OnAfterSpawn();
 *         _producerBehavior?.Start();
 *     }
 *
 *     public override void OnDelete()
 *     {
 *         _producerBehavior?.Stop();
 *         base.OnDelete();
 *     }
 *
 *     [AfterDeserialization]
 *     private void AfterDeserialization()
 *     {
 *         _producerBehavior?.Start();
 *     }
 *
 *     public override void VendorBuy(Mobile from)
 *     {
 *         _producerBehavior?.OnTradeStarted();
 *         base.VendorBuy(from);
 *     }
 *
 *     public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
 *     {
 *         var result = base.OnBuyItems(buyer, list);
 *         _producerBehavior?.OnTradeFinished();
 *         return result;
 *     }
 *
 *     public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
 *     {
 *         var result = base.OnSellItems(seller, list);
 *         _producerBehavior?.OnTradeFinished();
 *         return result;
 *     }
 * }
 *
 * Customize the following items in your own class:
 * - Base class: replace TEMPLATE_BaseVendor with an actual existing vendor base class.
 * - Class name: replace TEMPLATE_AdvancedVendor with a real class name.
 * - Tool detection predicate: implement IsMyTool with actual tool logic.
 * - Phrases: use working/resting/trading arrays appropriate for your vendor.
 *
 * This file is intentionally not valid C# code to prevent compilation errors.
 */
