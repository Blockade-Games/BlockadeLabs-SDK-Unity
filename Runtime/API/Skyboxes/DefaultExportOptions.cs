using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    public static class DefaultExportOptions
    {
        [Preserve]
        public static SkyboxExportOption Equirectangular_JPG { get; } = new SkyboxExportOption(1, "JPG", SkyboxExportOption.Equirectangular_JPG, false);

        [Preserve]
        public static SkyboxExportOption Equirectangular_PNG { get; } = new SkyboxExportOption(2, "PNG", SkyboxExportOption.Equirectangular_PNG, false);

        [Preserve]
        public static SkyboxExportOption CubeMap_Roblox_PNG { get; } = new SkyboxExportOption(3, "Cube Map - Roblox", SkyboxExportOption.CubeMap_Roblox_PNG, true);

        [Preserve]
        public static SkyboxExportOption HDRI_HDR { get; } = new SkyboxExportOption(4, "HDRI HDR", SkyboxExportOption.HDRI_HDR, true);

        [Preserve]
        public static SkyboxExportOption HDRI_EXR { get; } = new SkyboxExportOption(5, "HDRI EXR", SkyboxExportOption.HDRI_EXR, true);

        [Preserve]
        public static SkyboxExportOption DepthMap_PNG { get; } = new SkyboxExportOption(6, "Depth Map", SkyboxExportOption.DepthMap_PNG, true);

        [Preserve]
        public static SkyboxExportOption Video_LandScape_MP4 { get; } = new SkyboxExportOption(7, "Video Landscape", SkyboxExportOption.Video_LandScape_MP4, true);

        [Preserve]
        public static SkyboxExportOption Video_Portrait_MP4 { get; } = new SkyboxExportOption(8, "Video Portrait", SkyboxExportOption.Video_Portrait_MP4, true);

        [Preserve]
        public static SkyboxExportOption Video_Square_MP4 { get; } = new SkyboxExportOption(9, "Video Square", SkyboxExportOption.Video_Square_MP4, true);

        [Preserve]
        public static SkyboxExportOption CubeMap_PNG { get; } = new SkyboxExportOption(10, "Cube Map - Default", SkyboxExportOption.CubeMap_PNG, true);
    }
}
