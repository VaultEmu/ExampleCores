using System.Text;
using Vault.Input;
using VaultCore.Features;
using VaultCore.ImGuiWindowsAPI;
using VaultCore.Input.Source.Features;
using VaultCore.Rendering;

namespace ExampleCore.Examples;

public class InputExample : IExampleItem
{
    private IInputReceiver _inputReceiver;
    private readonly ISoftwareRendering _softwareRenderer;
    private ILogging _logging;
    private readonly IImGuiMenuManager _imGuiMenuManager;

    private readonly PixelData _outputScreenBuffer;
    private RenderOutputHandle _outputHandle = RenderOutputHandle.InvalidHandle;

    private readonly ImGuiMenuItem _showInputExampleMenuItem;

    private readonly BlitFontLarge _blitFont;

    private readonly StringBuilder _stringBuilder = new StringBuilder(1024);

    public InputExample(
        IInputReceiver inputReceiver,
        ISoftwareRendering softwareRenderer,
        ILogging logging,
        IImGuiMenuManager imGuiMenuManager)
    {
        _inputReceiver = inputReceiver;
        _softwareRenderer = softwareRenderer;
        _logging = logging;
        _imGuiMenuManager = imGuiMenuManager;

        _outputScreenBuffer = new PixelData(1024, 768);

        _showInputExampleMenuItem = new ImGuiMenuItem(
            "Examples/Input Test",
            ToggleShowInputTest, null, () => _outputHandle != RenderOutputHandle.InvalidHandle);

        _imGuiMenuManager.RegisterMenuItem(_showInputExampleMenuItem);

        _blitFont = new BlitFontLarge()
        {
            WrapMode = WrapModes.Wrap,
            FontScale = 4
        };
    }

    public void Update(float deltaTime)
    {
        if(_outputHandle == RenderOutputHandle.InvalidHandle)
        {
            return;
        }

        _outputScreenBuffer.Clear(new Color32(100, 100, 100));
        
        _stringBuilder.Clear();
        
        CheckKeyboard();
        CheckMouse();

        _blitFont.DrawText(_outputScreenBuffer, Color32.White, 10, 10, _stringBuilder.ToString());
        _softwareRenderer.OnFrameReadyToDisplayOnOutput(_outputHandle, _outputScreenBuffer);
    }

    private void CheckMouse()
    {
        var mouseDevice = _inputReceiver.MouseDevice;
        
        _stringBuilder
            .AppendLine()
            .Append("Mouse: ");

        if(mouseDevice == null)
        {
            _stringBuilder.AppendLine("None Attached");
        }
        else
        {
            _stringBuilder
                .AppendFormat("{0} ({1})", mouseDevice.DeviceName, mouseDevice.DeviceId._id)
                .AppendLine()
                .AppendFormat("   Mouse X {0:0.00} ", mouseDevice.MouseX.Value)
                .AppendLine()
                .AppendFormat("   Mouse Y {0:0.00} ", mouseDevice.MouseY.Value)
                .AppendLine()
                .AppendFormat("   Mouse Scroll {0:0.00} ", mouseDevice.Scroll.Value)
                .AppendLine();
            
            _stringBuilder.AppendLine("   Mouse Buttons Down:");
            
            foreach(var input in mouseDevice.Inputs)
            {
                if(input is not DigitalInput)
                {
                    continue;
                }
                
                if(input.Down)
                {
                    _stringBuilder
                        .Append("      ")
                        .AppendLine(input.InputName);
                }
            }
            
            if(_softwareRenderer.GetMouseAbsolutePosition(_outputHandle, mouseDevice, out var mousePosOut))
            {
                var mousePixelPosX = (uint)Math.Floor(mousePosOut.X * _outputScreenBuffer.Width);
                var mousePixelPosY = (uint)Math.Floor(mousePosOut.Y * _outputScreenBuffer.Height);
                
                for(uint pixelY = mousePixelPosY - 5; pixelY < mousePixelPosY + 5; ++pixelY)
                {
                    for(uint pixelX = mousePixelPosX - 5; pixelX < mousePixelPosX + 5; ++pixelX)
                    {
                        if(pixelX < _outputScreenBuffer.Width && pixelY < _outputScreenBuffer.Height)
                        {
                            var mouseColor = Color32.Cyan;
                            
                            if(mouseDevice.LeftMouseButton.WasDoubleClicked)
                            {
                                mouseColor = Color32.Magenta;
                            }
                            else if(mouseDevice.LeftMouseButton.Down)
                            {
                                mouseColor = Color32.Yellow;
                            }
                            
                            _outputScreenBuffer.SetPixel(mouseColor, pixelX, pixelY);
                        }
                    }
                }
            }
        }
    }

    private void CheckKeyboard()
    {
        var keyboardDevice = _inputReceiver.KeyboardDevice;

        _stringBuilder
            .AppendLine("INPUT TEST")
            .AppendLine()
            .Append("Keyboard: ");

        if(keyboardDevice == null)
        {
            _stringBuilder.AppendLine("None Attached");
        }
        else
        {
            _stringBuilder
                .AppendFormat("{0} ({1})", keyboardDevice.DeviceName, keyboardDevice.DeviceId._id)
                .AppendLine()
                .Append("   Modifiers Active: ");

            bool firstModifier = true;

            if(keyboardDevice.Modifiers.Ctrl.Down)
            {
                if(firstModifier == false)
                {
                    _stringBuilder.Append(" + ");
                }

                _stringBuilder.Append("Ctrl");
                firstModifier = false;
            }

            if(keyboardDevice.Modifiers.Shift.Down)
            {
                if(firstModifier == false)
                {
                    _stringBuilder.Append(" + ");
                }

                _stringBuilder.Append("Shift");
                firstModifier = false;
            }

            if(keyboardDevice.Modifiers.Alt.Down)
            {
                if(firstModifier == false)
                {
                    _stringBuilder.Append(" + ");
                }

                _stringBuilder.Append("Alt");
            }

            _stringBuilder.AppendLine();
            _stringBuilder.AppendLine("   Keys Down:");

            foreach (var input in keyboardDevice.Inputs)
            {
                if(input == null)
                {
                    continue;
                }

                if(input.Down)
                {
                    _stringBuilder
                        .Append("      ")
                        .AppendLine(input.InputName);
                }
            }
        }
    }

    public void Dispose()
    {
        _imGuiMenuManager.UnregisterMenuItem(_showInputExampleMenuItem);

        if(_outputHandle != RenderOutputHandle.InvalidHandle)
        {
            _softwareRenderer.DestroyOutput(_outputHandle);
        }
    }

    private void ToggleShowInputTest()
    {
        if(_outputHandle == RenderOutputHandle.InvalidHandle)
        {
            _outputHandle = _softwareRenderer.CreateOutput("Input Test");
        }
        else
        {
            _softwareRenderer.DestroyOutput(_outputHandle);
            _outputHandle = RenderOutputHandle.InvalidHandle;
        }
    }
}