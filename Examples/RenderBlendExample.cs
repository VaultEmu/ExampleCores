using System.Reflection;
using System.Text;
using VaultCore.ImGuiWindowsAPI;
using VaultCore.Rendering;

namespace ExampleCore.Examples;

public class RenderBlendExample : IExampleItem
{
    private readonly ISoftwareRendering _softwareRenderer;
    private readonly IImGuiMenuManager _imGuiMenuManager;

    private readonly PixelData _outputScreenBuffer;
    private RenderOutputHandle _outputHandle = RenderOutputHandle.InvalidHandle;

    private readonly ImGuiMenuItem _showInputExampleMenuItem;
    
    private readonly Color32[] _baseBlendTexture;

    private readonly Color32[] _alphaBlendTexture;
    private readonly uint _alphaBlendTextureWidth;
    private readonly uint _alphaBlendTextureHeight;
    
    private readonly BlitFontLarge _blitFont;
    private readonly StringBuilder _stringBuilder = new StringBuilder(1024);

    public RenderBlendExample(
        ISoftwareRendering softwareRenderer,
        IImGuiMenuManager imGuiMenuManager,
        ITextureManager textureManager)
    {
        _softwareRenderer = softwareRenderer;
        _imGuiMenuManager = imGuiMenuManager;

        _outputScreenBuffer = new PixelData(1300, 1100);

        _showInputExampleMenuItem = new ImGuiMenuItem(
            "Examples/Render Blend Test",
            ToggleShowRenderBlendFunctionTest, null, () => _outputHandle != RenderOutputHandle.InvalidHandle);
        
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if(basePath == null)
        {
            throw new InvalidOperationException("Unable to get path that core Dll exists in");
        }
        
        _alphaBlendTexture =  textureManager.LoadTextureFromDiskAsColor32Array(Path.Combine(basePath, @"Assets\AlphaTest.png"),
            out _alphaBlendTextureWidth, out _alphaBlendTextureHeight);
        
        _baseBlendTexture = new Color32[_alphaBlendTextureWidth * _alphaBlendTextureHeight];
        
        _blitFont  = new BlitFontLarge()
        {
            FontScale = 3
        };
        
        for (uint indexY = 0; indexY < _alphaBlendTextureHeight; ++indexY)
        {
            for (uint indexX = 0; indexX < _alphaBlendTextureWidth; ++indexX)
            {
                var r = (byte)(255 * (indexX / (float)_alphaBlendTextureWidth));
                var g = (byte)(255 * (indexY / (float)_alphaBlendTextureHeight));
                
                _baseBlendTexture[indexX + indexY * _alphaBlendTextureWidth] = new Color32
                {
                    R = r,
                    G = g,
                    B = 0,
                    A = 255
                };
            }
        }

        _imGuiMenuManager.RegisterMenuItem(_showInputExampleMenuItem);
    }

    public void Update(float deltaTime)
    {
        if(_outputHandle == RenderOutputHandle.InvalidHandle)
        {
            return;
        }

        _outputScreenBuffer.Clear(new Color32(100, 100, 100));
        
        _stringBuilder.Clear();
        
        _stringBuilder
            .AppendLine("BLEND MODE TESTS")
            .AppendLine()
            .AppendLine("BASE TEXTURE         OTHER TEXTURE")
            .AppendLine("                   COLOR        ALPHA");
        
        _blitFont.DrawText(_outputScreenBuffer, Color32.White, 50, 10, _stringBuilder.ToString());
        
        const int firstLineY = 120;
        
        _outputScreenBuffer.CopyFromColor32Array(_baseBlendTexture, _alphaBlendTextureWidth, _alphaBlendTextureHeight, 
            new Rect(0,0,_alphaBlendTextureWidth, _alphaBlendTextureHeight), 50, firstLineY);

        for (uint indexY = 0; indexY < _alphaBlendTextureHeight; ++indexY)
        {
            for (uint indexX = 0; indexX < _alphaBlendTextureWidth; ++indexX)
            {
                var pixel = _alphaBlendTexture[indexX + indexY * _alphaBlendTextureWidth];
                _outputScreenBuffer.SetPixel(new Color32(
                    pixel.R,
                    pixel.G,
                    pixel.B), 350 + indexX, firstLineY + indexY);
                
                _outputScreenBuffer.SetPixel(new Color32(
                    pixel.A,
                    pixel.A,
                    pixel.A), 575 + indexX, firstLineY + indexY);
            }
        }
        
        DrawSample(50, 400, PixelBlendFactor.One, PixelBlendFactor.One);
        DrawSample(400, 400, PixelBlendFactor.One, PixelBlendFactor.Zero);
        DrawSample(750, 400, PixelBlendFactor.SourceAlpha, PixelBlendFactor.OneMinusSourceAlpha);
        
        DrawSample(50, 750, PixelBlendFactor.DestinationColor, PixelBlendFactor.Zero);
        DrawSample(400, 750, PixelBlendFactor.DestinationColor, PixelBlendFactor.SourceColor);
        DrawSample(750, 750, PixelBlendFactor.OneMinusDestinationColor, PixelBlendFactor.One);
        
        _softwareRenderer.OnFrameReadyToDisplayOnOutput(_outputHandle, _outputScreenBuffer);
    }

    public void Dispose()
    {
        _imGuiMenuManager.UnregisterMenuItem(_showInputExampleMenuItem);

        if(_outputHandle != RenderOutputHandle.InvalidHandle)
        {
            _softwareRenderer.DestroyOutput(_outputHandle);
        }
    }
    
    private void DrawSample(uint x, uint y, PixelBlendFactor sourceBlendFactor, PixelBlendFactor destBlendFactor)
    {
        _stringBuilder.Clear();
        
        _stringBuilder.AppendFormat("SRC : {0}\nDEST: {1}", GetBlendFactorName(sourceBlendFactor), GetBlendFactorName(destBlendFactor));

        _blitFont.DrawText(_outputScreenBuffer, Color32.White, (int)x, (int)y, _stringBuilder.ToString());
        
        y += 75;

        _outputScreenBuffer.CopyFromColor32Array(_baseBlendTexture, _alphaBlendTextureWidth, _alphaBlendTextureHeight, 
            new Rect(0,0,_alphaBlendTextureWidth, _alphaBlendTextureHeight), x, y);

        for (uint indexY = 0; indexY < _alphaBlendTextureHeight; ++indexY)
        {
            for (uint indexX = 0; indexX < _alphaBlendTextureWidth; ++indexX)
            {
                var pixel = _alphaBlendTexture[indexX + indexY * _alphaBlendTextureWidth];
                
                var index = x + indexX + (y + indexY) * _outputScreenBuffer.Width;
                
                _outputScreenBuffer.SetPixelBlended(
                    pixel, index,
                    sourceBlendFactor, destBlendFactor,
                    PixelBlendFactor.One, PixelBlendFactor.One);
            }
        }
    }

    private void ToggleShowRenderBlendFunctionTest()
    {
        if(_outputHandle == RenderOutputHandle.InvalidHandle)
        {
            _outputHandle = _softwareRenderer.CreateOutput("Render Blend Test");
        }
        else
        {
            _softwareRenderer.DestroyOutput(_outputHandle);
            _outputHandle = RenderOutputHandle.InvalidHandle;
        }
    }
    
    private string GetBlendFactorName(PixelBlendFactor blendFactor)
    {
        switch(blendFactor)
        {
            case PixelBlendFactor.Zero:
                return "Zero";
            case PixelBlendFactor.One:
                return "One";
            case PixelBlendFactor.SourceColor:
                return "Src Color";
            case PixelBlendFactor.OneMinusSourceColor:
                return "1 - Src Color";
            case PixelBlendFactor.SourceAlpha:
                return "Src Alpha";
            case PixelBlendFactor.OneMinusSourceAlpha:
                return "1 - Src Alpha";
            case PixelBlendFactor.DestinationColor:
                return "Dest Color";
            case PixelBlendFactor.OneMinusDestinationColor:
                return "1 - Dest Color";
            case PixelBlendFactor.DestinationAlpha:
                return "Dest Alpha";
            case PixelBlendFactor.OneMinusDestinationAlpha:
                return "1 - Dest Alpha";
            default:
                throw new ArgumentOutOfRangeException(nameof(blendFactor), blendFactor, null);
        }
    }
}