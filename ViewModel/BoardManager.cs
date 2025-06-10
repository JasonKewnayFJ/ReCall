using Microsoft.Win32;
using ReCall___.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using TextCopy;

namespace ReCall___.ViewModel
{
    public class BoardManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        public ObservableCollection<string> PreviewNoteList { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> NotesList { get; } = new ObservableCollection<string>();
        CancellationTokenSource _cts = new CancellationTokenSource();

        public ICommand Finder { get; }
        public ICommand Refresher { get; }
        public ICommand Cleaner { get; }
        public ICommand CopyCurrent { get; }
        public ICommand CopyPrevious { get; }

        private string _currentBoardNote = string.Empty;
        private string _previousBoardNote = "";


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


        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    FindNote(_searchQuery); 
                }
            }
        }



        public BoardManager ()
        {
            Finder = new RelayCommand<object>(FindNote);
            Refresher = new RelayCommand(_ => RefreshBoard());
            Cleaner = new RelayCommand(_ => ClearAllNotes());
            CopyCurrent = new RelayCommand(_ => CurCop());
            CopyPrevious = new RelayCommand(_ => PrevCop());

            _currentBoardNote = ClipboardService.GetText()?.Trim() ?? string.Empty;
            _previousBoardNote = string.Empty;
            if (!string.IsNullOrEmpty(_currentBoardNote))
            {
                NotesList.Add(_currentBoardNote);
                PreviewNoteList.Add(_currentBoardNote);
            }
        }

        public async Task Main ()
        {
            await BoardChecker(_cts.Token);
        }

        void FindNote (object parameter )  
        {

            if (parameter is string searchText && !string.IsNullOrEmpty(searchText))
            {
                var foundNotes = NotesList.Where(note => note.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList(); 
                PreviewNoteList.Clear();
                foreach (var note in foundNotes)
                {
                    PreviewNoteList.Add(note);
                }
            }

            else if (string.IsNullOrEmpty(parameter as string)) 
            {
                PreviewNoteList.Clear();
                foreach (var note in NotesList)
                {
                    PreviewNoteList.Add(note);
                }
            }

        }
        void RefreshBoard ()
        {
            PreviewNoteList.Clear();
            foreach (var note in NotesList)
            {
                PreviewNoteList.Add(note);
            }
            CurrentBoardNote = ClipboardService.GetText()?.Trim() ?? string.Empty;
            PreviousBoardNote = string.Empty;
        }

        public void ClearAllNotes ()
        {
            PreviewNoteList.Clear();
        }

        void CurCop ()
        {
            if (!string.IsNullOrEmpty(CurrentBoardNote))
            {
                ClipboardService.SetText(CurrentBoardNote);
            }
        }

        void PrevCop ()
        {
            if (!string.IsNullOrEmpty(PreviousBoardNote))
            {
                ClipboardService.SetText(PreviousBoardNote);
            }
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
                    !NotesList.Contains(text))
                {
                    PreviousBoardNote = CurrentBoardNote;
                    CurrentBoardNote = text;
                    NotesList.Insert(0, CurrentBoardNote);
                    PreviewNoteList.Insert(0, CurrentBoardNote);
                }

            }
        }


        public void StopChecker ()
        {
            _cts?.Cancel();
        }


        protected void OnPropertyChanged ( string prop )
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }
}
