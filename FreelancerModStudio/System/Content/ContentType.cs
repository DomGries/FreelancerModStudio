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
        ZoneCylinder,
        ZoneRing,
        ZoneBox,

        ZoneSphereExclusion,
        ZoneEllipsoidExclusion,
        ZoneCylinderExclusion,
        ZoneBoxExclusion,

        ZoneVignette,
        ZonePath,
        ZonePathTrade,
        ZonePathTradeLane,

        // special types
        ModelPreview,
    }
}
