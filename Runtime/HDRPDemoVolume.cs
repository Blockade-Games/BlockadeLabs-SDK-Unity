using UnityEngine;
using UnityEngine.Rendering;

namespace BlockadeLabsSDK
{
    [ExecuteAlways]
    internal class HDRPDemoVolume : MonoBehaviour
    {
        [SerializeField]
        private BlockadeLabsSkyboxGenerator _generator;

#if UNITY_HDRP && UNITY_EDITOR
        private const string _defaultVolumeProfileGuid = "0b478194c970ff9418e805599ea5b1c6";

        private void OnEnable()
        {
            var volume = GetComponent<Volume>();

            if (!volume)
            {
                volume = gameObject.AddComponent<Volume>();
                _generator.HDRPVolume = volume;
                volume.sharedProfile = AssetUtils.LoadAsset<VolumeProfile>(_defaultVolumeProfileGuid);
                _generator.HDRPVolumeProfile = volume.sharedProfile;
            }
        }
#endif
    }
}