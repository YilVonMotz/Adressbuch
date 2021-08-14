using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Configuration;
using System.Data;


namespace AdressbuchWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SQLiteConnection connection;
        private SQLiteDataAdapter dataAdapter;
        private DataTable dataTable;
        private SQLiteDataReader reader;

        private List<string> mitarbeiterColumnNames = new List<string>();
        private List<string> organisationColumnNames = new List<string>();

        private List<InputBox> inputBoxes = new List<InputBox>();
        
        private Dictionary<string, string> datagridRowDict = new Dictionary<string, string>();

        public string SelectedTable { get { return selectedTable; } }
        private string selectedTable;

        private SQLiteCommand com_GetMitarbeiterFields;
        private string com_GetMitarbeiterFieldsString = "select name from pragma_table_info('Mitarbeiter')";

        private SQLiteCommand com_GetOrganisationFields;
        private string com_GetOrganisationFieldsString = "select name from pragma_table_info('Organisation')";

        private SQLiteCommand com_GetAllMitarbeiter;        

        private SQLiteCommand com_GetAllOrganisation;

        private StringBuilder sqlQuerySB = new StringBuilder();

        private SQLiteCommand com_select;
        private SQLiteCommand com_insert;
        private SQLiteCommand com_delete;
        private SQLiteCommand com_update;
        
        

        




        public MainWindow()
        {

            
            InitializeComponent();

            try
            {
                //connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection = new SQLiteConnection("Data Source = Adressbuch.db; Version = 3");
                connection.Open();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            Console.WriteLine(connection.State.ToString());
            InitializeCommands();
            List<string> comboBoxContent = new List<string>();
            comboBoxContent.Add("Mitarbeiter");
            comboBoxContent.Add("Organisation");
            comboBox1.ItemsSource = comboBoxContent;

            Console.WriteLine("UIAenderung");            

        }



        private void InitializeCommands()
        {           

            try
            {
                com_GetMitarbeiterFields = connection.CreateCommand();
                com_GetOrganisationFields = connection.CreateCommand();
                com_GetAllMitarbeiter = connection.CreateCommand();
                com_GetAllOrganisation = connection.CreateCommand();
                com_update = connection.CreateCommand();
                com_select = connection.CreateCommand();
                com_insert = connection.CreateCommand();
                com_delete = connection.CreateCommand();
                com_GetMitarbeiterFields.CommandText = com_GetMitarbeiterFieldsString;
                com_GetOrganisationFields.CommandText = com_GetOrganisationFieldsString;
                
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            
        }


        private void FillDataGrid(DataGrid dataGrid, SQLiteCommand command)
        {
            try
            {
                dataAdapter = new SQLiteDataAdapter(command.CommandText, connection);
                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }  
           
        }


        private List<string> GetDataGridRowValues(DataGrid grid)
        {
            List<string> result = new List<string>();

            Console.WriteLine(grid.CurrentItem.ToString());
            return result;
                
        }




        internal int GetDBEntriesCount(string tableName, string columnName ,string searchPhrase)
        {
            SQLiteCommand com = null;
            try
            {
                com = connection.CreateCommand();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            com.CommandText = "select * from "+tableName+" where "+columnName+" like '"+searchPhrase+"'";


            int stepCount = 0;        
            
            try
            {
                reader = com.ExecuteReader();
                while (reader.Read()) ;
                stepCount = reader.StepCount;
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            return stepCount;            
        }



        private void InitiateInputBoxes()
        {
            if (selectedTable == "Mitarbeiter")
            {
                for(int i = 0; i < mitarbeiterColumnNames.Count; i++)
                {
                    InputBox inputBox = new InputBox();
                    inputBox.SetLabel(mitarbeiterColumnNames[i]);                    
                    StackPanel_InputBoxes.Children.Add(inputBox);
                    inputBoxes.Add(inputBox);
                }
            }
            else
            {
                for (int i = 0; i < organisationColumnNames.Count; i++)
                {
                    InputBox inputBox = new InputBox();
                    inputBox.SetLabel(organisationColumnNames[i]);
                    StackPanel_InputBoxes.Children.Add(inputBox);
                    inputBoxes.Add(inputBox);
                }
            }
        }


        private void TableSelect_Dropdown(object sender, EventArgs e)
        {            

            if(comboBox1.SelectedItem == null)
            {
                return;
            }

            selectedTable = comboBox1.SelectedItem.ToString();

            StackPanel_InputBoxes.Children.Clear();
            inputBoxes.Clear();
            

            switch (selectedTable)
            {
                case "Mitarbeiter":

                    com_GetAllMitarbeiter.CommandText = "select * from " + selectedTable;
                    try
                    {
                        reader = com_GetMitarbeiterFields.ExecuteReader();
                    }
                    catch(SQLiteException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    FillDataGrid(dataGridOutput,com_GetAllMitarbeiter);
                   
                   
                    break;
                case "Organisation":

                    com_GetAllOrganisation.CommandText = "select * from " + selectedTable;
                    try
                    {
                        reader = com_GetOrganisationFields.ExecuteReader();
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    FillDataGrid(dataGridOutput,com_GetAllOrganisation);
                    
                    break;
                default:
                    break;
            }

            mitarbeiterColumnNames.Clear();
            organisationColumnNames.Clear();

            while (reader.Read())
            {
                string value = reader.GetString(0);

                if (selectedTable == "Mitarbeiter")
                {
                    mitarbeiterColumnNames.Add(value);
                }
                else
                {
                    organisationColumnNames.Add(value);
                }

            }

            InitiateInputBoxes();
            try
            {
                reader.Close();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);                
            }

        }






        private string BuildSelectString()
        {
            StringBuilder sb = new StringBuilder("select * from " + selectedTable+" where ");
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach(InputBox item in inputBoxes)
            {
                if (item.TextChanged)
                {
                    keyValuePairs.Add(item.GetLabel(), item.GetInputText());
                }
            }

            
            for(int i = 0; i < keyValuePairs.Count; i++)
            {
                if (GetDBEntriesCount(selectedTable,keyValuePairs.ElementAt(i).Key,keyValuePairs.ElementAt(i).Value) == 0){
                    if (i == 0)
                    {
                        sb.Append(keyValuePairs.ElementAt(i).Key + " like '" + keyValuePairs.ElementAt(i).Value + "%' ");
                    }
                    else
                    {
                        sb.Append("and " + keyValuePairs.ElementAt(i).Key + " like '" + keyValuePairs.ElementAt(i).Value + "%'");
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        sb.Append(keyValuePairs.ElementAt(i).Key + " like '" + keyValuePairs.ElementAt(i).Value + "' ");
                    }
                    else
                    {
                        sb.Append("and " + keyValuePairs.ElementAt(i).Key + " like '" + keyValuePairs.ElementAt(i).Value + "'");
                    }
                }
                
            }


            return sb.ToString();
        }
        


        private void BuildInsertString()
        {
            if (comboBox1.SelectedItem == null)
            {
                return;
            }
            StringBuilder insertString = new StringBuilder("Insert into " + selectedTable);
            StringBuilder values = new StringBuilder();
            StringBuilder labels = new StringBuilder();
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach(InputBox item in inputBoxes)
            {
                if(item.GetInputText() != string.Empty)
                {
                    keyValuePairs.Add(item.GetLabel(), item.GetInputText());
                }
            }


            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                
                if (i > 0)
                {
                    values.Append(",");
                    values.Append("'" + keyValuePairs.ElementAt(i).Value+ "'");

                    labels.Append(",");
                    labels.Append("'" + keyValuePairs.ElementAt(i).Key + "'");

                }
                else
                {
                    values.Append("'" + keyValuePairs.ElementAt(i).Value + "'");
                    labels.Append("'" + keyValuePairs.ElementAt(i).Key + "'");
                }

            }
           

            insertString.Append("("+labels.ToString()+") VALUES (" + values.ToString() + ")");
            com_insert.CommandText = insertString.ToString();

        }

       

        private void DeleteEntry(string pKvalue)
        {
            StringBuilder deleteEntryString = new StringBuilder();
            if (selectedTable == "Mitarbeiter")
            {
                deleteEntryString.Append("delete from Mitarbeiter where ID = ' "+pKvalue+" '");
            }
            else
            {
                deleteEntryString.Append("delete from Organisation where Name = ' " + pKvalue + " '");
            }
            
            com_delete.CommandText = deleteEntryString.ToString();

            try
            {
                com_delete.ExecuteNonQuery();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }            
            
        }





        private void ModifyEntry(Dictionary<string, string> rowElements,string primaryKey)
        {            
            StringBuilder stringBuilder;
            string tableName = comboBox1.SelectedItem.ToString();
            string pKeyValue = primaryKey;
            
            
            stringBuilder = new StringBuilder("Update "+tableName+" SET ");

            for(int i = 0; i < rowElements.Count; i++)
            {
                if (i < rowElements.Count - 1)
                {
                    stringBuilder.Append( rowElements.Keys.ElementAt(i)+ "='" + rowElements.Values.ElementAt(i) + "',");
                }
                else
                {
                    stringBuilder.Append(rowElements.Keys.ElementAt(i) + "='" + rowElements.Values.ElementAt(i) + "'");
                }
            }



            if (tableName == "Mitarbeiter")
            {
                stringBuilder.Append(" where ID='" + pKeyValue+"'");
            }
            else
            {
                stringBuilder.Append(" where name=" + pKeyValue+"'");
            }          
            

            try
            {
                com_update.CommandText = stringBuilder.ToString();
                com_update.ExecuteNonQuery();
                
            }catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Bitte erst Suchkontext auswählen");
                hilfeBox.Text = "Tabelle auswählen, Suchwerte eingeben dann auf -SUCHEN- klicken. Groß und -Klein Schreibung beachten";
            }
            else
            {
                com_select.CommandText = BuildSelectString();
                FillDataGrid(dataGridOutput, com_select);
            }
                
                 
        }



        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Bitte erst Suchkontext auswählen");
                hilfeBox.Text = "Tabelle wählen, die zu füllenden Felder auswählen dann auf -EINTRAG EINFÜGEN- klicken";
            }
            else
            {
                
                BuildInsertString();
                try
                {
                    com_insert.ExecuteNonQuery();
                    
                    if(comboBox1.SelectedItem.ToString() == "Mitarbeiter")
                    {
                        FillDataGrid(dataGridOutput, com_GetAllMitarbeiter);
                    }
                    else
                    {
                        FillDataGrid(dataGridOutput, com_GetAllOrganisation);
                    }
                    
                }
                catch(SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }
            
        }




        private void Button3_Click(object sender, RoutedEventArgs e)
        {   
            if(dataGridOutput.SelectedItem == null && dataGridOutput.SelectedItem.GetType() == typeof(DataRowView))
            {
                MessageBox.Show("Bitte die zu löschende Zeile auswählen");
                hilfeBox.Text = "Die zu löschende Zeile anklicken dann auf Eintrag löschen klicken";
            }
            else
            {
                if (selectedTable == "Mitarbeiter")
                {
                    DeleteEntry(((DataRowView)dataGridOutput.SelectedItem).Row["ID"].ToString());
                    FillDataGrid(dataGridOutput, com_GetAllMitarbeiter);
                }
                else
                {
                    DeleteEntry(((DataRowView)dataGridOutput.SelectedItem).Row["Name"].ToString());
                    FillDataGrid(dataGridOutput, com_GetAllOrganisation);
                }


            }
            
        }


       

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Mitarbeiter")
            {
                ModifyEntry(datagridRowDict, ((DataRowView)e.Row.Item).Row["ID"].ToString());
            }
            else
            {
                ModifyEntry(datagridRowDict, ((DataRowView)e.Row.Item).Row["Name"].ToString());
            }
            datagridRowDict.Clear();
            
        }

        

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {            
            Console.WriteLine(e.Column.Header.ToString()+"="+((TextBox)e.EditingElement).Text);
            datagridRowDict.Add(e.Column.Header.ToString(), ((TextBox)e.EditingElement).Text);            
            
        }

        
    }
}
