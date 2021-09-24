#nullable disable

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Jellyfin.Maui
{
    /// <summary>
    /// The main page.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            CounterLabel.Text = $"Current count: {count}";

            SemanticScreenReader.Announce(CounterLabel.Text);
        }
    }
}
