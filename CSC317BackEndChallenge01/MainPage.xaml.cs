using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace CSC317BackEndChallenge01
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Note> notes;

        //We create a "connection" between our data (notes) and
        //the dropdown box.   Essentially, we create a list of
        //the titles of each note and set the list to the
        //dropdown.
        public ObservableCollection<string> PickerViewData 
        {
            get
            {
                ObservableCollection<string> result = new();
               
                foreach (Note note in notes)
                {
                    result.Add(note.Title);
                }

                return result;
            }
        }
        
        //The save location for the XML data will be in the
        //local app directroy specified by the OS.
        public string SaveLocation
        {
            get
            {
                return FileSystem.AppDataDirectory + "\\notedata.xml";
            }
        }

        public MainPage()
        {

            /*notes = new ObservableCollection<Note>
            {
                new Note{Title="Main", Contents="Default Note"},
                new Note{Title="Test2", Contents="New Note 2"},
                new Note{Title="Test3", Contents="New Note 3"},
                new Note{Title="Test4", Contents="New Note 4"}
            };*/
            LoadData();

            //Our back end data must be created/loaded first before
            //initializing the componenet so that the front end
            //doesn't give an error.
            InitializeComponent();

            if (Preferences.ContainsKey("fontsize"))
            {
                slrFontSize.Value = Preferences.Get("fontsize", 15.0);
            }

            //Note: I modified this so that the binding is done on the front-end.
            //      Notice in the XAML I did the folllowing:
            //      1. Give the ContentPage a name (x:Name="page")
            //      2. Set the BindingContext to page.
            //      3. Set the source equal to the PickerViewData property.
            //I now no longer need the following line.
            //pckNoteSelectionBox.ItemsSource = PickerViewData;

            pckNoteSelectionBox.SelectedIndex = 0;
            
        }

        /*Implement in Class....*/
        private void FontChanged(object sender, ValueChangedEventArgs e)
        {
            Preferences.Set("fontsize", slrFontSize.Value);
        }

        //When a new note is selected from the drop down, update the Picker box.
        private void LoadNote(object sender, EventArgs e)
        {
            RebindNote();
        }

        private async void AddNote(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("New Note", "Enter the Note Title");

            if (result != null)
            {
                notes.Add(new Note { Title = result });
                pckNoteSelectionBox.ItemsSource = PickerViewData;
                pckNoteSelectionBox.SelectedIndex = pckNoteSelectionBox.ItemsSource.Count - 1;
                RebindNote();
            }
        } 

        private async void RemoveNote(object sender, EventArgs e)
        {
            //Store the currently shown note.
            int indexRemove = pckNoteSelectionBox.SelectedIndex;
            string noteTitle = pckNoteSelectionBox.ItemsSource[indexRemove] as string;
            
            //Delete the note and update the list.
            notes.RemoveAt(indexRemove);
            pckNoteSelectionBox.ItemsSource = PickerViewData;

            //Rebind the shown note to the first note stored in the list.
            pckNoteSelectionBox.SelectedIndex = 0;
            RebindNote();

            await DisplayAlert("Deleted Note", $"Note \"{noteTitle}\" has been deleted.", "Ok");
        }

        private async void ClearNotes(object sender, EventArgs e)
        {
            int numDelete = notes.Count;
            for(int i = 1; i < numDelete; i++)
            {
                notes.RemoveAt(1);
            }

            pckNoteSelectionBox.ItemsSource = PickerViewData;
            pckNoteSelectionBox.SelectedIndex = 0;
            notes[0].Contents = "";
            RebindNote();

            await DisplayAlert("Cleared Notes", $"Notes have beeen cleared and Main has been reset.", "Ok");
        }

        //Called when the page is changed or when the application exit.
        private void ExitApp(object sender, EventArgs e)
        {
            SaveData();
        }

        //Should be called when the selected note is changed.
        private void RebindNote()
        {
            int index = pckNoteSelectionBox.SelectedIndex;

            if (index == -1)
            {
                etrNoteView.Text = "";
                return;
            }

            etrNoteView.BindingContext = notes[index];
            etrNoteView.SetBinding(Editor.TextProperty, "Contents", BindingMode.TwoWay);
        }

        //Called when the app closes and data needs to be saved.
        private void SaveData()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Note>));
            StreamWriter sw = new StreamWriter(SaveLocation);
            xs.Serialize(sw, notes);
        }
        
        //Called when the ap is opened and loads note data, if it exists.
        //if it doesn't exist, then generates a new note set.
        private void LoadData()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Note>));
                StreamReader streamReader = new StreamReader(SaveLocation);
                notes = (ObservableCollection<Note>)xs.Deserialize(streamReader);
            }
            catch(FileNotFoundException)
            {
                //Create starting data here.
                notes = new ObservableCollection<Note>
                {
                    new Note{Title="Main", Contents="Default Note"}
                };
            }
        }
    }
}
