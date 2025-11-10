using VaultCore.CoreAPI;
using VaultCore.Features;

[VaultCoreDescription("Example Other Core", "Test Core for testing core interfaces - Testing 2 cores in one dll", "Special System", "0.0.1")]
[VaultCoreUsesFeature(typeof(ILogging))]
public class ExampleOtherCore : VaultCoreBase
{
    private bool _firstUpdateDone;
    private ILogging _logging = null!;
    

    protected override void InitialiseCore()
    {
        _logging = GetFeatureImplementation<ILogging>();
        _logging.Log("Example Other Core Initialised");
    }

    public override void Update(float deltaTime)
    {
        if(_firstUpdateDone == false)
        {
            _logging.Log("Example Other Core First Update Ran");
            _firstUpdateDone = true;
        }
    }

    protected override void ShutDownCore()
    {
        _logging.Log("Example Other Core Shutdown");
    }
}