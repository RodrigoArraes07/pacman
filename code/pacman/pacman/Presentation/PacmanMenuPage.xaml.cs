using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace pacman.Presentation;
/// <summary>
/// This code is used to manipulate page.
/// </summary>
public sealed partial class PacmanMenuPage : Page
{
    public PacmanMenuPage()
    {
        this.InitializeComponent();
        Console.WriteLine(DataContext == null ? "❌ DataContext NULL" : $"✅ DataContext OK: {DataContext}");
    }
}

