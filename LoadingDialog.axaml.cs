using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YATT.Services;

namespace YATT
{
    public partial class LoadingDialog : Window
    {
        public LoadingDialog(Window owner)
        {
            InitializeComponent();
            this.Owner = owner; // Set the owner of the window (optional, if needed)
        }
    }
}