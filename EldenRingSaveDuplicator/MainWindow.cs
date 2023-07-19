using System.Collections.Generic;
using DirectN;
using Wice.Effects;
using Wice;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics.Eventing.Reader;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using Gameloop.Vdf.Linq;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Windows.Gaming.Input;

namespace EldenwarfareHelper
{
    public class MainWindow : Window
    {
        public static _D3DCOLORVALUE ButtonColor;
        public static _D3DCOLORVALUE ButtonShadowColor;
        public static _D3DCOLORVALUE TextColor;

        private const int _headersMargin = 10;
        private readonly List<SymbolHeader> _headers = new List<SymbolHeader>();


        static MainWindow()
        {
            TextColor = new _D3DCOLORVALUE(0XFFFFFFFF);
        }

        // define Window settings
        public MainWindow()
        {
            // we draw our own titlebar using Wice itself
            WindowsFrameMode = WindowsFrameMode.None;

            // resize to 66% of the screen
            var monitor = Monitor.Primary.Bounds;
            ResizeClient(monitor.Width * 1 / 5, monitor.Height * 1 / 2);

            // the EnableBlurBehind call is necessary when using the Windows' acrylic
            // otherwise the window will be (almost) black
            //Native.EnableBlurBehind();
            RenderBrush = AcrylicBrush.CreateAcrylicBrush(
                CompositionDevice,
                new _D3DCOLORVALUE(0xFF1E1E1E),
                1.0f,
                useWindowsAcrylic: false
                );

            // uncomment this to enable Pointer messages
            //WindowsFunctions.EnableMouseInPointer();
            AddControls();

            AddContent();
        }

        // add basic controls for layout
        private void AddControls()
        {
            // add a Wice titlebar (looks similar to UWP)
            var titleBar = new TitleBar { IsMain = true };
            titleBar.Title.SetSolidColor(TextColor);
            titleBar.Title.SetFontSize(20);
            titleBar.Margin = D2D_RECT_F.Thickness(_headersMargin, _headersMargin, _headersMargin, _headersMargin);
            titleBar.Title.SetFontStretch(DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXTRA_CONDENSED);
            titleBar.MaxButton.Remove();
            Children.Add(titleBar);

            titleBar.MinButton.Path.Shape.StrokeThickness = 2f;
            titleBar.MinButton.Path.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);

            titleBar.CloseButton.Path.Shape.StrokeThickness = 2f;
            titleBar.CloseButton.Path.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
        }

        private void AddContent()
        {
            var stack = new Stack();
            Wice.Image img = new Wice.Image();
            TextBox info1 = new TextBox();
            var stack2 = new Stack();
            TextBox saveExtentionsText = new TextBox();
            TextBox saveExtension = new TextBox();
            TextBox buttonText = new TextBox();
            Button button = new Button();
            TextBox status = new TextBox();

            stack.Orientation = Orientation.Vertical;
            stack.HorizontalAlignment = Alignment.Center;
            stack.VerticalAlignment = Alignment.Center;
            stack.Width = Width;
            stack.Height = Height;

            img.Width = Width / 2;
            img.Height = 300;
            img.Margin = 10;
            img.InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC;
            img.Source = Wice.Application.Current.ResourceManager.GetWicBitmapSource(System.Reflection.Assembly.GetExecutingAssembly(), "EldenRingSaveDuplicator.Assets.grace.png");

            info1.Width = Width / 2;
            info1.Text = "Write your desired extension then click duplicate.";
            info1.SetSolidColor(TextColor);
            info1.SetFontSize(20);
            info1.Height = 50;
            info1.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            info1.VerticalAlignment = Alignment.Center;
            info1.HorizontalAlignment = Alignment.Center;

            stack2.Orientation = Orientation.Horizontal;
            stack2.HorizontalAlignment = Alignment.Center;
            stack2.VerticalAlignment = Alignment.Center;
            stack2.Width = Width;
            stack2.Height = Height;

            saveExtentionsText.Width = Width / 2;
            saveExtentionsText.Text = "ER0000.";
            saveExtentionsText.SetSolidColor(TextColor);
            saveExtentionsText.SetFontSize(36);
            saveExtentionsText.Height = 50;
            saveExtentionsText.Margin = D2D_RECT_F.Thickness(0, 0, 5, 0);
            saveExtentionsText.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            saveExtentionsText.VerticalAlignment = Alignment.Center;
            saveExtentionsText.HorizontalAlignment = Alignment.Center;

            saveExtension.Width = 80;
            saveExtension.Height = 50;
            saveExtension.SetFontSize(36);
            saveExtension.IsEditable = true;
            saveExtension.BackgroundColor = new _D3DCOLORVALUE(0XDDDDDDDD);
            saveExtension.VerticalAlignment = Alignment.Center;
            saveExtension.HorizontalAlignment = Alignment.Center;
            saveExtentionsText.Padding = D2D_RECT_F.Thickness(2, 2, 2, 2);
            saveExtension.Text = "CO2";

            buttonText.Margin = D2D_RECT_F.Thickness(5, 0, 5, 5);
            buttonText.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
            buttonText.Text = "Duplicate";
            buttonText.HorizontalAlignment = Alignment.Center;
            buttonText.VerticalAlignment = Alignment.Center;
            buttonText.FontSize = 20;
            Dock.SetDockType(buttonText, DockType.Top);

            button.Width = 300;
            button.Height = 50;
            button.VerticalAlignment = Alignment.Center;
            button.Margin = D2D_RECT_F.Sized(0, 20, 0, 20);
            button.Child = new Dock();
            button.Child.Margin = D2D_RECT_F.Thickness(10);
            button.Child = buttonText;
            button.DoWhenAttachedToComposition(() =>
            button.RenderBrush = button.Compositor.CreateColorBrush(_D3DCOLORVALUE.LightGray.ChangeAlpha(128)));
            button.Click += (s, e) =>
            {
                status.Text = "Duplicating...";
                Duplicate(saveExtension.Text);
                status.Text = "Done!!";
            };

            status.Width = Width / 2;
            status.Text = "Status: ...";
            status.SetSolidColor(TextColor);
            status.SetFontSize(40);
            status.Height = 50;
            status.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            status.VerticalAlignment = Alignment.Center;
            status.HorizontalAlignment = Alignment.Center;

            stack2.Children.Add(saveExtentionsText);
            stack2.Children.Add(saveExtension);

            stack.Children.Add(img);
            stack.Children.Add(info1);
            stack.Children.Add(stack2);
            stack.Children.Add(button);
            stack.Children.Add(status);

            Children.Add(stack);
        }

        private void Duplicate(string extension)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gamePath = System.IO.Path.Combine(appDataPath, "EldenRing");
            var directories = Directory.EnumerateDirectories(gamePath);
            foreach ( var directory in directories) {
                var saveFile = System.IO.Path.Combine(directory, "ER0000.sl2");
                if (File.Exists(saveFile))
                {
                    var newSaveFile = System.IO.Path.Combine(directory, $"ER0000.{extension}");
                    if (File.Exists(newSaveFile)) File.Copy(newSaveFile, newSaveFile + ".backup", true);
                    File.Copy(saveFile, newSaveFile, true);
                }
            }
        }
    }

}
