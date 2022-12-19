using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Editor
{
    public partial class Editor : Form
    {
        private const string EDITORNAME = " - Editor";
        private const string NAMELESS = "Namnlös";
        private string fileName = NAMELESS;
        private string fileExtension = "";
        private bool textChanged = false;
        private bool oppnadViaDragAndDrop = false;
        private string allText = ""; //Sparar richTextBox.Text här, för att kunna spara när Form är stängd
        //Hittar inte möjligheten att fånga en eventhandler innan Form stängs så sparar text i allText

        public Editor()
        {
            InitializeComponent();

            // Sets the control to allow drops, and then adds the necessary event handlers.
            richTextBox.AllowDrop = true;
            richTextBox.DragDrop += RichTextBox_DragDrop;

            // Handle the ApplicationExit event to know when the application is exiting.
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            this.Text = fileName + EDITORNAME;
        }

        //Om exit med X
        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (textChanged)
            {
                var result = MessageBox.Show(
                    $"Vill du spara ändringarna för {fileName}",
                    "Editor Spara?", MessageBoxButtons.YesNo
                    );
                if (result == DialogResult.Yes)
                {
                    //Save file
                    SparaMedEllerUtanDialog();
                }
            }
            Application.Exit();
        }

        //File - New
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SparaEller())
            {
                richTextBox.Clear();
                fileName = NAMELESS;
                this.Text = fileName + EDITORNAME;
                textChanged = false;
            }
            UpdateStatusBar();
        }

        /// <summary>
        /// Sparar fil med en dialogruta
        /// </summary>
        private void SparaFilMedDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Textfil|*.txt|Alla filer|*.*";
            var saveFileResult = saveFileDialog.ShowDialog();
            if (saveFileResult == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, allText);
                fileName = Path.GetFileName(saveFileDialog.FileName);
                fileExtension = Path.GetExtension(saveFileDialog.FileName);
                textChanged = false;
                this.Text = fileName + EDITORNAME;
            }
        }

        /// <summary>
        /// Spara fil utan dialog
        /// </summary>
        private void SparaFilUtanDialog()
        {
            File.WriteAllText(fileExtension, allText);
            textChanged = false;
            this.Text = fileName + EDITORNAME;
        }

        /// <summary>
        /// Sparar fil med eller utan dialog
        /// </summary>
        private void SparaMedEllerUtanDialog()
        {
            if (textChanged)
            {
                if (fileName == NAMELESS)
                {
                    SparaFilMedDialog();
                }
                else
                {
                    SparaFilUtanDialog();
                }
            }
        }

        /// <summary>
        /// Kontrollerar om filen skall sparas med eller utan dialog
        /// och sparar filen med eller utan dialog.
        /// </summary>
        /// <returns>false - om åtgärd skall avbrytas</returns>
        private bool SparaEller()
        {
            if (textChanged)
            {
                var result = MessageBox.Show(
                            $"Vill du spara ändringarna för {fileName}",
                            "Editor Spara?", MessageBoxButtons.YesNoCancel
                             );
                if (result == DialogResult.Yes)
                {
                    //Save file
                    if (fileName == NAMELESS)
                    {
                        SparaFilMedDialog();
                    }
                    else
                    {
                        SparaFilUtanDialog();
                    }
                    return true;
                }
                else if (result == DialogResult.No)
                {
                    //Don't save 
                    return true;
                }
                else
                {
                    //Don't do anything!
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Räknar antalet ord i str
        /// </summary>
        /// <param name="str"> sträng</param>
        /// <returns>Antal ord</returns>
        private int WordCount(string str)
        {
            int count = 0;
            string pattern = "[^\\w]";
            string[] words = null;
            words = Regex.Split(str, pattern, RegexOptions.IgnoreCase);
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Räknar antalet förekomna t i str
        /// </summary>
        /// <param name="str">Sträng</param>
        /// <param name="t">Tecken</param>
        /// <returns>Antalet t i str</returns>
        private int CharCount(string str, char t)
        {
            int count = 0;
            foreach (char c in str)
            {
                if (c.Equals(t))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Updaterar statusbaren
        /// </summary>
        private void UpdateStatusBar()
        {
            int nrOfChars = richTextBox.TextLength - richTextBox.Text.Count(char.IsControl);
            toolStripStatusLabelTecken.Text = nrOfChars.ToString();
            toolStripStatusLabelOrd.Text = WordCount(richTextBox.Text).ToString();
            toolStripStatusLabelRader.Text = (CharCount(richTextBox.Text, '\n') + 1).ToString();
            toolStripStatusLabelTeckenUM.Text = (nrOfChars - CharCount(richTextBox.Text, ' ')).ToString();
        }

        //Om texten ändras i textboxen
        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            if (oppnadViaDragAndDrop) //När fil öppnas via D&D
            {
                oppnadViaDragAndDrop = false;
            }
            else
            {
                textChanged = true;
                this.Text = "*" + fileName + EDITORNAME; //Ändring av fil markeras med * framför filnamn
            }

            //Spara för stängning med X (Finns det bättre sätt?)
            allText = richTextBox.Text;
            UpdateStatusBar();
        }

        //File - Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Spara bara om texten är ändrad
            SparaMedEllerUtanDialog();
        }

        //File - Save As
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SparaFilMedDialog();
        }

        //Edit - Open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SparaEller())
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Textfil|*.txt|Alla filer|*.*";
                var openFileResult = openFileDialog.ShowDialog();
                if (openFileResult == DialogResult.OK)
                {
                    richTextBox.Text = File.ReadAllText(openFileDialog.FileName);
                    fileName = openFileDialog.SafeFileName;
                    fileExtension = openFileDialog.FileName;
                    textChanged = false;
                    this.Text = fileName + EDITORNAME;
                }
            }
            UpdateStatusBar();
        }

        //File - Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SparaEller())
            {
                //Exit
                Application.Exit();
            }
        }

        private void exit()
        {
            if (SparaEller())
            {
                //Exit
                Application.Exit();
            }
        }

        /// <summary>
        /// Hanterar D&D event
        /// Kan lägga till text - läggs till där det släpps
        /// Hanterar filer - ctrl - shift - none
        /// Fungerar bara som fristående program, inte när jag kör debug is VS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_DragDrop(object sender, DragEventArgs e)
        {
            // Paste the text into the RichTextBox where at selection location.
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                richTextBox.SelectedText = e.Data.GetData("System.String", true).ToString();
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data != null)
                {
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    var filnamn = data as string[];
                    if (filnamn.Length > 0)
                    {
                        if ((e.KeyState & 4) == 4) // shift
                        {
                            richTextBox.SelectedText = File.ReadAllText(filnamn[0]);
                        }
                        else if ((e.KeyState & 8) == 8) //ctrl
                        {
                            richTextBox.Text += File.ReadAllText(filnamn[0]);
                        }
                        else if ((e.KeyState & 0) == 0) //none
                        {
                            if (SparaEller())
                            {
                                richTextBox.Text = File.ReadAllText(filnamn[0]);
                                fileName = Path.GetFileName(filnamn[0]);
                                textChanged = false;
                                this.Text = fileName + EDITORNAME;
                                oppnadViaDragAndDrop = true;
                            }
                        }

                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Editor\nA program by Ola P", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
