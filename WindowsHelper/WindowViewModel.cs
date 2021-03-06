using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Serilog;
using WindowsHelper.Annotations;

namespace WindowsHelper
{
    public class WindowVm : INotifyPropertyChanged, IDisposable
    {
        public window Window { get; }

        private readonly Unitor _unitor;
        private ObservableCollection<size> _size;
        private ObservableCollection<position> _position;
        private bool _enabled;

        public bool Enabled
        {
            get => _unitor.Started;
            set
            {
                if (_unitor.Started)
                    _unitor.Stop();
                else
                    _unitor.Run();

                OnPropertyChanged();
            }
        }

        public string Process => Window.process;
        public windowMode Mode => Window.mode;
        public List<condition> Condition => Window.condition;

        public ObservableCollection<size> Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }

        public int SelectedSize
        {
            get => Window.selectedsize;
            set
            {
                Window.selectedsize = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<position> Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }

        public int SelectedPosition
        {
            get => Window.selectedposition;
            set
            {
                Window.selectedposition = value;
                OnPropertyChanged();
            }
        }

        public WindowVm(window window)
        {
            Window = window;
            Size = new ObservableCollection<size>(window.size);
            Size.CollectionChanged += SizeOnCollectionChanged;
            Position = new ObservableCollection<position>(window.position);
            Position.CollectionChanged += PositionOnCollectionChanged;

            _unitor = new Unitor(this);
        }

        private void SizeOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var eNewItem in e.NewItems)
                    {
                        if (eNewItem is size size)
                            Window.size.Add(size);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var eOldItem in e.OldItems)
                    {
                        if (eOldItem is size size)
                            Window.size.Add(size);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void PositionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var eNewItem in e.NewItems)
                    {
                        if (eNewItem is position position)
                            Window.position.Add(position);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var eOldItem in e.OldItems)
                    {
                        if (eOldItem is position position)
                            Window.position.Add(position);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Pause() => _unitor.Pause();

        public void Resume() => _unitor.Resume();

        public void Stop() => _unitor.Stop();

        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _unitor?.Stop();

            Log.Debug("ViewModel {Process} Disposed", Process);
        }
    }
}