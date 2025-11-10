using System.Reflection;
using VaultCore.Features;
using VaultCore.ImGuiWindowsAPI;
using VaultCore.Input.Source.Features;
using VaultCore.Rendering;

namespace ExampleCore.Examples;

public class SimpleRenderExample : IExampleItem
{
    private readonly IInputReceiver _inputReceiver;
    private readonly ISoftwareRendering _softwareRenderer;
    private ILogging _logging;
    private readonly IHighResTimer _highResTimer;
    private readonly IImGuiMenuManager _imGuiMenuManager;

    private readonly PixelData _outputAScreenBuffer = null!;
    private readonly PixelData _outputBScreenBuffer = null!;

    private readonly Color32[] _splatPixelsTestA = new Color32[320 * 64];
    private readonly Color32[] _splatPixelsTestB = new Color32[80 * 80];
    private readonly Color32[] _splatPixelsTestLoadedFromFile = null!;
    private readonly uint _splatPixelsTestLoadedFromFileWidth;
    private readonly uint _splatPixelsTestLoadedFromFileHeight;

    private RenderOutputHandle _outputA = RenderOutputHandle.InvalidHandle;
    private RenderOutputHandle _outputB = RenderOutputHandle.InvalidHandle;

    private bool _renderingOutputB = true;

    private readonly ImGuiMenuItem _showOutputAMenuItem;
    private readonly ImGuiMenuItem _showOutputBMenuItem;
    private readonly ImGuiMenuItem _OutputBRenderMenuItem;

    public SimpleRenderExample(
        IInputReceiver inputReceiver,
        ISoftwareRendering softwareRenderer,
        ILogging logging,
        IImGuiMenuManager imGuiMenuManager,
        ITextureManager textureManager, IHighResTimer highResTimer)
    {
        _inputReceiver = inputReceiver;
        _softwareRenderer = softwareRenderer;
        _logging = logging;
        _imGuiMenuManager = imGuiMenuManager;
        _highResTimer = highResTimer;

        _outputAScreenBuffer = new PixelData(320, 224);

        _outputBScreenBuffer = new PixelData(128, 128);

        for (var index = 0; index < _splatPixelsTestA.Length; ++index)
        {
            _splatPixelsTestA[index] = new Color32(0, 255, 255);
        }

        for (var index = 0; index < _splatPixelsTestB.Length; ++index)
        {
            _splatPixelsTestB[index] = new Color32(255, 0, 255);
        }

        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if(basePath == null)
        {
            throw new InvalidOperationException("Unable to get path that core Dll exists in");
        }

        _splatPixelsTestLoadedFromFile = textureManager.LoadTextureFromDiskAsColor32Array(Path.Combine(basePath, @"Assets\Debug.png"),
            out _splatPixelsTestLoadedFromFileWidth, out _splatPixelsTestLoadedFromFileHeight);

        _outputAScreenBuffer.Clear(new Color32(255, 255, 0));
        _outputBScreenBuffer.Clear(new Color32(0, 255, 255));

        
        
        _showOutputAMenuItem = new ImGuiMenuItem(
            "Examples/Simple Render/Show Simple Render Output",
            ToggleShowOutputA, null, () => _outputA != RenderOutputHandle.InvalidHandle, -1000);

        _showOutputBMenuItem = new ImGuiMenuItem(
            "Examples/Simple Render/Show Second Render Output",
            ToggleShowOutputB, null, () => _outputB != RenderOutputHandle.InvalidHandle);

        _OutputBRenderMenuItem = new ImGuiMenuItem(
            "Examples/Simple Render/Render To Second Output",
            ToggleOutputBRendering, null, () => _renderingOutputB, 10);

        _imGuiMenuManager.RegisterMenuItem(_showOutputAMenuItem);
        _imGuiMenuManager.RegisterMenuItem(_showOutputBMenuItem);
        _imGuiMenuManager.RegisterMenuItem(_OutputBRenderMenuItem);
    }

    public void Update(float deltaTime)
    {
        if(_inputReceiver.KeyboardDevice != null)
        {
            if(_inputReceiver.KeyboardDevice.Keys.R.WasPressed)
            {
                ToggleOutputBRendering();
            }

            if(_inputReceiver.KeyboardDevice.Keys.T.WasPressed)
            {
                ToggleShowOutputB();
            }
        }

        if(_outputA != RenderOutputHandle.InvalidHandle)
        {
            var timeSinceStartup = _highResTimer.TimeSinceStartup;

            for (uint indexY = 16; indexY < _outputAScreenBuffer.Height - 16; ++indexY)
            {
                for (uint indexX = 16; indexX < _outputAScreenBuffer.Width - 16; ++indexX)
                {
                    var offsetR = timeSinceStartup * 0.1f % 1.0f;
                    var offsetG = timeSinceStartup * 0.2f % 1.0f;

                    var r = (byte)(255 * (indexX / (float)_outputAScreenBuffer.Width - offsetR));
                    var g = (byte)(255 * (indexY / (float)_outputAScreenBuffer.Height - offsetG));

                    _outputAScreenBuffer.SetPixel(new Color32
                    {
                        R = r,
                        G = g,
                        B = 0,
                        A = 255
                    }, indexX, indexY);
                }
            }

            _outputAScreenBuffer.CopyFromColor32Array(_splatPixelsTestA, 320, 64, new Rect(0, 0, 320, 64), 0, 20);
            _outputAScreenBuffer.CopyFromColor32Array(_splatPixelsTestB, 80, 80, new Rect(0, 0, 80, 80), 100, 100);
            _outputAScreenBuffer.CopyFromColor32Array(_splatPixelsTestLoadedFromFile, _splatPixelsTestLoadedFromFileWidth, _splatPixelsTestLoadedFromFileHeight,
                new Rect(0, _splatPixelsTestLoadedFromFileHeight / 2, _splatPixelsTestLoadedFromFileWidth, _splatPixelsTestLoadedFromFileHeight / 2),
                10, _outputAScreenBuffer.Height - (_splatPixelsTestLoadedFromFileHeight + 10));

            _softwareRenderer.OnFrameReadyToDisplayOnOutput(_outputA, _outputAScreenBuffer);
        }

        if(_renderingOutputB && _outputB != RenderOutputHandle.InvalidHandle)
        {
            for (uint indexY = 0; indexY < _outputBScreenBuffer.Height; ++indexY)
            {
                for (uint indexX = 0; indexX < _outputBScreenBuffer.Width; ++indexX)
                {
                    var r = (byte)(255 * (indexX / (float)_outputBScreenBuffer.Width));
                    var b = (byte)(255 * (indexY / (float)_outputBScreenBuffer.Height));

                    _outputBScreenBuffer.SetPixel(new Color32
                    {
                        R = r,
                        G = 0,
                        B = b,
                        A = 255
                    }, indexX, indexY);
                }
            }

            _outputBScreenBuffer.CopyFromColor32Array(_splatPixelsTestA, 320, 64, new Rect(0, 0, _outputBScreenBuffer.Width, 64), 0, 20);
            _outputBScreenBuffer.CopyFromColor32Array(_splatPixelsTestB, 80, 80, new Rect(0, 0, 10, 10), 100, 100);
            _outputBScreenBuffer.CopyFromColor32Array(_splatPixelsTestLoadedFromFile, _splatPixelsTestLoadedFromFileWidth, _splatPixelsTestLoadedFromFileHeight,
                new Rect(0, _splatPixelsTestLoadedFromFileHeight / 2, _splatPixelsTestLoadedFromFileWidth, _splatPixelsTestLoadedFromFileHeight / 2),
                10, _outputBScreenBuffer.Height - (_splatPixelsTestLoadedFromFileHeight + 10));

            _softwareRenderer.OnFrameReadyToDisplayOnOutput(_outputB, _outputBScreenBuffer);
        }
    }

    public void Dispose()
    {
        _imGuiMenuManager.UnregisterMenuItem(_showOutputAMenuItem);
        _imGuiMenuManager.UnregisterMenuItem(_showOutputBMenuItem);
        _imGuiMenuManager.UnregisterMenuItem(_OutputBRenderMenuItem);

        if(_outputA != RenderOutputHandle.InvalidHandle)
        {
            _softwareRenderer.DestroyOutput(_outputA);
        }

        if(_outputB != RenderOutputHandle.InvalidHandle)
        {
            _softwareRenderer.DestroyOutput(_outputB);
        }
    }
    
    private void ToggleShowOutputA()
    {
        if(_outputA == RenderOutputHandle.InvalidHandle)
        {
            _outputA = _softwareRenderer.CreateOutput("Output A");
        }
        else
        {
            _softwareRenderer.DestroyOutput(_outputA);
            _outputA = RenderOutputHandle.InvalidHandle;
        }
    }

    private void ToggleShowOutputB()
    {
        if(_outputB == RenderOutputHandle.InvalidHandle)
        {
            _outputB = _softwareRenderer.CreateOutput("Output B");
        }
        else
        {
            _softwareRenderer.DestroyOutput(_outputB);
            _outputB = RenderOutputHandle.InvalidHandle;
        }
    }

    private void ToggleOutputBRendering()
    {
        if(_renderingOutputB)
        {
            _renderingOutputB = false;
            if(_outputB != RenderOutputHandle.InvalidHandle)
            {
                _softwareRenderer.ResetOutput(_outputB);
            }
        }
        else
        {
            _renderingOutputB = true;
        }
    }
}