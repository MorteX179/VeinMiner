using Vintagestory.API.Common;

public class VeinMinerModSystem : ModSystem
{
    public static VeinMinerConfig Config;

    public override void Start(ICoreAPI api)
    {
        Config = api.LoadModConfig<VeinMinerConfig>("veinminer.json");

        if (Config == null)
        {
            Config = new VeinMinerConfig();
            api.StoreModConfig(Config, "veinminer.json");
        }
    }
}
