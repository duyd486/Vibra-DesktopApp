using System;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Singleton
{
    public sealed class SidebarSelectionBus
    {
        private static readonly Lazy<SidebarSelectionBus> _instance = new(() => new SidebarSelectionBus());
        public static SidebarSelectionBus GetInstance() => _instance.Value;

        private SidebarSelectionBus() { }

        public event EventHandler<SidebarSelectionChangedEventArgs>? SelectionChanged;

        public void Publish(NavigationItem navigationItem, int? id)
        {
            SelectionChanged?.Invoke(this, new SidebarSelectionChangedEventArgs(navigationItem, id));
        }
    }

    public sealed class SidebarSelectionChangedEventArgs : EventArgs
    {
        public NavigationItem NavigationItem { get; }
        public int? Id { get; }

        public SidebarSelectionChangedEventArgs(NavigationItem navigationItem, int? id)
        {
            NavigationItem = navigationItem;
            Id = id;
        }
    }
}
