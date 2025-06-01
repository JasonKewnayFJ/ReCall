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
            await BoardChecker();
        }



        public async Task BoardChecker ()
        {
            while (true)
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

                    Saver();
                }
            }
        }



        void Saver ()
        {
            var SB = new StoryBoard()
            {
                CurrentBoardNote = this.CurrentBoardNote,
                PreviousBoardNote = this.PreviousBoardNote,
                Stories = NotesList.ToList(),
            };

            string json = JsonSerializer.Serialize(SB, new JsonSerializerOptions { WriteIndented = true });

            


            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = System.IO.Path.Combine(folder, "user.json");

            File.WriteAllText(path, json);
        }

        public void ReUseNote (string selectedNote)
        {
            ClipboardService.SetText(selectedNote);
        }

        protected void OnPropertyChanged ( string prop )
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }
}
