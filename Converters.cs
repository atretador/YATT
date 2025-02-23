using Avalonia.Data.Converters;
using Avalonia.Controls;
using System;
using System.Globalization;
using MonoTorrent.Client;

namespace YATT
{
    public class EmptyListVisibilityConverter : IValueConverter
    {
        // Returns true (visible) if there are no torrents (i.e. count == 0)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class InverseEmptyListVisibilityConverter : IValueConverter
    {
        // Returns true (visible) if there are torrents (i.e. count > 0)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PauseButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TorrentState state)
            {
                return state == TorrentState.Downloading || state == TorrentState.Seeding;
            }
            return false;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Returns true when the torrent is Paused.
    public class ResumeButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TorrentState state)
            {
                return state == TorrentState.Paused || state == TorrentState.Stopped;
            }
            return false;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}