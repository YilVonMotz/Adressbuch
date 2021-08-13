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


namespace AdressbuckWPF
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
        private List<string> fields = new List<string>();
        public static List<string> availableFields = new List<string>();

        private List<Search> searchGroups = new List<Search>();

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
        
        private string tableName;

        




        public MainWindow()
        {

            
            InitializeComponent();

            try
            {
                connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
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



        private void FillDataGrid(SQLiteCommand command)
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




        private string CombineStrings(params string[] strings)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string item in strings)
            {
                sb.Append(item);
            }
            return sb.ToString();
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




        private void ComboBox1_DropDownClosed(object sender, EventArgs e)
        {
            RemoveAllSearchGroups();

            if(comboBox1.SelectedItem == null)
            {
                return;
            }

            string itemName = comboBox1.SelectedItem.ToString();
            tableName = itemName;

            switch (itemName)
            {
                case "Mitarbeiter":

                    com_GetAllMitarbeiter.CommandText = "select * from " + itemName;
                    try
                    {
                        reader = com_GetMitarbeiterFields.ExecuteReader();
                    }
                    catch(SQLiteException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                    FillDataGrid(com_GetAllMitarbeiter);
                   
                   
                    break;
                case "Organisation":

                    com_GetAllOrganisation.CommandText = "select * from " + itemName;
                    try
                    {
                        reader = com_GetOrganisationFields.ExecuteReader();
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                    FillDataGrid(com_GetAllOrganisation);
                    
                    break;
                default:
                    break;
            }

            fields.Clear();

            
            while (reader.Read())
            {
                string value = reader.GetString(0);

                if(comboBox1.SelectedItem.ToString() == "Mitarbeiter")
                {
                    mitarbeiterColumnNames.Add(value);
                }
                else
                {
                    organisationColumnNames.Add(value);
                }
                fields.Add(value);
            }

            availableFields.Clear();
            availableFields.AddRange(fields);

            try
            {
                reader.Close();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            AddSearchGroup();

        }



       
        private int GetEmptyComboBoxes(Search search)
        {
            int emptyCounts = 0;
            foreach (Search item in searchGroups)
            {
                if (item.GetComboBox().SelectedItem == null)
                    emptyCounts++;
            }

            return emptyCounts;
        }




        public void OnRemoveButtonClicked(Search _search)
        {
            
            if (GetEmptyComboBoxes(_search) <= 1 && _search.GetComboBox().SelectedItem == null)
                return;
            RemoveSearchGroup(_search);
        }




        public void OnComboBoxClosed(Search search)
        {
            //if (search.GetTextBox().Text == string.Empty)
            //    return;

            availableFields.Remove(search.GetComboBox().SelectedItem.ToString());

            if (GetEmptyComboBoxes(search) > 0)
                return;
            
            AddSearchGroup();
            
        }


        
        public int OnSearchTextEntry(Search search)
        {
            int result =  GetDBEntriesCount(comboBox1.SelectedItem.ToString(),search.GetComboBox().SelectedItem.ToString(), search.GetTextBox().Text );
            search.GetTextBlock().Text = result.ToString()+" Treffer";
            return result;
        }



        private void AddSearchGroup()
        {
            if (availableFields.Count > 0)
            {
                Search search = new Search(OnRemoveButtonClicked, OnComboBoxClosed, OnSearchTextEntry);
                StackPanel_SearchGroup.Children.Add(search);
                searchGroups.Add(search);
                
            }
            
        }



        private void RemoveSearchGroup(Search search_)
        {
            StackPanel_SearchGroup.Children.Remove(search_);
            searchGroups.Remove(search_);

            if(search_.comboBoxElement.SelectedItem != null)
            {
                availableFields.Add(search_.comboBoxElement.SelectedItem.ToString());
            }
            
        }



        private void RemoveAllSearchGroups()
        {            
            StackPanel_SearchGroup.Children.Clear();
            searchGroups.Clear();
        }
        


        private string BuildSelectString()
        {
            StringBuilder sb = new StringBuilder("select * from " + comboBox1.SelectedItem.ToString());

            for (int i = 0; i < searchGroups.Count; i++)
            {
                if(searchGroups[i].comboBoxElement.SelectedItem != null)
                {
                    if (i == 0)
                    {
                        if (searchGroups[i].textBoxElement.Text != string.Empty)
                        {
                            if(GetDBEntriesCount(comboBox1.SelectedItem.ToString() , searchGroups[i].GetComboBox().SelectedItem.ToString(), searchGroups[i].textBoxElement.Text) > 0)
                            {
                                sb.Append(" where " + searchGroups[i].comboBoxElement.SelectedItem.ToString() + " like '" + searchGroups[i].textBoxElement.Text + "'");
                            }
                            else 
                            {
                                sb.Append(" where " + searchGroups[i].comboBoxElement.SelectedItem.ToString() + " like '" + searchGroups[i].textBoxElement.Text + "%'");
                            }

                            
                        }
                    }
                    else
                    {
                        if (GetDBEntriesCount(comboBox1.SelectedItem.ToString(), searchGroups[i].GetComboBox().SelectedItem.ToString(), searchGroups[i].textBoxElement.Text) > 0)
                        {
                            sb.Append(" and " + searchGroups[i].comboBoxElement.SelectedItem.ToString() + " like '" + searchGroups[i].textBoxElement.Text + "'");
                        }
                        else
                        {
                            sb.Append(" and " + searchGroups[i].comboBoxElement.SelectedItem.ToString() + " like '" + searchGroups[i].textBoxElement.Text + "%'");
                        }
                    }
                }               
                
            }
            

            return sb.ToString();
        }
        


        private void BuildInsertString()
        {
            if(comboBox1.SelectedItem == null)
            {
                return;
            }
            StringBuilder insertString = new StringBuilder("Insert into "+comboBox1.SelectedItem);
            StringBuilder columnNames = new StringBuilder();
            StringBuilder values = new StringBuilder();

            for(int i = 0; i < searchGroups.Count; i++)
            {
                if (searchGroups[i].GetComboBox().SelectedItem == null)
                    continue;
                if (i > 0)
                {
                    columnNames.Append(",");
                    columnNames.Append("'"+searchGroups[i].GetComboBox().SelectedItem.ToString()+ "'");

                }
                else
                {
                    columnNames.Append("'"+searchGroups[i].GetComboBox().SelectedItem.ToString()+ "'");
                }
                
            }


            for (int i = 0; i < searchGroups.Count; i++)
            {
                if (searchGroups[i].GetComboBox().SelectedItem == null)
                    continue;
                if (i > 0)
                {
                    values.Append(",");
                    values.Append("'"+searchGroups[i].GetTextBox().Text+ "'");
                    

                }
                else
                {
                    values.Append("'"+searchGroups[i].GetTextBox().Text+ "'");
                }

            }

            insertString.Append("(" + columnNames.ToString() + ") VALUES (" + values.ToString()+")");
            com_insert.CommandText = insertString.ToString();



        }

       

        private void DeleteMitarbeiterEntry(string ID)
        {
            StringBuilder deleteString = new StringBuilder("delete from Mitarbeiter where ID = '" +ID+"'");
            com_delete.CommandText = deleteString.ToString();

            try
            {
                com_delete.ExecuteNonQuery();
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }            
            
        }



        private void DeleteOrganisationEntry(string name)
        {
            StringBuilder deleteString = new StringBuilder("delete from Organisation where name = '" + name + "'");
            com_delete.CommandText = deleteString.ToString();

            try
            {
                com_delete.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void ModifyEntry()
        {
            List<string> rowItems = new List<string>();
            object[] rowObjects = ((DataRowView)dataGrid.SelectedItem).Row.ItemArray;
            StringBuilder stringBuilder;

            foreach(Object item in rowObjects)
            {
                rowItems.Add(item.ToString());
            }
            
            if(comboBox1.SelectedItem.ToString() == "Mitarbeiter")
            {
                string pKeyValue = ((DataRowView)dataGrid.SelectedItem).Row["ID"].ToString();
                stringBuilder = new StringBuilder("Update Mitarbeiter SET ");

                for(int i = 0; i < rowItems.Count; i++)
                {
                    if (i < organisationColumnNames.Count - 1)
                    {
                        stringBuilder.Append(mitarbeiterColumnNames[i] + "='" + rowItems[i].ToString() + "',");
                    }
                    else
                    {
                        stringBuilder.Append(mitarbeiterColumnNames[i] + "='" + rowItems[i].ToString() + "'");
                    }
                }
                
            }
            else
            {
                string pKeyValue = ((DataRowView)dataGrid.SelectedItem).Row["Name"].ToString();
                stringBuilder = new StringBuilder("Update Organisation SET ");

                for (int i = 0; i < rowItems.Count; i++)
                {
                    if (i < organisationColumnNames.Count - 1)
                    {
                        stringBuilder.Append(organisationColumnNames[i] + "='" + rowItems[i].ToString() + "',");
                    }
                    else
                    {
                        stringBuilder.Append(organisationColumnNames[i] + "='" + rowItems[i].ToString() + "'");
                    }
                    
                }

            }

            try
            {
                com_update.CommandText = stringBuilder.ToString();
                com_update.ExecuteNonQuery();
                FillDataGrid(com_update);
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

            }
            else
            {
                com_select.CommandText = BuildSelectString();
                FillDataGrid(com_select);
            }
                
                 
        }



        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Bitte erst Suchkontext auswählen");

            }
            else
            {
                
                BuildInsertString();
                try
                {
                    com_insert.ExecuteNonQuery();
                    
                    if(comboBox1.SelectedItem.ToString() == "Mitarbeiter")
                    {
                        FillDataGrid(com_GetAllMitarbeiter);
                    }
                    else
                    {
                        FillDataGrid(com_GetAllOrganisation);
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
            if(dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte die zu löschende Zeile auswählen");
            }
            else
            {
                if(comboBox1.SelectedItem.ToString() == "Mitarbeiter")
                {
                    DeleteMitarbeiterEntry(((DataRowView)dataGrid.SelectedItem).Row["ID"].ToString());
                    FillDataGrid(com_GetAllMitarbeiter);
                }
                else
                {
                    DeleteOrganisationEntry(((DataRowView)dataGrid.SelectedItem).Row["Name"].ToString());
                    FillDataGrid(com_GetAllOrganisation);
                }

               
            }
            
        }




        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte die zu löschende Zeile auswählen");
            }
            else
            {
                if (comboBox1.SelectedItem.ToString() == "Mitarbeiter")
                {
                    DeleteMitarbeiterEntry(((DataRowView)dataGrid.SelectedItem).Row["ID"].ToString());
                    FillDataGrid(com_GetAllMitarbeiter);
                }
                else
                {
                    DeleteOrganisationEntry(((DataRowView)dataGrid.SelectedItem).Row["Name"].ToString());
                    FillDataGrid(com_GetAllOrganisation);
                }


            }

        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //int itemCount = ((DataRowView)((DataGrid)sender).SelectedItem).Row.ItemArray.Length;
            //for (int i = 0; i < itemCount; i++)
            //{
            //    Console.WriteLine(((DataRowView)((DataGrid)sender).SelectedItem).Row[i]);
            //}

            ModifyEntry();
            
        }
    }
}
