using Microsoft.Win32;
using ReCall___.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using TextCopy;

namespace ReCall___.ViewModel
{
    public class BoardManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> NotesList { get; } = new ObservableCollection<string>();
        CancellationTokenSource _cts = new CancellationTokenSource();


        private string _currentBoardNote;
        private string _previousBoardNote;


        public string CurrentBoardNote
        {
            get => _currentBoardNote;
            set
            {
                if (_currentBoardNote != value)
                {
                    _currentBoardNote = value;
                    OnPropertyChanged(nameof(CurrentBoardNote));
                }
            }
        }

        public string PreviousBoardNote
        {
            get => _previousBoardNote;
            set
            {
                if (_previousBoardNote != value)
                {
                    _previousBoardNote = value;
                    OnPropertyChanged(nameof(PreviousBoardNote));
                }
            }
        }


        public async Task Main ()
        {
            CurrentBoardNote = ClipboardService.GetText();
            if (!string.IsNullOrEmpty(CurrentBoardNote))
            {
                if (NotesList.Contains(CurrentBoardNote))
                { return; }
                NotesList.Add(CurrentBoardNote);
            }
            await BoardChecker(_cts.Token);
        }



        public async Task BoardChecker ( CancellationToken token )
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(500);

                var text = ClipboardService.GetText()?.Trim();

                if (!string.IsNullOrEmpty(text) &&
                    text != CurrentBoardNote &&
                    text != PreviousBoardNote &&
                    !NotesList.Contains(text.Trim()))
                {
                    PreviousBoardNote = CurrentBoardNote;
                    CurrentBoardNote = text;
                    NotesList.Insert(0, CurrentBoardNote);
                }
            }
        }


        public void StopChecker ()
        {
            _cts?.Cancel();
        }



        public void ReUseNote (string selectedNote)
        {
            ClipboardService.SetText(selectedNote);
        }

        public void ClearAllNotes ()
        {
            NotesList.Clear();
        }

        protected void OnPropertyChanged ( string prop )
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }
}
