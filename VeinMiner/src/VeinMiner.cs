using Vintagestory.API.Common;

public class VeinMinerMod : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass(
            "VeinMinerPickaxeBehavior",
            typeof(VeinMinerPickaxeBehavior)
        );
    }
}
