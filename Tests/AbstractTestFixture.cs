namespace BlockadeLabsSDK.Tests
{
    internal abstract class AbstractTestFixture
    {
        protected readonly BlockadeLabsClient BlockadeLabsClient;

        protected AbstractTestFixture()
        {
            var auth = new BlockadeLabsAuthentication().LoadDefaultsReversed();
            var settings = new BlockadeLabsSettings();
            BlockadeLabsClient = new BlockadeLabsClient(auth, settings)
            {
                EnableDebug = true
            };
        }
    }
}
