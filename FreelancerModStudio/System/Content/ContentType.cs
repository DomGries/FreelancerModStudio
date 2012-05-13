namespace FreelancerModStudio.SystemPresenter.Content
{
    public enum ContentType
    {
        None,
        
        // [System]
        System,

        // [LightSource]
        LightSource,

        // [Object]
        Construct,
        Depot,
        DockingRing,
        JumpGate,
        JumpHole,
        Planet,
        Satellite,
        Ship,
        Station,
        Sun,
        TradeLane,
        WeaponsPlatform,

        // [Zone]
        ZoneSphere,
        ZoneEllipsoid,
        ZoneCylinderOrRing,
        ZoneBox,

        ZoneSphereExclusion,
        ZoneEllipsoidExclusion,
        ZoneCylinderOrRingExclusion,
        ZoneBoxExclusion,

        ZoneVignette,
        ZonePath,
        ZonePathTrade,
        ZonePathTradeLane
    }
}