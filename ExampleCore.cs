using ExampleCore.Examples;
using VaultCore.CoreAPI;
using VaultCore.Features;
using VaultCore.ImGuiWindowsAPI;
using VaultCore.Input.Source.Features;
using VaultCore.Rendering;

namespace ExampleCore;

[VaultCoreDescription("Example Core", "Test Core for testing core interfaces", "Special System", "0.0.1")]
[VaultCoreUsesFeature(typeof(ILogging))]
[VaultCoreUsesFeature(
    typeof(ISoftwareRendering),
    typeof(IHighResTimer),
    typeof(ITextureManager),
    typeof(IImGuiMenuManager),
    typeof(IInputReceiver))]
public class ExampleCore : VaultCoreBase
{
    private ILogging _logging = null!;
    private ISoftwareRendering _softwareRenderer = null!;
    private IHighResTimer _highResTimer = null!;
    private IImGuiMenuManager _imGuiMenuManager = null!;
    private IInputReceiver _inputReceiver = null!;

    private readonly List<IExampleItem> _examples = new();

    public override float FixedUpdateRateMs => 1.0f / 60.0f;

    protected override void InitialiseCore()
    {
        _logging = GetFeatureImplementation<ILogging>();
        _softwareRenderer = GetFeatureImplementation<ISoftwareRendering>();
        _highResTimer = GetFeatureImplementation<IHighResTimer>();
        _imGuiMenuManager = GetFeatureImplementation<IImGuiMenuManager>();
        _inputReceiver = GetFeatureImplementation<IInputReceiver>();

        var textureManager = GetFeatureImplementation<ITextureManager>();

        _examples.Add(new SimpleRenderExample(
            _inputReceiver, _softwareRenderer, _logging,
            _imGuiMenuManager, textureManager, _highResTimer));

        _examples.Add(new InputExample(
            _inputReceiver, _softwareRenderer, _logging,
            _imGuiMenuManager));
        
        _examples.Add(new RenderBlendExample(_softwareRenderer, _imGuiMenuManager, textureManager));
        
        _logging.Log("Example Core Initialised");
    }

    public override void Update(float deltaTime)
    {
        foreach (var example in _examples)
        {
            example.Update(deltaTime);
        }
    }


    protected override void ShutDownCore()
    {
        foreach (var example in _examples)
        {
            example.Dispose();
        }

        _examples.Clear();

        _logging.Log("Example Core Shutdown");
    }
}